using Microsoft.AspNetCore.Http;
using Orion.Application.Abstractions;

namespace Orion.Infrastructure.Auth;

/// <summary>Lê a unidade ativa do header <c>X-Unidade-Id</c> (enviado pelo app.restaurante).</summary>
public sealed class HttpCurrentUnidadeAccessor(IHttpContextAccessor http) : ICurrentUnidadeAccessor
{
    public int? UnidadeId =>
        int.TryParse(http.HttpContext?.Request.Headers["X-Unidade-Id"].FirstOrDefault(), out var id) && id > 0
            ? id
            : null;
}
