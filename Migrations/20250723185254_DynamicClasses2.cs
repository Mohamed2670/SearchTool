using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class DynamicClasses2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BestACQ",
                table: "ClassInsurances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BestInsurancePayment",
                table: "ClassInsurances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BestPatientPayment",
                table: "ClassInsurances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Qty",
                table: "ClassInsurances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BestACQ",
                table: "ClassInsurances");

            migrationBuilder.DropColumn(
                name: "BestInsurancePayment",
                table: "ClassInsurances");

            migrationBuilder.DropColumn(
                name: "BestPatientPayment",
                table: "ClassInsurances");

            migrationBuilder.DropColumn(
                name: "Qty",
                table: "ClassInsurances");
        }
    }
}
