namespace Orion.Application.Estoque;

/// <summary>
/// Chamadas de serviço ao Core (header <c>X-Internal-Service-Token</c>, sem contexto de utilizador).
/// <paramref name="empresaId"/> vem sempre do JWT do chamador do Orion.
/// </summary>
public interface ICoreEstoqueClient
{
    Task<ProdutoCore?> BuscarProdutoPorCodigoBarrasAsync(int empresaId, string codigoBarras, CancellationToken ct = default);

    Task<IReadOnlyList<ProdutoCore>> BuscarProdutosAsync(int empresaId, string? search, CancellationToken ct = default);

    Task<ProdutoCore> CriarProdutoAsync(int empresaId, CriarProdutoCore produto, CancellationToken ct = default);

    Task<SaldoCore> ConsultarSaldoAsync(int produtoId, int unidadeId, CancellationToken ct = default);

    Task<IReadOnlyList<SaldoCore>> ListarSaldosAsync(int empresaId, int? unidadeId, string? search, CancellationToken ct = default);

    /// <summary>Lote atômico de ENTRADA/AJUSTE. Idempotente por <paramref name="referenciaExterna"/>.</summary>
    Task<IReadOnlyList<MovimentacaoCore>> RegistrarEntradasLoteAsync(
        int empresaId, int unidadeId, TipoEntrada tipo, OrigemEntrada origem,
        string? documento, string referenciaExterna, IReadOnlyList<EntradaLoteItemCore> itens,
        CancellationToken ct = default);
}
