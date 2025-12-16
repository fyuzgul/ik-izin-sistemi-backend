using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagement.Entity.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Employees");
        }
    }
}
