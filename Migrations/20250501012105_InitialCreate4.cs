using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_InsuranceRxes_InsuranceRxId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchLogs_InsurancePCNs_InsurancePCNId",
                table: "SearchLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchLogs_InsuranceRxes_InsuranceRxId",
                table: "SearchLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchLogs_Insurances_InsuranceId",
                table: "SearchLogs");

            migrationBuilder.AlterColumn<int>(
                name: "RxgroupId",
                table: "SearchLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PcnId",
                table: "SearchLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "InsuranceRxId",
                table: "SearchLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "InsurancePCNId",
                table: "SearchLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "InsuranceId",
                table: "SearchLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "BinId",
                table: "SearchLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "InsuranceRxId",
                table: "OrderItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_InsuranceRxes_InsuranceRxId",
                table: "OrderItems",
                column: "InsuranceRxId",
                principalTable: "InsuranceRxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchLogs_InsurancePCNs_InsurancePCNId",
                table: "SearchLogs",
                column: "InsurancePCNId",
                principalTable: "InsurancePCNs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchLogs_InsuranceRxes_InsuranceRxId",
                table: "SearchLogs",
                column: "InsuranceRxId",
                principalTable: "InsuranceRxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchLogs_Insurances_InsuranceId",
                table: "SearchLogs",
                column: "InsuranceId",
                principalTable: "Insurances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_InsuranceRxes_InsuranceRxId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchLogs_InsurancePCNs_InsurancePCNId",
                table: "SearchLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchLogs_InsuranceRxes_InsuranceRxId",
                table: "SearchLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchLogs_Insurances_InsuranceId",
                table: "SearchLogs");

            migrationBuilder.AlterColumn<int>(
                name: "RxgroupId",
                table: "SearchLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PcnId",
                table: "SearchLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InsuranceRxId",
                table: "SearchLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InsurancePCNId",
                table: "SearchLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InsuranceId",
                table: "SearchLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BinId",
                table: "SearchLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InsuranceRxId",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_InsuranceRxes_InsuranceRxId",
                table: "OrderItems",
                column: "InsuranceRxId",
                principalTable: "InsuranceRxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchLogs_InsurancePCNs_InsurancePCNId",
                table: "SearchLogs",
                column: "InsurancePCNId",
                principalTable: "InsurancePCNs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchLogs_InsuranceRxes_InsuranceRxId",
                table: "SearchLogs",
                column: "InsuranceRxId",
                principalTable: "InsuranceRxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchLogs_Insurances_InsuranceId",
                table: "SearchLogs",
                column: "InsuranceId",
                principalTable: "Insurances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
