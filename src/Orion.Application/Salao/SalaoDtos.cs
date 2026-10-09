namespace Orion.Application.Salao;

public sealed record AmbienteDto(long Id, string Nome, int Ordem, bool Ativo);

public sealed record AmbienteRequest(string Nome, int Ordem, bool Ativo);

public sealed record MesaDto(long Id, long AmbienteId, string AmbienteNome, string Rotulo, int LugaresPadrao, FormaMesa Forma, SituacaoMesa Situacao);

public sealed record MesaRequest(long AmbienteId, string Rotulo, int LugaresPadrao, FormaMesa Forma, SituacaoMesa Situacao);

public sealed record CartaCategoriaDto(long Id, string Nome, int Ordem, bool Ativo);

public sealed record CartaCategoriaRequest(string Nome, int Ordem, bool Ativo);

public sealed record CartaItemDto(long Id, long CategoriaId, string CategoriaNome, string Nome, decimal Preco, DestinoPedido Destino, bool Ativo);

public sealed record CartaItemRequest(long CategoriaId, string Nome, decimal Preco, DestinoPedido Destino, bool Ativo);

public sealed record MapaLugarDto(long Id, int Ordem, string? NomeCliente, bool Ocupado, bool ContaFechada);

public sealed record MapaMesaDto(
    long Id,
    long AmbienteId,
    string Rotulo,
    int LugaresPadrao,
    FormaMesa Forma,
    SituacaoMesa Situacao,
    long? AtendimentoId,
    StatusAtendimento? StatusAtendimento,
    string? AbertoPorNome,
    int Lugares,
    IReadOnlyList<MapaLugarDto> LugaresVisita);

public sealed record MapaAmbienteDto(long Id, string Nome, int Ordem, IReadOnlyList<MapaMesaDto> Mesas);

public sealed record LugarDto(long Id, int Ordem, string? NomeCliente, bool Ocupado, bool ContaFechada, decimal Total);

public sealed record ItemDto(
    long Id,
    long? LugarId,
    long? CartaItemId,
    string Descricao,
    decimal PrecoUnitario,
    int Quantidade,
    decimal Total,
    string? Observacao,
    DestinoPedido Destino,
    StatusItem Status,
    int LancadoPorUsuarioId,
    string LancadoPorNome,
    DateTime LancadoEm,
    string? MotivoCancelamento,
    string? CanceladoPorNome);

public sealed record GrupoCobrancaDto(long Id, ModoGrupoCobranca Modo, IReadOnlyList<long> LugarIds);

public sealed record AtendimentoDto(
    long Id,
    long MesaId,
    string MesaRotulo,
    string AmbienteNome,
    StatusAtendimento Status,
    int AbertoPorUsuarioId,
    string AbertoPorNome,
    DateTime AbertoEm,
    DateTime? FechadoEm,
    ModoFechamento? ModoFechamento,
    long? AnfitriaoLugarId,
    IReadOnlyList<long> MesasIds,
    IReadOnlyList<LugarDto> Lugares,
    IReadOnlyList<ItemDto> Itens,
    IReadOnlyList<GrupoCobrancaDto> Grupos,
    decimal Total,
    decimal CotaIgual);

public sealed record LancarItemRequest(long? LugarId, long CartaItemId, int Quantidade, string? Observacao, DestinoPedido? Destino);

public sealed record NomearLugarRequest(string? NomeCliente);

public sealed record TransferirItemRequest(long? LugarId);

public sealed record StatusItemRequest(StatusItem Status);

public sealed record CancelarItemRequest(string? Motivo);

public sealed record JuntarMesaRequest(long MesaId);

public sealed record FecharAtendimentoRequest(ModoFechamento Modo, long? LugarId);
