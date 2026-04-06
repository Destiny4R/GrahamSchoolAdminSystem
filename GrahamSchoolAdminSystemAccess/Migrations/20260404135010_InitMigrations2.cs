using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrahamSchoolAdminSystemAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitMigrations2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "PTAFeesPayments",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "FeesPayments",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "PTAFeesPayments");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "FeesPayments");
        }
    }
}
