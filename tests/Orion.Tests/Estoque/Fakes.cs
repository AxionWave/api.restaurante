using Orion.Application.Abstractions;
using Orion.Application.Estoque;
using Orion.Application.Fiscal;
using Orion.Core.Auth;

namespace Orion.Tests.Estoque;

public sealed class FakeCurrentUserAccessor(int empresaId, int userId = 1) : ICurrentUserAccessor
{
    public CurrentUser User { get; } = new(userId, "teste", "teste@ex.com", empresaId, [], ["ORI0000004"]);
}

public sealed class FakeCurrentUnidadeAccessor(int? unidadeId) : ICurrentUnidadeAccessor
{
    public int? UnidadeId => unidadeId;
}

public sealed class FakeFornecedorNfe : IFornecedorNfe
{
    public bool Disponivel => false;

    public Task<NfeImportadaDto> ObterPorChaveAsync(string chave, CancellationToken ct = default) =>
        throw new FornecedorNfeNaoConfiguradoException();
}

/// <summary>Catálogo falso em memória + simula o comportamento de idempotência do Core para entradas em lote.</summary>
public sealed class FakeCoreEstoqueClient : ICoreEstoqueClient
{
    private readonly Dictionary<int, ProdutoCore> _produtos = [];
    private readonly Dictionary<int, decimal> _saldos = [];
    private readonly Dictionary<string, List<MovimentacaoCore>> _lotesProcessados = [];
    private long _proximoMovId = 1;
    private int _proximoProdutoId = 1;

    public int ChamadasRegistrarEntradasLote { get; private set; }

    public ProdutoCore AdicionarProduto(string codigo, string nome, string? codigoBarras = null,
        string? unidadeCompra = null, decimal? fatorConversao = null)
    {
        var p = new ProdutoCore(_proximoProdutoId++, codigo, nome, codigoBarras, "UN", unidadeCompra, fatorConversao, null);
        _produtos[p.Id] = p;
        return p;
    }

    public Task<ProdutoCore?> BuscarProdutoPorCodigoBarrasAsync(int empresaId, string codigoBarras, CancellationToken ct = default) =>
        Task.FromResult(_produtos.Values.FirstOrDefault(p => p.CodigoBarras == codigoBarras));

    public Task<IReadOnlyList<ProdutoCore>> BuscarProdutosAsync(int empresaId, string? search, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ProdutoCore>>(_produtos.Values.ToList());

    public Task<ProdutoCore> CriarProdutoAsync(int empresaId, CriarProdutoCore produto, CancellationToken ct = default)
    {
        var p = AdicionarProduto(produto.Codigo, produto.Nome, produto.CodigoBarras, produto.UnidadeCompra, produto.FatorConversao);
        return Task.FromResult(p);
    }

    public Task<SaldoCore> ConsultarSaldoAsync(int produtoId, int unidadeId, CancellationToken ct = default)
    {
        var p = _produtos[produtoId];
        _saldos.TryGetValue(produtoId, out var q);
        return Task.FromResult(new SaldoCore(produtoId, p.Codigo, p.Nome, p.UnidadeMedida, unidadeId, q, null, p.EstoqueMinimo, false));
    }

    public Task<IReadOnlyList<SaldoCore>> ListarSaldosAsync(int empresaId, int? unidadeId, string? search, CancellationToken ct = default)
    {
        var lista = _produtos.Values.Select(p =>
        {
            _saldos.TryGetValue(p.Id, out var q);
            return new SaldoCore(p.Id, p.Codigo, p.Nome, p.UnidadeMedida, unidadeId, q, null, p.EstoqueMinimo, false);
        }).ToList();
        return Task.FromResult<IReadOnlyList<SaldoCore>>(lista);
    }

    public Task<IReadOnlyList<MovimentacaoCore>> RegistrarEntradasLoteAsync(
        int empresaId, int unidadeId, TipoEntrada tipo, OrigemEntrada origem,
        string? documento, string referenciaExterna, IReadOnlyList<EntradaLoteItemCore> itens, CancellationToken ct = default)
    {
        ChamadasRegistrarEntradasLote++;
        if (_lotesProcessados.TryGetValue(referenciaExterna, out var existente))
        {
            return Task.FromResult<IReadOnlyList<MovimentacaoCore>>(existente);
        }

        var movs = new List<MovimentacaoCore>();
        foreach (var item in itens)
        {
            _saldos.TryGetValue(item.ProdutoId, out var atual);
            var posterior = tipo == TipoEntrada.Ajuste ? item.Quantidade : atual + item.Quantidade;
            _saldos[item.ProdutoId] = posterior;
            var p = _produtos[item.ProdutoId];
            movs.Add(new MovimentacaoCore(_proximoMovId++, item.ProdutoId, p.Codigo,
                tipo.ToString().ToUpperInvariant(), origem.ToString().ToUpperInvariant(),
                item.Quantidade, posterior, item.CustoUnitario));
        }
        _lotesProcessados[referenciaExterna] = movs;
        return Task.FromResult<IReadOnlyList<MovimentacaoCore>>(movs);
    }

    public decimal SaldoAtual(int produtoId) => _saldos.GetValueOrDefault(produtoId);
}

public sealed class FakeCoreIdentidadeClient(int unidadeId = 100, string empresaCnpj = "98765432000188") : ICoreIdentidadeClient
{
    public Task<IReadOnlyList<UnidadeCore>> ListarUnidadesAsync(int empresaId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<UnidadeCore>>([new UnidadeCore(unidadeId, "Unidade Teste", "U1", "SP", "SP")]);

    public Task<EmpresaCore?> BuscarEmpresaAsync(int empresaId, CancellationToken ct = default) =>
        Task.FromResult<EmpresaCore?>(new EmpresaCore(empresaId, "Empresa Teste LTDA", empresaCnpj));
}
