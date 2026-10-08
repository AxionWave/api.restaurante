using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Orion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Salao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ambientes",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa_id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ambientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "carta_categorias",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa_id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carta_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mesas",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa_id = table.Column<int>(type: "integer", nullable: false),
                    ambiente_id = table.Column<long>(type: "bigint", nullable: false),
                    rotulo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    lugares_padrao = table.Column<int>(type: "integer", nullable: false),
                    forma = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    situacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    atendimento_aberto_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mesas", x => x.id);
                    table.ForeignKey(
                        name: "FK_mesas_ambientes_ambiente_id",
                        column: x => x.ambiente_id,
                        principalSchema: "orion",
                        principalTable: "ambientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carta_itens",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa_id = table.Column<int>(type: "integer", nullable: false),
                    categoria_id = table.Column<long>(type: "bigint", nullable: false),
                    nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    preco = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    destino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carta_itens", x => x.id);
                    table.ForeignKey(
                        name: "FK_carta_itens_carta_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalSchema: "orion",
                        principalTable: "carta_categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "atendimentos",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa_id = table.Column<int>(type: "integer", nullable: false),
                    mesa_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    aberto_por_usuario_id = table.Column<int>(type: "integer", nullable: false),
                    aberto_por_nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    aberto_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fechado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modo_fechamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    anfitriao_lugar_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atendimentos", x => x.id);
                    table.ForeignKey(
                        name: "FK_atendimentos_mesas_mesa_id",
                        column: x => x.mesa_id,
                        principalSchema: "orion",
                        principalTable: "mesas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "atendimento_lugares",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    atendimento_id = table.Column<long>(type: "bigint", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    nome_cliente = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ocupado = table.Column<bool>(type: "boolean", nullable: false),
                    conta_fechada = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atendimento_lugares", x => x.id);
                    table.ForeignKey(
                        name: "FK_atendimento_lugares_atendimentos_atendimento_id",
                        column: x => x.atendimento_id,
                        principalSchema: "orion",
                        principalTable: "atendimentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comanda_itens",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    atendimento_id = table.Column<long>(type: "bigint", nullable: false),
                    lugar_id = table.Column<long>(type: "bigint", nullable: true),
                    carta_item_id = table.Column<long>(type: "bigint", nullable: true),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    destino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    lancado_por_usuario_id = table.Column<int>(type: "integer", nullable: false),
                    lancado_por_nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    lancado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cancelado_por_usuario_id = table.Column<int>(type: "integer", nullable: true),
                    cancelado_por_nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    cancelado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comanda_itens", x => x.id);
                    table.ForeignKey(
                        name: "FK_comanda_itens_atendimentos_atendimento_id",
                        column: x => x.atendimento_id,
                        principalSchema: "orion",
                        principalTable: "atendimentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ambientes_empresa_id_nome",
                schema: "orion",
                table: "ambientes",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ambientes_empresa_id_ordem",
                schema: "orion",
                table: "ambientes",
                columns: new[] { "empresa_id", "ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_atendimento_lugares_atendimento_id_ordem",
                schema: "orion",
                table: "atendimento_lugares",
                columns: new[] { "atendimento_id", "ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_atendimentos_empresa_id_status",
                schema: "orion",
                table: "atendimentos",
                columns: new[] { "empresa_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_atendimentos_mesa_id",
                schema: "orion",
                table: "atendimentos",
                column: "mesa_id");

            migrationBuilder.CreateIndex(
                name: "IX_carta_categorias_empresa_id_nome",
                schema: "orion",
                table: "carta_categorias",
                columns: new[] { "empresa_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_carta_itens_categoria_id",
                schema: "orion",
                table: "carta_itens",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_carta_itens_empresa_id_categoria_id",
                schema: "orion",
                table: "carta_itens",
                columns: new[] { "empresa_id", "categoria_id" });

            migrationBuilder.CreateIndex(
                name: "IX_comanda_itens_atendimento_id",
                schema: "orion",
                table: "comanda_itens",
                column: "atendimento_id");

            migrationBuilder.CreateIndex(
                name: "IX_comanda_itens_lugar_id",
                schema: "orion",
                table: "comanda_itens",
                column: "lugar_id");

            migrationBuilder.CreateIndex(
                name: "IX_mesas_ambiente_id",
                schema: "orion",
                table: "mesas",
                column: "ambiente_id");

            migrationBuilder.CreateIndex(
                name: "IX_mesas_atendimento_aberto_id",
                schema: "orion",
                table: "mesas",
                column: "atendimento_aberto_id");

            migrationBuilder.CreateIndex(
                name: "IX_mesas_empresa_id_rotulo",
                schema: "orion",
                table: "mesas",
                columns: new[] { "empresa_id", "rotulo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atendimento_lugares",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "carta_itens",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "comanda_itens",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "carta_categorias",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "atendimentos",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "mesas",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "ambientes",
                schema: "orion");
        }
    }
}
