using Microsoft.EntityFrameworkCore.Migrations;

namespace BudgetPlanner.Web.Data.Migrations
{
    public partial class AddCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Type = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    IsDefault = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId",
                table: "Categories",
                column: "UserId");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Incomes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "RecurringIncomes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Expenses",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "OneTimeExpenses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CategoryId", table: "Incomes");
            migrationBuilder.DropColumn(name: "CategoryId", table: "RecurringIncomes");
            migrationBuilder.DropColumn(name: "CategoryId", table: "Expenses");
            migrationBuilder.DropColumn(name: "CategoryId", table: "OneTimeExpenses");
            migrationBuilder.DropTable(name: "Categories");
        }
    }
}
