using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeaveManagement.Entity.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceRoleWithTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Roles_RoleId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Employees");

            // First, set all TitleId to NULL to avoid foreign key constraint issues
            migrationBuilder.Sql("UPDATE \"Employees\" SET \"RoleId\" = NULL;");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Employees",
                newName: "TitleId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_RoleId",
                table: "Employees",
                newName: "IX_Employees_TitleId");

            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Titles_Name",
                table: "Titles",
                column: "Name",
                unique: true);

            // Insert seed data for Titles
            migrationBuilder.Sql(@"
                INSERT INTO ""Titles"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""CreatedDate"") VALUES
                (1, 'Yönetici', 'Yönetici', true, NOW()),
                (2, 'Uzman', 'Uzman', true, NOW()),
                (3, 'Uzman Yardımcısı', 'Uzman Yardımcısı', true, NOW()),
                (4, 'Takım Lideri Operatör', 'Takım Lideri Operatör', true, NOW()),
                (5, 'Güvenlik Personeli', 'Güvenlik Personeli', true, NOW()),
                (6, 'Şoför', 'Şoför', true, NOW()),
                (7, 'Direktör', 'Direktör', true, NOW()),
                (8, 'Ambalaj Şefi', 'Ambalaj Şefi', true, NOW()),
                (9, 'PLASTİKHANE ŞEFİ', 'PLASTİKHANE ŞEFİ', true, NOW()),
                (10, 'VARDİYA ŞEFİ', 'VARDİYA ŞEFİ', true, NOW()),
                (11, 'BAHÇIVAN', 'BAHÇIVAN', true, NOW()),
                (12, 'Personel', 'Personel', true, NOW());
            ");

            // Reset the sequence to start from 13
            migrationBuilder.Sql("SELECT setval('\"Titles_Id_seq\"', 12, true);");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Titles_TitleId",
                table: "Employees",
                column: "TitleId",
                principalTable: "Titles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Titles_TitleId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "Titles");

            migrationBuilder.RenameColumn(
                name: "TitleId",
                table: "Employees",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_TitleId",
                table: "Employees",
                newName: "IX_Employees_RoleId");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CanApproveLeaveRequests = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageDepartments = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageEmployees = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageLeaveTypes = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageSystemSettings = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewAllLeaveRequests = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Roles_RoleId",
                table: "Employees",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
