using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BudgetPlanner.Web.Data;
using BudgetPlanner.Web.Models;
using BudgetPlanner.Web.Services;

namespace BudgetPlanner.Web.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly BudgetService _budgetService;

        public BudgetController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            BudgetService budgetService)
        {
            _context = context;
            _userManager = userManager;
            _budgetService = budgetService;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        public IActionResult Dashboard(int? year, int? month)
        {
            var dt = DateTime.Now;
            var y = year ?? dt.Year;
            var m = month ?? dt.Month;

            var userId = GetUserId();
            var income = _budgetService.GetMonthlyIncome(userId, y, m);
            var (totalExpense, items) = _budgetService.GetMonthlyExpenseBreakdown(userId, y, m);
            var netBalance = income - totalExpense;
            var moveToSavings = items.Where(i => i.IsMonthlyBill).Sum(i => i.Amount);
            var chitPayment = items.Where(i => i.IsChitDue).Sum(i => i.Amount);
            var chitAllocation = items.Where(i => !i.IsMonthlyBill && !i.IsChitDue).Sum(i => i.Amount);

            ViewBag.Year = y;
            ViewBag.Month = m;
            ViewBag.MonthName = BudgetService.GetMonthName(m);
            ViewBag.TotalIncome = income;
            ViewBag.TotalExpenses = totalExpense;
            ViewBag.NetBalance = netBalance;
            ViewBag.MoveToSavings = moveToSavings;
            ViewBag.ChitPaymentThisMonth = chitPayment;
            ViewBag.ChitAllocationThisMonth = chitAllocation;

            return View();
        }

        #region Income


        public IActionResult Incomes(int? year, int? month)
        {
            var dt = DateTime.Now;
            var y = year ?? dt.Year;
            var m = month ?? dt.Month;

            var userId = GetUserId();
            var start = new DateTime(y, m, 1);
            var end = start.AddMonths(1);

            var allIncomes = _context.Incomes
                .Where(i => i.UserId == userId)
                .ToList();
            var items = allIncomes
                .Where(i => i.IncomeDate >= start && i.IncomeDate < end)
                .OrderByDescending(i => i.IncomeDate)
                .ToList();

            ViewBag.Year = y;
            ViewBag.Month = m;
            ViewBag.MonthName = BudgetService.GetMonthName(m);
            ViewBag.Total = _budgetService.GetMonthlyIncome(userId, y, m); // Includes one-time + recurring
            ViewBag.RecurringIncomes = _context.RecurringIncomes.Where(r => r.UserId == userId).OrderBy(r => r.Description).ToList();

            return View(items);
        }

        [HttpGet]
        public IActionResult AddIncome()
        {
            return View(new Income { IncomeDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIncome(Income model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = GetUserId();
                _context.Incomes.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Incomes));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditIncome(int id)
        {
            var item = await _context.Incomes.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIncome(int id, Income model)
        {
            if (id != model.Id) return NotFound();
            var item = await _context.Incomes.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();

            if (ModelState.IsValid)
            {
                item.Description = model.Description;
                item.Amount = model.Amount;
                item.IncomeDate = model.IncomeDate;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Incomes));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteIncome(int id)
        {
            var item = await _context.Incomes.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            _context.Incomes.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Incomes));
        }

        #endregion

        #region Recurring Income

        public IActionResult RecurringIncomes()
        {
            var userId = GetUserId();
            var items = _context.RecurringIncomes
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.Description)
                .ToList();
            return View(items);
        }

        [HttpGet]
        public IActionResult AddRecurringIncome()
        {
            return View(new RecurringIncome { StartDate = DateTime.Today, IntervalMonths = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecurringIncome(RecurringIncome model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = GetUserId();
                _context.RecurringIncomes.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RecurringIncomes));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditRecurringIncome(int id)
        {
            var item = await _context.RecurringIncomes.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRecurringIncome(int id, RecurringIncome model)
        {
            if (id != model.Id) return NotFound();
            var item = await _context.RecurringIncomes.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();

            if (ModelState.IsValid)
            {
                item.Description = model.Description;
                item.Amount = model.Amount;
                item.IntervalMonths = model.IntervalMonths;
                item.StartDate = model.StartDate;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RecurringIncomes));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecurringIncome(int id)
        {
            var item = await _context.RecurringIncomes.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            _context.RecurringIncomes.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(RecurringIncomes));
        }

        #endregion

        #region Expenses

        public IActionResult Expenses()
        {
            var userId = GetUserId();
            var items = _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.Type)
                .ThenBy(e => e.Description)
                .ToList();
            var reserves = _context.ReserveAccounts
                .Where(r => r.UserId == userId)
                .Select(r => new { r.ExpenseId, r.Id })
                .ToList();
            ViewBag.ExpenseIdsWithReserve = reserves.Select(r => r.ExpenseId).ToHashSet();
            ViewBag.ExpenseIdToReserveId = reserves.ToDictionary(r => r.ExpenseId, r => r.Id);

            return View(items);
        }

        [HttpGet]
        public IActionResult AddExpense()
        {
            return View(new Expense { StartDate = DateTime.Today, IntervalMonths = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(Expense model)
        {
            if (model.Type == ExpenseType.Monthly)
                model.IntervalMonths = 1;

            if (ModelState.IsValid)
            {
                model.UserId = GetUserId();
                _context.Expenses.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Expenses));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditExpense(int id)
        {
            var item = await _context.Expenses.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExpense(int id, Expense model)
        {
            if (id != model.Id) return NotFound();
            var exp = await _context.Expenses.FindAsync(id);
            if (exp == null || exp.UserId != GetUserId()) return NotFound();

            if (model.Type == ExpenseType.Monthly)
                model.IntervalMonths = 1;

            if (ModelState.IsValid)
            {
                exp.Description = model.Description;
                exp.Amount = model.Amount;
                exp.Type = model.Type;
                exp.IntervalMonths = model.IntervalMonths;
                exp.StartDate = model.StartDate;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Expenses));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var item = await _context.Expenses.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            _context.Expenses.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Expenses));
        }

        #endregion

        #region Monthly Breakdown

        public IActionResult MonthlyBreakdown(int? year, int? month)
        {
            var dt = DateTime.Now;
            var y = year ?? dt.Year;
            var m = month ?? dt.Month;

            var userId = GetUserId();
            var income = _budgetService.GetMonthlyIncome(userId, y, m);
            var (totalExpenses, items) = _budgetService.GetMonthlyExpenseBreakdown(userId, y, m);
            var netBalance = income - totalExpenses;

            var vm = new MonthlyBreakdownViewModel
            {
                Year = y,
                Month = m,
                MonthName = BudgetService.GetMonthName(m),
                TotalIncome = income,
                TotalExpenses = totalExpenses,
                NetBalance = netBalance,
                MoveToSavings = items.Where(i => i.IsMonthlyBill).Sum(i => i.Amount),
                ChitPaymentThisMonth = items.Where(i => i.IsChitDue).Sum(i => i.Amount),
                ChitAllocationThisMonth = items.Where(i => !i.IsMonthlyBill && !i.IsChitDue).Sum(i => i.Amount),
                Items = items
            };

            ViewBag.Year = y;
            ViewBag.Month = m;
            return View(vm);
        }

        #endregion

        #region Reserve Accounts

        public IActionResult ReserveAccounts(int? year, int? month)
        {
            var dt = DateTime.Now;
            var y = year ?? dt.Year;
            var m = month ?? dt.Month;

            var userId = GetUserId();
            var reserves = _budgetService.GetReserveAccounts(userId, y, m);

            ViewBag.Year = y;
            ViewBag.Month = m;
            ViewBag.MonthName = BudgetService.GetMonthName(m);
            return View(reserves);
        }

        public async Task<IActionResult> ReserveAccountDetail(int id)
        {
            var userId = GetUserId();
            var vm = _budgetService.GetReserveAccountDetail(userId, id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpGet]
        public IActionResult AddAllocation(int reserveAccountId, int? year, int? month)
        {
            var dt = DateTime.Now;
            var y = year ?? dt.Year;
            var m = month ?? dt.Month;
            ViewBag.ReserveAccountId = reserveAccountId;
            ViewBag.AllocationDate = new DateTime(y, m, 1);
            ViewBag.PlannedAmount = 0m; // Will be set from query or default
            return View(new AddAllocationViewModel { ReserveAccountId = reserveAccountId, Amount = 0, AllocationDate = new DateTime(y, m, 1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAllocation(AddAllocationViewModel model)
        {
            if (ModelState.IsValid && model.Amount > 0)
            {
                await _budgetService.RecordAllocation(GetUserId(), model.ReserveAccountId, model.Amount, model.AllocationDate);
                return RedirectToAction(nameof(ReserveAccountDetail), new { id = model.ReserveAccountId });
            }
            ViewBag.ReserveAccountId = model.ReserveAccountId;
            ViewBag.AllocationDate = model.AllocationDate;
            return View(model);
        }

        [HttpGet]
        public IActionResult AddPayment(int reserveAccountId)
        {
            return View(new AddPaymentViewModel { ReserveAccountId = reserveAccountId, PaymentDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(AddPaymentViewModel model)
        {
            if (ModelState.IsValid && model.Amount > 0)
            {
                await _budgetService.RecordPayment(GetUserId(), model.ReserveAccountId, model.Amount, model.PaymentDate, model.Notes);
                return RedirectToAction(nameof(ReserveAccountDetail), new { id = model.ReserveAccountId });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateReserve(int expenseId)
        {
            var expense = await _context.Expenses.FindAsync(expenseId);
            if (expense == null || expense.UserId != GetUserId()) return NotFound();
            if (_budgetService.ExpenseHasReserve(GetUserId(), expenseId))
                return RedirectToAction(nameof(Expenses));
            ViewBag.Expense = expense;
            return View(new CreateReserveViewModel { ExpenseId = expenseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReserve(CreateReserveViewModel model)
        {
            var reserve = await _budgetService.CreateReserveAccount(GetUserId(), model.ExpenseId, model.TotalAmount, model.TenureEnd, model.Notes);
            if (reserve == null) return RedirectToAction(nameof(Expenses));
            return RedirectToAction(nameof(ReserveAccountDetail), new { id = reserve.Id });
        }

        #endregion
    }
}
