using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bumbo.Migrations
{
    /// <inheritdoc />
    public partial class FK_Dienst_To_Afdeling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AfdelingId",
                table: "diensten",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AfdelingenAfdelingId",
                table: "diensten",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_diensten_AfdelingenAfdelingId",
                table: "diensten",
                column: "AfdelingenAfdelingId");

            migrationBuilder.AddForeignKey(
                name: "FK_diensten_afdelingen_AfdelingenAfdelingId",
                table: "diensten",
                column: "AfdelingenAfdelingId",
                principalTable: "afdelingen",
                principalColumn: "afdeling_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_diensten_afdelingen_AfdelingenAfdelingId",
                table: "diensten");

            migrationBuilder.DropIndex(
                name: "IX_diensten_AfdelingenAfdelingId",
                table: "diensten");

            migrationBuilder.DropColumn(
                name: "AfdelingId",
                table: "diensten");

            migrationBuilder.DropColumn(
                name: "AfdelingenAfdelingId",
                table: "diensten");
        }
    }
}
