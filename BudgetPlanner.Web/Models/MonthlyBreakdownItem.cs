using System;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// Represents how much to allocate for an expense in a given month.
    /// </summary>
    public class MonthlyBreakdownItem
    {
        public string ExpenseDescription { get; set; }
        public decimal Amount { get; set; }
        public ExpenseType Type { get; set; }
        public int IntervalMonths { get; set; }
        public string Notes { get; set; }
        /// <summary>True if this is a monthly/yearly bill to pay (move to savings). False if chit allocation (keep in salary).</summary>
        public bool IsMonthlyBill { get; set; }
        /// <summary>True if chit is due this month (pay from salary). False if chit allocation (accumulate in salary).</summary>
        public bool IsChitDue { get; set; }
    }

    public class MonthlyBreakdownViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetBalance { get; set; }
        /// <summary>Monthly/yearly bills – move to savings & pay.</summary>
        public decimal MoveToSavings { get; set; }
        /// <summary>Chits due this month – pay from salary account.</summary>
        public decimal ChitPaymentThisMonth { get; set; }
        /// <summary>Chit allocation (not due) – keep in salary, accumulate.</summary>
        public decimal ChitAllocationThisMonth { get; set; }
        public System.Collections.Generic.List<MonthlyBreakdownItem> Items { get; set; } = new System.Collections.Generic.List<MonthlyBreakdownItem>();
    }
}
