using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sentinel.Api.Migrations
{
    /// <inheritdoc />
    public partial class CompanyIdFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "MonthlyPaycheck",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Costs",
                newName: "CompanyId");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Employees",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "CostCategories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Costs_CategoryId",
                table: "Costs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Costs_EmployeeId",
                table: "Costs",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Costs_CostCategories_CategoryId",
                table: "Costs",
                column: "CategoryId",
                principalTable: "CostCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Costs_Employees_EmployeeId",
                table: "Costs",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Costs_CostCategories_CategoryId",
                table: "Costs");

            migrationBuilder.DropForeignKey(
                name: "FK_Costs_Employees_EmployeeId",
                table: "Costs");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Costs_CategoryId",
                table: "Costs");

            migrationBuilder.DropIndex(
                name: "IX_Costs_EmployeeId",
                table: "Costs");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "CostCategories");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Costs",
                newName: "DepartmentId");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPaycheck",
                table: "Employees",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
