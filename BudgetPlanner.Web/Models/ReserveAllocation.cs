using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// A monthly deposit into a reserve account (saving for when payment is due).
    /// </summary>
    public class ReserveAllocation
    {
        public int Id { get; set; }

        [Required]
        public int ReserveAccountId { get; set; }
        public ReserveAccount ReserveAccount { get; set; }

        public string UserId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// The month this allocation is for (start of month).
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Allocation Date")]
        public DateTime AllocationDate { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }
    }
}
