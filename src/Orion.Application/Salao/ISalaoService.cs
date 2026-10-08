namespace Orion.Application.Salao;

public interface ISalaoService
{
    Task<IReadOnlyList<AmbienteDto>> ListarAmbientesAsync(int empresaId, bool somenteAtivos, CancellationToken ct);
    Task<AmbienteDto> SalvarAmbienteAsync(int empresaId, long? id, AmbienteRequest req, CancellationToken ct);

    Task<IReadOnlyList<MesaDto>> ListarMesasAsync(int empresaId, CancellationToken ct);
    Task<MesaDto> SalvarMesaAsync(int empresaId, long? id, MesaRequest req, CancellationToken ct);

    Task<IReadOnlyList<CartaCategoriaDto>> ListarCategoriasAsync(int empresaId, bool somenteAtivas, CancellationToken ct);
    Task<CartaCategoriaDto> SalvarCategoriaAsync(int empresaId, long? id, CartaCategoriaRequest req, CancellationToken ct);
    Task<IReadOnlyList<CartaItemDto>> ListarItensCartaAsync(int empresaId, bool somenteAtivos, CancellationToken ct);
    Task<CartaItemDto> SalvarItemCartaAsync(int empresaId, long? id, CartaItemRequest req, CancellationToken ct);

    Task<IReadOnlyList<MapaAmbienteDto>> MapaAsync(int empresaId, CancellationToken ct);
    Task<IReadOnlyList<AtendimentoDto>> ListarAbertosAsync(int empresaId, CancellationToken ct);
    Task<AtendimentoDto> ObterAtendimentoAsync(int empresaId, long id, CancellationToken ct);
    Task<AtendimentoDto> AbrirAsync(int empresaId, long mesaId, int usuarioId, string nome, CancellationToken ct);
    Task<AtendimentoDto> PedirContaAsync(int empresaId, long id, CancellationToken ct);
    Task<AtendimentoDto> ReabrirAsync(int empresaId, long id, CancellationToken ct);
    Task<AtendimentoDto> AdicionarLugarAsync(int empresaId, long id, CancellationToken ct);
    Task<AtendimentoDto> RemoverLugarAsync(int empresaId, long lugarId, CancellationToken ct);
    Task<AtendimentoDto> NomearLugarAsync(int empresaId, long lugarId, NomearLugarRequest req, CancellationToken ct);
    Task<AtendimentoDto> FecharLugarAsync(int empresaId, long lugarId, CancellationToken ct);
    Task<AtendimentoDto> LancarItemAsync(int empresaId, long atendimentoId, int usuarioId, string nome, LancarItemRequest req, CancellationToken ct);
    Task<AtendimentoDto> TransferirItemAsync(int empresaId, long itemId, TransferirItemRequest req, CancellationToken ct);
    Task<AtendimentoDto> MudarStatusItemAsync(int empresaId, long itemId, StatusItemRequest req, CancellationToken ct);
    Task<AtendimentoDto> CancelarItemAsync(int empresaId, long itemId, int usuarioId, string nome, CancelarItemRequest req, CancellationToken ct);
    Task<AtendimentoDto> JuntarMesaAsync(int empresaId, long atendimentoId, JuntarMesaRequest req, CancellationToken ct);
    Task<AtendimentoDto> FecharAsync(int empresaId, long id, FecharAtendimentoRequest req, CancellationToken ct);
}
