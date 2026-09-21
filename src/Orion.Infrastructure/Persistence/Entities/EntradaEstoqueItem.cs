using Orion.Application.Estoque;

namespace Orion.Infrastructure.Persistence.Entities;

/// <summary>Item da entrada. Quantidades em <b>unidade de compra</b>; convertidas ao confirmar.</summary>
public class EntradaEstoqueItem
{
    public long Id { get; set; }
    public long EntradaEstoqueId { get; set; }
    public EntradaEstoque? Entrada { get; set; }

    public int? ProdutoCoreId { get; set; }
    public string? ProdutoCodigo { get; set; }

    public string? DescricaoNf { get; set; }
    public string? CodigoBarrasNf { get; set; }
    public string? Ncm { get; set; }
    public string? UnidadeComercialNf { get; set; }

    public decimal? QuantidadeNf { get; set; }
    public decimal QuantidadeRecebida { get; set; }
    public decimal FatorConversao { get; set; } = 1m;
    public decimal? ValorUnitarioNf { get; set; }
    public string? MotivoDivergencia { get; set; }

    public StatusVinculoItem StatusVinculo { get; set; } = StatusVinculoItem.Pendente;
    public long? MovimentacaoCoreId { get; set; }
    public string ReferenciaExternaItem { get; set; } = string.Empty;

    // Calculados
    public decimal QuantidadeEstoque => decimal.Round(QuantidadeRecebida * (FatorConversao <= 0 ? 1m : FatorConversao), 3);
    public decimal? CustoUnitarioEstoque =>
        ValorUnitarioNf is { } v && FatorConversao > 0 ? decimal.Round(v / FatorConversao, 4) : ValorUnitarioNf;
    public bool Divergencia => QuantidadeNf is { } qn && qn != QuantidadeRecebida;
}
