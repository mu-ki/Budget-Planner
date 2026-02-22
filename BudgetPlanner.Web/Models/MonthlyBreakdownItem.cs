using System;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// Represents how much to allocate for an expense in a given month.
    /// Driven by the Payment Strategy Engine (PayNow | AccumulateInBank).
    /// </summary>
    public class MonthlyBreakdownItem
    {
        public string ExpenseDescription { get; set; }
        public decimal Amount { get; set; }
        public ExpenseType Type { get; set; }
        public int IntervalMonths { get; set; }
        public string Notes { get; set; }
        /// <summary>Payment strategy for this item. Exactly two: PayNow or AccumulateInBank.</summary>
        public PaymentStrategy Strategy { get; set; }
        /// <summary>Display: monthly/yearly bill (move to savings & pay). Subset of PayNow.</summary>
        public bool IsMonthlyBill { get; set; }
        /// <summary>Display: chit due this month (pay from bank). Subset of PayNow.</summary>
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
        /// <summary>Pay Now total – move to savings + chit due (Strategy = PayNow).</summary>
        public decimal PayNowTotal { get; set; }
        /// <summary>Accumulate in Bank total – sinking fund allocation (Strategy = AccumulateInBank).</summary>
        public decimal AccumulateInBankTotal { get; set; }
        /// <summary>Monthly/yearly bills – move to savings & pay. Display subcategory of PayNow.</summary>
        public decimal MoveToSavings { get; set; }
        /// <summary>Chits due this month – pay from salary account. Display subcategory of PayNow.</summary>
        public decimal ChitPaymentThisMonth { get; set; }
        /// <summary>Chit allocation (not due) – keep in salary, accumulate. Same as AccumulateInBankTotal.</summary>
        public decimal ChitAllocationThisMonth { get; set; }
        public System.Collections.Generic.List<MonthlyBreakdownItem> Items { get; set; } = new System.Collections.Generic.List<MonthlyBreakdownItem>();
    }
}
