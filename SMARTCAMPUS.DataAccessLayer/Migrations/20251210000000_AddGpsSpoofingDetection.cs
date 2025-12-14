using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMARTCAMPUS.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddGpsSpoofingDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns to AttendanceSessions
            migrationBuilder.AddColumn<DateTime>(
                name: "QrCodeExpiresAt",
                table: "AttendanceSessions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QrCodeGeneratedAt",
                table: "AttendanceSessions",
                type: "datetime(6)",
                nullable: true);

            // Add columns to AttendanceRecords
            migrationBuilder.AddColumn<string>(
                name: "DeviceInfo",
                table: "AttendanceRecords",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "FraudScore",
                table: "AttendanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AttendanceRecords",
                type: "varchar(45)",
                maxLength: 45,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsMockLocation",
                table: "AttendanceRecords",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Velocity",
                table: "AttendanceRecords",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrCodeExpiresAt",
                table: "AttendanceSessions");

            migrationBuilder.DropColumn(
                name: "QrCodeGeneratedAt",
                table: "AttendanceSessions");

            migrationBuilder.DropColumn(
                name: "DeviceInfo",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "FraudScore",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "IsMockLocation",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "Velocity",
                table: "AttendanceRecords");
        }
    }
}

