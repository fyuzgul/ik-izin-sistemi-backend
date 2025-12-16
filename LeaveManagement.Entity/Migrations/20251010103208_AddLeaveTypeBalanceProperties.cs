using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagement.Entity.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveTypeBalanceProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeductsFromBalance",
                table: "LeaveTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresBalance",
                table: "LeaveTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeductsFromBalance",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "RequiresBalance",
                table: "LeaveTypes");
        }
    }
}
