using Microsoft.EntityFrameworkCore;
using Orion.Application.Abstractions;
using Orion.Application.Estoque;
using Orion.Application.Fiscal;
using Orion.Infrastructure.Persistence;
using Orion.Infrastructure.Persistence.Entities;

namespace Orion.Infrastructure.Estoque;

public sealed class EntradaEstoqueService(
    OrionDbContext db,
    ICoreEstoqueClient core,
    ICoreIdentidadeClient identidade,
    IFornecedorNfe fornecedorNfe,
    ICurrentUserAccessor user,
    ICurrentUnidadeAccessor unidadeAtiva) : IEntradaEstoqueService
{
    private int EmpresaId => user.User.EmpresaId
        ?? throw new EstoqueRegraException("Sem empresa no token.");
    private int UsuarioId => user.User.UserId ?? 0;

    // ---------- Consultas auxiliares ----------

    public Task<IReadOnlyList<UnidadeCore>> ListarUnidadesAsync(CancellationToken ct = default) =>
        identidade.ListarUnidadesAsync(EmpresaId, ct);

    public async Task<IReadOnlyList<FornecedorDto>> BuscarFornecedoresAsync(string? search, CancellationToken ct = default)
    {
        var q = db.Fornecedores.Where(f => f.EmpresaId == EmpresaId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(f => f.Nome.ToLower().Contains(s) || (f.Cnpj != null && f.Cnpj.Contains(s)));
        }
        return await q.OrderBy(f => f.Nome).Take(20)
            .Select(f => new FornecedorDto(f.Id, f.Nome, f.Cnpj)).ToListAsync(ct);
    }

    public Task<IReadOnlyList<ProdutoCore>> BuscarProdutosAsync(string? search, CancellationToken ct = default) =>
        core.BuscarProdutosAsync(EmpresaId, search, ct);

    public Task<IReadOnlyList<SaldoCore>> ListarSaldosAsync(string? search, CancellationToken ct = default) =>
        core.ListarSaldosAsync(EmpresaId, unidadeAtiva.UnidadeId, search, ct);

    public async Task<ProdutoCore?> ResolverCodigoBarrasAsync(string codigoBarras, CancellationToken ct = default)
    {
        var cb = codigoBarras.Trim();
        var p = await core.BuscarProdutoPorCodigoBarrasAsync(EmpresaId, cb, ct);
        if (p is not null)
        {
            await AtualizarMapaAsync(cb, p.Id, ct);
        }
        return p;
    }

    public async Task<ProdutoCore> CriarProdutoAsync(NovoProdutoRequest req, CancellationToken ct = default)
    {
        var p = await core.CriarProdutoAsync(EmpresaId, new CriarProdutoCore(
            req.Codigo, req.Nome, req.UnidadeMedida, req.UnidadeCompra, req.FatorConversao,
            req.CodigoBarras, req.Categoria, req.EstoqueMinimo), ct);
        if (!string.IsNullOrWhiteSpace(req.CodigoBarras))
        {
            await AtualizarMapaAsync(req.CodigoBarras!, p.Id, ct);
        }
        return p;
    }

    // ---------- Criar / importar ----------

    public async Task<EntradaDto> CriarAsync(CriarEntradaRequest req, CancellationToken ct = default)
    {
        var existente = await db.EntradasEstoque.Include(e => e.Itens)
            .FirstOrDefaultAsync(e => e.ChaveIdempotencia == req.ChaveIdempotencia, ct);
        if (existente is not null)
        {
            return await ToDtoAsync(existente, ct);
        }

        var unidadeId = await ResolverUnidadeAsync(req.UnidadeId, ct);
        var temScanner = req.Itens.Any(i => !string.IsNullOrWhiteSpace(i.CodigoBarras));

        var entrada = new EntradaEstoque
        {
            EmpresaId = EmpresaId,
            UnidadeId = unidadeId,
            ChaveIdempotencia = req.ChaveIdempotencia,
            Tipo = req.Tipo,
            Origem = req.Origem,
            OrigemCadastro = temScanner ? OrigemCadastro.Scanner : OrigemCadastro.Manual,
            Status = StatusEntrada.Rascunho,
            DataEntrada = req.DataEntrada ?? DateTime.UtcNow,
            Observacao = Limitar(req.Observacao, 2000),
            CriadoPorUsuarioId = UsuarioId,
        };
        await AplicarFiscalAsync(entrada, req.Fiscal, ct);

        foreach (var it in req.Itens)
        {
            entrada.Itens.Add(await MontarItemAsync(it, ct));
        }

        db.EntradasEstoque.Add(entrada);
        await db.SaveChangesAsync(ct);
        entrada.ReferenciaExternaCore = $"orion:entrada:{entrada.Id}";
        await db.SaveChangesAsync(ct);
        return await ToDtoAsync(entrada, ct);
    }

    public async Task<EntradaDto> ImportarXmlAsync(Stream xml, int? unidadeId, CancellationToken ct = default)
    {
        using var reader = new StreamReader(xml);
        var texto = await reader.ReadToEndAsync(ct);
        var nfe = NfeXmlParser.Parse(texto);
        await ValidarDestinatarioAsync(nfe.DestinatarioCnpj, ct);
        return await CriarDeNfeAsync(nfe, texto, unidadeId, OrigemCadastro.XmlNfe, ct);
    }

    public async Task<(EntradaDto Entrada, bool Completa)> ImportarPorChaveAsync(string chave, int? unidadeId, CancellationToken ct = default)
    {
        chave = new string(chave.Where(char.IsDigit).ToArray());
        if (!NfeXmlParser.ChaveValida(chave))
        {
            throw new EstoqueRegraException("Chave de acesso inválida.");
        }

        var existente = await db.EntradasEstoque.Include(e => e.Itens)
            .FirstOrDefaultAsync(e => e.EmpresaId == EmpresaId && e.ChaveAcesso == chave, ct);
        if (existente is not null)
        {
            return (await ToDtoAsync(existente, ct), existente.Itens.Count > 0);
        }

        if (fornecedorNfe.Disponivel)
        {
            var nfe = await fornecedorNfe.ObterPorChaveAsync(chave, ct);
            await ValidarDestinatarioAsync(nfe.DestinatarioCnpj, ct);
            return (await CriarDeNfeAsync(nfe, null, unidadeId, OrigemCadastro.ApiFiscal, ct), true);
        }

        // Sem provedor: registra só a chave.
        var uid = await ResolverUnidadeAsync(unidadeId, ct);
        var stub = new EntradaEstoque
        {
            EmpresaId = EmpresaId,
            UnidadeId = uid,
            ChaveIdempotencia = Guid.NewGuid(),
            Tipo = TipoEntrada.Entrada,
            Origem = OrigemEntrada.Compra,
            OrigemCadastro = OrigemCadastro.ApiFiscal,
            Status = StatusEntrada.Rascunho,
            ChaveAcesso = chave,
            ModeloFiscal = chave.Substring(20, 2),
            DataEntrada = DateTime.UtcNow,
            CriadoPorUsuarioId = UsuarioId,
        };
        db.EntradasEstoque.Add(stub);
        await db.SaveChangesAsync(ct);
        stub.ReferenciaExternaCore = $"orion:entrada:{stub.Id}";
        await db.SaveChangesAsync(ct);
        return (await ToDtoAsync(stub, ct), false);
    }

    private async Task<EntradaDto> CriarDeNfeAsync(NfeImportadaDto nfe, string? xml, int? unidadeId, OrigemCadastro origemCad, CancellationToken ct)
    {
        var existente = await db.EntradasEstoque.Include(e => e.Itens)
            .FirstOrDefaultAsync(e => e.EmpresaId == EmpresaId && e.ChaveAcesso == nfe.Chave, ct);

        // Já confirmada/cancelada, ou já tem itens (XML já foi importado antes): idempotente, devolve como está.
        if (existente is not null && (existente.Status != StatusEntrada.Rascunho || existente.Itens.Count > 0))
        {
            return await ToDtoAsync(existente, ct);
        }

        // Rascunho "stub" criado por importar-chave sem provedor fiscal: preenche com os dados da NF agora obtidos.
        var entrada = existente ?? new EntradaEstoque
        {
            EmpresaId = EmpresaId,
            UnidadeId = await ResolverUnidadeAsync(unidadeId, ct),
            ChaveIdempotencia = Guid.NewGuid(),
            Status = StatusEntrada.Rascunho,
            DataEntrada = DateTime.UtcNow,
            CriadoPorUsuarioId = UsuarioId,
        };
        entrada.Tipo = TipoEntrada.Entrada;
        entrada.Origem = OrigemEntrada.Compra;
        entrada.OrigemCadastro = origemCad;
        entrada.ModeloFiscal = nfe.Modelo;
        entrada.ChaveAcesso = nfe.Chave;
        entrada.NumeroNf = Limitar(nfe.Numero, 20);
        entrada.Serie = Limitar(nfe.Serie, 10);
        entrada.FornecedorNome = Limitar(nfe.FornecedorNome, 200);
        entrada.FornecedorCnpj = nfe.FornecedorCnpj;
        entrada.DataEmissao = nfe.DataEmissao;
        entrada.ValorTotal = nfe.ValorTotal;
        entrada.XmlOriginal = xml ?? entrada.XmlOriginal;
        entrada.FornecedorId = await UpsertFornecedorAsync(nfe.FornecedorNome, nfe.FornecedorCnpj, ct);

        foreach (var it in nfe.Itens)
        {
            var item = new EntradaEstoqueItem
            {
                DescricaoNf = Limitar(it.DescricaoNf, 300),
                CodigoBarrasNf = it.CodigoBarras,
                Ncm = Limitar(it.Ncm, 10),
                UnidadeComercialNf = Limitar(it.UnidadeComercial, 10),
                QuantidadeNf = it.Quantidade,
                QuantidadeRecebida = it.Quantidade,
                ValorUnitarioNf = it.ValorUnitario,
                FatorConversao = 1m,
                StatusVinculo = StatusVinculoItem.Pendente,
            };
            if (!string.IsNullOrWhiteSpace(it.CodigoBarras))
            {
                var p = await core.BuscarProdutoPorCodigoBarrasAsync(EmpresaId, it.CodigoBarras!, ct);
                if (p is not null)
                {
                    VincularItem(item, p);
                }
            }
            entrada.Itens.Add(item);
        }

        if (existente is null)
        {
            db.EntradasEstoque.Add(entrada);
        }
        await db.SaveChangesAsync(ct);
        if (string.IsNullOrEmpty(entrada.ReferenciaExternaCore))
        {
            entrada.ReferenciaExternaCore = $"orion:entrada:{entrada.Id}";
            await db.SaveChangesAsync(ct);
        }
        return await ToDtoAsync(entrada, ct);
    }

    // ---------- Edição do rascunho ----------

    public async Task<EntradaDto?> ObterAsync(long id, CancellationToken ct = default)
    {
        var e = await CarregarAsync(id, ct);
        return e is null ? null : await ToDtoAsync(e, ct);
    }

    public async Task<EntradaDto> AtualizarAsync(long id, AtualizarEntradaRequest req, CancellationToken ct = default)
    {
        var e = await ExigirRascunhoAsync(id, ct);
        if (req.Tipo is { } t) e.Tipo = t;
        if (req.Origem is { } o) e.Origem = o;
        if (req.Observacao is not null) e.Observacao = Limitar(req.Observacao, 2000);
        await AplicarFiscalAsync(e, req.Fiscal, ct);
        await db.SaveChangesAsync(ct);
        return await ToDtoAsync(e, ct);
    }

    public async Task<EntradaDto> AtualizarItemAsync(long id, long itemId, AtualizarItemRequest req, CancellationToken ct = default)
    {
        var e = await ExigirRascunhoAsync(id, ct);
        var item = e.Itens.FirstOrDefault(i => i.Id == itemId)
            ?? throw new EstoqueRegraException("Item não encontrado.", 404);

        if (req.NovoProduto is not null)
        {
            var p = await CriarProdutoAsync(req.NovoProduto, ct);
            VincularItem(item, p, novo: true);
        }
        else if (req.ProdutoCoreId is { } pid)
        {
            var p = (await core.BuscarProdutosAsync(EmpresaId, null, ct)).FirstOrDefault(x => x.Id == pid)
                ?? throw new EstoqueRegraException("Produto não encontrado no catálogo.", 404);
            VincularItem(item, p);
        }
        if (req.QuantidadeRecebida is { } q) item.QuantidadeRecebida = q;
        if (req.FatorConversao is { } f) item.FatorConversao = f <= 0 ? 1m : f;
        if (req.ValorUnitarioNf is { } v) item.ValorUnitarioNf = v < 0 ? null : v;
        if (req.MotivoDivergencia is not null) item.MotivoDivergencia = Limitar(req.MotivoDivergencia, 300);

        await db.SaveChangesAsync(ct);
        return await ToDtoAsync(e, ct);
    }

    public async Task<EntradaDto> RemoverItemAsync(long id, long itemId, CancellationToken ct = default)
    {
        var e = await ExigirRascunhoAsync(id, ct);
        var item = e.Itens.FirstOrDefault(i => i.Id == itemId)
            ?? throw new EstoqueRegraException("Item não encontrado.", 404);
        e.Itens.Remove(item);
        db.EntradasEstoqueItens.Remove(item);
        await db.SaveChangesAsync(ct);
        return await ToDtoAsync(e, ct);
    }

    // ---------- Confirmar / cancelar ----------

    public async Task<EntradaDto> ConfirmarAsync(long id, CancellationToken ct = default)
    {
        var e = await CarregarAsync(id, ct) ?? throw new EstoqueRegraException("Entrada não encontrada.", 404);
        if (e.Status == StatusEntrada.Confirmada)
        {
            return await ToDtoAsync(e, ct);
        }
        if (e.Status == StatusEntrada.Cancelada)
        {
            throw new EstoqueRegraException("Entrada cancelada.");
        }
        if (e.Itens.Count == 0)
        {
            throw new EstoqueRegraException("Entrada sem itens.");
        }
        var pendentes = e.Itens.Where(i => i.ProdutoCoreId is null).Select(i => i.DescricaoNf ?? i.CodigoBarrasNf ?? $"#{i.Id}").ToList();
        if (pendentes.Count > 0)
        {
            throw new EstoqueRegraException("Itens sem produto vinculado: " + string.Join(", ", pendentes), 422);
        }

        var (tipo, origem) = MapearParaCore(e);
        var itensCore = e.Itens.Select(i => new EntradaLoteItemCore(
            i.ProdutoCoreId!.Value, i.QuantidadeEstoque, i.CustoUnitarioEstoque,
            $"{e.ReferenciaExternaCore}#{i.ProdutoCoreId}")).ToList();

        var movs = await core.RegistrarEntradasLoteAsync(
            e.EmpresaId, e.UnidadeId, tipo, origem, e.NumeroNf, e.ReferenciaExternaCore, itensCore, ct);

        var porProduto = movs.ToDictionary(m => m.ProdutoId);
        foreach (var i in e.Itens)
        {
            if (porProduto.TryGetValue(i.ProdutoCoreId!.Value, out var m))
            {
                i.MovimentacaoCoreId = m.Id;
            }
            if (!string.IsNullOrWhiteSpace(i.CodigoBarrasNf))
            {
                await AtualizarMapaAsync(i.CodigoBarrasNf!, i.ProdutoCoreId!.Value, ct);
            }
        }
        e.Status = StatusEntrada.Confirmada;
        e.ConfirmadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await ToDtoAsync(e, ct);
    }

    public async Task<EntradaDto> CancelarAsync(long id, CancellationToken ct = default)
    {
        var e = await CarregarAsync(id, ct) ?? throw new EstoqueRegraException("Entrada não encontrada.", 404);
        if (e.Status != StatusEntrada.Rascunho)
        {
            throw new EstoqueRegraException("Só é possível cancelar entradas em rascunho.");
        }
        e.Status = StatusEntrada.Cancelada;
        e.CanceladoPorUsuarioId = UsuarioId;
        e.CanceladoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await ToDtoAsync(e, ct);
    }

    public async Task<IReadOnlyList<EntradaDto>> ListarAsync(
        StatusEntrada? status, OrigemEntrada? origem, long? fornecedorId,
        DateTime? de, DateTime? ate, int page, int size, CancellationToken ct = default)
    {
        var q = db.EntradasEstoque.Include(e => e.Itens).Where(e => e.EmpresaId == EmpresaId);
        if (status is { } s) q = q.Where(e => e.Status == s);
        if (origem is { } o) q = q.Where(e => e.Origem == o);
        if (fornecedorId is { } f) q = q.Where(e => e.FornecedorId == f);
        if (de is { } d) q = q.Where(e => e.DataEntrada >= d);
        if (ate is { } a) q = q.Where(e => e.DataEntrada <= a);

        var lista = await q.OrderByDescending(e => e.DataEntrada)
            .Skip(Math.Max(page, 0) * Math.Clamp(size, 1, 100)).Take(Math.Clamp(size, 1, 100))
            .ToListAsync(ct);
        return lista.Select(MapDto).ToList();
    }

    // ---------- Helpers ----------

    private static (TipoEntrada tipo, OrigemEntrada origem) MapearParaCore(EntradaEstoque e) =>
        e.Tipo == TipoEntrada.Ajuste
            ? (TipoEntrada.Ajuste, OrigemEntrada.Inventario)
            : (TipoEntrada.Entrada, e.Origem == OrigemEntrada.Compra ? OrigemEntrada.Compra : OrigemEntrada.Manual);

    private async Task<int> ResolverUnidadeAsync(int? pedido, CancellationToken ct)
    {
        var unidadeId = pedido ?? unidadeAtiva.UnidadeId;
        var unidades = await identidade.ListarUnidadesAsync(EmpresaId, ct);
        if (unidadeId is null)
        {
            if (unidades.Count == 1)
            {
                return unidades[0].Id;
            }
            throw new EstoqueRegraException("Informe a unidade (X-Unidade-Id ou unidadeId).");
        }
        if (unidades.All(u => u.Id != unidadeId))
        {
            throw new EstoqueRegraException("Unidade não pertence à empresa.", 403);
        }
        return unidadeId.Value;
    }

    private async Task ValidarDestinatarioAsync(string? cnpjNfe, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cnpjNfe))
        {
            return;
        }
        var empresa = await identidade.BuscarEmpresaAsync(EmpresaId, ct);
        if (empresa is not null && !string.IsNullOrWhiteSpace(empresa.Cnpj)
            && new string(empresa.Cnpj.Where(char.IsDigit).ToArray()) != cnpjNfe)
        {
            throw new EstoqueRegraException("O destinatário da NF-e não é o CNPJ desta empresa.", 422);
        }
    }

    private async Task AplicarFiscalAsync(EntradaEstoque e, DadosFiscaisRequest? f, CancellationToken ct)
    {
        if (f is null)
        {
            return;
        }
        e.NumeroNf = Limitar(f.NumeroNf, 20);
        e.Serie = Limitar(f.Serie, 10);
        e.DataEmissao = f.DataEmissao;
        if (!string.IsNullOrWhiteSpace(f.ChaveAcesso))
        {
            var chave = new string(f.ChaveAcesso!.Where(char.IsDigit).ToArray());
            if (NfeXmlParser.ChaveValida(chave))
            {
                e.ChaveAcesso = chave;
                e.ModeloFiscal = chave.Substring(20, 2);
            }
        }
        if (f.FornecedorId is { } fid)
        {
            var forn = await db.Fornecedores.FirstOrDefaultAsync(x => x.Id == fid && x.EmpresaId == EmpresaId, ct);
            if (forn is not null)
            {
                e.FornecedorId = forn.Id;
                e.FornecedorNome = forn.Nome;
                e.FornecedorCnpj = forn.Cnpj;
            }
        }
        else if (!string.IsNullOrWhiteSpace(f.FornecedorNome))
        {
            e.FornecedorNome = Limitar(f.FornecedorNome, 200);
            e.FornecedorCnpj = f.FornecedorCnpj is null ? null : new string(f.FornecedorCnpj.Where(char.IsDigit).ToArray());
            e.FornecedorId = await UpsertFornecedorAsync(f.FornecedorNome, e.FornecedorCnpj, ct);
        }
    }

    private async Task<long?> UpsertFornecedorAsync(string? nome, string? cnpj, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(nome) && string.IsNullOrWhiteSpace(cnpj))
        {
            return null;
        }
        cnpj = cnpj is null ? null : new string(cnpj.Where(char.IsDigit).ToArray());
        Fornecedor? f = null;
        if (!string.IsNullOrWhiteSpace(cnpj))
        {
            f = await db.Fornecedores.FirstOrDefaultAsync(x => x.EmpresaId == EmpresaId && x.Cnpj == cnpj, ct);
        }
        if (f is null)
        {
            f = new Fornecedor { EmpresaId = EmpresaId, Nome = Limitar(nome, 200) ?? cnpj!, Cnpj = cnpj };
            db.Fornecedores.Add(f);
            await db.SaveChangesAsync(ct);
        }
        else if (!string.IsNullOrWhiteSpace(nome) && f.Nome != nome)
        {
            f.Nome = Limitar(nome, 200)!;
            f.AtualizadoEm = DateTime.UtcNow;
        }
        return f.Id;
    }

    private async Task<EntradaEstoqueItem> MontarItemAsync(ItemEntradaRequest it, CancellationToken ct)
    {
        var item = new EntradaEstoqueItem
        {
            CodigoBarrasNf = string.IsNullOrWhiteSpace(it.CodigoBarras) ? null : it.CodigoBarras!.Trim(),
            UnidadeComercialNf = Limitar(it.UnidadeComercialNf, 10),
            QuantidadeNf = it.QuantidadeNf,
            QuantidadeRecebida = it.QuantidadeRecebida,
            ValorUnitarioNf = it.ValorUnitarioNf is < 0 ? null : it.ValorUnitarioNf,
            FatorConversao = it.FatorConversao is { } f and > 0 ? f : 1m,
            StatusVinculo = StatusVinculoItem.Pendente,
        };
        if (it.NovoProduto is not null)
        {
            var p = await CriarProdutoAsync(it.NovoProduto, ct);
            VincularItem(item, p, novo: true);
        }
        else if (it.ProdutoCoreId is { } pid)
        {
            var p = (await core.BuscarProdutosAsync(EmpresaId, null, ct)).FirstOrDefault(x => x.Id == pid);
            if (p is not null) VincularItem(item, p);
            else { item.ProdutoCoreId = pid; item.StatusVinculo = StatusVinculoItem.Vinculado; }
        }
        else if (item.CodigoBarrasNf is not null)
        {
            var p = await core.BuscarProdutoPorCodigoBarrasAsync(EmpresaId, item.CodigoBarrasNf, ct);
            if (p is not null) VincularItem(item, p);
        }
        return item;
    }

    private static void VincularItem(EntradaEstoqueItem item, ProdutoCore p, bool novo = false)
    {
        item.ProdutoCoreId = p.Id;
        item.ProdutoCodigo = p.Codigo;
        item.StatusVinculo = novo ? StatusVinculoItem.ProdutoNovo : StatusVinculoItem.Vinculado;
        if (item.FatorConversao <= 1m && p.FatorConversao is { } fc and > 0)
        {
            item.FatorConversao = fc;
        }
        if (string.IsNullOrWhiteSpace(item.CodigoBarrasNf) && !string.IsNullOrWhiteSpace(p.CodigoBarras))
        {
            item.CodigoBarrasNf = p.CodigoBarras;
        }
    }

    private async Task AtualizarMapaAsync(string codigoBarras, int produtoId, CancellationToken ct)
    {
        var cb = codigoBarras.Trim();
        var m = await db.MapaCodigoBarras.FirstOrDefaultAsync(x => x.EmpresaId == EmpresaId && x.CodigoBarras == cb, ct);
        if (m is null)
        {
            db.MapaCodigoBarras.Add(new MapaCodigoBarras { EmpresaId = EmpresaId, CodigoBarras = cb, ProdutoCoreId = produtoId });
        }
        else
        {
            m.ProdutoCoreId = produtoId;
            m.AtualizadoEm = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
    }

    private Task<EntradaEstoque?> CarregarAsync(long id, CancellationToken ct) =>
        db.EntradasEstoque.Include(e => e.Itens)
            .FirstOrDefaultAsync(e => e.Id == id && e.EmpresaId == EmpresaId, ct);

    private async Task<EntradaEstoque> ExigirRascunhoAsync(long id, CancellationToken ct)
    {
        var e = await CarregarAsync(id, ct) ?? throw new EstoqueRegraException("Entrada não encontrada.", 404);
        if (e.Status != StatusEntrada.Rascunho)
        {
            throw new EstoqueRegraException("Entrada não está em rascunho.");
        }
        return e;
    }

    private async Task<EntradaDto> ToDtoAsync(EntradaEstoque e, CancellationToken ct)
    {
        Dictionary<int, decimal>? saldos = null;
        if (e.Status == StatusEntrada.Confirmada)
        {
            saldos = new();
            foreach (var pid in e.Itens.Where(i => i.ProdutoCoreId is not null).Select(i => i.ProdutoCoreId!.Value).Distinct())
            {
                try { saldos[pid] = (await core.ConsultarSaldoAsync(pid, e.UnidadeId, ct)).Quantidade; }
                catch { /* saldo é informativo */ }
            }
        }
        return MapDto(e, saldos);
    }

    private static EntradaDto MapDto(EntradaEstoque e) => MapDto(e, null);

    private static EntradaDto MapDto(EntradaEstoque e, Dictionary<int, decimal>? saldos) => new(
        e.Id, e.ChaveIdempotencia, e.EmpresaId, e.UnidadeId,
        Contrato(e.Tipo), Contrato(e.Origem), Contrato(e.OrigemCadastro), Contrato(e.Status),
        e.FornecedorId, e.FornecedorNome, e.FornecedorCnpj, e.ModeloFiscal, e.ChaveAcesso, e.NumeroNf, e.Serie,
        e.DataEmissao, e.DataEntrada, e.ValorTotal, e.Observacao, e.CriadoEm, e.ConfirmadoEm,
        e.Itens.OrderBy(i => i.Id).Select(i => new ItemDto(
            i.Id, i.ProdutoCoreId, i.ProdutoCodigo, i.DescricaoNf, i.CodigoBarrasNf, i.Ncm, i.UnidadeComercialNf,
            i.QuantidadeNf, i.QuantidadeRecebida, i.FatorConversao, i.QuantidadeEstoque, i.ValorUnitarioNf,
            i.CustoUnitarioEstoque, i.Divergencia, i.MotivoDivergencia, Contrato(i.StatusVinculo),
            i.MovimentacaoCoreId,
            i.ProdutoCoreId is { } pid && saldos is not null && saldos.TryGetValue(pid, out var s) ? s : null))
            .ToList());

    /// <summary>PascalCase do enum → SCREAMING_SNAKE_CASE do contrato da API (ex.: XmlNfe -> XML_NFE), alinhado ao Core.</summary>
    private static string Contrato(Enum valor)
    {
        var nome = valor.ToString();
        var sb = new System.Text.StringBuilder(nome.Length + 4);
        for (int i = 0; i < nome.Length; i++)
        {
            if (i > 0 && char.IsUpper(nome[i]))
            {
                sb.Append('_');
            }
            sb.Append(char.ToUpperInvariant(nome[i]));
        }
        return sb.ToString();
    }

    private static string? Limitar(string? s, int max)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }
        s = s.Trim();
        return s.Length > max ? s[..max] : s;
    }
}

public sealed class EstoqueRegraException(string message, int status = 400) : Exception(message)
{
    public int Status { get; } = status;
}
