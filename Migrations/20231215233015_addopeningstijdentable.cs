using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bumbo.Migrations
{
    /// <inheritdoc />
    public partial class addopeningstijdentable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "dienste_id",
                table: "inklokken",
                newName: "diensten_id");

            migrationBuilder.CreateTable(
                name: "openingstijden",
                columns: table => new
                {
                    openingstijden_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dag_van_week = table.Column<int>(type: "int", nullable: false),
                    openingstijd = table.Column<TimeSpan>(type: "time", nullable: false),
                    sluitingstijd = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_openingstijden", x => x.openingstijden_id);
                });

            migrationBuilder.CreateTable(
                name: "filialen_has_openingstijden",
                columns: table => new
                {
                    filiaal_id = table.Column<int>(type: "int", nullable: false),
                    openingstijden_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_openingstijden_1", x => new { x.filiaal_id, x.openingstijden_id });
                    table.ForeignKey(
                        name: "fk_filialen_has_openingstijd_filialen1",
                        column: x => x.filiaal_id,
                        principalTable: "filialen",
                        principalColumn: "filiaal_id");
                    table.ForeignKey(
                        name: "fk_filialen_has_openingstijd_openingstijd1",
                        column: x => x.openingstijden_id,
                        principalTable: "openingstijden",
                        principalColumn: "openingstijden_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_filialen_has_openingstijden_openingstijden_id",
                table: "filialen_has_openingstijden",
                column: "openingstijden_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "filialen_has_openingstijden");

            migrationBuilder.DropTable(
                name: "openingstijden");

            migrationBuilder.RenameColumn(
                name: "diensten_id",
                table: "inklokken",
                newName: "dienste_id");
        }
    }
}
