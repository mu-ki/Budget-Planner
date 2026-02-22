using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// A sinking-fund "bucket" for an expense obligation.
    /// Money is saved here each month (Allocations) and paid out when due (Payments).
    /// </summary>
    public class ReserveAccount
    {
        public int Id { get; set; }

        /// <summary>
        /// Links to the expense this reserve is for (chit, loan, bill).
        /// </summary>
        [Required]
        public int ExpenseId { get; set; }
        public Expense Expense { get; set; }

        public string UserId { get; set; }

        /// <summary>
        /// Total amount for the obligation (e.g. chit total ₹15,00,000). Optional.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// When this obligation ends (e.g. chit tenure end). Optional.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Tenure End Date")]
        public DateTime? TenureEnd { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
