namespace Orion.Infrastructure.Integracao;

public sealed class CoreServiceOptions
{
    public const string Section = "Services:Core";

    /// <summary>Base URL do Core (direto, sem Gateway). Ex.: http://core:8081</summary>
    public string BaseUrl { get; set; } = "http://localhost:8081";

    /// <summary>Mesmo valor de GATEWAY_INTERNAL_TOKEN configurado no Core/Gateway.</summary>
    public string InternalToken { get; set; } = string.Empty;
}

public sealed class FiscalNfeOptions
{
    public const string Section = "Services:FiscalNfe";

    /// <summary>none | nuvemfiscal | arquivei | ... (só "none" implementado).</summary>
    public string Provider { get; set; } = "none";
}
