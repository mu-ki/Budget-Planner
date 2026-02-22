using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// One-time expense that occurs on a specific date (e.g. car repair, one-off purchase).
    /// </summary>
    public class OneTimeExpense
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Expense Date")]
        public DateTime ExpenseDate { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        public string UserId { get; set; }
    }
}
