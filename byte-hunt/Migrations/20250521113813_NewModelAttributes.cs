using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace byte_hunt.Migrations
{
    /// <inheritdoc />
    public partial class NewModelAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Descricao_Contribuicao",
                table: "Contribuicoes",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Data",
                table: "Contribuicoes",
                newName: "DataContribuicao");

            migrationBuilder.AddColumn<string>(
                name: "FotoPerfil",
                table: "Utilizadores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "Preco",
                table: "Itens",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "FotoItem",
                table: "Itens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataReview",
                table: "Contribuicoes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetalhesContribuicao",
                table: "Contribuicoes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ResponsavelId",
                table: "Contribuicoes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contribuicoes_ResponsavelId",
                table: "Contribuicoes",
                column: "ResponsavelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contribuicoes_Utilizadores_ResponsavelId",
                table: "Contribuicoes",
                column: "ResponsavelId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contribuicoes_Utilizadores_ResponsavelId",
                table: "Contribuicoes");

            migrationBuilder.DropIndex(
                name: "IX_Contribuicoes_ResponsavelId",
                table: "Contribuicoes");

            migrationBuilder.DropColumn(
                name: "FotoPerfil",
                table: "Utilizadores");

            migrationBuilder.DropColumn(
                name: "FotoItem",
                table: "Itens");

            migrationBuilder.DropColumn(
                name: "DataReview",
                table: "Contribuicoes");

            migrationBuilder.DropColumn(
                name: "DetalhesContribuicao",
                table: "Contribuicoes");

            migrationBuilder.DropColumn(
                name: "ResponsavelId",
                table: "Contribuicoes");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Contribuicoes",
                newName: "Descricao_Contribuicao");

            migrationBuilder.RenameColumn(
                name: "DataContribuicao",
                table: "Contribuicoes",
                newName: "Data");

            migrationBuilder.AlterColumn<decimal>(
                name: "Preco",
                table: "Itens",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }
    }
}
