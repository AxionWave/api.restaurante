using Orion.Application.Fiscal;

namespace Orion.Infrastructure.Integracao.Fiscal;

/// <summary>Sem provedor fiscal configurado. Modo "importar por chave" fica inerte.</summary>
public sealed class FornecedorNfeNaoConfigurado : IFornecedorNfe
{
    public bool Disponivel => false;

    public Task<NfeImportadaDto> ObterPorChaveAsync(string chave, CancellationToken ct = default) =>
        throw new FornecedorNfeNaoConfiguradoException();
}
