using System;
using System.Collections.Generic;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// View model for a reserve account with balance and transaction history.
    /// </summary>
    public class ReserveAccountViewModel
    {
        public int Id { get; set; }
        public int ExpenseId { get; set; }
        public string ExpenseDescription { get; set; }
        public ExpenseType ExpenseType { get; set; }
        public int IntervalMonths { get; set; }
        public decimal InstallmentAmount { get; set; }
        public DateTime? TenureEnd { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Notes { get; set; }

        /// <summary>
        /// Total allocated (saved) so far.
        /// </summary>
        public decimal TotalAllocated { get; set; }

        /// <summary>
        /// Total paid out so far.
        /// </summary>
        public decimal TotalPaid { get; set; }

        /// <summary>
        /// Current balance = TotalAllocated - TotalPaid.
        /// </summary>
        public decimal Balance => TotalAllocated - TotalPaid;

        /// <summary>
        /// Planned allocation for current month (from expense schedule).
        /// </summary>
        public decimal PlannedMonthlyAllocation { get; set; }

        /// <summary>
        /// Whether payment is due this month.
        /// </summary>
        public bool IsDueThisMonth { get; set; }

        public List<ReserveAllocationViewModel> Allocations { get; set; } = new List<ReserveAllocationViewModel>();
        public List<ReservePaymentViewModel> Payments { get; set; } = new List<ReservePaymentViewModel>();
    }

    public class ReserveAllocationViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime AllocationDate { get; set; }
        public string Notes { get; set; }
    }

    public class ReservePaymentViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Notes { get; set; }
    }
}
