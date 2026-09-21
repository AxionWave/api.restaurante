using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Orion.Application.Estoque;

namespace Orion.Infrastructure.Integracao;

public sealed class CoreEstoqueClient(HttpClient http) : ICoreEstoqueClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<ProdutoCore?> BuscarProdutoPorCodigoBarrasAsync(int empresaId, string codigoBarras, CancellationToken ct = default)
    {
        var resp = await http.GetAsync(
            $"/api/internal/estoque/produtos/por-codigo-barras?empresaId={empresaId}&codigoBarras={Uri.EscapeDataString(codigoBarras)}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<ProdutoJson>(Json, ct);
        return dto?.ToCore();
    }

    public async Task<IReadOnlyList<ProdutoCore>> BuscarProdutosAsync(int empresaId, string? search, CancellationToken ct = default)
    {
        var url = $"/api/internal/estoque/produtos?empresaId={empresaId}&size=50";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }
        var page = await http.GetFromJsonAsync<SpringPage<ProdutoJson>>(url, Json, ct);
        return page?.Content?.Select(p => p.ToCore()).ToList() ?? [];
    }

    public async Task<ProdutoCore> CriarProdutoAsync(int empresaId, CriarProdutoCore produto, CancellationToken ct = default)
    {
        var body = new
        {
            empresaId,
            produto.Codigo,
            produto.Nome,
            produto.UnidadeMedida,
            produto.UnidadeCompra,
            produto.FatorConversao,
            produto.CodigoBarras,
            produto.Categoria,
            produto.EstoqueMinimo,
        };
        var resp = await http.PostAsJsonAsync("/api/internal/estoque/produtos", body, Json, ct);
        await EnsureOk(resp, ct);
        var dto = await resp.Content.ReadFromJsonAsync<ProdutoJson>(Json, ct);
        return dto!.ToCore();
    }

    public async Task<SaldoCore> ConsultarSaldoAsync(int produtoId, int unidadeId, CancellationToken ct = default)
    {
        var dto = await http.GetFromJsonAsync<SaldoJson>(
            $"/api/internal/estoque/saldos/{produtoId}?unidadeId={unidadeId}", Json, ct);
        return new SaldoCore(dto!.ProdutoId ?? produtoId, dto.ProdutoCodigo ?? "", dto.ProdutoNome ?? "",
            dto.UnidadeMedida ?? "UN", dto.UnidadeId, dto.Quantidade ?? 0m, dto.CustoMedio);
    }

    public async Task<IReadOnlyList<SaldoCore>> ListarSaldosAsync(int empresaId, int? unidadeId, string? search, CancellationToken ct = default)
    {
        var url = $"/api/internal/estoque/saldos?empresaId={empresaId}&size=200";
        if (unidadeId.HasValue)
        {
            url += $"&unidadeId={unidadeId.Value}";
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }
        var page = await http.GetFromJsonAsync<SpringPage<SaldoJson>>(url, Json, ct);
        return page?.Content?.Select(dto => new SaldoCore(dto.ProdutoId ?? 0, dto.ProdutoCodigo ?? "", dto.ProdutoNome ?? "",
            dto.UnidadeMedida ?? "UN", dto.UnidadeId, dto.Quantidade ?? 0m, dto.CustoMedio)).ToList() ?? [];
    }

    public async Task<IReadOnlyList<MovimentacaoCore>> RegistrarEntradasLoteAsync(
        int empresaId, int unidadeId, TipoEntrada tipo, OrigemEntrada origem,
        string? documento, string referenciaExterna, IReadOnlyList<EntradaLoteItemCore> itens, CancellationToken ct = default)
    {
        var body = new
        {
            empresaId,
            unidadeId,
            tipo = tipo == TipoEntrada.Ajuste ? "AJUSTE" : "ENTRADA",
            origem = origem.ToString().ToUpperInvariant(),
            documento,
            referenciaExterna,
            itens = itens.Select(i => new { i.ProdutoId, i.Quantidade, i.CustoUnitario, i.ReferenciaExterna }),
        };
        var resp = await http.PostAsJsonAsync("/api/internal/estoque/entradas-lote", body, Json, ct);
        await EnsureOk(resp, ct);
        var movs = await resp.Content.ReadFromJsonAsync<List<MovimentacaoJson>>(Json, ct) ?? [];
        return movs.Select(m => new MovimentacaoCore(
            m.Id ?? 0, m.ProdutoId ?? 0, m.ProdutoCodigo ?? "", m.Tipo ?? "", m.Origem ?? "",
            m.Quantidade ?? 0m, m.QuantidadePosterior, m.CustoUnitario)).ToList();
    }

    private static async Task EnsureOk(HttpResponseMessage resp, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode)
        {
            return;
        }
        var texto = await resp.Content.ReadAsStringAsync(ct);
        throw new CoreClientException((int)resp.StatusCode, texto);
    }

    // ---- JSON DTOs (contrato do Core) ----

    private sealed record ProdutoJson(
        int Id, string Codigo, string Nome, string? CodigoBarras, string? UnidadeMedida,
        string? UnidadeCompra, decimal? FatorConversao, decimal? EstoqueMinimo)
    {
        public ProdutoCore ToCore() => new(Id, Codigo, Nome, CodigoBarras, UnidadeMedida ?? "UN",
            UnidadeCompra, FatorConversao, EstoqueMinimo);
    }

    private sealed record SaldoJson(int? ProdutoId, string? ProdutoCodigo, string? ProdutoNome,
        string? UnidadeMedida, int? UnidadeId, decimal? Quantidade, decimal? CustoMedio);

    private sealed record MovimentacaoJson(long? Id, int? ProdutoId, string? ProdutoCodigo, string? Tipo, string? Origem,
        decimal? Quantidade, decimal? QuantidadePosterior, decimal? CustoUnitario);

    private sealed record SpringPage<T>(List<T>? Content);
}

public sealed class CoreClientException(int status, string body)
    : Exception($"Core respondeu {status}: {body}")
{
    public int Status { get; } = status;
    public string Body { get; } = body;
}
