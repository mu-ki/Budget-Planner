using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BudgetPlanner.Web.Data;

namespace BudgetPlanner.Web
{
    public class Program
    {
        private static void EnsureReserveTablesExist(ApplicationDbContext db)
        {
            try
            {
                db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""ReserveAccounts"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""ExpenseId"" INTEGER NOT NULL,
                    ""UserId"" TEXT,
                    ""TotalAmount"" REAL,
                    ""StartDate"" TEXT,
                    ""TenureEnd"" TEXT,
                    ""Notes"" TEXT,
                    FOREIGN KEY(""ExpenseId"") REFERENCES ""Expenses""(""Id"")
                )");
                db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_ReserveAccounts_UserId"" ON ""ReserveAccounts"" (""UserId"")");
                db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_ReserveAccounts_ExpenseId"" ON ""ReserveAccounts"" (""ExpenseId"")");

                db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""ReserveAllocations"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""ReserveAccountId"" INTEGER NOT NULL,
                    ""UserId"" TEXT,
                    ""Amount"" REAL NOT NULL,
                    ""AllocationDate"" TEXT NOT NULL,
                    ""Notes"" TEXT,
                    FOREIGN KEY(""ReserveAccountId"") REFERENCES ""ReserveAccounts""(""Id"") ON DELETE CASCADE
                )");
                db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_ReserveAllocations_ReserveAccountId"" ON ""ReserveAllocations"" (""ReserveAccountId"")");

                db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""ReservePayments"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""ReserveAccountId"" INTEGER NOT NULL,
                    ""UserId"" TEXT,
                    ""Amount"" REAL NOT NULL,
                    ""PaymentDate"" TEXT NOT NULL,
                    ""Notes"" TEXT,
                    FOREIGN KEY(""ReserveAccountId"") REFERENCES ""ReserveAccounts""(""Id"") ON DELETE CASCADE
                )");
                db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_ReservePayments_ReserveAccountId"" ON ""ReservePayments"" (""ReserveAccountId"")");

                db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""RecurringIncomes"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""Description"" TEXT NOT NULL,
                    ""Amount"" REAL NOT NULL,
                    ""IntervalMonths"" INTEGER NOT NULL,
                    ""StartDate"" TEXT NOT NULL,
                    ""UserId"" TEXT
                )");
                db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_RecurringIncomes_UserId"" ON ""RecurringIncomes"" (""UserId"")");

                db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""OneTimeExpenses"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""Description"" TEXT NOT NULL,
                    ""Amount"" REAL NOT NULL,
                    ""ExpenseDate"" TEXT NOT NULL,
                    ""UserId"" TEXT
                )");
                db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_OneTimeExpenses_UserId"" ON ""OneTimeExpenses"" (""UserId"")");
                db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_OneTimeExpenses_ExpenseDate"" ON ""OneTimeExpenses"" (""ExpenseDate"")");

                db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""Categories"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""Name"" TEXT NOT NULL,
                    ""Type"" INTEGER NOT NULL,
                    ""UserId"" TEXT,
                    ""IsDefault"" INTEGER NOT NULL DEFAULT 0
                )");
                db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_Categories_UserId"" ON ""Categories"" (""UserId"")");
            }
            catch (Exception)
            {
                // Tables may already exist or Expenses table missing - let app run and show proper error
            }
        }

        private static bool ColumnExists(ApplicationDbContext db, string table, string column)
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info(\"{table}\") WHERE name=\"{column}\"";
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        private static void AddColumnIfNotExists(ApplicationDbContext db, string table, string column, string type)
        {
            if (!ColumnExists(db, table, column))
                db.Database.ExecuteSqlRaw("ALTER TABLE \"" + table + "\" ADD COLUMN \"" + column + "\" " + type);
        }

        private static void EnsureReserveStartDateColumn(ApplicationDbContext db)
        {
            AddColumnIfNotExists(db, "ReserveAccounts", "StartDate", "TEXT");
        }

        private static void EnsureCategoryColumnsExist(ApplicationDbContext db)
        {
            AddColumnIfNotExists(db, "Incomes", "CategoryId", "INTEGER");
            AddColumnIfNotExists(db, "RecurringIncomes", "CategoryId", "INTEGER");
            AddColumnIfNotExists(db, "Expenses", "CategoryId", "INTEGER");
            AddColumnIfNotExists(db, "OneTimeExpenses", "CategoryId", "INTEGER");
        }

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    db.Database.Migrate();
                }
                catch (Exception)
                {
                    // Migrate failed - ensure tables exist as fallback
                }
                EnsureReserveTablesExist(db);
                EnsureReserveStartDateColumn(db);
                EnsureCategoryColumnsExist(db);
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
