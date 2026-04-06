using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrahamSchoolAdminSystemAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitMigrations4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "PTAFeesPayments",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Narration",
                table: "PTAFeesPayments",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "PTAFeesPayments");

            migrationBuilder.DropColumn(
                name: "Narration",
                table: "PTAFeesPayments");
        }
    }
}
