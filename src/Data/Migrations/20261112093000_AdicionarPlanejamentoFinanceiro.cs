using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Financas.Data.Migrations;

public partial class AdicionarPlanejamentoFinanceiro : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("orcamentos", table => new
        {
            Id = table.Column<Guid>(type: "char(36)", nullable: false),
            UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false),
            Categoria = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4"),
            LimiteMensal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_orcamentos", x => x.Id))
        .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable("despesas_recorrentes", table => new
        {
            Id = table.Column<Guid>(type: "char(36)", nullable: false),
            UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false),
            Descricao = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
            Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            Categoria = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
            DiaVencimento = table.Column<int>(type: "int", nullable: false),
            ProximoVencimento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_despesas_recorrentes", x => x.Id))
        .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable("metas_economia", table => new
        {
            Id = table.Column<Guid>(type: "char(36)", nullable: false),
            UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false),
            Nome = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
            ValorAlvo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            ValorGuardado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
            Prazo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
            CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_metas_economia", x => x.Id))
        .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex("IX_orcamentos_UsuarioId_Categoria", "orcamentos", new[] { "UsuarioId", "Categoria" }, unique: true);
        migrationBuilder.CreateIndex("IX_despesas_recorrentes_UsuarioId_ProximoVencimento", "despesas_recorrentes", new[] { "UsuarioId", "ProximoVencimento" });
        migrationBuilder.CreateIndex("IX_metas_economia_UsuarioId_Prazo", "metas_economia", new[] { "UsuarioId", "Prazo" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("orcamentos");
        migrationBuilder.DropTable("despesas_recorrentes");
        migrationBuilder.DropTable("metas_economia");
    }
}
