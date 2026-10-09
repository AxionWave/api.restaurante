using Orion.Application.Abstractions;
using Orion.Application.Estoque;
using Orion.Application.Fiscal;
using Orion.Infrastructure.Auth;
using Orion.Application.Salao;
using Orion.Infrastructure.Estoque;
using Orion.Infrastructure.Salao;
using Orion.Infrastructure.Integracao;
using Orion.Infrastructure.Integracao.Fiscal;
using Orion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Orion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
        services.AddScoped<ICurrentUnidadeAccessor, HttpCurrentUnidadeAccessor>();

        var conn = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(conn))
        {
            services.AddDbContext<OrionDbContext>(o => o.UseNpgsql(conn, npg => npg.MigrationsHistoryTable("__ef_migrations_history", "orion")));
        }

        services.Configure<CoreServiceOptions>(configuration.GetSection(CoreServiceOptions.Section));
        services.Configure<FiscalNfeOptions>(configuration.GetSection(FiscalNfeOptions.Section));

        var coreBaseUrl = configuration[$"{CoreServiceOptions.Section}:BaseUrl"] ?? "http://localhost:8081";

        services.AddTransient<TokenInternoHandler>();
        services.AddTransient<RepasseTokenUsuarioHandler>();

        services.AddHttpClient<ICoreEstoqueClient, CoreEstoqueClient>(c => c.BaseAddress = new Uri(coreBaseUrl))
            .AddHttpMessageHandler<TokenInternoHandler>()
            .AddStandardResilienceHandler();

        services.AddHttpClient<ICoreIdentidadeClient, CoreIdentidadeClient>(c => c.BaseAddress = new Uri(coreBaseUrl))
            .AddHttpMessageHandler<RepasseTokenUsuarioHandler>()
            .AddStandardResilienceHandler();

        // Provedor fiscal (modo "importar por chave") — plugável; hoje só "none".
        services.AddSingleton<IFornecedorNfe, FornecedorNfeNaoConfigurado>();

        services.AddScoped<IEntradaEstoqueService, EntradaEstoqueService>();
        services.AddScoped<ISalaoService, SalaoService>();
        services.AddScoped<IPedidoService, PedidoService>();

        return services;
    }
}
