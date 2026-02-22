using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BudgetPlanner.Web.Data;
using BudgetPlanner.Web.Models;

namespace BudgetPlanner.Web.Services
{
    public class BudgetService
    {
        private readonly ApplicationDbContext _context;

        public BudgetService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get total income for a user in a given month/year.
        /// Includes one-time incomes and recurring incomes (monthly/yearly).
        /// </summary>
        public decimal GetMonthlyIncome(string userId, int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);
            var targetDate = new DateTime(year, month, 1);

            var oneTime = _context.Incomes
                .Where(i => i.UserId == userId && i.IncomeDate >= start && i.IncomeDate < end)
                .Sum(i => i.Amount);

            var recurring = _context.RecurringIncomes
                .Where(r => r.UserId == userId)
                .ToList()
                .Where(r => IsRecurringIncomeDue(r, targetDate))
                .Sum(r => r.Amount);

            return oneTime + recurring;
        }

        private static bool IsRecurringIncomeDue(RecurringIncome r, DateTime targetMonth)
        {
            if (r.IntervalMonths == 1) return true; // Monthly
            var start = r.StartDate.Date;
            if (targetMonth < start) return false;
            var monthsSinceStart = ((targetMonth.Year - start.Year) * 12) + (targetMonth.Month - start.Month);
            return monthsSinceStart % r.IntervalMonths == 0; // Yearly (12) or other
        }

        /// <summary>
        /// Get total income for a user across all records (for dashboard summary).
        /// </summary>
        public decimal GetTotalIncomeForMonth(string userId, int year, int month)
        {
            return GetMonthlyIncome(userId, year, month);
        }

        /// <summary>
        /// Compute how much to allocate for each expense in a given month.
        /// Uses Payment Strategy Engine: PayNow or AccumulateInBank only.
        /// </summary>
        public (decimal totalExpense, List<MonthlyBreakdownItem> items) GetMonthlyExpenseBreakdown(string userId, int year, int month)
        {
            var items = new List<MonthlyBreakdownItem>();
            var targetDate = new DateTime(year, month, 1);
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);
            decimal total = 0;

            // One-time expenses for this month (always Pay Now)
            var oneTimeExpenses = _context.OneTimeExpenses
                .Where(o => o.UserId == userId && o.ExpenseDate >= start && o.ExpenseDate < end)
                .ToList();
            foreach (var o in oneTimeExpenses)
            {
                items.Add(new MonthlyBreakdownItem
                {
                    ExpenseDescription = o.Description,
                    Amount = o.Amount,
                    Type = ExpenseType.OneTime,
                    IntervalMonths = 1,
                    Notes = "One-time expense",
                    Strategy = PaymentStrategy.PayNow,
                    IsMonthlyBill = true,
                    IsChitDue = false
                });
                total += o.Amount;
            }

            var expenses = _context.Expenses.Where(e => e.UserId == userId).ToList();

            foreach (var exp in expenses)
            {
                var strategy = GetPaymentStrategy(exp, targetDate);
                var isYearly = exp.Type == ExpenseType.RecurringInterval && exp.IntervalMonths == 12;
                var isChit = exp.Type == ExpenseType.Chit;

                if (strategy == PaymentStrategy.PayNow)
                {
                    items.Add(new MonthlyBreakdownItem
                    {
                        ExpenseDescription = exp.Description,
                        Amount = exp.Amount,
                        Type = exp.Type,
                        IntervalMonths = exp.Type == ExpenseType.Monthly ? 1 : exp.IntervalMonths,
                        Notes = isYearly ? "Pay this month (yearly bill)" : exp.Type == ExpenseType.Monthly ? "Move to savings & pay" : "Pay from bank account (chit due)",
                        Strategy = PaymentStrategy.PayNow,
                        IsMonthlyBill = exp.Type == ExpenseType.Monthly || isYearly,
                        IsChitDue = !isYearly && (exp.Type == ExpenseType.RecurringInterval || isChit)
                    });
                    total += exp.Amount;
                }
                else
                {
                    var monthlyAllocation = exp.Amount / exp.IntervalMonths;
                    items.Add(new MonthlyBreakdownItem
                    {
                        ExpenseDescription = exp.Description,
                        Amount = monthlyAllocation,
                        Type = exp.Type,
                        IntervalMonths = exp.IntervalMonths,
                        Notes = isChit ? "Chit – keep in bank, accumulate, pay when turn comes" : "Keep in bank account – accumulate (sinking fund)",
                        Strategy = PaymentStrategy.AccumulateInBank,
                        IsMonthlyBill = false,
                        IsChitDue = false
                    });
                    total += monthlyAllocation;
                }
            }

            return (total, items);
        }

        /// <summary>
        /// Payment Strategy Engine: returns PayNow or AccumulateInBank.
        /// PayNow = obligation due this month; AccumulateInBank = not due, allocate monthly.
        /// </summary>
        private PaymentStrategy GetPaymentStrategy(Expense exp, DateTime targetMonth)
        {
            var isDue = IsDueInMonth(exp, targetMonth);
            return isDue ? PaymentStrategy.PayNow : PaymentStrategy.AccumulateInBank;
        }

        /// <summary>
        /// Whether a recurring expense falls due in the given month.
        /// </summary>
        private bool IsDueInMonth(Expense exp, DateTime targetMonth)
        {
            if (exp.Type == ExpenseType.Monthly) return true;

            var start = exp.StartDate.Date;
            var target = targetMonth.Date;

            if (target < start) return false;

            var monthsSinceStart = ((target.Year - start.Year) * 12) + (target.Month - start.Month);
            return monthsSinceStart % exp.IntervalMonths == 0;
        }

        public static string GetMonthName(int month)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }

        #region Reserve Accounts

        /// <summary>
        /// Get all reserve accounts for a user with balance info, for a given month/year.
        /// </summary>
        public List<ReserveAccountViewModel> GetReserveAccounts(string userId, int year, int month)
        {
            var targetMonth = new DateTime(year, month, 1);
            var reserves = _context.ReserveAccounts
                .Where(r => r.UserId == userId)
                .Include(r => r.Expense)
                .OrderBy(r => r.Expense.Description)
                .ToList();

            var result = new List<ReserveAccountViewModel>();
            foreach (var r in reserves)
            {
                var exp = r.Expense;
                var totalAllocated = _context.ReserveAllocations
                    .Where(a => a.ReserveAccountId == r.Id)
                    .Sum(a => a.Amount);
                var totalPaid = _context.ReservePayments
                    .Where(p => p.ReserveAccountId == r.Id)
                    .Sum(p => p.Amount);
                var plannedAlloc = exp.Type == ExpenseType.Monthly
                    ? exp.Amount
                    : exp.Amount / exp.IntervalMonths;
                var isDue = IsDueInMonth(exp, targetMonth);

                result.Add(new ReserveAccountViewModel
                {
                    Id = r.Id,
                    ExpenseId = r.ExpenseId,
                    ExpenseDescription = exp.Description,
                    ExpenseType = exp.Type,
                    IntervalMonths = exp.IntervalMonths,
                    InstallmentAmount = exp.Amount,
                    StartDate = r.StartDate,
                    TenureEnd = r.TenureEnd,
                    TotalAmount = r.TotalAmount,
                    Notes = r.Notes,
                    TotalAllocated = totalAllocated,
                    TotalPaid = totalPaid,
                    PlannedMonthlyAllocation = plannedAlloc,
                    IsDueThisMonth = isDue
                });
            }
            return result;
        }

        /// <summary>
        /// Get a single reserve account with full transaction history.
        /// </summary>
        public ReserveAccountViewModel GetReserveAccountDetail(string userId, int reserveAccountId)
        {
            var r = _context.ReserveAccounts
                .Include(ra => ra.Expense)
                .FirstOrDefault(ra => ra.Id == reserveAccountId && ra.UserId == userId);
            if (r == null) return null;

            var exp = r.Expense;
            var allocations = _context.ReserveAllocations
                .Where(a => a.ReserveAccountId == r.Id)
                .OrderByDescending(a => a.AllocationDate)
                .Select(a => new ReserveAllocationViewModel
                {
                    Id = a.Id,
                    Amount = a.Amount,
                    AllocationDate = a.AllocationDate,
                    Notes = a.Notes
                })
                .ToList();
            var payments = _context.ReservePayments
                .Where(p => p.ReserveAccountId == r.Id)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new ReservePaymentViewModel
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    Notes = p.Notes
                })
                .ToList();
            var totalAllocated = allocations.Sum(a => a.Amount);
            var totalPaid = payments.Sum(p => p.Amount);

            return new ReserveAccountViewModel
            {
                Id = r.Id,
                ExpenseId = r.ExpenseId,
                ExpenseDescription = exp.Description,
                ExpenseType = exp.Type,
                IntervalMonths = exp.IntervalMonths,
                InstallmentAmount = exp.Amount,
                StartDate = r.StartDate,
                TenureEnd = r.TenureEnd,
                TotalAmount = r.TotalAmount,
                Notes = r.Notes,
                TotalAllocated = totalAllocated,
                TotalPaid = totalPaid,
                PlannedMonthlyAllocation = exp.Type == ExpenseType.Monthly ? exp.Amount : exp.Amount / exp.IntervalMonths,
                IsDueThisMonth = IsDueInMonth(exp, new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)),
                Allocations = allocations,
                Payments = payments
            };
        }

        /// <summary>
        /// Record a monthly allocation (deposit) into a reserve account.
        /// </summary>
        public async Task RecordAllocation(string userId, int reserveAccountId, decimal amount, DateTime allocationDate)
        {
            var reserve = await _context.ReserveAccounts
                .FirstOrDefaultAsync(r => r.Id == reserveAccountId && r.UserId == userId);
            if (reserve == null) return;

            _context.ReserveAllocations.Add(new ReserveAllocation
            {
                ReserveAccountId = reserveAccountId,
                UserId = userId,
                Amount = amount,
                AllocationDate = new DateTime(allocationDate.Year, allocationDate.Month, 1),
                Notes = null
            });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Record a payment (withdrawal) from a reserve account.
        /// </summary>
        public async Task RecordPayment(string userId, int reserveAccountId, decimal amount, DateTime paymentDate, string notes = null)
        {
            var reserve = await _context.ReserveAccounts
                .FirstOrDefaultAsync(r => r.Id == reserveAccountId && r.UserId == userId);
            if (reserve == null) return;

            _context.ReservePayments.Add(new ReservePayment
            {
                ReserveAccountId = reserveAccountId,
                UserId = userId,
                Amount = amount,
                PaymentDate = paymentDate,
                Notes = notes
            });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete an allocation. Returns reserve account id if found and deleted, null otherwise.
        /// </summary>
        public async Task<int?> DeleteAllocation(string userId, int allocationId)
        {
            var a = await _context.ReserveAllocations
                .FirstOrDefaultAsync(x => x.Id == allocationId && x.UserId == userId);
            if (a == null) return null;
            var reserveId = a.ReserveAccountId;
            _context.ReserveAllocations.Remove(a);
            await _context.SaveChangesAsync();
            return reserveId;
        }

        /// <summary>
        /// Delete a payment. Returns reserve account id if found and deleted, null otherwise.
        /// </summary>
        public async Task<int?> DeletePayment(string userId, int paymentId)
        {
            var p = await _context.ReservePayments
                .FirstOrDefaultAsync(x => x.Id == paymentId && x.UserId == userId);
            if (p == null) return null;
            var reserveId = p.ReserveAccountId;
            _context.ReservePayments.Remove(p);
            await _context.SaveChangesAsync();
            return reserveId;
        }

        /// <summary>
        /// Check if an expense already has a reserve account.
        /// </summary>
        public bool ExpenseHasReserve(string userId, int expenseId)
        {
            return _context.ReserveAccounts.Any(r => r.ExpenseId == expenseId && r.UserId == userId);
        }

        /// <summary>
        /// Create a reserve account for an expense.
        /// </summary>
        public async Task<ReserveAccount> CreateReserveAccount(string userId, int expenseId, decimal? totalAmount = null, DateTime? startDate = null, DateTime? tenureEnd = null, string notes = null)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId);
            if (expense == null) return null;
            if (ExpenseHasReserve(userId, expenseId)) return null;

            var reserve = new ReserveAccount
            {
                ExpenseId = expenseId,
                UserId = userId,
                TotalAmount = totalAmount,
                StartDate = startDate,
                TenureEnd = tenureEnd,
                Notes = notes
            };
            _context.ReserveAccounts.Add(reserve);
            await _context.SaveChangesAsync();
            return reserve;
        }
        #endregion
    }
}
