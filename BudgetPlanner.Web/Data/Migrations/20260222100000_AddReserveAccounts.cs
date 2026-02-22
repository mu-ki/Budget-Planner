using Microsoft.EntityFrameworkCore.Migrations;

namespace BudgetPlanner.Web.Data.Migrations
{
    public partial class AddReserveAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReserveAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExpenseId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    TotalAmount = table.Column<decimal>(nullable: true),
                    TenureEnd = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReserveAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReserveAccounts_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReserveAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReserveAccountId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    AllocationDate = table.Column<string>(nullable: false),
                    Notes = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReserveAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReserveAllocations_ReserveAccounts_ReserveAccountId",
                        column: x => x.ReserveAccountId,
                        principalTable: "ReserveAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservePayments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReserveAccountId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    PaymentDate = table.Column<string>(nullable: false),
                    Notes = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservePayments_ReserveAccounts_ReserveAccountId",
                        column: x => x.ReserveAccountId,
                        principalTable: "ReserveAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReserveAccounts_UserId",
                table: "ReserveAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveAccounts_ExpenseId",
                table: "ReserveAccounts",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveAllocations_ReserveAccountId",
                table: "ReserveAllocations",
                column: "ReserveAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservePayments_ReserveAccountId",
                table: "ReservePayments",
                column: "ReserveAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ReserveAllocations");
            migrationBuilder.DropTable(name: "ReservePayments");
            migrationBuilder.DropTable(name: "ReserveAccounts");
        }
    }
}
