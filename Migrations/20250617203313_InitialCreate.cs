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
                name: "AuditTrails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    PrimaryKey = table.Column<string>(type: "text", nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: false),
                    NewValues = table.Column<string>(type: "text", nullable: false),
                    PerformedBy = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrails", x => x.Id);
                });

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
                    DrugClassV2Id = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV3Id = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV4Id = table.Column<int>(type: "integer", nullable: false),
                    ACQ = table.Column<decimal>(type: "numeric", nullable: false),
                    AWP = table.Column<decimal>(type: "numeric", nullable: false),
                    Rxcui = table.Column<decimal>(type: "numeric", nullable: true),
                    Route = table.Column<string>(type: "text", nullable: true),
                    TECode = table.Column<string>(type: "text", nullable: true),
                    Ingrdient = table.Column<string>(type: "text", nullable: true),
                    ApplicationNumber = table.Column<string>(type: "text", nullable: true),
                    ApplicationType = table.Column<string>(type: "text", nullable: true),
                    StrengthUnit = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.Id);
                    table.UniqueConstraint("AK_Drugs_NDC", x => x.NDC);
                    table.ForeignKey(
                        name: "FK_Drugs_DrugClassV2s_DrugClassV2Id",
                        column: x => x.DrugClassV2Id,
                        principalTable: "DrugClassV2s",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Drugs_DrugClassV3s_DrugClassV3Id",
                        column: x => x.DrugClassV3Id,
                        principalTable: "DrugClassV3s",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Drugs_DrugClassV4s_DrugClassV4Id",
                        column: x => x.DrugClassV4Id,
                        principalTable: "DrugClassV4s",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "DrugMedis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
                    DrugNDC = table.Column<string>(type: "text", nullable: false),
                    PriorAuthorization = table.Column<string>(type: "text", nullable: false),
                    ExtendedDuration = table.Column<string>(type: "text", nullable: false),
                    CostCeilingTier = table.Column<string>(type: "text", nullable: false),
                    NonCapitatedDrugIndicator = table.Column<string>(type: "text", nullable: false),
                    CCSPanelAuthority = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugMedis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugMedis_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
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
                name: "ClassInsuranceV2s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV2Id = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    InsuranceName = table.Column<string>(type: "text", nullable: false),
                    BestNet = table.Column<decimal>(type: "numeric", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
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
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV3Id = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    InsuranceName = table.Column<string>(type: "text", nullable: false),
                    BestNet = table.Column<decimal>(type: "numeric", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
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
                    InsuranceId = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV4Id = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    InsuranceName = table.Column<string>(type: "text", nullable: false),
                    BestNet = table.Column<decimal>(type: "numeric", nullable: false),
                    DrugId = table.Column<int>(type: "integer", nullable: false),
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
                name: "DrugBranches",
                columns: table => new
                {
                    DrugNDC = table.Column<string>(type: "text", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: true),
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugBranches", x => new { x.DrugNDC, x.BranchId });
                    table.ForeignKey(
                        name: "FK_DrugBranches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugBranches_Drugs_DrugNDC",
                        column: x => x.DrugNDC,
                        principalTable: "Drugs",
                        principalColumn: "NDC",
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
                    DrugClassV2Id = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV3Id = table.Column<int>(type: "integer", nullable: false),
                    DrugClassV4Id = table.Column<int>(type: "integer", nullable: false),
                    ScriptCode = table.Column<string>(type: "text", nullable: true),
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
                    table.UniqueConstraint("AK_Users_Email", x => x.Email);
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
                    UserEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logs_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    TotalNet = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPatientPay = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalInsurancePay = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAcquisitionCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AddtionalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "Email",
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
                name: "SearchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RxgroupId = table.Column<int>(type: "integer", nullable: true),
                    BinId = table.Column<int>(type: "integer", nullable: true),
                    PcnId = table.Column<int>(type: "integer", nullable: true),
                    DrugNDC = table.Column<string>(type: "text", nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OrderItemId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    SearchType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchLogs_Drugs_DrugNDC",
                        column: x => x.DrugNDC,
                        principalTable: "Drugs",
                        principalColumn: "NDC",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SearchLogs_InsurancePCNs_PcnId",
                        column: x => x.PcnId,
                        principalTable: "InsurancePCNs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SearchLogs_InsuranceRxes_BinId",
                        column: x => x.BinId,
                        principalTable: "InsuranceRxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SearchLogs_Insurances_RxgroupId",
                        column: x => x.RxgroupId,
                        principalTable: "Insurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SearchLogs_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    DrugName = table.Column<string>(type: "text", nullable: false),
                    DrugNDC = table.Column<string>(type: "text", nullable: false),
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
                        name: "FK_OrderItems_Drugs_DrugNDC",
                        column: x => x.DrugNDC,
                        principalTable: "Drugs",
                        principalColumn: "NDC",
                        onDelete: ReferentialAction.Restrict);
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
                    UserEmail = table.Column<string>(type: "text", nullable: false),
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
                        name: "FK_ScriptItems_Users_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SearchLogReadDto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RxgroupId = table.Column<int>(type: "integer", nullable: true),
                    RxgroupName = table.Column<string>(type: "text", nullable: false),
                    BinName = table.Column<string>(type: "text", nullable: false),
                    PcnName = table.Column<string>(type: "text", nullable: false),
                    BinId = table.Column<int>(type: "integer", nullable: true),
                    PcnId = table.Column<int>(type: "integer", nullable: true),
                    NDC = table.Column<string>(type: "text", nullable: false),
                    DrugName = table.Column<string>(type: "text", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false),
                    OrderItemId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SearchType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLogReadDto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchLogReadDto_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
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
                values: new object[,]
                {
                    { 1, "California Dermatology", 1 },
                    { 2, "Spark Medi-Cal", 1 }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Id", "Code", "Location", "MainCompanyId", "Name" },
                values: new object[,]
                {
                    { 1, "1", "Thousand Oaks", 1, "California Dermatology Institute Thousand Oaks" },
                    { 2, "2", "Northridge", 1, "California Dermatology Institute Northridge" },
                    { 3, "3", "Huntington Park", 1, "California Dermatology Institute Huntington Park" },
                    { 4, "4", "Palmdale", 1, "California Dermatology Institute Palmdale" },
                    { 5, "5", "VIRTUAL", 1, "VIRTUAL" },
                    { 6, "6", "VIRTUAL", 2, "ASP" }
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
                name: "IX_DrugMedis_DrugId",
                table: "DrugMedis",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_DrugClassId",
                table: "Drugs",
                column: "DrugClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_DrugClassV2Id",
                table: "Drugs",
                column: "DrugClassV2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_DrugClassV3Id",
                table: "Drugs",
                column: "DrugClassV3Id");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_DrugClassV4Id",
                table: "Drugs",
                column: "DrugClassV4Id");

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePCNs_InsuranceId",
                table: "InsurancePCNs",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRxes_InsurancePCNId",
                table: "InsuranceRxes",
                column: "InsurancePCNId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserEmail",
                table: "Logs",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_MainCompanies_SpecialtyId",
                table: "MainCompanies",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_DrugNDC",
                table: "OrderItems",
                column: "DrugNDC");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_InsuranceRxId",
                table: "OrderItems",
                column: "InsuranceRxId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserEmail",
                table: "Orders",
                column: "UserEmail");

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
                name: "IX_ScriptItems_ScriptId",
                table: "ScriptItems",
                column: "ScriptId");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptItems_UserEmail",
                table: "ScriptItems",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_BranchId",
                table: "Scripts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_UserId",
                table: "Scripts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogReadDto_OrderItemId",
                table: "SearchLogReadDto",
                column: "OrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_BinId",
                table: "SearchLogs",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_DrugNDC",
                table: "SearchLogs",
                column: "DrugNDC");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_PcnId",
                table: "SearchLogs",
                column: "PcnId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_RxgroupId",
                table: "SearchLogs",
                column: "RxgroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_UserEmail",
                table: "SearchLogs",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                table: "Users",
                column: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTrails");

            migrationBuilder.DropTable(
                name: "ClassInsurances");

            migrationBuilder.DropTable(
                name: "ClassInsuranceV2s");

            migrationBuilder.DropTable(
                name: "ClassInsuranceV3s");

            migrationBuilder.DropTable(
                name: "ClassInsuranceV4s");

            migrationBuilder.DropTable(
                name: "DrugBranches");

            migrationBuilder.DropTable(
                name: "DrugInsurances");

            migrationBuilder.DropTable(
                name: "DrugMedis");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "ScriptItems");

            migrationBuilder.DropTable(
                name: "SearchLogReadDto");

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
                name: "DrugClassV2s");

            migrationBuilder.DropTable(
                name: "DrugClassV3s");

            migrationBuilder.DropTable(
                name: "DrugClassV4s");

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
