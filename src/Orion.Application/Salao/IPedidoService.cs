namespace Orion.Application.Salao;

/// <summary>
/// Comanda da visita (mesmo agregado de <see cref="ISalaoService"/>).
/// Não cria mesa: o cadastro permanece no salão; daqui só se abre e altera a passagem.
/// </summary>
public interface IPedidoService
{
    Task<AtendimentoDto> ObterVisitaDaMesaAsync(int empresaId, long mesaId, CancellationToken ct);
    Task<AtendimentoDto> LancarItemAsync(int empresaId, long atendimentoId, int usuarioId, string nome, LancarItemRequest req, CancellationToken ct);
    Task<AtendimentoDto> DefinirGruposAsync(int empresaId, long atendimentoId, DefinirGruposRequest req, CancellationToken ct);
    Task<ContaDto> ObterContaAsync(int empresaId, long atendimentoId, CancellationToken ct);
}
