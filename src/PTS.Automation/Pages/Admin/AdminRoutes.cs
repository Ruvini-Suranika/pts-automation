namespace PTS.Automation.Pages.Admin;

/// <summary>
/// Relative URL paths for Admin <c>AdminController</c> actions used by P0 tests.
/// Source: <c>Views/Shared/AdminMenu.cshtml</c> (<c>Url.Action(..., "Admin")</c>).
/// </summary>
public static class AdminRoutes
{
    /// <summary>Bank → Trust Accounts.</summary>
    public const string TrustAccounts = "Admin/TrustAccounts";

    /// <summary>Bank → Reconciliation (MVC action name retains typo <c>Reconcilation</c>).</summary>
    public const string PreviousTravelReconciliationTravel = "Admin/PreviousTravelReconcilation?sectorName=travel";

    public const string DebitsUnauthorised = "Admin/DebitsUnAuthorised";
    public const string DebitsAuthorised   = "Admin/GetDebitsAuthorised";
    public const string DebitsGrouping     = "Admin/DebitsGrouping";
    public const string DebitsTrustees     = "Admin/DebitsTrustees";
    public const string UnclaimedCredits   = "Admin/UnclaimedCredits";
    public const string UnassignedCredits  = "Admin/UnassignedCredits";

    /// <summary>Trust Accounts balance drill-down — query params from TrustAccounts.cshtml.</summary>
    public const string AdminTransactionsPathPrefix = "Admin/AdminTransactions";
}
