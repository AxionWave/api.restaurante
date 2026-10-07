using System.Text;
using Microsoft.EntityFrameworkCore;
using Orion.Application.Estoque;
using Orion.Infrastructure.Estoque;
using Orion.Infrastructure.Persistence;
using Orion.Tests.Fiscal;

namespace Orion.Tests.Estoque;

public class EntradaEstoqueServiceTests
{
    private const int EmpresaId = 10;
    private const int UnidadeId = 100;

    private static (EntradaEstoqueService Servico, FakeCoreEstoqueClient Core, OrionDbContext Db) Criar(int? unidadeAtiva = UnidadeId)
    {
        var options = new DbContextOptionsBuilder<OrionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new OrionDbContext(options);
        var core = new FakeCoreEstoqueClient();
        var identidade = new FakeCoreIdentidadeClient(UnidadeId);
        var servico = new EntradaEstoqueService(
            db, core, identidade, new FakeFornecedorNfe(),
            new FakeCurrentUserAccessor(EmpresaId), new FakeCurrentUnidadeAccessor(unidadeAtiva));
        return (servico, core, db);
    }

    private static CriarEntradaRequest EntradaManual(int produtoId, decimal quantidade, TipoEntrada tipo = TipoEntrada.Entrada,
        OrigemEntrada origem = OrigemEntrada.Manual, decimal? valorUnitario = null, decimal? quantidadeNf = null) =>
        new(Guid.NewGuid(), UnidadeId, tipo, origem, null, null, null,
            [new ItemEntradaRequest(produtoId, null, null, null, quantidadeNf, quantidade, null, valorUnitario)]);

    [Fact]
    public async Task CriarEConfirmar_SemNota_CreditaSaldo()
    {
        var (servico, core, _) = Criar();
        var produto = core.AdicionarProduto("CAFE-500", "Café 500g");

        var rascunho = await servico.CriarAsync(EntradaManual(produto.Id, 24));
        Assert.Equal("RASCUNHO", rascunho.Status);
        Assert.Equal("VINCULADO", rascunho.Itens[0].StatusVinculo);

        var confirmada = await servico.ConfirmarAsync(rascunho.Id);

        Assert.Equal("CONFIRMADA", confirmada.Status);
        Assert.Equal(24, core.SaldoAtual(produto.Id));
        Assert.NotNull(confirmada.Itens[0].MovimentacaoCoreId);
    }

    [Fact]
    public async Task Confirmar_ConverteUnidadeDeCompra()
    {
        var (servico, core, _) = Criar();
        var produto = core.AdicionarProduto("CAFE-500", "Café 500g", unidadeCompra: "CX", fatorConversao: 12m);

        var req = EntradaManual(produto.Id, 2, origem: OrigemEntrada.Compra, valorUnitario: 60.00m);
        var rascunho = await servico.CriarAsync(req);
        Assert.Equal(12m, rascunho.Itens[0].FatorConversao); // herdado do produto
        Assert.Equal(24m, rascunho.Itens[0].QuantidadeEstoque);
        Assert.Equal(5.00m, rascunho.Itens[0].CustoUnitarioEstoque);

        await servico.ConfirmarAsync(rascunho.Id);

        Assert.Equal(24, core.SaldoAtual(produto.Id));
    }

    [Fact]
    public async Task Confirmar_Contagem_UsaAjusteEDetectaDivergencia()
    {
        var (servico, core, _) = Criar();
        var produto = core.AdicionarProduto("ARROZ-5KG", "Arroz 5kg");

        var req = new CriarEntradaRequest(Guid.NewGuid(), UnidadeId, TipoEntrada.Ajuste, OrigemEntrada.Inventario,
            null, null, null,
            [new ItemEntradaRequest(produto.Id, null, null, null, QuantidadeNf: 10, QuantidadeRecebida: 7, null, null)]);
        var rascunho = await servico.CriarAsync(req);

        Assert.True(rascunho.Itens[0].Divergencia);

        var confirmada = await servico.ConfirmarAsync(rascunho.Id);

        Assert.Equal("CONFIRMADA", confirmada.Status);
        Assert.Equal(7, core.SaldoAtual(produto.Id)); // AJUSTE define o saldo absoluto, não soma
    }

    [Fact]
    public async Task CriarComMesmaChaveIdempotencia_DevolveAMesmaEntrada()
    {
        var (servico, core, _) = Criar();
        var produto = core.AdicionarProduto("CAFE-500", "Café 500g");
        var chave = Guid.NewGuid();
        var req = new CriarEntradaRequest(chave, UnidadeId, TipoEntrada.Entrada, OrigemEntrada.Manual, null, null, null,
            [new ItemEntradaRequest(produto.Id, null, null, null, null, 5, null, null)]);

        var primeira = await servico.CriarAsync(req);
        var segunda = await servico.CriarAsync(req);

        Assert.Equal(primeira.Id, segunda.Id);
    }

    [Fact]
    public async Task Reconfirmar_NaoDuplicaSaldo()
    {
        var (servico, core, _) = Criar();
        var produto = core.AdicionarProduto("CAFE-500", "Café 500g");
        var rascunho = await servico.CriarAsync(EntradaManual(produto.Id, 10));

        await servico.ConfirmarAsync(rascunho.Id);
        await servico.ConfirmarAsync(rascunho.Id);
        await servico.ConfirmarAsync(rascunho.Id);

        Assert.Equal(10, core.SaldoAtual(produto.Id));
    }

    [Fact]
    public async Task Confirmar_ComItemPendente_LancaErro()
    {
        var (servico, _, _) = Criar();
        var req = new CriarEntradaRequest(Guid.NewGuid(), UnidadeId, TipoEntrada.Entrada, OrigemEntrada.Manual, null, null, null,
            [new ItemEntradaRequest(null, "0000000000001", null, null, null, 3, null, null)]); // código de barras não cadastrado

        var rascunho = await servico.CriarAsync(req);
        Assert.Equal("PENDENTE", rascunho.Itens[0].StatusVinculo);

        await Assert.ThrowsAsync<EstoqueRegraException>(() => servico.ConfirmarAsync(rascunho.Id));
    }

    [Fact]
    public async Task SemUnidadeInformadaEMultiplasUnidades_ExigeEscolha()
    {
        var options = new DbContextOptionsBuilder<OrionDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new OrionDbContext(options);
        var core = new FakeCoreEstoqueClient();
        var identidadeComDuasUnidades = new MultiUnidadeIdentidadeClient();
        var servico = new EntradaEstoqueService(db, core, identidadeComDuasUnidades, new FakeFornecedorNfe(),
            new FakeCurrentUserAccessor(EmpresaId), new FakeCurrentUnidadeAccessor(null));
        var produto = core.AdicionarProduto("X", "X");

        var req = new CriarEntradaRequest(Guid.NewGuid(), null, TipoEntrada.Entrada, OrigemEntrada.Manual, null, null, null,
            [new ItemEntradaRequest(produto.Id, null, null, null, null, 1, null, null)]);

        await Assert.ThrowsAsync<EstoqueRegraException>(() => servico.CriarAsync(req));
    }

    [Fact]
    public async Task ChaveSemProvedor_DepoisImportarXmlMesmaChave_PreencheOMesmoRascunho()
    {
        var (servico, core, _) = Criar();
        core.AdicionarProduto("CAFE-500", "Café 500g", codigoBarras: "7891910000197");
        var chave = NfeFixtures.GerarChave("55");

        var (stub, completa) = await servico.ImportarPorChaveAsync(chave, UnidadeId);
        Assert.False(completa);
        Assert.Empty(stub.Itens);
        Assert.Equal(chave, stub.ChaveAcesso);

        var xmlBytes = Encoding.UTF8.GetBytes(NfeFixtures.MontarXml(chave, "55"));
        var preenchida = await servico.ImportarXmlAsync(new MemoryStream(xmlBytes), UnidadeId);

        Assert.Equal(stub.Id, preenchida.Id); // mesmo registro, não duplicou
        Assert.Equal(2, preenchida.Itens.Count);
        Assert.Equal("VINCULADO", preenchida.Itens[0].StatusVinculo); // achou pelo EAN
        Assert.Equal("PENDENTE", preenchida.Itens[1].StatusVinculo); // sem EAN cadastrado
    }

    private sealed class MultiUnidadeIdentidadeClient : ICoreIdentidadeClient
    {
        public Task<IReadOnlyList<UnidadeCore>> ListarUnidadesAsync(int empresaId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<UnidadeCore>>([new UnidadeCore(1, "A", null, null, null), new UnidadeCore(2, "B", null, null, null)]);

        public Task<EmpresaCore?> BuscarEmpresaAsync(int empresaId, CancellationToken ct = default) =>
            Task.FromResult<EmpresaCore?>(new EmpresaCore(empresaId, "E", "00000000000000"));
    }
}
