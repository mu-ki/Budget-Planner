using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    public class CreateReserveViewModel
    {
        public int ExpenseId { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Total Amount (optional)")]
        public decimal? TotalAmount { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Tenure End (optional)")]
        public DateTime? TenureEnd { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
