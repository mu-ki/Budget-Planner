using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// Category for organizing income or expenses.
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        public CategoryType Type { get; set; }

        public string UserId { get; set; }

        /// <summary>
        /// If true, this is a system default category that cannot be deleted.
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
