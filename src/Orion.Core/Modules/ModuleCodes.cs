namespace Orion.Core.Modules;

/// <summary>
/// Códigos de módulo do Orion. Devem existir em core.modulos.
/// Raiz de segurança: ORION000000 (legado ORI0000000 ainda aceito).
/// </summary>
public static class ModuleCodes
{
    public const string Raiz = "ORION000000";
    public const string RaizLegado = "ORI0000000";
    public const string Cardapio = "ORI0000001";
    public const string Pedidos = "ORI0000002";
    public const string Mesas = "ORI0000003";
    public const string Estoque = "ORI0000004";

    public static readonly string[] Raizes = [Raiz, RaizLegado];

    public static readonly string[] Todos =
    [
        Raiz,
        RaizLegado,
        Cardapio,
        Pedidos,
        Mesas,
        Estoque
    ];
}
