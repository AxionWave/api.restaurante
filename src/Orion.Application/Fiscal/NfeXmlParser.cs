using System.Globalization;
using System.Xml.Linq;

namespace Orion.Application.Fiscal;

/// <summary>Parse puro de XML de NF-e / NFC-e (<c>nfeProc</c> ou <c>NFe</c>).</summary>
public static class NfeXmlParser
{
    private static readonly XNamespace Nfe = "http://www.portalfiscal.inf.br/nfe";

    public static NfeImportadaDto Parse(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new NfeXmlInvalidoException("XML vazio.");
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch (Exception ex)
        {
            throw new NfeXmlInvalidoException("XML mal formado: " + ex.Message);
        }

        var infNFe = doc.Descendants(Nfe + "infNFe").FirstOrDefault()
            ?? throw new NfeXmlInvalidoException("Elemento infNFe não encontrado — não parece uma NF-e.");

        var chave = (infNFe.Attribute("Id")?.Value ?? string.Empty)
            .Replace("NFe", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        if (!ChaveValida(chave))
        {
            throw new NfeXmlInvalidoException("Chave de acesso ausente ou inválida (DV).");
        }

        var ide = infNFe.Element(Nfe + "ide");
        var emit = infNFe.Element(Nfe + "emit");
        var dest = infNFe.Element(Nfe + "dest");
        var total = infNFe.Element(Nfe + "total")?.Element(Nfe + "ICMSTot");

        var modelo = El(ide, "mod") ?? chave.Substring(20, 2);

        var itens = infNFe.Elements(Nfe + "det")
            .Select(det => det.Element(Nfe + "prod"))
            .Where(prod => prod is not null)
            .Select(prod => new NfeItemImportadoDto(
                CodigoFornecedor: El(prod, "cProd"),
                CodigoBarras: NormalizarEan(El(prod, "cEAN")),
                DescricaoNf: El(prod, "xProd") ?? "(sem descrição)",
                Ncm: El(prod, "NCM"),
                UnidadeComercial: El(prod, "uCom"),
                Quantidade: Dec(El(prod, "qCom")) ?? 0m,
                ValorUnitario: Dec(El(prod, "vUnCom")),
                ValorTotal: Dec(El(prod, "vProd"))))
            .ToList();

        return new NfeImportadaDto(
            Chave: chave,
            Modelo: modelo,
            Numero: El(ide, "nNF"),
            Serie: El(ide, "serie"),
            FornecedorNome: El(emit, "xNome"),
            FornecedorCnpj: SoDigitos(El(emit, "CNPJ")),
            DestinatarioCnpj: SoDigitos(El(dest, "CNPJ")),
            DataEmissao: Data(El(ide, "dhEmi") ?? El(ide, "dEmi")),
            ValorTotal: Dec(El(total, "vNF")),
            Itens: itens);
    }

    public static bool ChaveValida(string? chave)
    {
        if (chave is null || chave.Length != 44 || !chave.All(char.IsDigit))
        {
            return false;
        }
        int soma = 0, peso = 2;
        for (int i = 42; i >= 0; i--)
        {
            soma += (chave[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }
        int resto = soma % 11;
        int dv = resto is 0 or 1 ? 0 : 11 - resto;
        return dv == chave[43] - '0';
    }

    private static string? El(XElement? parent, string name) =>
        string.IsNullOrWhiteSpace(parent?.Element(Nfe + name)?.Value) ? null : parent!.Element(Nfe + name)!.Value.Trim();

    private static decimal? Dec(string? v) =>
        decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;

    private static DateTime? Data(string? v) =>
        DateTimeOffset.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.UtcDateTime : null;

    private static string? SoDigitos(string? v) =>
        v is null ? null : new string(v.Where(char.IsDigit).ToArray()) is { Length: > 0 } s ? s : null;

    /// <summary>cEAN pode vir "SEM GTIN" ou vazio.</summary>
    private static string? NormalizarEan(string? v)
    {
        var s = v?.Trim();
        if (string.IsNullOrEmpty(s) || !s.All(char.IsDigit) || s.Length is < 8 or > 14)
        {
            return null;
        }
        return s;
    }
}

public sealed class NfeXmlInvalidoException(string message) : Exception(message);
