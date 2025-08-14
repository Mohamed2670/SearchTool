using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class reporthistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceStatuses_InsuranceRxes_InsuranceRxId",
                table: "InsuranceStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceStatuses_Users_UserEmail",
                table: "InsuranceStatuses");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceStatuses_UserEmail",
                table: "InsuranceStatuses");

            migrationBuilder.DropColumn(
                name: "AdditionalInfo",
                table: "InsuranceStatuses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InsuranceStatuses");

            migrationBuilder.DropColumn(
                name: "StatusDate",
                table: "InsuranceStatuses");

            migrationBuilder.DropColumn(
                name: "StatusDescription",
                table: "InsuranceStatuses");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "InsuranceStatuses");

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_InsuranceStatuses_SourceDrugNDC_TargetDrugNDC_Insur~",
                        columns: x => new { x.SourceDrugNDC, x.TargetDrugNDC, x.InsuranceRxId },
                        principalTable: "InsuranceStatuses",
                        principalColumns: new[] { "SourceDrugNDC", "TargetDrugNDC", "InsuranceRxId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_InsuranceRxId_SourceDrugNDC_TargetDrugNDC_StatusDate",
                table: "Reports",
                columns: new[] { "InsuranceRxId", "SourceDrugNDC", "TargetDrugNDC", "StatusDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_SourceDrugNDC_TargetDrugNDC_InsuranceRxId",
                table: "Reports",
                columns: new[] { "SourceDrugNDC", "TargetDrugNDC", "InsuranceRxId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserEmail",
                table: "Reports",
                column: "UserEmail");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceStatuses_InsuranceRxes_InsuranceRxId",
                table: "InsuranceStatuses",
                column: "InsuranceRxId",
                principalTable: "InsuranceRxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceStatuses_InsuranceRxes_InsuranceRxId",
                table: "InsuranceStatuses");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalInfo",
                table: "InsuranceStatuses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "InsuranceStatuses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusDate",
                table: "InsuranceStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "StatusDescription",
                table: "InsuranceStatuses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "InsuranceStatuses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceStatuses_UserEmail",
                table: "InsuranceStatuses",
                column: "UserEmail");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceStatuses_InsuranceRxes_InsuranceRxId",
                table: "InsuranceStatuses",
                column: "InsuranceRxId",
                principalTable: "InsuranceRxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceStatuses_Users_UserEmail",
                table: "InsuranceStatuses",
                column: "UserEmail",
                principalTable: "Users",
                principalColumn: "Email",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
