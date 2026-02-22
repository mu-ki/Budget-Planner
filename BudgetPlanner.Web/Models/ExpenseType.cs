namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// Type of expense recurrence.
    /// OneTime = single occurrence on a specific date.
    /// Monthly = fixed monthly bill (rent, utilities, etc.)
    /// RecurringInterval = every N months (e.g. quarterly savings)
    /// Chit = chit fund – every N months, accumulate in bank, pay when turn comes
    /// </summary>
    public enum ExpenseType
    {
        OneTime = -1,       // Single occurrence (stored in OneTimeExpenses table)
        Monthly = 0,        // Every month
        RecurringInterval = 1,  // Every N months (e.g. quarterly)
        Chit = 2            // Chit fund – every N months (4, 6, etc.), accumulate & pay when due
    }
}
