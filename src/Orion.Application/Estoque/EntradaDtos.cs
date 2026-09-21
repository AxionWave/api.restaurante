namespace Orion.Application.Estoque;

// ---------- Requests ----------

public sealed record CriarEntradaRequest(
    Guid ChaveIdempotencia,
    int? UnidadeId,
    TipoEntrada Tipo,
    OrigemEntrada Origem,
    DateTime? DataEntrada,
    string? Observacao,
    DadosFiscaisRequest? Fiscal,
    IReadOnlyList<ItemEntradaRequest> Itens);

public sealed record DadosFiscaisRequest(
    long? FornecedorId,
    string? FornecedorNome,
    string? FornecedorCnpj,
    string? NumeroNf,
    string? Serie,
    string? ChaveAcesso,
    DateTime? DataEmissao);

public sealed record ItemEntradaRequest(
    int? ProdutoCoreId,
    string? CodigoBarras,
    NovoProdutoRequest? NovoProduto,
    string? UnidadeComercialNf,
    decimal? QuantidadeNf,
    decimal QuantidadeRecebida,
    decimal? FatorConversao,
    decimal? ValorUnitarioNf);

public sealed record NovoProdutoRequest(
    string Codigo,
    string Nome,
    string UnidadeMedida,
    string? UnidadeCompra,
    decimal? FatorConversao,
    string? CodigoBarras,
    string? Categoria,
    decimal? EstoqueMinimo);

public sealed record AtualizarItemRequest(
    int? ProdutoCoreId,
    NovoProdutoRequest? NovoProduto,
    decimal? QuantidadeRecebida,
    decimal? FatorConversao,
    decimal? ValorUnitarioNf,
    string? MotivoDivergencia);

public sealed record AtualizarEntradaRequest(
    TipoEntrada? Tipo,
    OrigemEntrada? Origem,
    string? Observacao,
    DadosFiscaisRequest? Fiscal);

// ---------- Responses ----------

public sealed record EntradaDto(
    long Id,
    Guid ChaveIdempotencia,
    int EmpresaId,
    int UnidadeId,
    string Tipo,
    string Origem,
    string OrigemCadastro,
    string Status,
    long? FornecedorId,
    string? FornecedorNome,
    string? FornecedorCnpj,
    string? ModeloFiscal,
    string? ChaveAcesso,
    string? NumeroNf,
    string? Serie,
    DateTime? DataEmissao,
    DateTime DataEntrada,
    decimal? ValorTotal,
    string? Observacao,
    DateTime CriadoEm,
    DateTime? ConfirmadoEm,
    IReadOnlyList<ItemDto> Itens);

public sealed record ItemDto(
    long Id,
    int? ProdutoCoreId,
    string? ProdutoCodigo,
    string? DescricaoNf,
    string? CodigoBarrasNf,
    string? Ncm,
    string? UnidadeComercialNf,
    decimal? QuantidadeNf,
    decimal QuantidadeRecebida,
    decimal FatorConversao,
    decimal QuantidadeEstoque,
    decimal? ValorUnitarioNf,
    decimal? CustoUnitarioEstoque,
    bool Divergencia,
    string? MotivoDivergencia,
    string StatusVinculo,
    long? MovimentacaoCoreId,
    decimal? SaldoResultante);

public sealed record FornecedorDto(long Id, string Nome, string? Cnpj);
