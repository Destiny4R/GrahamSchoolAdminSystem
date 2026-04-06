using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrahamSchoolAdminSystemAccess.Migrations
{
    /// <inheritdoc />
    public partial class OtherPaymentSetup_Migrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtherPayItemsTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherPayItemsTable", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OtherPayFeesSetUp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    SchoolClassId = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "double", nullable: false),
                    OtherPayId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherPayFeesSetUp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtherPayFeesSetUp_OtherPayItemsTable_OtherPayId",
                        column: x => x.OtherPayId,
                        principalTable: "OtherPayItemsTable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtherPayFeesSetUp_SchoolClasses_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "SchoolClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtherPayFeesSetUp_SessionYears_SessionId",
                        column: x => x.SessionId,
                        principalTable: "SessionYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OtherPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ItemAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TermRegId = table.Column<int>(type: "int", nullable: false),
                    PayFeesSetUpId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Narration = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentState = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StaffUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtherPayments_OtherPayFeesSetUp_PayFeesSetUpId",
                        column: x => x.PayFeesSetUpId,
                        principalTable: "OtherPayFeesSetUp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtherPayments_TermRegistrations_TermRegId",
                        column: x => x.TermRegId,
                        principalTable: "TermRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OtherPayFeesSetUp_OtherPayId",
                table: "OtherPayFeesSetUp",
                column: "OtherPayId");

            migrationBuilder.CreateIndex(
                name: "IX_OtherPayFeesSetUp_SchoolClassId",
                table: "OtherPayFeesSetUp",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_OtherPayFeesSetUp_SessionId",
                table: "OtherPayFeesSetUp",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_OtherPayments_PayFeesSetUpId",
                table: "OtherPayments",
                column: "PayFeesSetUpId");

            migrationBuilder.CreateIndex(
                name: "IX_OtherPayments_TermRegId",
                table: "OtherPayments",
                column: "TermRegId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtherPayments");

            migrationBuilder.DropTable(
                name: "OtherPayFeesSetUp");

            migrationBuilder.DropTable(
                name: "OtherPayItemsTable");
        }
    }
}
