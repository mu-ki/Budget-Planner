using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    public class AddPaymentViewModel
    {
        public int ReserveAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Installment Due Date")]
        public DateTime? InstallmentDueDate { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }
    }
}
