using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassV3Id",
                table: "ClassInsuranceV3s");

            migrationBuilder.DropColumn(
                name: "ClassV2Id",
                table: "ClassInsuranceV2s");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassV3Id",
                table: "ClassInsuranceV3s",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClassV2Id",
                table: "ClassInsuranceV2s",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
