namespace Orion.Infrastructure.Persistence.Entities;

/// <summary>Cache leve barcode → produto do Core, para acelerar scans repetidos.</summary>
public class MapaCodigoBarras
{
    public long Id { get; set; }
    public int EmpresaId { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public int ProdutoCoreId { get; set; }
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}
