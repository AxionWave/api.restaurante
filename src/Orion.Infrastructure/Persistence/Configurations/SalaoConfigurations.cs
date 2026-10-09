using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orion.Infrastructure.Persistence.Entities;

namespace Orion.Infrastructure.Persistence.Configurations;

public sealed class AmbienteConfig : IEntityTypeConfiguration<Ambiente>
{
    public void Configure(EntityTypeBuilder<Ambiente> b)
    {
        b.ToTable("ambientes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(80).IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Nome }).IsUnique();
        b.HasIndex(x => new { x.EmpresaId, x.Ordem });
        b.HasMany(x => x.Mesas).WithOne(x => x.Ambiente!).HasForeignKey(x => x.AmbienteId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class MesaConfig : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> b)
    {
        b.ToTable("mesas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Rotulo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Forma).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Situacao).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => new { x.EmpresaId, x.Rotulo }).IsUnique();
        b.HasIndex(x => x.AtendimentoAbertoId);
    }
}

public sealed class CartaCategoriaConfig : IEntityTypeConfiguration<CartaCategoria>
{
    public void Configure(EntityTypeBuilder<CartaCategoria> b)
    {
        b.ToTable("carta_categorias");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(80).IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Nome }).IsUnique();
        b.HasMany(x => x.Itens).WithOne(x => x.Categoria!).HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CartaItemConfig : IEntityTypeConfiguration<CartaItem>
{
    public void Configure(EntityTypeBuilder<CartaItem> b)
    {
        b.ToTable("carta_itens");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(160).IsRequired();
        b.Property(x => x.Preco).HasPrecision(12, 2);
        b.Property(x => x.Destino).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => new { x.EmpresaId, x.CategoriaId });
    }
}

public sealed class AtendimentoConfig : IEntityTypeConfiguration<Atendimento>
{
    public void Configure(EntityTypeBuilder<Atendimento> b)
    {
        b.ToTable("atendimentos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.ModoFechamento).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.AbertoPorNome).HasMaxLength(160).IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Status });
        b.HasOne(x => x.Mesa).WithMany().HasForeignKey(x => x.MesaId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Lugares).WithOne(x => x.Atendimento!).HasForeignKey(x => x.AtendimentoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Itens).WithOne(x => x.Atendimento!).HasForeignKey(x => x.AtendimentoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Grupos).WithOne(x => x.Atendimento!).HasForeignKey(x => x.AtendimentoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class GrupoCobrancaConfig : IEntityTypeConfiguration<GrupoCobranca>
{
    public void Configure(EntityTypeBuilder<GrupoCobranca> b)
    {
        b.ToTable("grupos_cobranca");
        b.HasKey(x => x.Id);
        b.Property(x => x.Modo).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => x.AtendimentoId);
        b.HasMany(x => x.Lugares).WithOne(x => x.Grupo!).HasForeignKey(x => x.GrupoCobrancaId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class GrupoCobrancaLugarConfig : IEntityTypeConfiguration<GrupoCobrancaLugar>
{
    public void Configure(EntityTypeBuilder<GrupoCobrancaLugar> b)
    {
        b.ToTable("grupo_cobranca_lugares");
        b.HasKey(x => new { x.GrupoCobrancaId, x.LugarId });
        b.HasIndex(x => x.LugarId).IsUnique();
        b.HasOne(x => x.Lugar).WithMany().HasForeignKey(x => x.LugarId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AtendimentoLugarConfig : IEntityTypeConfiguration<AtendimentoLugar>
{
    public void Configure(EntityTypeBuilder<AtendimentoLugar> b)
    {
        b.ToTable("atendimento_lugares");
        b.HasKey(x => x.Id);
        b.Property(x => x.NomeCliente).HasMaxLength(120);
        b.HasIndex(x => new { x.AtendimentoId, x.Ordem }).IsUnique();
    }
}

public sealed class ComandaItemConfig : IEntityTypeConfiguration<ComandaItem>
{
    public void Configure(EntityTypeBuilder<ComandaItem> b)
    {
        b.ToTable("comanda_itens");
        b.HasKey(x => x.Id);
        b.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
        b.Property(x => x.PrecoUnitario).HasPrecision(12, 2);
        b.Property(x => x.Observacao).HasMaxLength(500);
        b.Property(x => x.Destino).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.LancadoPorNome).HasMaxLength(160).IsRequired();
        b.Property(x => x.CanceladoPorNome).HasMaxLength(160);
        b.Property(x => x.MotivoCancelamento).HasMaxLength(300);
        b.HasIndex(x => x.AtendimentoId);
        b.HasIndex(x => x.LugarId);
    }
}
