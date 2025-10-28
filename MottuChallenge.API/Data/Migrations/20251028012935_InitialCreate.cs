using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MottuChallenge.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patios",
                columns: table => new
                {
                    NomePatio = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Localizacao = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    VagasTotais = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VagasOcupadas = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patios", x => x.NomePatio);
                });

            migrationBuilder.CreateTable(
                name: "Funcionarios",
                columns: table => new
                {
                    UsuarioFuncionario = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SenhaHash = table.Column<string>(type: "VARCHAR2(256)", maxLength: 256, nullable: false),
                    NomePatio = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionarios", x => x.UsuarioFuncionario);
                    table.ForeignKey(
                        name: "FK_Funcionarios_Patios_NomePatio",
                        column: x => x.NomePatio,
                        principalTable: "Patios",
                        principalColumn: "NomePatio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Motos",
                columns: table => new
                {
                    Placa = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    Modelo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Setor = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NomePatio = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    UsuarioFuncionario = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Quilometragem = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DataUltimaRevisao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    QuantidadeRevisoes = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PrecisaManutencao = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ProbabilidadeManutencao = table.Column<float>(type: "BINARY_FLOAT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motos", x => x.Placa);
                    table.ForeignKey(
                        name: "FK_Motos_Funcionarios_UsuarioFuncionario",
                        column: x => x.UsuarioFuncionario,
                        principalTable: "Funcionarios",
                        principalColumn: "UsuarioFuncionario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Motos_Patios_NomePatio",
                        column: x => x.NomePatio,
                        principalTable: "Patios",
                        principalColumn: "NomePatio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    UsuarioCliente = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SenhaHash = table.Column<string>(type: "VARCHAR2(256)", maxLength: 256, nullable: false),
                    MotoPlaca = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: true),
                    DataUltimaManutencao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    QuantidadeManutencoes = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.UsuarioCliente);
                    table.ForeignKey(
                        name: "FK_Clientes_Motos_MotoPlaca",
                        column: x => x.MotoPlaca,
                        principalTable: "Motos",
                        principalColumn: "Placa");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_MotoPlaca",
                table: "Clientes",
                column: "MotoPlaca");

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_NomePatio",
                table: "Funcionarios",
                column: "NomePatio");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_NomePatio",
                table: "Motos",
                column: "NomePatio");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_UsuarioFuncionario",
                table: "Motos",
                column: "UsuarioFuncionario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Motos");

            migrationBuilder.DropTable(
                name: "Funcionarios");

            migrationBuilder.DropTable(
                name: "Patios");
        }
    }
}
