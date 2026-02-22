using Microsoft.EntityFrameworkCore.Migrations;

namespace BudgetPlanner.Web.Data.Migrations
{
    public partial class AddReserveAccountStartDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StartDate",
                table: "ReserveAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ReserveAccounts");
        }
    }
}
