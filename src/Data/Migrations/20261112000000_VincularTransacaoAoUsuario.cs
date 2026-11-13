using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Financas.Data.Migrations;

public partial class VincularTransacaoAoUsuario : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "UsuarioId", table: "transacoes", type: "char(36)", collation: "ascii_general_ci", nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.DropIndex(name: "IX_transacoes_Data", table: "transacoes");
        migrationBuilder.CreateIndex(
            name: "IX_transacoes_UsuarioId_Data", table: "transacoes",
            columns: new[] { "UsuarioId", "Data" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_transacoes_UsuarioId_Data", table: "transacoes");
        migrationBuilder.DropColumn(name: "UsuarioId", table: "transacoes");
        migrationBuilder.CreateIndex(name: "IX_transacoes_Data", table: "transacoes", column: "Data");
    }
}
