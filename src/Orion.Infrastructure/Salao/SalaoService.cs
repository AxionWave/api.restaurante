using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Orion.Application.Salao;
using Orion.Infrastructure.Persistence;
using Orion.Infrastructure.Persistence.Entities;

namespace Orion.Infrastructure.Salao;

public sealed class SalaoService(OrionDbContext db) : ISalaoService
{
    public async Task<IReadOnlyList<AmbienteDto>> ListarAmbientesAsync(int empresaId, bool somenteAtivos, CancellationToken ct)
    {
        var q = db.Ambientes.AsNoTracking().Where(a => a.EmpresaId == empresaId);
        if (somenteAtivos) q = q.Where(a => a.Ativo);
        return await q.OrderBy(a => a.Ordem).ThenBy(a => a.Nome)
            .Select(a => new AmbienteDto(a.Id, a.Nome, a.Ordem, a.Ativo))
            .ToListAsync(ct);
    }

    public async Task<AmbienteDto> SalvarAmbienteAsync(int empresaId, long? id, AmbienteRequest req, CancellationToken ct)
    {
        var nome = Obrigatorio(req.Nome, "Dê um nome ao ambiente.");
        Ambiente row;
        if (id is null)
        {
            row = new Ambiente { EmpresaId = empresaId };
            db.Ambientes.Add(row);
        }
        else
        {
            row = await db.Ambientes.FirstOrDefaultAsync(a => a.Id == id && a.EmpresaId == empresaId, ct)
                ?? throw new SalaoRegraException("Ambiente não encontrado.", 404);
        }
        row.Nome = nome;
        row.Ordem = req.Ordem;
        row.Ativo = req.Ativo;
        await SalvarAsync(ct);
        return new AmbienteDto(row.Id, row.Nome, row.Ordem, row.Ativo);
    }

    public async Task<IReadOnlyList<MesaDto>> ListarMesasAsync(int empresaId, CancellationToken ct)
    {
        return await db.Mesas.AsNoTracking()
            .Where(m => m.EmpresaId == empresaId)
            .OrderBy(m => m.Ambiente!.Ordem).ThenBy(m => m.Rotulo)
            .Select(m => new MesaDto(m.Id, m.AmbienteId, m.Ambiente!.Nome, m.Rotulo, m.LugaresPadrao, m.Forma, m.Situacao))
            .ToListAsync(ct);
    }

    public async Task<MesaDto> SalvarMesaAsync(int empresaId, long? id, MesaRequest req, CancellationToken ct)
    {
        var rotulo = Obrigatorio(req.Rotulo, "Dê um número ou nome à mesa.");
        if (req.LugaresPadrao < 1) throw new SalaoRegraException("A mesa precisa de ao menos um lugar.");
        var ambiente = await db.Ambientes.FirstOrDefaultAsync(a => a.Id == req.AmbienteId && a.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Ambiente não encontrado.", 404);
        Mesa row;
        if (id is null)
        {
            row = new Mesa { EmpresaId = empresaId };
            db.Mesas.Add(row);
        }
        else
        {
            row = await db.Mesas.FirstOrDefaultAsync(m => m.Id == id && m.EmpresaId == empresaId, ct)
                ?? throw new SalaoRegraException("Mesa não encontrada.", 404);
            if (row.AtendimentoAbertoId is not null && req.Situacao == SituacaoMesa.Bloqueada)
                throw new SalaoRegraException("Feche a visita antes de bloquear esta mesa.");
        }
        row.AmbienteId = ambiente.Id;
        row.Rotulo = rotulo;
        row.LugaresPadrao = req.LugaresPadrao;
        row.Forma = req.Forma;
        row.Situacao = req.Situacao;
        await SalvarAsync(ct);
        return new MesaDto(row.Id, row.AmbienteId, ambiente.Nome, row.Rotulo, row.LugaresPadrao, row.Forma, row.Situacao);
    }

    public async Task<IReadOnlyList<CartaCategoriaDto>> ListarCategoriasAsync(int empresaId, bool somenteAtivas, CancellationToken ct)
    {
        var q = db.CartaCategorias.AsNoTracking().Where(c => c.EmpresaId == empresaId);
        if (somenteAtivas) q = q.Where(c => c.Ativo);
        return await q.OrderBy(c => c.Ordem).ThenBy(c => c.Nome)
            .Select(c => new CartaCategoriaDto(c.Id, c.Nome, c.Ordem, c.Ativo))
            .ToListAsync(ct);
    }

    public async Task<CartaCategoriaDto> SalvarCategoriaAsync(int empresaId, long? id, CartaCategoriaRequest req, CancellationToken ct)
    {
        var nome = Obrigatorio(req.Nome, "Dê um nome à categoria.");
        CartaCategoria row;
        if (id is null)
        {
            row = new CartaCategoria { EmpresaId = empresaId };
            db.CartaCategorias.Add(row);
        }
        else
        {
            row = await db.CartaCategorias.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId, ct)
                ?? throw new SalaoRegraException("Categoria não encontrada.", 404);
        }
        row.Nome = nome;
        row.Ordem = req.Ordem;
        row.Ativo = req.Ativo;
        await SalvarAsync(ct);
        return new CartaCategoriaDto(row.Id, row.Nome, row.Ordem, row.Ativo);
    }

    public async Task<IReadOnlyList<CartaItemDto>> ListarItensCartaAsync(int empresaId, bool somenteAtivos, CancellationToken ct)
    {
        var q = db.CartaItens.AsNoTracking().Where(i => i.EmpresaId == empresaId);
        if (somenteAtivos) q = q.Where(i => i.Ativo && i.Categoria!.Ativo);
        return await q.OrderBy(i => i.Categoria!.Ordem).ThenBy(i => i.Nome)
            .Select(i => new CartaItemDto(i.Id, i.CategoriaId, i.Categoria!.Nome, i.Nome, i.Preco, i.Destino, i.Ativo))
            .ToListAsync(ct);
    }

    public async Task<CartaItemDto> SalvarItemCartaAsync(int empresaId, long? id, CartaItemRequest req, CancellationToken ct)
    {
        var nome = Obrigatorio(req.Nome, "Dê um nome ao prato.");
        if (req.Preco < 0) throw new SalaoRegraException("O preço não pode ser negativo.");
        var categoria = await db.CartaCategorias.FirstOrDefaultAsync(c => c.Id == req.CategoriaId && c.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Categoria não encontrada.", 404);
        CartaItem row;
        if (id is null)
        {
            row = new CartaItem { EmpresaId = empresaId };
            db.CartaItens.Add(row);
        }
        else
        {
            row = await db.CartaItens.FirstOrDefaultAsync(i => i.Id == id && i.EmpresaId == empresaId, ct)
                ?? throw new SalaoRegraException("Prato não encontrado.", 404);
        }
        row.CategoriaId = categoria.Id;
        row.Nome = nome;
        row.Preco = req.Preco;
        row.Destino = req.Destino;
        row.Ativo = req.Ativo;
        await SalvarAsync(ct);
        return new CartaItemDto(row.Id, row.CategoriaId, categoria.Nome, row.Nome, row.Preco, row.Destino, row.Ativo);
    }

    public async Task<IReadOnlyList<MapaAmbienteDto>> MapaAsync(int empresaId, CancellationToken ct)
    {
        var ambientes = await db.Ambientes.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.Ativo)
            .OrderBy(a => a.Ordem).ThenBy(a => a.Nome)
            .ToListAsync(ct);
        var mesas = await db.Mesas.AsNoTracking()
            .Where(m => m.EmpresaId == empresaId)
            .OrderBy(m => m.Rotulo)
            .ToListAsync(ct);
        var abertos = mesas.Where(m => m.AtendimentoAbertoId is not null).Select(m => m.AtendimentoAbertoId!.Value).Distinct().ToList();
        var visitas = abertos.Count == 0
            ? new Dictionary<long, Atendimento>()
            : await db.Atendimentos.AsNoTracking()
                .Include(a => a.Lugares)
                .Where(a => abertos.Contains(a.Id) && a.EmpresaId == empresaId)
                .ToDictionaryAsync(a => a.Id, ct);

        return ambientes.Select(amb => new MapaAmbienteDto(
            amb.Id,
            amb.Nome,
            amb.Ordem,
            mesas.Where(m => m.AmbienteId == amb.Id).Select(m =>
            {
                visitas.TryGetValue(m.AtendimentoAbertoId ?? 0, out var visita);
                var lugares = visita?.Lugares.OrderBy(l => l.Ordem).ToList() ?? [];
                return new MapaMesaDto(
                    m.Id,
                    m.AmbienteId,
                    m.Rotulo,
                    m.LugaresPadrao,
                    m.Forma,
                    m.Situacao,
                    visita?.Id,
                    visita?.Status,
                    visita?.AbertoPorNome,
                    visita is null ? m.LugaresPadrao : lugares.Count,
                    lugares.Select(l => new MapaLugarDto(l.Id, l.Ordem, l.NomeCliente, l.Ocupado, l.ContaFechada)).ToList());
            }).ToList()
        )).ToList();
    }

    public async Task<IReadOnlyList<AtendimentoDto>> ListarAbertosAsync(int empresaId, CancellationToken ct)
    {
        var ids = await db.Atendimentos.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.Status != StatusAtendimento.Fechada)
            .OrderByDescending(a => a.AbertoEm)
            .Select(a => a.Id)
            .ToListAsync(ct);
        var lista = new List<AtendimentoDto>(ids.Count);
        foreach (var id in ids)
            lista.Add(await ObterAtendimentoAsync(empresaId, id, ct));
        return lista;
    }

    public async Task<AtendimentoDto> ObterAtendimentoAsync(int empresaId, long id, CancellationToken ct)
    {
        var atendimento = await CarregarAsync(empresaId, id, ct);
        return Projetar(atendimento, await MesasDoAtendimentoAsync(atendimento.Id, ct));
    }

    public async Task<AtendimentoDto> AbrirAsync(int empresaId, long mesaId, int usuarioId, string nome, CancellationToken ct)
    {
        var mesa = await db.Mesas.Include(m => m.Ambiente)
            .FirstOrDefaultAsync(m => m.Id == mesaId && m.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Mesa não encontrada.", 404);
        if (mesa.Situacao == SituacaoMesa.Bloqueada)
            throw new SalaoRegraException("Esta mesa está bloqueada.");
        if (mesa.AtendimentoAbertoId is not null)
            throw new SalaoRegraException("Esta mesa já tem uma visita aberta.", 409);

        await using var tx = await BeginTxAsync(ct);
        var atendimento = new Atendimento
        {
            EmpresaId = empresaId,
            MesaId = mesa.Id,
            AbertoPorUsuarioId = usuarioId,
            AbertoPorNome = nome,
            AbertoEm = DateTime.UtcNow,
            Status = StatusAtendimento.Aberta
        };
        for (var i = 1; i <= mesa.LugaresPadrao; i++)
            atendimento.Lugares.Add(new AtendimentoLugar { Ordem = i });
        db.Atendimentos.Add(atendimento);
        await db.SaveChangesAsync(ct);

        var claimed = await ClaimMesaAsync(mesa.Id, atendimento.Id, ct);
        if (claimed == 0)
            throw new SalaoRegraException("Esta mesa já tem uma visita aberta.", 409);

        if (tx is not null) await tx.CommitAsync(ct);
        db.ChangeTracker.Clear();
        return await ObterAtendimentoAsync(empresaId, atendimento.Id, ct);
    }

    public async Task<AtendimentoDto> PedirContaAsync(int empresaId, long id, CancellationToken ct)
    {
        var atendimento = await CarregarAsync(empresaId, id, ct);
        ExigirAberto(atendimento);
        atendimento.Status = StatusAtendimento.Conta;
        await db.SaveChangesAsync(ct);
        return Projetar(atendimento, await MesasDoAtendimentoAsync(id, ct));
    }

    public async Task<AtendimentoDto> ReabrirAsync(int empresaId, long id, CancellationToken ct)
    {
        var atendimento = await CarregarAsync(empresaId, id, ct);
        if (atendimento.Status == StatusAtendimento.Fechada)
            throw new SalaoRegraException("Esta visita já foi fechada.");
        atendimento.Status = StatusAtendimento.Aberta;
        await db.SaveChangesAsync(ct);
        return Projetar(atendimento, await MesasDoAtendimentoAsync(id, ct));
    }

    public async Task<AtendimentoDto> AdicionarLugarAsync(int empresaId, long id, CancellationToken ct)
    {
        var atendimento = await CarregarAsync(empresaId, id, ct);
        ExigirAberto(atendimento);
        var ordem = atendimento.Lugares.Count == 0 ? 1 : atendimento.Lugares.Max(l => l.Ordem) + 1;
        atendimento.Lugares.Add(new AtendimentoLugar { Ordem = ordem });
        await db.SaveChangesAsync(ct);
        return Projetar(atendimento, await MesasDoAtendimentoAsync(id, ct));
    }

    public async Task<AtendimentoDto> RemoverLugarAsync(int empresaId, long lugarId, CancellationToken ct)
    {
        var lugar = await db.AtendimentoLugares.Include(l => l.Atendimento).ThenInclude(a => a!.Lugares)
            .FirstOrDefaultAsync(l => l.Id == lugarId && l.Atendimento!.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Lugar não encontrado.", 404);
        var atendimento = lugar.Atendimento!;
        ExigirAberto(atendimento);
        if (atendimento.Lugares.Count <= 1)
            throw new SalaoRegraException("A visita precisa manter ao menos um lugar.");
        var temItem = await db.ComandaItens.AnyAsync(i => i.LugarId == lugarId && i.Status != StatusItem.Cancelado, ct);
        if (temItem)
            throw new SalaoRegraException("Transfira ou cancele os pratos deste lugar antes de removê-lo.");
        db.AtendimentoLugares.Remove(lugar);
        await LimparGruposOrfaosAsync(atendimento.Id, ct);
        await db.SaveChangesAsync(ct);
        return await ObterAtendimentoAsync(empresaId, atendimento.Id, ct);
    }

    public async Task<AtendimentoDto> NomearLugarAsync(int empresaId, long lugarId, NomearLugarRequest req, CancellationToken ct)
    {
        var lugar = await db.AtendimentoLugares.Include(l => l.Atendimento)
            .FirstOrDefaultAsync(l => l.Id == lugarId && l.Atendimento!.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Lugar não encontrado.", 404);
        ExigirAberto(lugar.Atendimento!);
        var nome = req.NomeCliente?.Trim();
        lugar.NomeCliente = string.IsNullOrEmpty(nome) ? null : nome;
        lugar.Ocupado = lugar.NomeCliente is not null;
        await db.SaveChangesAsync(ct);
        return await ObterAtendimentoAsync(empresaId, lugar.AtendimentoId, ct);
    }

    public async Task<AtendimentoDto> FecharLugarAsync(int empresaId, long lugarId, CancellationToken ct)
    {
        var lugar = await db.AtendimentoLugares.Include(l => l.Atendimento)
            .FirstOrDefaultAsync(l => l.Id == lugarId && l.Atendimento!.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Lugar não encontrado.", 404);
        ExigirAberto(lugar.Atendimento!);
        lugar.ContaFechada = true;
        await db.SaveChangesAsync(ct);
        return await ObterAtendimentoAsync(empresaId, lugar.AtendimentoId, ct);
    }

    public async Task<AtendimentoDto> LancarItemAsync(int empresaId, long atendimentoId, int usuarioId, string nome, LancarItemRequest req, CancellationToken ct)
    {
        var atendimento = await CarregarAsync(empresaId, atendimentoId, ct);
        ExigirAberto(atendimento);
        if (req.Quantidade < 1) throw new SalaoRegraException("A quantidade precisa ser ao menos 1.");
        var prato = await db.CartaItens.FirstOrDefaultAsync(i => i.Id == req.CartaItemId && i.EmpresaId == empresaId && i.Ativo, ct)
            ?? throw new SalaoRegraException("Prato não encontrado na carta.", 404);
        var lugar = ExigirLugarNomeado(atendimento, req.LugarId);
        atendimento.Itens.Add(new ComandaItem
        {
            LugarId = lugar.Id,
            CartaItemId = prato.Id,
            Descricao = prato.Nome,
            PrecoUnitario = prato.Preco,
            Quantidade = req.Quantidade,
            Observacao = string.IsNullOrWhiteSpace(req.Observacao) ? null : req.Observacao.Trim(),
            Destino = req.Destino ?? prato.Destino,
            Status = StatusItem.Lancado,
            LancadoPorUsuarioId = usuarioId,
            LancadoPorNome = nome,
            LancadoEm = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return Projetar(atendimento, await MesasDoAtendimentoAsync(atendimentoId, ct));
    }

    public async Task<AtendimentoDto> TransferirItemAsync(int empresaId, long itemId, TransferirItemRequest req, CancellationToken ct)
    {
        var item = await db.ComandaItens.Include(i => i.Atendimento).ThenInclude(a => a!.Lugares)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Atendimento!.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Item não encontrado.", 404);
        ExigirAberto(item.Atendimento!);
        if (item.Status == StatusItem.Cancelado)
            throw new SalaoRegraException("Item cancelado não muda de lugar.");
        var lugar = ExigirLugarNomeado(item.Atendimento!, req.LugarId);
        item.LugarId = lugar.Id;
        await db.SaveChangesAsync(ct);
        return await ObterAtendimentoAsync(empresaId, item.AtendimentoId, ct);
    }

    public async Task<AtendimentoDto> MudarStatusItemAsync(int empresaId, long itemId, StatusItemRequest req, CancellationToken ct)
    {
        var item = await db.ComandaItens.Include(i => i.Atendimento)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Atendimento!.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Item não encontrado.", 404);
        if (item.Atendimento!.Status == StatusAtendimento.Fechada)
            throw new SalaoRegraException("Esta visita já foi fechada.");
        if (item.Status == StatusItem.Cancelado)
            throw new SalaoRegraException("Item cancelado não volta para a cozinha.");
        if (req.Status is StatusItem.Cancelado)
            throw new SalaoRegraException("Use o cancelamento para registrar o motivo.");
        if (Ordem(req.Status) < Ordem(item.Status))
            throw new SalaoRegraException("O prato não volta para um status anterior.");
        item.Status = req.Status;
        await db.SaveChangesAsync(ct);
        return await ObterAtendimentoAsync(empresaId, item.AtendimentoId, ct);
    }

    public async Task<AtendimentoDto> CancelarItemAsync(int empresaId, long itemId, int usuarioId, string nome, CancelarItemRequest req, CancellationToken ct)
    {
        var item = await db.ComandaItens.Include(i => i.Atendimento)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Atendimento!.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Item não encontrado.", 404);
        ExigirAberto(item.Atendimento!);
        if (item.Status is StatusItem.Cancelado or StatusItem.Entregue)
            throw new SalaoRegraException("Este prato não pode mais ser cancelado.");
        var motivo = req.Motivo?.Trim();
        if (item.Status != StatusItem.Lancado && string.IsNullOrEmpty(motivo))
            throw new SalaoRegraException("Depois de enviado, o cancelamento precisa de um motivo.");
        item.Status = StatusItem.Cancelado;
        item.MotivoCancelamento = string.IsNullOrEmpty(motivo) ? null : motivo;
        item.CanceladoPorUsuarioId = usuarioId;
        item.CanceladoPorNome = nome;
        item.CanceladoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await ObterAtendimentoAsync(empresaId, item.AtendimentoId, ct);
    }

    public async Task<AtendimentoDto> JuntarMesaAsync(int empresaId, long atendimentoId, JuntarMesaRequest req, CancellationToken ct)
    {
        var atendimento = await CarregarAsync(empresaId, atendimentoId, ct);
        ExigirAberto(atendimento);
        if (req.MesaId == atendimento.MesaId)
            throw new SalaoRegraException("Escolha outra mesa para juntar.");
        var mesa = await db.Mesas.FirstOrDefaultAsync(m => m.Id == req.MesaId && m.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Mesa não encontrada.", 404);
        if (mesa.Situacao == SituacaoMesa.Bloqueada)
            throw new SalaoRegraException("Esta mesa está bloqueada.");
        var claimed = await ClaimMesaAsync(mesa.Id, atendimento.Id, ct);
        if (claimed == 0)
            throw new SalaoRegraException("Esta mesa já tem uma visita aberta.", 409);
        return await ObterAtendimentoAsync(empresaId, atendimentoId, ct);
    }

    public async Task<AtendimentoDto> FecharAsync(int empresaId, long id, FecharAtendimentoRequest req, CancellationToken ct)
    {
        var atendimento = await CarregarAsync(empresaId, id, ct);
        ExigirAberto(atendimento);
        if (req.Modo == ModoFechamento.Anfitriao)
        {
            if (req.LugarId is not long lugarId || atendimento.Lugares.All(l => l.Id != lugarId))
                throw new SalaoRegraException("Escolha quem paga a mesa.");
            atendimento.AnfitriaoLugarId = lugarId;
        }
        await using var tx = await BeginTxAsync(ct);
        atendimento.ModoFechamento = req.Modo;
        atendimento.Status = StatusAtendimento.Fechada;
        atendimento.FechadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await LiberarMesasAsync(id, ct);
        if (tx is not null) await tx.CommitAsync(ct);
        return Projetar(atendimento, []);
    }

    private async Task<Atendimento> CarregarAsync(int empresaId, long id, CancellationToken ct) =>
        await db.Atendimentos
            .Include(a => a.Mesa).ThenInclude(m => m!.Ambiente)
            .Include(a => a.Lugares)
            .Include(a => a.Itens)
            .Include(a => a.Grupos).ThenInclude(g => g.Lugares)
            .FirstOrDefaultAsync(a => a.Id == id && a.EmpresaId == empresaId, ct)
        ?? throw new SalaoRegraException("Visita não encontrada.", 404);

    private async Task<IReadOnlyList<long>> MesasDoAtendimentoAsync(long atendimentoId, CancellationToken ct) =>
        await db.Mesas.AsNoTracking().Where(m => m.AtendimentoAbertoId == atendimentoId).Select(m => m.Id).ToListAsync(ct);

    private static AtendimentoDto Projetar(Atendimento atendimento, IReadOnlyList<long> mesasIds)
    {
        var ativos = atendimento.Itens.Where(i => i.Status != StatusItem.Cancelado).ToList();
        decimal TotalDe(long? lugarId) => ativos.Where(i => i.LugarId == lugarId).Sum(i => i.PrecoUnitario * i.Quantidade);
        var lugares = atendimento.Lugares.OrderBy(l => l.Ordem)
            .Select(l => new LugarDto(l.Id, l.Ordem, l.NomeCliente, l.Ocupado, l.ContaFechada, TotalDe(l.Id)))
            .ToList();
        var total = ativos.Sum(i => i.PrecoUnitario * i.Quantidade);
        var pessoas = Math.Max(1, lugares.Count);
        var itens = atendimento.Itens.OrderBy(i => i.LancadoEm)
            .Select(i => new ItemDto(
                i.Id, i.LugarId, i.CartaItemId, i.Descricao, i.PrecoUnitario, i.Quantidade,
                i.PrecoUnitario * i.Quantidade, i.Observacao, i.Destino, i.Status,
                i.LancadoPorUsuarioId, i.LancadoPorNome, i.LancadoEm, i.MotivoCancelamento, i.CanceladoPorNome))
            .ToList();
        var grupos = (atendimento.Grupos ?? [])
            .Select(g => new GrupoCobrancaDto(g.Id, g.Modo, g.Lugares.Select(l => l.LugarId).ToList()))
            .ToList();
        return new AtendimentoDto(
            atendimento.Id,
            atendimento.MesaId,
            atendimento.Mesa?.Rotulo ?? "",
            atendimento.Mesa?.Ambiente?.Nome ?? "",
            atendimento.Status,
            atendimento.AbertoPorUsuarioId,
            atendimento.AbertoPorNome,
            atendimento.AbertoEm,
            atendimento.FechadoEm,
            atendimento.ModoFechamento,
            atendimento.AnfitriaoLugarId,
            mesasIds,
            lugares,
            itens,
            grupos,
            total,
            decimal.Round(total / pessoas, 2, MidpointRounding.AwayFromZero));
    }

    internal static AtendimentoLugar ExigirLugarNomeado(Atendimento atendimento, long? lugarId)
    {
        if (lugarId is not long id || id <= 0)
            throw new SalaoRegraException("Escolha quem pediu.");
        var lugar = atendimento.Lugares.FirstOrDefault(l => l.Id == id)
            ?? throw new SalaoRegraException("Lugar não encontrado nesta visita.", 404);
        if (lugar.ContaFechada)
            throw new SalaoRegraException("A conta deste lugar já foi fechada.");
        if (string.IsNullOrWhiteSpace(lugar.NomeCliente) || !lugar.Ocupado)
            throw new SalaoRegraException("Dê um nome a este lugar antes de lançar.");
        return lugar;
    }

    private async Task LimparGruposOrfaosAsync(long atendimentoId, CancellationToken ct)
    {
        var grupos = await db.GruposCobranca.Include(g => g.Lugares)
            .Where(g => g.AtendimentoId == atendimentoId)
            .ToListAsync(ct);
        foreach (var grupo in grupos)
        {
            if (grupo.Lugares.Count == 0 || (grupo.Modo == ModoGrupoCobranca.Junto && grupo.Lugares.Count < 2))
                db.GruposCobranca.Remove(grupo);
        }
    }

    private async Task<int> ClaimMesaAsync(long mesaId, long atendimentoId, CancellationToken ct)
    {
        if (!db.Database.IsRelational())
        {
            var mesa = await db.Mesas.FirstAsync(m => m.Id == mesaId, ct);
            if (mesa.AtendimentoAbertoId is not null) return 0;
            mesa.AtendimentoAbertoId = atendimentoId;
            await db.SaveChangesAsync(ct);
            return 1;
        }

        return await db.Mesas
            .Where(m => m.Id == mesaId && m.AtendimentoAbertoId == null)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.AtendimentoAbertoId, atendimentoId), ct);
    }

    private async Task LiberarMesasAsync(long atendimentoId, CancellationToken ct)
    {
        if (!db.Database.IsRelational())
        {
            var mesas = await db.Mesas.Where(m => m.AtendimentoAbertoId == atendimentoId).ToListAsync(ct);
            foreach (var mesa in mesas) mesa.AtendimentoAbertoId = null;
            await db.SaveChangesAsync(ct);
            return;
        }

        await db.Mesas.Where(m => m.AtendimentoAbertoId == atendimentoId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.AtendimentoAbertoId, (long?)null), ct);
    }

    private async Task<IDbContextTransaction?> BeginTxAsync(CancellationToken ct)
    {
        if (!db.Database.IsRelational()) return null;
        return await db.Database.BeginTransactionAsync(ct);
    }

    private static void ExigirAberto(Atendimento atendimento)
    {
        if (atendimento.Status == StatusAtendimento.Fechada)
            throw new SalaoRegraException("Esta visita já foi fechada.");
    }

    private static int Ordem(StatusItem status) => status switch
    {
        StatusItem.Lancado => 0,
        StatusItem.EmPreparo => 1,
        StatusItem.Pronto => 2,
        StatusItem.Entregue => 3,
        _ => 9
    };

    private static string Obrigatorio(string? valor, string mensagem)
    {
        var texto = valor?.Trim();
        if (string.IsNullOrEmpty(texto)) throw new SalaoRegraException(mensagem);
        return texto;
    }

    private async Task SalvarAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new SalaoRegraException("Já existe um cadastro com este nome.", 409);
        }
    }
}
