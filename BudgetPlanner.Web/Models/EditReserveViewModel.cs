using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    public class EditReserveViewModel
    {
        public int Id { get; set; }
        public string ExpenseDescription { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? TenureEnd { get; set; }
    }
}
