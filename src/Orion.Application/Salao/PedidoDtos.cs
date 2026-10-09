namespace Orion.Application.Salao;

public sealed record GrupoCobrancaRequest(ModoGrupoCobranca Modo, IReadOnlyList<long> LugarIds);

public sealed record DefinirGruposRequest(IReadOnlyList<GrupoCobrancaRequest> Grupos);

public sealed record ContaItemDto(
    long Id,
    string Descricao,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Total);

public sealed record ContaPessoaDto(
    long LugarId,
    int Ordem,
    string Nome,
    decimal Subtotal,
    IReadOnlyList<ContaItemDto> Itens);

public sealed record ContaGrupoDto(
    long? Id,
    ModoGrupoCobranca Modo,
    IReadOnlyList<long> LugarIds,
    string Titulo,
    decimal Total,
    IReadOnlyList<ContaPessoaDto> Pessoas);

public sealed record ContaDto(
    long AtendimentoId,
    long MesaId,
    string MesaRotulo,
    decimal Total,
    IReadOnlyList<ContaPessoaDto> PorPessoa,
    IReadOnlyList<ContaGrupoDto> PorGrupo);
