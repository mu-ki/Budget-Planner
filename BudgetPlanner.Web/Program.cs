using System;
using System.Collections.Generic;
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
            }
            catch (Exception)
            {
                // Tables may already exist or Expenses table missing - let app run and show proper error
            }
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
