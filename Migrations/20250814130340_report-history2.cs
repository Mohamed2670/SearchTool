using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class reporthistory2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedStatus",
                table: "InsuranceStatuses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PriorAuthorizationStatus",
                table: "InsuranceStatuses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedStatus",
                table: "InsuranceStatuses");

            migrationBuilder.DropColumn(
                name: "PriorAuthorizationStatus",
                table: "InsuranceStatuses");
        }
    }
}
