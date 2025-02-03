using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrugClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Insurances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insurances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NDC = table.Column<string>(type: "text", nullable: false),
                    Form = table.Column<string>(type: "text", nullable: true),
                    Strength = table.Column<string>(type: "text", nullable: true),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    DrugClassId = table.Column<int>(type: "integer", nullable: true),
                    ACQ = table.Column<decimal>(type: "numeric", nullable: false),
                    AWP = table.Column<decimal>(type: "numeric", nullable: false),
                    Rxcui = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drugs_DrugClasses_DrugClassId",
                        column: x => x.DrugClassId,
                        principalTable: "DrugClasses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DrugInsurances",
                columns: table => new
                {
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    NDCCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugInsurances", x => new { x.InsuranceId, x.DrugId });
                    table.ForeignKey(
                        name: "FK_DrugInsurances_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugInsurances_Insurances_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "Insurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scripts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScriptCode = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    InsurancePay = table.Column<decimal>(type: "numeric", nullable: false),
                    Net = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    PatientPay = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    NDCCode = table.Column<string>(type: "text", nullable: false),
                    DrugInsuranceId = table.Column<int>(type: "integer", nullable: false),
                    DrugInsuranceInsuranceId = table.Column<int>(type: "integer", nullable: false),
                    DrugInsuranceDrugId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scripts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scripts_DrugInsurances_DrugInsuranceInsuranceId_DrugInsuran~",
                        columns: x => new { x.DrugInsuranceInsuranceId, x.DrugInsuranceDrugId },
                        principalTable: "DrugInsurances",
                        principalColumns: new[] { "InsuranceId", "DrugId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scripts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugInsurances_DrugId",
                table: "DrugInsurances",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_DrugClassId",
                table: "Drugs",
                column: "DrugClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_DrugInsuranceInsuranceId_DrugInsuranceDrugId",
                table: "Scripts",
                columns: new[] { "DrugInsuranceInsuranceId", "DrugInsuranceDrugId" });

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_UserId",
                table: "Scripts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Scripts");

            migrationBuilder.DropTable(
                name: "DrugInsurances");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropTable(
                name: "Insurances");

            migrationBuilder.DropTable(
                name: "DrugClasses");
        }
    }
}
