using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class DynamicClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassInsuranceV2s");

            migrationBuilder.DropTable(
                name: "ClassInsuranceV3s");

            migrationBuilder.DropTable(
                name: "ClassInsuranceV4s");

            migrationBuilder.DropTable(
                name: "DrugEPCMOAs");

            migrationBuilder.DropTable(
                name: "DrugClassV2s");

            migrationBuilder.DropTable(
                name: "DrugClassV3s");

            migrationBuilder.DropTable(
                name: "DrugClassV4s");

            migrationBuilder.DropTable(
                name: "EPCMOAClasses");

            migrationBuilder.CreateTable(
                name: "DrugModals",
                columns: table => new
                {
                    NDC = table.Column<string>(type: "text", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugModals", x => new { x.UserEmail, x.NDC });
                    table.ForeignKey(
                        name: "FK_DrugModals_Drugs_NDC",
                        column: x => x.NDC,
                        principalTable: "Drugs",
                        principalColumn: "NDC",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugModals_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugModals_NDC",
                table: "DrugModals",
                column: "NDC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrugModals");

            migrationBuilder.CreateTable(
                name: "DrugClassV2s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugClassV2s", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugClassV3s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugClassV3s", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugClassV4s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugClassV4s", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EPCMOAClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EPCMOAClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassInsuranceV2s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV2Id = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    BestNet = table.Column<decimal>(type: "numeric", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InsuranceName = table.Column<string>(type: "text", nullable: false),
                    ScriptCode = table.Column<string>(type: "text", nullable: false),
                    ScriptDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV3Id = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    BestNet = table.Column<decimal>(type: "numeric", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InsuranceName = table.Column<string>(type: "text", nullable: false),
                    ScriptCode = table.Column<string>(type: "text", nullable: false),
                    ScriptDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ClassInsuranceV4s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV4Id = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    BestNet = table.Column<decimal>(type: "numeric", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InsuranceName = table.Column<string>(type: "text", nullable: false),
                    ScriptCode = table.Column<string>(type: "text", nullable: false),
                    ScriptDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassInsuranceV4s", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV4s_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV4s_DrugClassV4s_DrugClassV4Id",
                        column: x => x.DrugClassV4Id,
                        principalTable: "DrugClassV4s",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV4s_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsuranceV4s_InsuranceRxes_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugEPCMOAs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    EPCMOAClassId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugEPCMOAs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugEPCMOAs_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugEPCMOAs_EPCMOAClasses_EPCMOAClassId",
                        column: x => x.EPCMOAClassId,
                        principalTable: "EPCMOAClasses",
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

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV4s_BranchId",
                table: "ClassInsuranceV4s",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV4s_DrugClassV4Id",
                table: "ClassInsuranceV4s",
                column: "DrugClassV4Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV4s_DrugId",
                table: "ClassInsuranceV4s",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsuranceV4s_InsuranceId",
                table: "ClassInsuranceV4s",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugEPCMOAs_DrugId",
                table: "DrugEPCMOAs",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugEPCMOAs_EPCMOAClassId",
                table: "DrugEPCMOAs",
                column: "EPCMOAClassId");
        }
    }
}
