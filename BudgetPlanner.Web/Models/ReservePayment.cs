using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// A withdrawal from a reserve account when the obligation is due (chit turn, loan EMI, bill).
    /// </summary>
    public class ReservePayment
    {
        public int Id { get; set; }

        [Required]
        public int ReserveAccountId { get; set; }
        public ReserveAccount ReserveAccount { get; set; }

        public string UserId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }
    }
}
