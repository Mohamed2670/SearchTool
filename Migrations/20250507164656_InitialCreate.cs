using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SearchTool_ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrugClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Insurances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Bin = table.Column<string>(type: "text", nullable: false),
                    HelpDeskNumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insurances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Specialties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NDC = table.Column<string>(type: "text", nullable: false),
                    Form = table.Column<string>(type: "text", nullable: true),
                    Strength = table.Column<string>(type: "text", nullable: true),
                    DrugClassId = table.Column<int>(type: "integer", nullable: false),
                    ACQ = table.Column<decimal>(type: "numeric", nullable: false),
                    AWP = table.Column<decimal>(type: "numeric", nullable: false),
                    Rxcui = table.Column<decimal>(type: "numeric", nullable: true),
                    Route = table.Column<string>(type: "text", nullable: true),
                    TECode = table.Column<string>(type: "text", nullable: true),
                    Ingrdient = table.Column<string>(type: "text", nullable: true),
                    ApplicationNumber = table.Column<string>(type: "text", nullable: true),
                    ApplicationType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drugs_DrugClasses_DrugClassId",
                        column: x => x.DrugClassId,
                        principalTable: "DrugClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InsurancePCNs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PCN = table.Column<string>(type: "text", nullable: false),
                    InsuranceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsurancePCNs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsurancePCNs_Insurances_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "Insurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MainCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SpecialtyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainCompanies_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceRxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RxGroup = table.Column<string>(type: "text", nullable: false),
                    InsurancePCNId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceRxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsuranceRxes_InsurancePCNs_InsurancePCNId",
                        column: x => x.InsurancePCNId,
                        principalTable: "InsurancePCNs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    MainCompanyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_MainCompanies_MainCompanyId",
                        column: x => x.MainCompanyId,
                        principalTable: "MainCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassInsurances",
                columns: table => new
                {
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    InsuranceName = table.Column<string>(type: "text", nullable: false),
                    BestNet = table.Column<decimal>(type: "numeric", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    ScriptCode = table.Column<string>(type: "text", nullable: false),
                    ScriptDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassInsurances", x => new { x.InsuranceId, x.ClassId, x.Date, x.BranchId });
                    table.ForeignKey(
                        name: "FK_ClassInsurances_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsurances_DrugClasses_ClassId",
                        column: x => x.ClassId,
                        principalTable: "DrugClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsurances_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassInsurances_InsuranceRxes_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugBranches",
                columns: table => new
                {
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugBranches", x => new { x.DrugId, x.BranchId });
                    table.ForeignKey(
                        name: "FK_DrugBranches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugBranches_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugInsurances",
                columns: table => new
                {
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    NDCCode = table.Column<string>(type: "text", nullable: false),
                    DrugClassId = table.Column<int>(type: "integer", nullable: false),
                    Net = table.Column<decimal>(type: "numeric", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Prescriber = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<string>(type: "text", nullable: false),
                    AcquisitionCost = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    InsurancePayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PatientPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugInsurances", x => new { x.InsuranceId, x.DrugId, x.BranchId });
                    table.ForeignKey(
                        name: "FK_DrugInsurances_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugInsurances_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugInsurances_InsuranceRxes_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "Scripts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScriptCode = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scripts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scripts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scripts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
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
                    InsuranceRxId = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: false)
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScriptItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScriptId = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    RxNumber = table.Column<string>(type: "text", nullable: false),
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    DrugClassId = table.Column<int>(type: "integer", nullable: false),
                    PrescriberId = table.Column<int>(type: "integer", nullable: false),
                    PF = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<string>(type: "text", nullable: false),
                    AcquisitionCost = table.Column<decimal>(type: "numeric", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric", nullable: false),
                    InsurancePayment = table.Column<decimal>(type: "numeric", nullable: false),
                    PatientPayment = table.Column<decimal>(type: "numeric", nullable: false),
                    NDCCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScriptItems_DrugClasses_DrugClassId",
                        column: x => x.DrugClassId,
                        principalTable: "DrugClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScriptItems_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScriptItems_InsuranceRxes_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScriptItems_Scripts_ScriptId",
                        column: x => x.ScriptId,
                        principalTable: "Scripts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScriptItems_Users_PrescriberId",
                        column: x => x.PrescriberId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SearchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RxgroupId = table.Column<int>(type: "integer", nullable: true),
                    BinId = table.Column<int>(type: "integer", nullable: true),
                    PcnId = table.Column<int>(type: "integer", nullable: true),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    OrderItemId = table.Column<int>(type: "integer", nullable: false),
                    InsuranceId = table.Column<int>(type: "integer", nullable: true),
                    InsuranceRxId = table.Column<int>(type: "integer", nullable: true),
                    InsurancePCNId = table.Column<int>(type: "integer", nullable: true),
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SearchLogs_InsuranceRxes_InsuranceRxId",
                        column: x => x.InsuranceRxId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SearchLogs_Insurances_InsuranceId",
                        column: x => x.InsuranceId,
                        principalTable: "Insurances",
                        principalColumn: "Id");
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

            migrationBuilder.InsertData(
                table: "Specialties",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Dermatology specialty" });

            migrationBuilder.InsertData(
                table: "MainCompanies",
                columns: new[] { "Id", "Name", "SpecialtyId" },
                values: new object[] { 1, "California Dermatology", 1 });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Id", "Code", "Location", "MainCompanyId", "Name" },
                values: new object[,]
                {
                    { 1, "1", "Thousand Oaks", 1, "California Dermatology Institute Thousand Oaks" },
                    { 2, "2", "Northridge", 1, "California Dermatology Institute Northridge" },
                    { 3, "3", "Huntington Park", 1, "California Dermatology Institute Huntington Park" },
                    { 4, "4", "Palmdale", 1, "California Dermatology Institute Palmdale" },
                    { 5, "5", "VIRTUAL", 1, "VIRTUAL" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_MainCompanyId",
                table: "Branches",
                column: "MainCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsurances_BranchId",
                table: "ClassInsurances",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsurances_ClassId",
                table: "ClassInsurances",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInsurances_DrugId",
                table: "ClassInsurances",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugBranches_BranchId",
                table: "DrugBranches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugInsurances_BranchId",
                table: "DrugInsurances",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugInsurances_DrugId",
                table: "DrugInsurances",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_DrugClassId",
                table: "Drugs",
                column: "DrugClassId");

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePCNs_InsuranceId",
                table: "InsurancePCNs",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRxes_InsurancePCNId",
                table: "InsuranceRxes",
                column: "InsurancePCNId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserId",
                table: "Logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MainCompanies_SpecialtyId",
                table: "MainCompanies",
                column: "SpecialtyId");

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
                name: "IX_ScriptItems_DrugClassId",
                table: "ScriptItems",
                column: "DrugClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptItems_DrugId",
                table: "ScriptItems",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptItems_InsuranceId",
                table: "ScriptItems",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptItems_PrescriberId",
                table: "ScriptItems",
                column: "PrescriberId");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptItems_ScriptId",
                table: "ScriptItems",
                column: "ScriptId");

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_BranchId",
                table: "Scripts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_UserId",
                table: "Scripts",
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

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                table: "Users",
                column: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassInsurances");

            migrationBuilder.DropTable(
                name: "DrugBranches");

            migrationBuilder.DropTable(
                name: "DrugInsurances");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "ScriptItems");

            migrationBuilder.DropTable(
                name: "SearchLogs");

            migrationBuilder.DropTable(
                name: "Scripts");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropTable(
                name: "InsuranceRxes");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "DrugClasses");

            migrationBuilder.DropTable(
                name: "InsurancePCNs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Insurances");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "MainCompanies");

            migrationBuilder.DropTable(
                name: "Specialties");
        }
    }
}
