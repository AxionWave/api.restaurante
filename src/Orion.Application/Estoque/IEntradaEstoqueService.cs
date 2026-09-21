namespace Orion.Application.Estoque;

/// <summary>Orquestra o documento de Entrada de Estoque do Orion + as chamadas ao Core.</summary>
public interface IEntradaEstoqueService
{
    Task<IReadOnlyList<UnidadeCore>> ListarUnidadesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<FornecedorDto>> BuscarFornecedoresAsync(string? search, CancellationToken ct = default);

    Task<IReadOnlyList<ProdutoCore>> BuscarProdutosAsync(string? search, CancellationToken ct = default);

    Task<ProdutoCore?> ResolverCodigoBarrasAsync(string codigoBarras, CancellationToken ct = default);

    Task<ProdutoCore> CriarProdutoAsync(NovoProdutoRequest req, CancellationToken ct = default);

    /// <summary>Saldo (quantidade em estoque) de cada produto na unidade ativa da requisição.</summary>
    Task<IReadOnlyList<SaldoCore>> ListarSaldosAsync(string? search, CancellationToken ct = default);

    Task<EntradaDto> CriarAsync(CriarEntradaRequest req, CancellationToken ct = default);

    Task<EntradaDto> ImportarXmlAsync(Stream xml, int? unidadeId, CancellationToken ct = default);

    /// <summary>Modo 3 (por chave). Devolve rascunho completo se houver provedor; senão um stub só com a chave.</summary>
    Task<(EntradaDto Entrada, bool Completa)> ImportarPorChaveAsync(string chave, int? unidadeId, CancellationToken ct = default);

    Task<EntradaDto?> ObterAsync(long id, CancellationToken ct = default);

    Task<EntradaDto> AtualizarAsync(long id, AtualizarEntradaRequest req, CancellationToken ct = default);

    Task<EntradaDto> AtualizarItemAsync(long id, long itemId, AtualizarItemRequest req, CancellationToken ct = default);

    Task<EntradaDto> RemoverItemAsync(long id, long itemId, CancellationToken ct = default);

    Task<EntradaDto> ConfirmarAsync(long id, CancellationToken ct = default);

    Task<EntradaDto> CancelarAsync(long id, CancellationToken ct = default);

    Task<IReadOnlyList<EntradaDto>> ListarAsync(
        StatusEntrada? status, OrigemEntrada? origem, long? fornecedorId,
        DateTime? de, DateTime? ate, int page, int size, CancellationToken ct = default);
}
