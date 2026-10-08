using Orion.API.Auth;
using Orion.Application.Abstractions;
using Orion.Core.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Orion.API.Controllers;

[ApiController]
[Authorize]
[Route("api/restaurante")]
public sealed class MeController(ICurrentUserAccessor currentUser) : ControllerBase
{
    /// <summary>Smoke: devolve claims do JWT (userId, empresaId, roles, modulos).</summary>
    [HttpGet("me")]
    [RequireModulo(ModuleCodes.Raiz, ModuleCodes.RaizLegado, ModuleCodes.Cardapio, ModuleCodes.Pedidos, ModuleCodes.Mesas, ModuleCodes.Estoque, ModuleCodes.Configuracoes)]
    public IActionResult Me()
    {
        var u = currentUser.User;
        return Ok(new
        {
            product = "Orion",
            sigla = "ORI",
            u.UserId,
            u.Username,
            u.Email,
            u.EmpresaId,
            u.Roles,
            u.Modulos
        });
    }
}
