using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bumbo.Migrations
{
    /// <inheritdoc />
    public partial class inklokkentable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inklokken",
                columns: table => new
                {
                    inklokken_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dienste_id = table.Column<int>(type: "int", nullable: false),
                    start = table.Column<TimeSpan>(type: "time", nullable: false),
                    eind = table.Column<TimeSpan>(type: "time", nullable: true),
                    goedgekeurd = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inklokken", x => x.inklokken_id);
                    table.ForeignKey(
                        name: "fk_inklokken_diensten",
                        column: x => x.dienste_id,
                        principalTable: "diensten",
                        principalColumn: "diensten_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inklokken_diensten_id",
                table: "inklokken",
                column: "dienste_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inklokken");
        }
    }
}
