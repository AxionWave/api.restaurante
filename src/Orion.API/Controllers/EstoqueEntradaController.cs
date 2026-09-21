using Orion.API.Auth;
using Orion.Application.Estoque;
using Orion.Core.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Orion.API.Controllers;

[ApiController]
[Authorize]
[RequireModulo(ModuleCodes.Estoque)]
[Route("api/restaurante/estoque")]
public sealed class EstoqueEntradaController(IEntradaEstoqueService servico) : ControllerBase
{
    // ---------- Contexto / catálogo ----------

    [HttpGet("unidades")]
    public async Task<IActionResult> Unidades(CancellationToken ct)
        => Ok(await servico.ListarUnidadesAsync(ct));

    [HttpGet("fornecedores")]
    public async Task<IActionResult> Fornecedores([FromQuery] string? search, CancellationToken ct)
        => Ok(await servico.BuscarFornecedoresAsync(search, ct));

    [HttpGet("produtos")]
    public async Task<IActionResult> Produtos([FromQuery] string? search, CancellationToken ct)
        => Ok(await servico.BuscarProdutosAsync(search, ct));

    [HttpGet("produtos/por-codigo-barras")]
    public async Task<IActionResult> ProdutoPorCodigoBarras([FromQuery] string codigo, CancellationToken ct)
    {
        var p = await servico.ResolverCodigoBarrasAsync(codigo, ct);
        return p is null
            ? NotFound(new { error = "produto_nao_encontrado", message = $"Sem produto com o código de barras {codigo}." })
            : Ok(p);
    }

    [HttpPost("produtos")]
    public async Task<IActionResult> CriarProduto([FromBody] NovoProdutoRequest req, CancellationToken ct)
        => StatusCode(201, await servico.CriarProdutoAsync(req, ct));

    [HttpGet("saldos")]
    public async Task<IActionResult> Saldos([FromQuery] string? search, CancellationToken ct)
        => Ok(await servico.ListarSaldosAsync(search, ct));

    // ---------- Entradas ----------

    [HttpPost("entradas")]
    public async Task<IActionResult> Criar([FromBody] CriarEntradaRequest req, CancellationToken ct)
        => StatusCode(201, await servico.CriarAsync(req, ct));

    [HttpPost("entradas/importar-xml")]
    [RequestSizeLimit(2_000_000)]
    public async Task<IActionResult> ImportarXml(IFormFile arquivo, [FromQuery] int? unidadeId, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new { error = "arquivo_ausente", message = "Envie o arquivo .xml no campo 'arquivo'." });
        }
        await using var stream = arquivo.OpenReadStream();
        return StatusCode(201, await servico.ImportarXmlAsync(stream, unidadeId, ct));
    }

    public sealed record ImportarChaveBody(string Chave);

    [HttpPost("entradas/importar-chave")]
    public async Task<IActionResult> ImportarChave([FromBody] ImportarChaveBody body, [FromQuery] int? unidadeId, CancellationToken ct)
    {
        var (entrada, completa) = await servico.ImportarPorChaveAsync(body.Chave, unidadeId, ct);
        return completa
            ? StatusCode(201, entrada)
            : Accepted(new { message = "Chave registrada. Importe o XML ou informe os itens manualmente.", entrada });
    }

    [HttpGet("entradas/{id:long}")]
    public async Task<IActionResult> Obter(long id, CancellationToken ct)
    {
        var e = await servico.ObterAsync(id, ct);
        return e is null ? NotFound(new { error = "nao_encontrada", message = "Entrada não encontrada." }) : Ok(e);
    }

    [HttpPut("entradas/{id:long}")]
    public async Task<IActionResult> Atualizar(long id, [FromBody] AtualizarEntradaRequest req, CancellationToken ct)
        => Ok(await servico.AtualizarAsync(id, req, ct));

    [HttpPut("entradas/{id:long}/itens/{itemId:long}")]
    public async Task<IActionResult> AtualizarItem(long id, long itemId, [FromBody] AtualizarItemRequest req, CancellationToken ct)
        => Ok(await servico.AtualizarItemAsync(id, itemId, req, ct));

    [HttpDelete("entradas/{id:long}/itens/{itemId:long}")]
    public async Task<IActionResult> RemoverItem(long id, long itemId, CancellationToken ct)
        => Ok(await servico.RemoverItemAsync(id, itemId, ct));

    [HttpPost("entradas/{id:long}/confirmar")]
    public async Task<IActionResult> Confirmar(long id, CancellationToken ct)
        => Ok(await servico.ConfirmarAsync(id, ct));

    [HttpPost("entradas/{id:long}/cancelar")]
    public async Task<IActionResult> Cancelar(long id, CancellationToken ct)
        => Ok(await servico.CancelarAsync(id, ct));

    [HttpGet("entradas")]
    public async Task<IActionResult> Listar(
        [FromQuery] StatusEntrada? status, [FromQuery] OrigemEntrada? origem, [FromQuery] long? fornecedorId,
        [FromQuery] DateTime? de, [FromQuery] DateTime? ate,
        [FromQuery] int page = 0, [FromQuery] int size = 20, CancellationToken ct = default)
        => Ok(await servico.ListarAsync(status, origem, fornecedorId, de, ate, page, size, ct));
}
