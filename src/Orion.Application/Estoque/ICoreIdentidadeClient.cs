namespace Orion.Application.Estoque;

/// <summary>Leitura de identidade no Core repassando o <c>Authorization: Bearer</c> do chamador.</summary>
public interface ICoreIdentidadeClient
{
    Task<IReadOnlyList<UnidadeCore>> ListarUnidadesAsync(int empresaId, CancellationToken ct = default);

    Task<EmpresaCore?> BuscarEmpresaAsync(int empresaId, CancellationToken ct = default);
}
