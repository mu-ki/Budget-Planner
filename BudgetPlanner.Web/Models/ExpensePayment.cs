using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// Records a payment for a recurring expense in a given period.
    /// Used for: Mark as Paid (bills) and Chit Contribution (transfer from salary to chit account).
    /// </summary>
    public class ExpensePayment
    {
        public int Id { get; set; }

        [Required]
        public int ExpenseId { get; set; }
        public Expense Expense { get; set; }

        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PaidDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>BillPaid = marked bill as paid; ChitContribution = transferred from salary to chit account.</summary>
        public ExpensePaymentType Type { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }

        public string UserId { get; set; }
    }

    public enum ExpensePaymentType
    {
        BillPaid = 0,
        ChitContribution = 1
    }
}
