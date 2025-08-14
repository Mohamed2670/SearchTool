using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class InsuranceStat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsuranceStatuses",
                columns: table => new
                {
                    SourceDrugNDC = table.Column<string>(type: "text", nullable: false),
                    TargetDrugNDC = table.Column<string>(type: "text", nullable: false),
                    InsuranceRxId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StatusDescription = table.Column<string>(type: "text", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "text", nullable: false),
                    StatusDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceStatuses", x => new { x.SourceDrugNDC, x.TargetDrugNDC, x.InsuranceRxId });
                    table.ForeignKey(
                        name: "FK_InsuranceStatuses_Drugs_SourceDrugNDC",
                        column: x => x.SourceDrugNDC,
                        principalTable: "Drugs",
                        principalColumn: "NDC",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InsuranceStatuses_Drugs_TargetDrugNDC",
                        column: x => x.TargetDrugNDC,
                        principalTable: "Drugs",
                        principalColumn: "NDC",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InsuranceStatuses_InsuranceRxes_InsuranceRxId",
                        column: x => x.InsuranceRxId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InsuranceStatuses_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceStatuses_InsuranceRxId",
                table: "InsuranceStatuses",
                column: "InsuranceRxId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceStatuses_TargetDrugNDC",
                table: "InsuranceStatuses",
                column: "TargetDrugNDC");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceStatuses_UserEmail",
                table: "InsuranceStatuses",
                column: "UserEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsuranceStatuses");
        }
    }
}
