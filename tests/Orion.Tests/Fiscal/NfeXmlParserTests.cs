using Orion.Application.Fiscal;

namespace Orion.Tests.Fiscal;

public class NfeXmlParserTests
{
    [Fact]
    public void ParseiaNfeModelo55ComDoisItens()
    {
        var chave = NfeFixtures.GerarChave("55");
        var xml = NfeFixtures.MontarXml(chave, "55");

        var nfe = NfeXmlParser.Parse(xml);

        Assert.Equal(chave, nfe.Chave);
        Assert.Equal("55", nfe.Modelo);
        Assert.Equal("12345", nfe.Numero);
        Assert.Equal("Atacadao Distribuidora SA", nfe.FornecedorNome);
        Assert.Equal("12345678000199", nfe.FornecedorCnpj);
        Assert.Equal("98765432000188", nfe.DestinatarioCnpj);
        Assert.Equal(209.88m, nfe.ValorTotal);
        Assert.Equal(2, nfe.Itens.Count);

        var cafe = nfe.Itens[0];
        Assert.Equal("7891910000197", cafe.CodigoBarras);
        Assert.Equal("CX", cafe.UnidadeComercial);
        Assert.Equal(2.0000m, cafe.Quantidade);
        Assert.Equal(60.0000m, cafe.ValorUnitario);

        var oleo = nfe.Itens[1];
        Assert.Null(oleo.CodigoBarras); // "SEM GTIN" normalizado para nulo
    }

    [Fact]
    public void ParseiaNfceModelo65()
    {
        var chave = NfeFixtures.GerarChave("65");
        var xml = NfeFixtures.MontarXml(chave, "65");

        var nfe = NfeXmlParser.Parse(xml);

        Assert.Equal("65", nfe.Modelo);
        Assert.Equal(chave, nfe.Chave);
    }

    [Fact]
    public void RejeitaChaveComDigitoVerificadorInvalido()
    {
        var chaveValida = NfeFixtures.GerarChave("55");
        var chaveInvalida = chaveValida[..43] + (chaveValida[43] == '0' ? '1' : '0');
        var xml = NfeFixtures.MontarXml(chaveInvalida, "55");

        var ex = Assert.Throws<NfeXmlInvalidoException>(() => NfeXmlParser.Parse(xml));
        Assert.Contains("chave", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RejeitaXmlSemInfNFe()
    {
        Assert.Throws<NfeXmlInvalidoException>(() => NfeXmlParser.Parse("<algumaCoisa/>"));
    }

    [Fact]
    public void RejeitaXmlMalFormado()
    {
        Assert.Throws<NfeXmlInvalidoException>(() => NfeXmlParser.Parse("<nao fecha"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("00000000000000000000000000000000000000000A")]
    public void ChaveValidaRejeitaFormatosInvalidos(string chave)
    {
        Assert.False(NfeXmlParser.ChaveValida(chave));
    }
}
