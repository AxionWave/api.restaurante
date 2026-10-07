using System.Text.Json;
using Orion.Application.Fiscal;
using Orion.Infrastructure.Estoque;
using Orion.Infrastructure.Integracao;

namespace Orion.API.Erros;

/// <summary>Traduz exceções de regra/integração para o contrato { error, message }.</summary>
public sealed class ErroEstoqueMiddleware(RequestDelegate next, ILogger<ErroEstoqueMiddleware> log)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (EstoqueRegraException ex)
        {
            await Escrever(ctx, ex.Status, "regra_estoque", ex.Message);
        }
        catch (NfeXmlInvalidoException ex)
        {
            await Escrever(ctx, 422, "xml_invalido", ex.Message);
        }
        catch (FornecedorNfeNaoConfiguradoException ex)
        {
            await Escrever(ctx, 422, "fiscal_provider_indisponivel", ex.Message);
        }
        catch (CoreClientException ex)
        {
            log.LogWarning("Core respondeu {Status}: {Body}", ex.Status, ex.Body);
            // 4xx do Core é erro de validação (ex.: código duplicado) — repassa a mensagem real
            // em vez de mascarar como indisponibilidade do serviço.
            if (ex.Status is >= 400 and < 500)
            {
                var (error, message) = ExtrairErroCore(ex.Body);
                await Escrever(ctx, ex.Status, error ?? "bad_request", message ?? "Não foi possível concluir a operação.");
                return;
            }
            await Escrever(ctx, 502, "core_indisponivel", "Falha ao falar com o serviço de estoque (Core).");
        }
    }

    private static (string? Error, string? Message) ExtrairErroCore(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            string? error = root.TryGetProperty("error", out var e) ? e.GetString() : null;
            string? message = root.TryGetProperty("message", out var m) ? m.GetString() : null;
            return (error, message);
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    private static Task Escrever(HttpContext ctx, int status, string error, string message)
    {
        if (ctx.Response.HasStarted)
        {
            return Task.CompletedTask;
        }
        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsJsonAsync(new { error, message });
    }
}
