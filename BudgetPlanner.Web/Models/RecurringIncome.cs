using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// Recurring income (e.g. monthly salary, yearly bonus).
    /// </summary>
    public class RecurringIncome
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// 1 = monthly, 12 = yearly.
        /// </summary>
        [Range(1, 12)]
        public int IntervalMonths { get; set; } = 1;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        public string UserId { get; set; }
    }
}
