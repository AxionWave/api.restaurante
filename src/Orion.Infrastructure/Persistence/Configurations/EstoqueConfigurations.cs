using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orion.Infrastructure.Persistence.Entities;

namespace Orion.Infrastructure.Persistence.Configurations;

public sealed class FornecedorConfig : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> b)
    {
        b.ToTable("fornecedores");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        b.Property(x => x.Cnpj).HasMaxLength(14);
        b.HasIndex(x => new { x.EmpresaId, x.Cnpj }).IsUnique().HasFilter("cnpj IS NOT NULL");
        b.HasIndex(x => new { x.EmpresaId, x.Nome });
    }
}

public sealed class EntradaEstoqueConfig : IEntityTypeConfiguration<EntradaEstoque>
{
    public void Configure(EntityTypeBuilder<EntradaEstoque> b)
    {
        b.ToTable("entradas_estoque");
        b.HasKey(x => x.Id);

        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Origem).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.OrigemCadastro).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        b.Property(x => x.FornecedorNome).HasMaxLength(200);
        b.Property(x => x.FornecedorCnpj).HasMaxLength(14);
        b.Property(x => x.ModeloFiscal).HasMaxLength(2);
        b.Property(x => x.ChaveAcesso).HasMaxLength(44);
        b.Property(x => x.NumeroNf).HasMaxLength(20);
        b.Property(x => x.Serie).HasMaxLength(10);
        b.Property(x => x.ValorTotal).HasPrecision(15, 2);
        b.Property(x => x.ReferenciaExternaCore).HasMaxLength(160).IsRequired();
        b.Property(x => x.Observacao).HasMaxLength(2000);
        b.Property(x => x.XmlOriginal).HasColumnType("text");

        b.HasIndex(x => x.ChaveIdempotencia).IsUnique();
        b.HasIndex(x => new { x.EmpresaId, x.ChaveAcesso }).IsUnique().HasFilter("chave_acesso IS NOT NULL");
        b.HasIndex(x => new { x.EmpresaId, x.Status, x.DataEntrada });

        b.HasMany(x => x.Itens)
            .WithOne(i => i.Entrada!)
            .HasForeignKey(i => i.EntradaEstoqueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class EntradaEstoqueItemConfig : IEntityTypeConfiguration<EntradaEstoqueItem>
{
    public void Configure(EntityTypeBuilder<EntradaEstoqueItem> b)
    {
        b.ToTable("entradas_estoque_itens");
        b.HasKey(x => x.Id);

        b.Property(x => x.ProdutoCodigo).HasMaxLength(40);
        b.Property(x => x.DescricaoNf).HasMaxLength(300);
        b.Property(x => x.CodigoBarrasNf).HasMaxLength(14);
        b.Property(x => x.Ncm).HasMaxLength(10);
        b.Property(x => x.UnidadeComercialNf).HasMaxLength(10);
        b.Property(x => x.QuantidadeNf).HasPrecision(15, 3);
        b.Property(x => x.QuantidadeRecebida).HasPrecision(15, 3);
        b.Property(x => x.FatorConversao).HasPrecision(15, 4).HasDefaultValue(1m);
        b.Property(x => x.ValorUnitarioNf).HasPrecision(15, 4);
        b.Property(x => x.MotivoDivergencia).HasMaxLength(300);
        b.Property(x => x.StatusVinculo).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.ReferenciaExternaItem).HasMaxLength(180);

        b.Ignore(x => x.QuantidadeEstoque);
        b.Ignore(x => x.CustoUnitarioEstoque);
        b.Ignore(x => x.Divergencia);

        b.HasIndex(x => x.EntradaEstoqueId);
    }
}

public sealed class MapaCodigoBarrasConfig : IEntityTypeConfiguration<MapaCodigoBarras>
{
    public void Configure(EntityTypeBuilder<MapaCodigoBarras> b)
    {
        b.ToTable("mapa_codigo_barras");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoBarras).HasMaxLength(14).IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.CodigoBarras }).IsUnique();
    }
}
