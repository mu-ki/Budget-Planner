using Microsoft.EntityFrameworkCore.Migrations;

namespace BudgetPlanner.Web.Data.Migrations
{
    public partial class AddBudgetModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Incomes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    IncomeDate = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incomes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    IntervalMonths = table.Column<int>(nullable: false),
                    StartDate = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_UserId",
                table: "Incomes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_IncomeDate",
                table: "Incomes",
                column: "IncomeDate");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_StartDate",
                table: "Expenses",
                column: "StartDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Incomes");
            migrationBuilder.DropTable(name: "Expenses");
        }
    }
}
