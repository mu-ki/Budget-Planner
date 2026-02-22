using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BudgetPlanner.Web.Models;

namespace BudgetPlanner.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Income> Incomes { get; set; }
        public DbSet<RecurringIncome> RecurringIncomes { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<OneTimeExpense> OneTimeExpenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ReserveAccount> ReserveAccounts { get; set; }
        public DbSet<ReserveAllocation> ReserveAllocations { get; set; }
        public DbSet<ReservePayment> ReservePayments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Income>()
                .HasIndex(i => i.UserId);
            builder.Entity<Income>()
                .HasIndex(i => i.IncomeDate);
            builder.Entity<Income>()
                .HasOne(i => i.Category)
                .WithMany()
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<RecurringIncome>()
                .HasIndex(r => r.UserId);
            builder.Entity<RecurringIncome>()
                .HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Expense>()
                .HasIndex(e => e.UserId);
            builder.Entity<Expense>()
                .HasIndex(e => e.StartDate);
            builder.Entity<Expense>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OneTimeExpense>()
                .HasIndex(o => o.UserId);
            builder.Entity<OneTimeExpense>()
                .HasIndex(o => o.ExpenseDate);
            builder.Entity<OneTimeExpense>()
                .HasOne(o => o.Category)
                .WithMany()
                .HasForeignKey(o => o.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Category>()
                .HasIndex(c => c.UserId);
            builder.Entity<Category>()
                .HasIndex(c => new { c.UserId, c.Type });

            builder.Entity<ReserveAccount>()
                .HasIndex(r => r.UserId);
            builder.Entity<ReserveAccount>()
                .HasOne(r => r.Expense)
                .WithMany()
                .HasForeignKey(r => r.ExpenseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ReserveAllocation>()
                .HasIndex(a => a.ReserveAccountId);
            builder.Entity<ReserveAllocation>()
                .HasOne(a => a.ReserveAccount)
                .WithMany()
                .HasForeignKey(a => a.ReserveAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReservePayment>()
                .HasIndex(p => p.ReserveAccountId);
            builder.Entity<ReservePayment>()
                .HasOne(p => p.ReserveAccount)
                .WithMany()
                .HasForeignKey(p => p.ReserveAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
