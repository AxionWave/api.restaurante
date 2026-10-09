using Microsoft.EntityFrameworkCore;
using Orion.Application.Salao;
using Orion.Infrastructure.Persistence;
using Orion.Infrastructure.Persistence.Entities;
using Orion.Infrastructure.Salao;

namespace Orion.Tests.Salao;

public class PedidoSalaoServiceTests
{
    private const int EmpresaA = 10;
    private const int EmpresaB = 20;

    private static (SalaoService Salao, PedidoService Pedidos, OrionDbContext Db) Criar()
    {
        var options = new DbContextOptionsBuilder<OrionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new OrionDbContext(options);
        var salao = new SalaoService(db);
        return (salao, new PedidoService(db, salao), db);
    }

    private static async Task<(Mesa Mesa, CartaItem Prato)> MontarCasaAsync(
        OrionDbContext db, int empresaId, int lugares = 4, string rotulo = "12")
    {
        var ambiente = new Ambiente { EmpresaId = empresaId, Nome = "Salão", Ordem = 1, Ativo = true };
        db.Ambientes.Add(ambiente);
        await db.SaveChangesAsync();

        var mesa = new Mesa
        {
            EmpresaId = empresaId,
            AmbienteId = ambiente.Id,
            Rotulo = rotulo,
            LugaresPadrao = lugares,
            Forma = FormaMesa.Retangular,
            Situacao = SituacaoMesa.Ativa
        };
        var categoria = new CartaCategoria { EmpresaId = empresaId, Nome = "Pratos", Ordem = 1, Ativo = true };
        db.Mesas.Add(mesa);
        db.CartaCategorias.Add(categoria);
        await db.SaveChangesAsync();

        var prato = new CartaItem
        {
            EmpresaId = empresaId,
            CategoriaId = categoria.Id,
            Nome = "Risoto",
            Preco = 40m,
            Destino = DestinoPedido.Cozinha,
            Ativo = true
        };
        db.CartaItens.Add(prato);
        await db.SaveChangesAsync();
        return (mesa, prato);
    }

    [Fact]
    public async Task AbrirMesa_CriaLugaresIguaisAoPadrao()
    {
        var (salao, _, db) = Criar();
        var (mesa, _) = await MontarCasaAsync(db, EmpresaA, lugares: 3);

        var visita = await salao.AbrirAsync(EmpresaA, mesa.Id, 1, "Ana", CancellationToken.None);

        Assert.Equal(3, visita.Lugares.Count);
        Assert.All(visita.Lugares, l => Assert.Null(l.NomeCliente));
        Assert.Equal(mesa.Id, visita.MesaId);
    }

    [Fact]
    public async Task Nomear_NaoCriaCadastro_EMapaFicaVazioAposFechar()
    {
        var (salao, _, db) = Criar();
        var (mesa, _) = await MontarCasaAsync(db, EmpresaA);
        var visita = await salao.AbrirAsync(EmpresaA, mesa.Id, 1, "Ana", CancellationToken.None);
        var lugar = visita.Lugares[0];

        visita = await salao.NomearLugarAsync(EmpresaA, lugar.Id, new NomearLugarRequest("João"), CancellationToken.None);

        Assert.Equal("João", visita.Lugares.First(l => l.Id == lugar.Id).NomeCliente);
        Assert.Equal(0, await db.Atendimentos.CountAsync(a => a.EmpresaId != EmpresaA));

        await salao.FecharAsync(EmpresaA, visita.Id, new FecharAtendimentoRequest(ModoFechamento.PorPessoa, null), CancellationToken.None);

        var mapa = await salao.MapaAsync(EmpresaA, CancellationToken.None);
        var mesaMapa = mapa.SelectMany(a => a.Mesas).Single(m => m.Id == mesa.Id);
        Assert.Null(mesaMapa.AtendimentoId);
        Assert.Empty(mesaMapa.LugaresVisita);

        var registro = await db.AtendimentoLugares.AsNoTracking().SingleAsync(l => l.Id == lugar.Id);
        Assert.Equal("João", registro.NomeCliente);
    }

    [Fact]
    public async Task LancarSemNome_LancaRegra()
    {
        var (salao, pedidos, db) = Criar();
        var (mesa, prato) = await MontarCasaAsync(db, EmpresaA);
        var visita = await salao.AbrirAsync(EmpresaA, mesa.Id, 1, "Ana", CancellationToken.None);
        var req = new LancarItemRequest(visita.Lugares[0].Id, prato.Id, 1, null, null);

        var erro = await Assert.ThrowsAsync<SalaoRegraException>(() =>
            pedidos.LancarItemAsync(EmpresaA, visita.Id, 1, "Ana", req, CancellationToken.None));

        Assert.Contains("nome", erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Grupos_JuntoSomaOsDois_SeparadoListaSubtotais_SolteiroSoODele()
    {
        var (salao, pedidos, db) = Criar();
        var (mesa, prato) = await MontarCasaAsync(db, EmpresaA, lugares: 5);
        var visita = await salao.AbrirAsync(EmpresaA, mesa.Id, 1, "Ana", CancellationToken.None);
        var l1 = visita.Lugares[0].Id;
        var l2 = visita.Lugares[1].Id;
        var l3 = visita.Lugares[2].Id;
        var l4 = visita.Lugares[3].Id;
        var l5 = visita.Lugares[4].Id;

        await salao.NomearLugarAsync(EmpresaA, l1, new NomearLugarRequest("A1"), CancellationToken.None);
        await salao.NomearLugarAsync(EmpresaA, l2, new NomearLugarRequest("A2"), CancellationToken.None);
        await salao.NomearLugarAsync(EmpresaA, l3, new NomearLugarRequest("B1"), CancellationToken.None);
        await salao.NomearLugarAsync(EmpresaA, l4, new NomearLugarRequest("B2"), CancellationToken.None);
        await salao.NomearLugarAsync(EmpresaA, l5, new NomearLugarRequest("C"), CancellationToken.None);

        await pedidos.LancarItemAsync(EmpresaA, visita.Id, 1, "Ana", new LancarItemRequest(l1, prato.Id, 1, null, null), CancellationToken.None);
        await pedidos.LancarItemAsync(EmpresaA, visita.Id, 1, "Ana", new LancarItemRequest(l2, prato.Id, 1, null, null), CancellationToken.None);
        await pedidos.LancarItemAsync(EmpresaA, visita.Id, 1, "Ana", new LancarItemRequest(l3, prato.Id, 1, null, null), CancellationToken.None);
        await pedidos.LancarItemAsync(EmpresaA, visita.Id, 1, "Ana", new LancarItemRequest(l4, prato.Id, 2, null, null), CancellationToken.None);
        await pedidos.LancarItemAsync(EmpresaA, visita.Id, 1, "Ana", new LancarItemRequest(l5, prato.Id, 1, null, null), CancellationToken.None);

        await pedidos.DefinirGruposAsync(EmpresaA, visita.Id, new DefinirGruposRequest(
        [
            new GrupoCobrancaRequest(ModoGrupoCobranca.Junto, [l1, l2]),
            new GrupoCobrancaRequest(ModoGrupoCobranca.Separado, [l3, l4])
        ]), CancellationToken.None);

        var conta = await pedidos.ObterContaAsync(EmpresaA, visita.Id, CancellationToken.None);

        Assert.Equal(240m, conta.Total);
        var junto = Assert.Single(conta.PorGrupo, g => g.Modo == ModoGrupoCobranca.Junto);
        Assert.Equal(80m, junto.Total);
        Assert.Equal(2, junto.Pessoas.Count);

        var separado = Assert.Single(conta.PorGrupo, g => g.Id is not null && g.Modo == ModoGrupoCobranca.Separado);
        Assert.Equal(120m, separado.Total);
        Assert.Equal(2, separado.Pessoas.Count);
        Assert.Contains(separado.Pessoas, p => p.LugarId == l3 && p.Subtotal == 40m);
        Assert.Contains(separado.Pessoas, p => p.LugarId == l4 && p.Subtotal == 80m);

        var solteiro = Assert.Single(conta.PorGrupo, g => g.Id is null);
        Assert.Equal(l5, Assert.Single(solteiro.LugarIds));
        Assert.Equal(40m, solteiro.Total);
    }

    [Fact]
    public async Task EmpresaA_NaoVeVisitaDaEmpresaB()
    {
        var (salao, pedidos, db) = Criar();
        var (mesaB, _) = await MontarCasaAsync(db, EmpresaB, rotulo: "B1");
        var visitaB = await salao.AbrirAsync(EmpresaB, mesaB.Id, 2, "Bia", CancellationToken.None);

        var erroVisita = await Assert.ThrowsAsync<SalaoRegraException>(() =>
            pedidos.ObterVisitaDaMesaAsync(EmpresaA, mesaB.Id, CancellationToken.None));
        Assert.Equal(404, erroVisita.Status);

        var erroConta = await Assert.ThrowsAsync<SalaoRegraException>(() =>
            pedidos.ObterContaAsync(EmpresaA, visitaB.Id, CancellationToken.None));
        Assert.Equal(404, erroConta.Status);

        var erroGrupo = await Assert.ThrowsAsync<SalaoRegraException>(() =>
            pedidos.DefinirGruposAsync(EmpresaA, visitaB.Id, new DefinirGruposRequest([]), CancellationToken.None));
        Assert.Equal(404, erroGrupo.Status);
    }

    [Fact]
    public async Task GrupoJunto_ExigeDoisLugaresDaMesmaVisita()
    {
        var (salao, pedidos, db) = Criar();
        var (mesa, _) = await MontarCasaAsync(db, EmpresaA, lugares: 2);
        var visita = await salao.AbrirAsync(EmpresaA, mesa.Id, 1, "Ana", CancellationToken.None);

        var erro = await Assert.ThrowsAsync<SalaoRegraException>(() =>
            pedidos.DefinirGruposAsync(EmpresaA, visita.Id, new DefinirGruposRequest(
            [
                new GrupoCobrancaRequest(ModoGrupoCobranca.Junto, [visita.Lugares[0].Id])
            ]), CancellationToken.None));

        Assert.Contains("duas", erro.Message, StringComparison.OrdinalIgnoreCase);
    }
}
