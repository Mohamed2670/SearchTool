using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MainCompanies",
                columns: new[] { "Id", "Name", "SpecialtyId" },
                values: new object[] { 2, "Spark Medi-Cal", 1 });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Id", "Code", "Location", "MainCompanyId", "Name" },
                values: new object[] { 6, "6", "VIRTUAL", 2, "VIRTUAL" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MainCompanies",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
