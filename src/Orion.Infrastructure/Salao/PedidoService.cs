using Microsoft.EntityFrameworkCore;
using Orion.Application.Salao;
using Orion.Infrastructure.Persistence;
using Orion.Infrastructure.Persistence.Entities;

namespace Orion.Infrastructure.Salao;

public sealed class PedidoService(OrionDbContext db, ISalaoService salao) : IPedidoService
{
    public async Task<AtendimentoDto> ObterVisitaDaMesaAsync(int empresaId, long mesaId, CancellationToken ct)
    {
        var mesa = await db.Mesas.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == mesaId && m.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Mesa não encontrada.", 404);
        if (mesa.AtendimentoAbertoId is not long id)
            throw new SalaoRegraException("Esta mesa não tem visita aberta.", 404);
        return await salao.ObterAtendimentoAsync(empresaId, id, ct);
    }

    public Task<AtendimentoDto> LancarItemAsync(int empresaId, long atendimentoId, int usuarioId, string nome, LancarItemRequest req, CancellationToken ct)
        => salao.LancarItemAsync(empresaId, atendimentoId, usuarioId, nome, req, ct);

    public async Task<AtendimentoDto> DefinirGruposAsync(int empresaId, long atendimentoId, DefinirGruposRequest req, CancellationToken ct)
    {
        var atendimento = await db.Atendimentos
            .Include(a => a.Lugares)
            .Include(a => a.Grupos).ThenInclude(g => g.Lugares)
            .FirstOrDefaultAsync(a => a.Id == atendimentoId && a.EmpresaId == empresaId, ct)
            ?? throw new SalaoRegraException("Visita não encontrada.", 404);
        if (atendimento.Status == StatusAtendimento.Fechada)
            throw new SalaoRegraException("Esta visita já foi fechada.");

        var grupos = req.Grupos ?? [];
        var vistos = new HashSet<long>();
        foreach (var grupo in grupos)
        {
            var ids = (grupo.LugarIds ?? []).Distinct().ToList();
            if (grupo.Modo == ModoGrupoCobranca.Junto && ids.Count < 2)
                throw new SalaoRegraException("Para pagar junto, escolha pelo menos duas pessoas da mesma visita.");
            if (ids.Count == 0)
                throw new SalaoRegraException("O grupo precisa de ao menos um lugar.");
            foreach (var lugarId in ids)
            {
                if (!vistos.Add(lugarId))
                    throw new SalaoRegraException("Uma pessoa não pode estar em dois grupos.");
                if (atendimento.Lugares.All(l => l.Id != lugarId))
                    throw new SalaoRegraException("Lugar não encontrado nesta visita.", 404);
            }
        }

        db.GruposCobranca.RemoveRange(atendimento.Grupos);
        foreach (var grupo in grupos)
        {
            var row = new GrupoCobranca { AtendimentoId = atendimento.Id, Modo = grupo.Modo };
            foreach (var lugarId in grupo.LugarIds.Distinct())
                row.Lugares.Add(new GrupoCobrancaLugar { LugarId = lugarId });
            db.GruposCobranca.Add(row);
        }

        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
        return await salao.ObterAtendimentoAsync(empresaId, atendimentoId, ct);
    }

    public async Task<ContaDto> ObterContaAsync(int empresaId, long atendimentoId, CancellationToken ct)
    {
        var visita = await salao.ObterAtendimentoAsync(empresaId, atendimentoId, ct);
        var ativos = visita.Itens.Where(i => i.Status != StatusItem.Cancelado).ToList();

        ContaPessoaDto Pessoa(LugarDto lugar)
        {
            var itens = ativos.Where(i => i.LugarId == lugar.Id)
                .Select(i => new ContaItemDto(i.Id, i.Descricao, i.Quantidade, i.PrecoUnitario, i.Total))
                .ToList();
            var nome = string.IsNullOrWhiteSpace(lugar.NomeCliente) ? $"Lugar {lugar.Ordem}" : lugar.NomeCliente!;
            return new ContaPessoaDto(lugar.Id, lugar.Ordem, nome, itens.Sum(i => i.Total), itens);
        }

        var porPessoa = visita.Lugares.OrderBy(l => l.Ordem).Select(Pessoa).ToList();
        var porLugar = porPessoa.ToDictionary(p => p.LugarId);
        var emGrupo = visita.Grupos.SelectMany(g => g.LugarIds).ToHashSet();

        var porGrupo = new List<ContaGrupoDto>();
        foreach (var grupo in visita.Grupos)
        {
            var pessoas = grupo.LugarIds.Select(id => porLugar[id]).OrderBy(p => p.Ordem).ToList();
            var titulo = string.Join(grupo.Modo == ModoGrupoCobranca.Junto ? " + " : " / ", pessoas.Select(p => p.Nome));
            porGrupo.Add(new ContaGrupoDto(grupo.Id, grupo.Modo, grupo.LugarIds, titulo, pessoas.Sum(p => p.Subtotal), pessoas));
        }

        foreach (var pessoa in porPessoa.Where(p => !emGrupo.Contains(p.LugarId)))
        {
            porGrupo.Add(new ContaGrupoDto(null, ModoGrupoCobranca.Separado, [pessoa.LugarId], pessoa.Nome, pessoa.Subtotal, [pessoa]));
        }

        return new ContaDto(visita.Id, visita.MesaId, visita.MesaRotulo, visita.Total, porPessoa, porGrupo);
    }
}
