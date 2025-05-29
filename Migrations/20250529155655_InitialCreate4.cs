using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassInsuranceV2s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    ClassV2Id = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    InsuranceName = table.Column<string>(type: "text", nullable: false),
                    BestNet = table.Column<decimal>(type: "numeric", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    ScriptCode = table.Column<string>(type: "text", nullable: false),
                    ScriptDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DrugClassV2Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassInsuranceV2s", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV2s_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV2s_DrugClassV2s_DrugClassV2Id",
                        column: x => x.DrugClassV2Id,
                        principalTable: "DrugClassV2s",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV2s_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV2s_InsuranceRxes_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassInsuranceV3s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    ClassV3Id = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    InsuranceName = table.Column<string>(type: "text", nullable: false),
                    BestNet = table.Column<decimal>(type: "numeric", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    ScriptCode = table.Column<string>(type: "text", nullable: false),
                    ScriptDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DrugClassV3Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassInsuranceV3s", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV3s_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV3s_DrugClassV3s_DrugClassV3Id",
                        column: x => x.DrugClassV3Id,
                        principalTable: "DrugClassV3s",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV3s_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV3s_InsuranceRxes_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV2s_BranchId",
                table: "ClassInsuranceV2s",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV2s_DrugClassV2Id",
                table: "ClassInsuranceV2s",
                column: "DrugClassV2Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV2s_DrugId",
                table: "ClassInsuranceV2s",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV2s_InsuranceId",
                table: "ClassInsuranceV2s",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV3s_BranchId",
                table: "ClassInsuranceV3s",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV3s_DrugClassV3Id",
                table: "ClassInsuranceV3s",
                column: "DrugClassV3Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV3s_DrugId",
                table: "ClassInsuranceV3s",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV3s_InsuranceId",
                table: "ClassInsuranceV3s",
                column: "InsuranceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassInsuranceV2s");

            migrationBuilder.DropTable(
                name: "ClassInsuranceV3s");
        }
    }
}
