using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrahamSchoolAdminSystemAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitMigrations5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsConfirm",
                table: "PTAFeesPayments");

            migrationBuilder.AlterColumn<string>(
                name: "Narration",
                table: "PTAFeesPayments",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "PTAFeesPayments",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PaymentState",
                table: "PTAFeesPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PTAFeesPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentState",
                table: "PTAFeesPayments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PTAFeesPayments");

            migrationBuilder.UpdateData(
                table: "PTAFeesPayments",
                keyColumn: "Narration",
                keyValue: null,
                column: "Narration",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Narration",
                table: "PTAFeesPayments",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "PTAFeesPayments",
                keyColumn: "Message",
                keyValue: null,
                column: "Message",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "PTAFeesPayments",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirm",
                table: "PTAFeesPayments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
