using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMARTCAMPUS.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAdminIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProcessedByAdminId",
                table: "FacultyCourseSectionRequests",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProcessedByAdminId",
                table: "FacultyCourseSectionRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(450)",
                oldMaxLength: 450,
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
