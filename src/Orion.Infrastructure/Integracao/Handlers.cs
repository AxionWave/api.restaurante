using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace Orion.Infrastructure.Integracao;

/// <summary>Injeta o header <c>X-Internal-Service-Token</c> em toda chamada ao Core.</summary>
public sealed class TokenInternoHandler(IOptions<CoreServiceOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = options.Value.InternalToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Remove("X-Internal-Service-Token");
            request.Headers.Add("X-Internal-Service-Token", token);
        }
        return base.SendAsync(request, ct);
    }
}

/// <summary>Repassa o <c>Authorization: Bearer</c> do request atual para o Core (endpoints com JWT).</summary>
public sealed class RepasseTokenUsuarioHandler(IHttpContextAccessor http) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var auth = http.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(auth))
        {
            request.Headers.Remove("Authorization");
            request.Headers.TryAddWithoutValidation("Authorization", auth);
        }
        return base.SendAsync(request, ct);
    }
}
