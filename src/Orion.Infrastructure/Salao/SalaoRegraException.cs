namespace Orion.Infrastructure.Salao;

public sealed class SalaoRegraException(string message, int status = 400) : Exception(message)
{
    public int Status { get; } = status;
}
