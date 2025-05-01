using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPatientPay = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalInsurancePay = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAcquisitionCost = table.Column<decimal>(type: "numeric", nullable: false),
                    AddtionalCost = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    NetPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    PatientPay = table.Column<decimal>(type: "numeric", nullable: false),
                    InsurancePay = table.Column<decimal>(type: "numeric", nullable: false),
                    AcquisitionCost = table.Column<decimal>(type: "numeric", nullable: false),
                    AddtionalCost = table.Column<decimal>(type: "numeric", nullable: false),
                    RxgroupId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    InsuranceRxId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_InsuranceRxes_InsuranceRxId",
                        column: x => x.InsuranceRxId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SearchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RxgroupId = table.Column<int>(type: "integer", nullable: false),
                    BinId = table.Column<int>(type: "integer", nullable: false),
                    PcnId = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    OrderItemId = table.Column<int>(type: "integer", nullable: false),
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    InsuranceRxId = table.Column<int>(type: "integer", nullable: false),
                    InsurancePCNId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SearchType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchLogs_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchLogs_InsurancePCNs_InsurancePCNId",
                        column: x => x.InsurancePCNId,
                        principalTable: "InsurancePCNs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchLogs_InsuranceRxes_InsuranceRxId",
                        column: x => x.InsuranceRxId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchLogs_Insurances_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "Insurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchLogs_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_DrugId",
                table: "OrderItems",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_InsuranceRxId",
                table: "OrderItems",
                column: "InsuranceRxId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_DrugId",
                table: "SearchLogs",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_InsuranceId",
                table: "SearchLogs",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_InsurancePCNId",
                table: "SearchLogs",
                column: "InsurancePCNId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_InsuranceRxId",
                table: "SearchLogs",
                column: "InsuranceRxId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_OrderItemId",
                table: "SearchLogs",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_UserId",
                table: "SearchLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchLogs");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
