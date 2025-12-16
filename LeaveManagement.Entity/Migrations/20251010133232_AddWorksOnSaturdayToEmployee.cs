using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagement.Entity.Migrations
{
    /// <inheritdoc />
    public partial class AddWorksOnSaturdayToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WorksOnSaturday",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorksOnSaturday",
                table: "Employees");
        }
    }
}
