using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrahamSchoolAdminSystemAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OtherPayment",
                table: "AppSettings",
                newName: "PaymentEvidence");

            migrationBuilder.AddColumn<string>(
                name: "EvidenceFilePath",
                table: "StudentPayments",
                type: "varchar(420)",
                maxLength: 420,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Narration",
                table: "StudentPayments",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RejectMessage",
                table: "StudentPayments",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "StudentPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvidenceFilePath",
                table: "StudentPayments");

            migrationBuilder.DropColumn(
                name: "Narration",
                table: "StudentPayments");

            migrationBuilder.DropColumn(
                name: "RejectMessage",
                table: "StudentPayments");

            migrationBuilder.DropColumn(
                name: "State",
                table: "StudentPayments");

            migrationBuilder.RenameColumn(
                name: "PaymentEvidence",
                table: "AppSettings",
                newName: "OtherPayment");
        }
    }
}
