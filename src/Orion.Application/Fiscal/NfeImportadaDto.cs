namespace Orion.Application.Fiscal;

/// <summary>Dados extraídos de uma NF-e (modelo 55) ou NFC-e (modelo 65), por parse de XML ou por provedor fiscal.</summary>
public sealed record NfeImportadaDto(
    string Chave,
    string Modelo,            // "55" | "65"
    string? Numero,
    string? Serie,
    string? FornecedorNome,
    string? FornecedorCnpj,
    string? DestinatarioCnpj,
    DateTime? DataEmissao,
    decimal? ValorTotal,
    IReadOnlyList<NfeItemImportadoDto> Itens);

public sealed record NfeItemImportadoDto(
    string? CodigoFornecedor,   // cProd
    string? CodigoBarras,       // cEAN
    string DescricaoNf,         // xProd
    string? Ncm,
    string? UnidadeComercial,   // uCom
    decimal Quantidade,         // qCom
    decimal? ValorUnitario,     // vUnCom
    decimal? ValorTotal);       // vProd
