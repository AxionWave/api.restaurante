namespace Orion.Application.Fiscal;

/// <summary>
/// Obtém a NF-e completa a partir da chave de acesso (44 dígitos). Implementação plugável:
/// sem provedor configurado, lança <see cref="FornecedorNfeNaoConfiguradoException"/>.
/// </summary>
public interface IFornecedorNfe
{
    bool Disponivel { get; }

    Task<NfeImportadaDto> ObterPorChaveAsync(string chave, CancellationToken ct = default);
}

public sealed class FornecedorNfeNaoConfiguradoException()
    : Exception("Nenhum provedor fiscal configurado para importar NF-e por chave de acesso.");
