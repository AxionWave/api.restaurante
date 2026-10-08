using Microsoft.EntityFrameworkCore;
using Orion.Infrastructure.Persistence.Entities;

namespace Orion.Infrastructure.Persistence;

/// <summary>
/// Banco de negócio do Orion. Identidade (usuarios/empresas) permanece no Core.
/// Schema padrão: orion. Nomes de tabela/coluna em snake_case.
/// </summary>
public sealed class OrionDbContext(DbContextOptions<OrionDbContext> options) : DbContext(options)
{
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<EntradaEstoque> EntradasEstoque => Set<EntradaEstoque>();
    public DbSet<EntradaEstoqueItem> EntradasEstoqueItens => Set<EntradaEstoqueItem>();
    public DbSet<MapaCodigoBarras> MapaCodigoBarras => Set<MapaCodigoBarras>();
    public DbSet<Ambiente> Ambientes => Set<Ambiente>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<CartaCategoria> CartaCategorias => Set<CartaCategoria>();
    public DbSet<CartaItem> CartaItens => Set<CartaItem>();
    public DbSet<Atendimento> Atendimentos => Set<Atendimento>();
    public DbSet<AtendimentoLugar> AtendimentoLugares => Set<AtendimentoLugar>();
    public DbSet<ComandaItem> ComandaItens => Set<ComandaItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orion");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrionDbContext).Assembly);
        AplicarSnakeCase(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void AplicarSnakeCase(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var prop in entity.GetProperties())
            {
                prop.SetColumnName(ToSnake(prop.GetColumnName()));
            }
        }
    }

    private static string ToSnake(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }
        var sb = new System.Text.StringBuilder(name.Length + 8);
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && (!char.IsUpper(name[i - 1]) || (i + 1 < name.Length && !char.IsUpper(name[i + 1]))))
                {
                    sb.Append('_');
                }
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
