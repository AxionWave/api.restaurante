namespace Orion.Infrastructure.Persistence.Entities;

public class Fornecedor
{
    public long Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}
