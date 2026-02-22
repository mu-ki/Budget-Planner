using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// Bank account for tracking where money flows (salary, savings, chit).
    /// </summary>
    public class Account
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public AccountType Type { get; set; }

        [StringLength(100)]
        [Display(Name = "Bank / Institution")]
        public string BankName { get; set; }

        [Display(Name = "Current Balance")]
        public decimal Balance { get; set; }

        public string UserId { get; set; }
    }

    public enum AccountType
    {
        Salary = 0,   // Where income lands (e.g. HDFC)
        Savings = 1,  // General savings (e.g. SBI)
        Chit = 2      // Chit account (e.g. Amma SBI)
    }
}
