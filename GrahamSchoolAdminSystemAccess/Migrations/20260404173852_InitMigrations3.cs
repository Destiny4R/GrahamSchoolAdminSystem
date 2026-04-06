using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrahamSchoolAdminSystemAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitMigrations3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffUserId",
                table: "PTAFeesPayments",
                type: "varchar(470)",
                maxLength: 470,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StaffUserId",
                table: "FeesPayments",
                type: "varchar(470)",
                maxLength: 470,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StaffUserId",
                table: "PTAFeesPayments");

            migrationBuilder.DropColumn(
                name: "StaffUserId",
                table: "FeesPayments");
        }
    }
}
