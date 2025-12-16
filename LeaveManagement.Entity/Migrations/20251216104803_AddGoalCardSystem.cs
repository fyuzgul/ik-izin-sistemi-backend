using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeaveManagement.Entity.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalCardSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoalCardTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    TitleId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByEmployeeId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalCardTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalCardTemplates_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoalCardTemplates_Employees_CreatedByEmployeeId",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoalCardTemplates_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoalTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeGoalCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    GoalCardTemplateId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByEmployeeId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeGoalCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeGoalCards_Employees_CreatedByEmployeeId",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeGoalCards_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeGoalCards_GoalCardTemplates_GoalCardTemplateId",
                        column: x => x.GoalCardTemplateId,
                        principalTable: "GoalCardTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoalCardItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoalCardTemplateId = table.Column<int>(type: "integer", nullable: false),
                    GoalTypeId = table.Column<int>(type: "integer", nullable: false),
                    Goal = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TargetDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Target80Percent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Target100Percent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Target120Percent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GoalDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalCardItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalCardItems_GoalCardTemplates_GoalCardTemplateId",
                        column: x => x.GoalCardTemplateId,
                        principalTable: "GoalCardTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalCardItems_GoalTypes_GoalTypeId",
                        column: x => x.GoalTypeId,
                        principalTable: "GoalTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeGoalCardItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeGoalCardId = table.Column<int>(type: "integer", nullable: false),
                    GoalCardItemId = table.Column<int>(type: "integer", nullable: false),
                    ActualCompletionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AchievementLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ManagerNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EmployeeNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeGoalCardItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeGoalCardItems_EmployeeGoalCards_EmployeeGoalCardId",
                        column: x => x.EmployeeGoalCardId,
                        principalTable: "EmployeeGoalCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeGoalCardItems_GoalCardItems_GoalCardItemId",
                        column: x => x.GoalCardItemId,
                        principalTable: "GoalCardItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeGoalCardItems_EmployeeGoalCardId",
                table: "EmployeeGoalCardItems",
                column: "EmployeeGoalCardId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeGoalCardItems_GoalCardItemId",
                table: "EmployeeGoalCardItems",
                column: "GoalCardItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeGoalCards_CreatedByEmployeeId",
                table: "EmployeeGoalCards",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeGoalCards_EmployeeId_GoalCardTemplateId_Year",
                table: "EmployeeGoalCards",
                columns: new[] { "EmployeeId", "GoalCardTemplateId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeGoalCards_GoalCardTemplateId",
                table: "EmployeeGoalCards",
                column: "GoalCardTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalCardItems_GoalCardTemplateId",
                table: "GoalCardItems",
                column: "GoalCardTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalCardItems_GoalTypeId",
                table: "GoalCardItems",
                column: "GoalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalCardTemplates_CreatedByEmployeeId",
                table: "GoalCardTemplates",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalCardTemplates_DepartmentId_TitleId_IsActive",
                table: "GoalCardTemplates",
                columns: new[] { "DepartmentId", "TitleId", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_GoalCardTemplates_TitleId",
                table: "GoalCardTemplates",
                column: "TitleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeGoalCardItems");

            migrationBuilder.DropTable(
                name: "EmployeeGoalCards");

            migrationBuilder.DropTable(
                name: "GoalCardItems");

            migrationBuilder.DropTable(
                name: "GoalCardTemplates");

            migrationBuilder.DropTable(
                name: "GoalTypes");
        }
    }
}
