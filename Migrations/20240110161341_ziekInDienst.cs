using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bumbo.Migrations
{
    /// <inheritdoc />
    public partial class ziekInDienst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ziek",
                table: "diensten",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ziek",
                table: "diensten");
        }
    }
}
