namespace BudgetPlanner.Web.Models
{
    /// <summary>
    /// Payment Strategy Engine – exactly two strategies. No additional modes.
    /// </summary>
    public enum PaymentStrategy
    {
        /// <summary>Pay the full amount this month. Money leaves the bank.</summary>
        PayNow = 0,

        /// <summary>Allocate monthly share; keep in bank until obligation is due (sinking fund).</summary>
        AccumulateInBank = 1
    }
}
