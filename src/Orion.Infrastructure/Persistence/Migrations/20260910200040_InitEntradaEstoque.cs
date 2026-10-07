using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Orion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitEntradaEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "orion");

            migrationBuilder.CreateTable(
                name: "entradas_estoque",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    chave_idempotencia = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<int>(type: "integer", nullable: false),
                    unidade_id = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    origem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    origem_cadastro = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fornecedor_id = table.Column<long>(type: "bigint", nullable: true),
                    fornecedor_nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fornecedor_cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    modelo_fiscal = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    chave_acesso = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    numero_nf = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    serie = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    xml_original = table.Column<string>(type: "text", nullable: true),
                    data_entrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    referencia_externa_core = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    criado_por_usuario_id = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelado_por_usuario_id = table.Column<int>(type: "integer", nullable: true),
                    cancelado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entradas_estoque", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fornecedores",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa_id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fornecedores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mapa_codigo_barras",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa_id = table.Column<int>(type: "integer", nullable: false),
                    codigo_barras = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    produto_core_id = table.Column<int>(type: "integer", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapa_codigo_barras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entradas_estoque_itens",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrada_estoque_id = table.Column<long>(type: "bigint", nullable: false),
                    produto_core_id = table.Column<int>(type: "integer", nullable: true),
                    produto_codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    descricao_nf = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    codigo_barras_nf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    ncm = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    unidade_comercial_nf = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    quantidade_nf = table.Column<decimal>(type: "numeric(15,3)", precision: 15, scale: 3, nullable: true),
                    quantidade_recebida = table.Column<decimal>(type: "numeric(15,3)", precision: 15, scale: 3, nullable: false),
                    fator_conversao = table.Column<decimal>(type: "numeric(15,4)", precision: 15, scale: 4, nullable: false, defaultValue: 1m),
                    valor_unitario_nf = table.Column<decimal>(type: "numeric(15,4)", precision: 15, scale: 4, nullable: true),
                    motivo_divergencia = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    status_vinculo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    movimentacao_core_id = table.Column<long>(type: "bigint", nullable: true),
                    referencia_externa_item = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entradas_estoque_itens", x => x.id);
                    table.ForeignKey(
                        name: "FK_entradas_estoque_itens_entradas_estoque_entrada_estoque_id",
                        column: x => x.entrada_estoque_id,
                        principalSchema: "orion",
                        principalTable: "entradas_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entradas_estoque_chave_idempotencia",
                schema: "orion",
                table: "entradas_estoque",
                column: "chave_idempotencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entradas_estoque_empresa_id_chave_acesso",
                schema: "orion",
                table: "entradas_estoque",
                columns: new[] { "empresa_id", "chave_acesso" },
                unique: true,
                filter: "chave_acesso IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_entradas_estoque_empresa_id_status_data_entrada",
                schema: "orion",
                table: "entradas_estoque",
                columns: new[] { "empresa_id", "status", "data_entrada" });

            migrationBuilder.CreateIndex(
                name: "IX_entradas_estoque_itens_entrada_estoque_id",
                schema: "orion",
                table: "entradas_estoque_itens",
                column: "entrada_estoque_id");

            migrationBuilder.CreateIndex(
                name: "IX_fornecedores_empresa_id_cnpj",
                schema: "orion",
                table: "fornecedores",
                columns: new[] { "empresa_id", "cnpj" },
                unique: true,
                filter: "cnpj IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_fornecedores_empresa_id_nome",
                schema: "orion",
                table: "fornecedores",
                columns: new[] { "empresa_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "IX_mapa_codigo_barras_empresa_id_codigo_barras",
                schema: "orion",
                table: "mapa_codigo_barras",
                columns: new[] { "empresa_id", "codigo_barras" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entradas_estoque_itens",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "fornecedores",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "mapa_codigo_barras",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "entradas_estoque",
                schema: "orion");
        }
    }
}
