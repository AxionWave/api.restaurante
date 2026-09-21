namespace Orion.Application.Abstractions;

/// <summary>
/// Unidade (filial) ativa da requisição. Hoje vem do header <c>X-Unidade-Id</c> enviado pelo app;
/// no futuro pode virar claim do JWT. Nulo quando não informada.
/// </summary>
public interface ICurrentUnidadeAccessor
{
    int? UnidadeId { get; }
}
