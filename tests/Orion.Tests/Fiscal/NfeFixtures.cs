namespace Orion.Tests.Fiscal;

/// <summary>Gera chaves de acesso válidas (DV correto) e XML de NF-e/NFC-e de exemplo para os testes.</summary>
public static class NfeFixtures
{
    public static string CalcularDv(string chave43)
    {
        int soma = 0, peso = 2;
        for (int i = 42; i >= 0; i--)
        {
            soma += (chave43[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }
        int resto = soma % 11;
        int dv = resto is 0 or 1 ? 0 : 11 - resto;
        return dv.ToString();
    }

    public static string GerarChave(string modelo = "55", string cnpjEmitente = "12345678000199")
    {
        var b43 = "35" + "2601" + cnpjEmitente + modelo + "001" + "000012345" + "1" + "12345678";
        return b43 + CalcularDv(b43);
    }

    public static string MontarXml(string chave, string modelo = "55", string cnpjDestinatario = "98765432000188") => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe">
          <NFe>
            <infNFe Id="NFe{chave}" versao="4.00">
              <ide>
                <cUF>35</cUF>
                <mod>{modelo}</mod>
                <serie>1</serie>
                <nNF>12345</nNF>
                <dhEmi>2026-01-15T10:30:00-03:00</dhEmi>
              </ide>
              <emit>
                <CNPJ>12345678000199</CNPJ>
                <xNome>Atacadao Distribuidora SA</xNome>
              </emit>
              <dest>
                <CNPJ>{cnpjDestinatario}</CNPJ>
              </dest>
              <det nItem="1">
                <prod>
                  <cProd>CAFE500</cProd>
                  <cEAN>7891910000197</cEAN>
                  <xProd>Cafe torrado 500g</xProd>
                  <NCM>09012100</NCM>
                  <uCom>CX</uCom>
                  <qCom>2.0000</qCom>
                  <vUnCom>60.0000</vUnCom>
                  <vProd>120.00</vProd>
                </prod>
              </det>
              <det nItem="2">
                <prod>
                  <cProd>OLEO900</cProd>
                  <cEAN>SEM GTIN</cEAN>
                  <xProd>Oleo de soja 900ml</xProd>
                  <NCM>15071000</NCM>
                  <uCom>UN</uCom>
                  <qCom>12.0000</qCom>
                  <vUnCom>7.4900</vUnCom>
                  <vProd>89.88</vProd>
                </prod>
              </det>
              <total>
                <ICMSTot>
                  <vNF>209.88</vNF>
                </ICMSTot>
              </total>
            </infNFe>
          </NFe>
        </nfeProc>
        """;
}
