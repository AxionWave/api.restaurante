using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Orion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GruposCobranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grupos_cobranca",
                schema: "orion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    atendimento_id = table.Column<long>(type: "bigint", nullable: false),
                    modo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos_cobranca", x => x.id);
                    table.ForeignKey(
                        name: "FK_grupos_cobranca_atendimentos_atendimento_id",
                        column: x => x.atendimento_id,
                        principalSchema: "orion",
                        principalTable: "atendimentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grupo_cobranca_lugares",
                schema: "orion",
                columns: table => new
                {
                    grupo_cobranca_id = table.Column<long>(type: "bigint", nullable: false),
                    lugar_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupo_cobranca_lugares", x => new { x.grupo_cobranca_id, x.lugar_id });
                    table.ForeignKey(
                        name: "FK_grupo_cobranca_lugares_atendimento_lugares_lugar_id",
                        column: x => x.lugar_id,
                        principalSchema: "orion",
                        principalTable: "atendimento_lugares",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grupo_cobranca_lugares_grupos_cobranca_grupo_cobranca_id",
                        column: x => x.grupo_cobranca_id,
                        principalSchema: "orion",
                        principalTable: "grupos_cobranca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_grupo_cobranca_lugares_lugar_id",
                schema: "orion",
                table: "grupo_cobranca_lugares",
                column: "lugar_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grupos_cobranca_atendimento_id",
                schema: "orion",
                table: "grupos_cobranca",
                column: "atendimento_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grupo_cobranca_lugares",
                schema: "orion");

            migrationBuilder.DropTable(
                name: "grupos_cobranca",
                schema: "orion");
        }
    }
}
