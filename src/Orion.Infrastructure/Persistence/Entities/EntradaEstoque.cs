using Orion.Application.Estoque;

namespace Orion.Infrastructure.Persistence.Entities;

/// <summary>Documento de entrada de estoque do Orion. Os campos fiscais são todos opcionais.</summary>
public class EntradaEstoque
{
    public long Id { get; set; }
    public Guid ChaveIdempotencia { get; set; }
    public int EmpresaId { get; set; }
    public int UnidadeId { get; set; }

    public TipoEntrada Tipo { get; set; } = TipoEntrada.Entrada;
    public OrigemEntrada Origem { get; set; } = OrigemEntrada.Manual;
    public OrigemCadastro OrigemCadastro { get; set; } = OrigemCadastro.Manual;
    public StatusEntrada Status { get; set; } = StatusEntrada.Rascunho;

    // Fiscais (nullable)
    public long? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public string? FornecedorCnpj { get; set; }
    public string? ModeloFiscal { get; set; }        // "55" | "65"
    public string? ChaveAcesso { get; set; }          // 44 dígitos
    public string? NumeroNf { get; set; }
    public string? Serie { get; set; }
    public DateTime? DataEmissao { get; set; }
    public decimal? ValorTotal { get; set; }
    public string? XmlOriginal { get; set; }

    public DateTime DataEntrada { get; set; } = DateTime.UtcNow;
    public string ReferenciaExternaCore { get; set; } = string.Empty;   // "orion:entrada:{id}"
    public string? Observacao { get; set; }

    public int CriadoPorUsuarioId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmadoEm { get; set; }
    public int? CanceladoPorUsuarioId { get; set; }
    public DateTime? CanceladoEm { get; set; }

    public List<EntradaEstoqueItem> Itens { get; set; } = [];
}
