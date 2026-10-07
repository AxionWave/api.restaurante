using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Orion.Application.Estoque;

namespace Orion.Infrastructure.Integracao;

public sealed class CoreIdentidadeClient(HttpClient http) : ICoreIdentidadeClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<UnidadeCore>> ListarUnidadesAsync(int empresaId, CancellationToken ct = default)
    {
        var lista = await http.GetFromJsonAsync<List<UnidadeJson>>($"/api/unidades?empresaId={empresaId}", Json, ct);
        return lista?.Select(u => new UnidadeCore(u.Id, u.Nome, u.Codigo, u.Cidade, u.Estado)).ToList() ?? [];
    }

    public async Task<EmpresaCore?> BuscarEmpresaAsync(int empresaId, CancellationToken ct = default)
    {
        var resp = await http.GetAsync($"/api/empresas/{empresaId}", ct);
        if (resp.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
        {
            return null;
        }
        resp.EnsureSuccessStatusCode();
        var e = await resp.Content.ReadFromJsonAsync<EmpresaJson>(Json, ct);
        return e is null ? null : new EmpresaCore(e.Id, e.RazaoSocial ?? "", new string((e.Cnpj ?? "").Where(char.IsDigit).ToArray()));
    }

    private sealed record UnidadeJson(int Id, string Nome, string? Codigo, string? Cidade, string? Estado);
    private sealed record EmpresaJson(int Id, string? RazaoSocial, string? Cnpj);
}
