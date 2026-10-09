using Orion.API.Auth;
using Orion.Application.Abstractions;
using Orion.Application.Salao;
using Orion.Core.Modules;
using Orion.Infrastructure.Salao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Orion.API.Controllers;

[ApiController]
[Authorize]
[Route("api/restaurante/casa")]
public sealed class SalaoController(ISalaoService salao, IPedidoService pedidos, ICurrentUserAccessor current) : ControllerBase
{
    [HttpGet("ambientes")]
    [RequireModulo(ModuleCodes.Configuracoes, ModuleCodes.Mesas)]
    public async Task<IActionResult> Ambientes([FromQuery] bool ativos, CancellationToken ct)
        => Ok(await salao.ListarAmbientesAsync(Empresa(), ativos, ct));

    [HttpPost("ambientes")]
    [RequireModulo(ModuleCodes.Configuracoes)]
    public async Task<IActionResult> CriarAmbiente([FromBody] AmbienteRequest req, CancellationToken ct)
        => StatusCode(201, await salao.SalvarAmbienteAsync(Empresa(), null, req, ct));

    [HttpPut("ambientes/{id:long}")]
    [RequireModulo(ModuleCodes.Configuracoes)]
    public async Task<IActionResult> AtualizarAmbiente(long id, [FromBody] AmbienteRequest req, CancellationToken ct)
        => Ok(await salao.SalvarAmbienteAsync(Empresa(), id, req, ct));

    [HttpGet("mesas")]
    [RequireModulo(ModuleCodes.Configuracoes, ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Mesas(CancellationToken ct)
        => Ok(await salao.ListarMesasAsync(Empresa(), ct));

    [HttpPost("mesas")]
    [RequireModulo(ModuleCodes.Configuracoes)]
    public async Task<IActionResult> CriarMesa([FromBody] MesaRequest req, CancellationToken ct)
        => StatusCode(201, await salao.SalvarMesaAsync(Empresa(), null, req, ct));

    [HttpPut("mesas/{id:long}")]
    [RequireModulo(ModuleCodes.Configuracoes)]
    public async Task<IActionResult> AtualizarMesa(long id, [FromBody] MesaRequest req, CancellationToken ct)
        => Ok(await salao.SalvarMesaAsync(Empresa(), id, req, ct));

    [HttpGet("carta/categorias")]
    [RequireModulo(ModuleCodes.Cardapio, ModuleCodes.Pedidos, ModuleCodes.Mesas)]
    public async Task<IActionResult> Categorias([FromQuery] bool ativas, CancellationToken ct)
        => Ok(await salao.ListarCategoriasAsync(Empresa(), ativas, ct));

    [HttpPost("carta/categorias")]
    [RequireModulo(ModuleCodes.Cardapio)]
    public async Task<IActionResult> CriarCategoria([FromBody] CartaCategoriaRequest req, CancellationToken ct)
        => StatusCode(201, await salao.SalvarCategoriaAsync(Empresa(), null, req, ct));

    [HttpPut("carta/categorias/{id:long}")]
    [RequireModulo(ModuleCodes.Cardapio)]
    public async Task<IActionResult> AtualizarCategoria(long id, [FromBody] CartaCategoriaRequest req, CancellationToken ct)
        => Ok(await salao.SalvarCategoriaAsync(Empresa(), id, req, ct));

    [HttpGet("carta/itens")]
    [RequireModulo(ModuleCodes.Cardapio, ModuleCodes.Pedidos, ModuleCodes.Mesas)]
    public async Task<IActionResult> Itens([FromQuery] bool ativos, CancellationToken ct)
        => Ok(await salao.ListarItensCartaAsync(Empresa(), ativos, ct));

    [HttpPost("carta/itens")]
    [RequireModulo(ModuleCodes.Cardapio)]
    public async Task<IActionResult> CriarItem([FromBody] CartaItemRequest req, CancellationToken ct)
        => StatusCode(201, await salao.SalvarItemCartaAsync(Empresa(), null, req, ct));

    [HttpPut("carta/itens/{id:long}")]
    [RequireModulo(ModuleCodes.Cardapio)]
    public async Task<IActionResult> AtualizarItem(long id, [FromBody] CartaItemRequest req, CancellationToken ct)
        => Ok(await salao.SalvarItemCartaAsync(Empresa(), id, req, ct));

    [HttpGet("mapa")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Mapa(CancellationToken ct)
        => Ok(await salao.MapaAsync(Empresa(), ct));

    [HttpGet("atendimentos")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Abertos(CancellationToken ct)
        => Ok(await salao.ListarAbertosAsync(Empresa(), ct));

    [HttpGet("atendimentos/{id:long}")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Obter(long id, CancellationToken ct)
        => Ok(await salao.ObterAtendimentoAsync(Empresa(), id, ct));

    [HttpGet("mesas/{mesaId:long}/visita")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> VisitaDaMesa(long mesaId, CancellationToken ct)
        => Ok(await pedidos.ObterVisitaDaMesaAsync(Empresa(), mesaId, ct));

    [HttpPost("mesas/{mesaId:long}/abrir")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Abrir(long mesaId, CancellationToken ct)
        => StatusCode(201, await salao.AbrirAsync(Empresa(), mesaId, Usuario(), Nome(), ct));

    [HttpGet("atendimentos/{id:long}/conta")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> RelatorioConta(long id, CancellationToken ct)
        => Ok(await pedidos.ObterContaAsync(Empresa(), id, ct));

    [HttpPut("atendimentos/{id:long}/grupos")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Grupos(long id, [FromBody] DefinirGruposRequest req, CancellationToken ct)
        => Ok(await pedidos.DefinirGruposAsync(Empresa(), id, req, ct));

    [HttpPost("atendimentos/{id:long}/conta")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> PedirConta(long id, CancellationToken ct)
        => Ok(await salao.PedirContaAsync(Empresa(), id, ct));

    [HttpPost("atendimentos/{id:long}/reabrir")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Reabrir(long id, CancellationToken ct)
        => Ok(await salao.ReabrirAsync(Empresa(), id, ct));

    [HttpPost("atendimentos/{id:long}/lugares")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> NovoLugar(long id, CancellationToken ct)
        => Ok(await salao.AdicionarLugarAsync(Empresa(), id, ct));

    [HttpDelete("lugares/{id:long}")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> RemoverLugar(long id, CancellationToken ct)
        => Ok(await salao.RemoverLugarAsync(Empresa(), id, ct));

    [HttpPut("lugares/{id:long}")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Nomear(long id, [FromBody] NomearLugarRequest req, CancellationToken ct)
        => Ok(await salao.NomearLugarAsync(Empresa(), id, req, ct));

    [HttpPost("lugares/{id:long}/fechar")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> FecharLugar(long id, CancellationToken ct)
        => Ok(await salao.FecharLugarAsync(Empresa(), id, ct));

    [HttpPost("atendimentos/{id:long}/itens")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Lancar(long id, [FromBody] LancarItemRequest req, CancellationToken ct)
        => StatusCode(201, await pedidos.LancarItemAsync(Empresa(), id, Usuario(), Nome(), req, ct));

    [HttpPost("itens/{id:long}/transferir")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Transferir(long id, [FromBody] TransferirItemRequest req, CancellationToken ct)
        => Ok(await salao.TransferirItemAsync(Empresa(), id, req, ct));

    [HttpPost("itens/{id:long}/status")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Status(long id, [FromBody] StatusItemRequest req, CancellationToken ct)
        => Ok(await salao.MudarStatusItemAsync(Empresa(), id, req, ct));

    [HttpPost("itens/{id:long}/cancelar")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Cancelar(long id, [FromBody] CancelarItemRequest req, CancellationToken ct)
        => Ok(await salao.CancelarItemAsync(Empresa(), id, Usuario(), Nome(), req, ct));

    [HttpPost("atendimentos/{id:long}/juntar")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Juntar(long id, [FromBody] JuntarMesaRequest req, CancellationToken ct)
        => Ok(await salao.JuntarMesaAsync(Empresa(), id, req, ct));

    [HttpPost("atendimentos/{id:long}/fechar")]
    [RequireModulo(ModuleCodes.Mesas, ModuleCodes.Pedidos)]
    public async Task<IActionResult> Fechar(long id, [FromBody] FecharAtendimentoRequest req, CancellationToken ct)
        => Ok(await salao.FecharAsync(Empresa(), id, req, ct));

    private int Empresa()
    {
        var id = current.User.EmpresaId;
        if (id is null or <= 0) throw new SalaoRegraException("Sua conta não está ligada a uma empresa.", 403);
        return id.Value;
    }

    private int Usuario()
    {
        var id = current.User.UserId;
        if (id is null or <= 0) throw new SalaoRegraException("Sessão sem usuário.", 401);
        return id.Value;
    }

    private string Nome()
    {
        var nome = current.User.Username;
        if (string.IsNullOrWhiteSpace(nome)) nome = current.User.Email;
        return string.IsNullOrWhiteSpace(nome) ? "Equipe" : nome.Trim();
    }
}
