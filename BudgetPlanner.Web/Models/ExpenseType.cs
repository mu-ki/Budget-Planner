namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// Type of expense recurrence.
    /// Monthly = fixed monthly bill (rent, utilities, etc.)
    /// RecurringInterval = every N months (e.g. quarterly savings, 4-month funds)
    /// </summary>
    public enum ExpenseType
    {
        Monthly = 0,        // Every month
        RecurringInterval = 1  // Every N months (3, 4, 6, 12, etc. - 12 = yearly)
    }
}
