namespace Orion.Application.Estoque;

/// <summary>Produto do catálogo central (Core), com dados de conversão de compra.</summary>
public sealed record ProdutoCore(
    int Id,
    string Codigo,
    string Nome,
    string? CodigoBarras,
    string UnidadeMedida,
    string? UnidadeCompra,
    decimal? FatorConversao,
    decimal? EstoqueMinimo);

public sealed record CriarProdutoCore(
    string Codigo,
    string Nome,
    string UnidadeMedida,
    string? UnidadeCompra,
    decimal? FatorConversao,
    string? CodigoBarras,
    string? Categoria,
    decimal? EstoqueMinimo);

public sealed record SaldoCore(
    int ProdutoId,
    string ProdutoCodigo,
    string ProdutoNome,
    string UnidadeMedida,
    int? UnidadeId,
    decimal Quantidade,
    decimal? CustoMedio,
    decimal? EstoqueMinimo,
    bool AbaixoMinimo);

public sealed record UnidadeCore(int Id, string Nome, string? Codigo, string? Cidade, string? Estado);

public sealed record EmpresaCore(int Id, string RazaoSocial, string Cnpj);

/// <summary>Item de um lote de entrada enviado ao Core (quantidades já na unidade de estoque).</summary>
public sealed record EntradaLoteItemCore(int ProdutoId, decimal Quantidade, decimal? CustoUnitario, string ReferenciaExterna);

public sealed record MovimentacaoCore(
    long Id,
    int ProdutoId,
    string ProdutoCodigo,
    string Tipo,
    string Origem,
    decimal Quantidade,
    decimal? QuantidadePosterior,
    decimal? CustoUnitario);
