using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace byte_hunt.Migrations
{
    /// <inheritdoc />
    public partial class attributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttrsJson",
                table: "Itens",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttrsJson",
                table: "Itens");
        }
    }
}
