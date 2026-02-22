using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
            var payNow = items.Where(i => i.Strategy == PaymentStrategy.PayNow).Sum(i => i.Amount);
            var accumulateInBank = items.Where(i => i.Strategy == PaymentStrategy.AccumulateInBank).Sum(i => i.Amount);
            var moveToSavings = items.Where(i => i.IsMonthlyBill).Sum(i => i.Amount);
            var chitPayment = items.Where(i => i.IsChitDue).Sum(i => i.Amount);
            var chitAllocation = accumulateInBank;

            var chitsUseReserves = _context.ReserveAccounts
                .Where(r => r.UserId == userId)
                .Join(_context.Expenses, r => r.ExpenseId, e => e.Id, (r, e) => e)
                .Any(e => e.Type == ExpenseType.RecurringInterval || e.Type == ExpenseType.Chit);

            ViewBag.Year = y;
            ViewBag.Month = m;
            ViewBag.MonthName = BudgetService.GetMonthName(m);
            ViewBag.TotalIncome = income;
            ViewBag.TotalExpenses = totalExpense;
            ViewBag.NetBalance = netBalance;
            ViewBag.MoveToSavings = moveToSavings;
            ViewBag.ChitPaymentThisMonth = chitPayment;
            ViewBag.ChitAllocationThisMonth = chitAllocation;
            ViewBag.ChitsUseReserves = chitsUseReserves;

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
            EnsureDefaultCategories(GetUserId());
            ViewBag.IncomeCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Income).OrderBy(c => c.Name).ToList(), "Id", "Name");
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
            EnsureDefaultCategories(GetUserId());
            ViewBag.IncomeCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Income).OrderBy(c => c.Name).ToList(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditIncome(int id)
        {
            EnsureDefaultCategories(GetUserId());
            ViewBag.IncomeCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Income).OrderBy(c => c.Name).ToList(), "Id", "Name");
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
                item.CategoryId = model.CategoryId;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Incomes));
            }
            EnsureDefaultCategories(GetUserId());
            ViewBag.IncomeCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Income).OrderBy(c => c.Name).ToList(), "Id", "Name");
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
            EnsureDefaultCategories(GetUserId());
            ViewBag.IncomeCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Income).OrderBy(c => c.Name).ToList(), "Id", "Name");
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
            EnsureDefaultCategories(GetUserId());
            ViewBag.IncomeCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Income).OrderBy(c => c.Name).ToList(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditRecurringIncome(int id)
        {
            EnsureDefaultCategories(GetUserId());
            ViewBag.IncomeCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Income).OrderBy(c => c.Name).ToList(), "Id", "Name");
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
                item.CategoryId = model.CategoryId;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RecurringIncomes));
            }
            EnsureDefaultCategories(GetUserId());
            ViewBag.IncomeCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Income).OrderBy(c => c.Name).ToList(), "Id", "Name");
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

        #region Categories

        public IActionResult Categories()
        {
            var userId = GetUserId();
            EnsureDefaultCategories(userId);
            var items = _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Name)
                .ToList();
            return View(items);
        }

        private void EnsureDefaultCategories(string userId)
        {
            if (_context.Categories.Any(c => c.UserId == userId)) return;

            var defaults = new[]
            {
                (CategoryType.Income, "Salary"), (CategoryType.Income, "Freelance"), (CategoryType.Income, "Investment"),
                (CategoryType.Income, "Rental"), (CategoryType.Income, "Other"),
                (CategoryType.Expense, "Housing"), (CategoryType.Expense, "Utilities"), (CategoryType.Expense, "Food"),
                (CategoryType.Expense, "Transport"), (CategoryType.Expense, "Healthcare"), (CategoryType.Expense, "Entertainment"),
                (CategoryType.Expense, "Shopping"), (CategoryType.Expense, "Other")
            };
            foreach (var (type, name) in defaults)
            {
                _context.Categories.Add(new Category { Name = name, Type = type, UserId = userId, IsDefault = true });
            }
            _context.SaveChanges();
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View(new Category { Type = CategoryType.Income });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = GetUserId();
                model.IsDefault = false;
                _context.Categories.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var item = await _context.Categories.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category model)
        {
            if (id != model.Id) return NotFound();
            var item = await _context.Categories.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();

            if (ModelState.IsValid)
            {
                item.Name = model.Name;
                item.Type = model.Type;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var item = await _context.Categories.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            _context.Categories.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        #endregion

        #region Expenses

        public IActionResult Expenses(int? year, int? month, int? categoryId)
        {
            var dt = DateTime.Now;
            var y = year ?? dt.Year;
            var m = month ?? dt.Month;

            var userId = GetUserId();
            EnsureDefaultCategories(userId);
            var start = new DateTime(y, m, 1);
            var end = start.AddMonths(1);

            var oneTimeQuery = _context.OneTimeExpenses
                .Include(o => o.Category)
                .Where(o => o.UserId == userId && o.ExpenseDate >= start && o.ExpenseDate < end);
            if (categoryId.HasValue)
                oneTimeQuery = oneTimeQuery.Where(o => o.CategoryId == categoryId.Value);
            var oneTimeItems = oneTimeQuery.OrderByDescending(o => o.ExpenseDate).ToList();

            var recurringQuery = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);
            if (categoryId.HasValue)
                recurringQuery = recurringQuery.Where(e => e.CategoryId == categoryId.Value);
            var recurringList = recurringQuery.OrderBy(e => e.Type).ThenBy(e => e.Description).ToList();

            ViewBag.Year = y;
            ViewBag.Month = m;
            ViewBag.MonthName = BudgetService.GetMonthName(m);
            ViewBag.RecurringExpenses = recurringList;
            ViewBag.TotalExpenseForMonth = _budgetService.GetMonthlyExpenseBreakdown(userId, y, m).Item1;
            ViewBag.ExpenseCategories = new SelectList(
                _context.Categories.Where(c => c.UserId == userId && c.Type == CategoryType.Expense).OrderBy(c => c.Name).ToList(),
                "Id", "Name", categoryId);
            ViewBag.SelectedCategoryId = categoryId;

            return View(oneTimeItems);
        }

        public IActionResult RecurringExpenses()
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
        public IActionResult AddOneTimeExpense(int? year, int? month)
        {
            EnsureDefaultCategories(GetUserId());
            ViewBag.ExpenseCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Expense).OrderBy(c => c.Name).ToList(), "Id", "Name");
            var dt = DateTime.Now;
            var y = year ?? dt.Year;
            var m = month ?? dt.Month;
            var defaultDate = new DateTime(y, m, 1);
            return View(new OneTimeExpense { ExpenseDate = defaultDate });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOneTimeExpense(OneTimeExpense model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = GetUserId();
                _context.OneTimeExpenses.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Expenses), new { year = model.ExpenseDate.Year, month = model.ExpenseDate.Month });
            }
            EnsureDefaultCategories(GetUserId());
            ViewBag.ExpenseCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Expense).OrderBy(c => c.Name).ToList(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditOneTimeExpense(int id)
        {
            EnsureDefaultCategories(GetUserId());
            ViewBag.ExpenseCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Expense).OrderBy(c => c.Name).ToList(), "Id", "Name");
            var item = await _context.OneTimeExpenses.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOneTimeExpense(int id, OneTimeExpense model)
        {
            if (id != model.Id) return NotFound();
            var item = await _context.OneTimeExpenses.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();

            if (ModelState.IsValid)
            {
                item.Description = model.Description;
                item.Amount = model.Amount;
                item.ExpenseDate = model.ExpenseDate;
                item.CategoryId = model.CategoryId;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Expenses), new { year = item.ExpenseDate.Year, month = item.ExpenseDate.Month });
            }
            EnsureDefaultCategories(GetUserId());
            ViewBag.ExpenseCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Expense).OrderBy(c => c.Name).ToList(), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOneTimeExpense(int id, int? year, int? month)
        {
            var item = await _context.OneTimeExpenses.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            var y = item.ExpenseDate.Year;
            var m = item.ExpenseDate.Month;
            _context.OneTimeExpenses.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Expenses), new { year = y, month = m });
        }

        [HttpGet]
        public IActionResult AddExpense()
        {
            EnsureDefaultCategories(GetUserId());
            ViewBag.ExpenseCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Expense).OrderBy(c => c.Name).ToList(), "Id", "Name");
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
                return RedirectToAction(nameof(RecurringExpenses));
            }
            EnsureDefaultCategories(GetUserId());
            ViewBag.ExpenseCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Expense).OrderBy(c => c.Name).ToList(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditExpense(int id)
        {
            EnsureDefaultCategories(GetUserId());
            ViewBag.ExpenseCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Expense).OrderBy(c => c.Name).ToList(), "Id", "Name");
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
                exp.CategoryId = model.CategoryId;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RecurringExpenses));
            }
            EnsureDefaultCategories(GetUserId());
            ViewBag.ExpenseCategories = new SelectList(_context.Categories.Where(c => c.UserId == GetUserId() && c.Type == CategoryType.Expense).OrderBy(c => c.Name).ToList(), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id, int? year, int? month)
        {
            var item = await _context.Expenses.FindAsync(id);
            if (item == null || item.UserId != GetUserId()) return NotFound();
            _context.Expenses.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(RecurringExpenses));
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

            var payNowTotal = items.Where(i => i.Strategy == PaymentStrategy.PayNow).Sum(i => i.Amount);
            var accumulateInBankTotal = items.Where(i => i.Strategy == PaymentStrategy.AccumulateInBank).Sum(i => i.Amount);
            var vm = new MonthlyBreakdownViewModel
            {
                Year = y,
                Month = m,
                MonthName = BudgetService.GetMonthName(m),
                TotalIncome = income,
                TotalExpenses = totalExpenses,
                NetBalance = netBalance,
                PayNowTotal = payNowTotal,
                AccumulateInBankTotal = accumulateInBankTotal,
                MoveToSavings = items.Where(i => i.IsMonthlyBill).Sum(i => i.Amount),
                ChitPaymentThisMonth = items.Where(i => i.IsChitDue).Sum(i => i.Amount),
                ChitAllocationThisMonth = accumulateInBankTotal,
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
                return RedirectToAction(nameof(RecurringExpenses));
            ViewBag.Expense = expense;
            return View(new CreateReserveViewModel { ExpenseId = expenseId, StartDate = expense.StartDate });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReserve(CreateReserveViewModel model)
        {
            var reserve = await _budgetService.CreateReserveAccount(GetUserId(), model.ExpenseId, model.TotalAmount, model.StartDate, model.TenureEnd, model.Notes);
            if (reserve == null) return RedirectToAction(nameof(RecurringExpenses));
            return RedirectToAction(nameof(ReserveAccountDetail), new { id = reserve.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditReserve(int id)
        {
            var userId = GetUserId();
            var vm = _budgetService.GetReserveAccountDetail(userId, id);
            if (vm == null) return NotFound();
            return View(new EditReserveViewModel
            {
                Id = vm.Id,
                ExpenseDescription = vm.ExpenseDescription,
                StartDate = vm.StartDate,
                TenureEnd = vm.TenureEnd
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReserve(int id, EditReserveViewModel model)
        {
            if (id != model.Id) return NotFound();
            var reserve = await _context.ReserveAccounts.FirstOrDefaultAsync(r => r.Id == id && r.UserId == GetUserId());
            if (reserve == null) return NotFound();

            reserve.StartDate = model.StartDate;
            reserve.TenureEnd = model.TenureEnd;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ReserveAccountDetail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllocation(int id)
        {
            var reserveId = await _budgetService.DeleteAllocation(GetUserId(), id);
            if (reserveId == null) return NotFound();
            return RedirectToAction(nameof(ReserveAccountDetail), new { id = reserveId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var reserveId = await _budgetService.DeletePayment(GetUserId(), id);
            if (reserveId == null) return NotFound();
            return RedirectToAction(nameof(ReserveAccountDetail), new { id = reserveId });
        }

        #endregion
    }
}
