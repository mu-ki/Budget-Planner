using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public ExpenseType Type { get; set; }

        /// <summary>
        /// For Monthly: always 1. For RecurringInterval: number of months (e.g. 3, 4).
        /// </summary>
        [Range(1, 120)]
        public int IntervalMonths { get; set; } = 1;

        /// <summary>
        /// When this expense first applies (start date for recurring calculation).
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        public string UserId { get; set; }
    }
}
