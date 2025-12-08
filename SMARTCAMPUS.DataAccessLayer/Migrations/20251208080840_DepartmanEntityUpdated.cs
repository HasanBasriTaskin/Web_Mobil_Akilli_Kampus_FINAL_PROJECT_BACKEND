using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMARTCAMPUS.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class DepartmanEntityUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacultyName",
                table: "Departments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacultyName",
                table: "Departments");
        }
    }
}
