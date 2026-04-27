namespace PTS.Automation.Infrastructure;

/// <summary>NUnit <c>[Category("…")]</c> constants. Keep them here so filters stay consistent.</summary>
public static class Categories
{
    public const string Smoke        = "Smoke";
    public const string Regression   = "Regression";
    public const string E2E          = "E2E";
    public const string Negative     = "Negative";
    public const string Slow         = "Slow";

    /// <summary>Risk / backlog tier from <c>docs/ADMIN-TEST-CASE-PRIORITY.md</c> — filter: <c>Category=P0</c>.</summary>
    public const string P0 = "P0";
    public const string P1 = "P1";
    public const string P2 = "P2";
    public const string P3 = "P3";

    /// <summary>Admin epic tags for suite grouping (same doc).</summary>
    public const string EpicTrustAccounts      = "Epic.TrustAccounts";
    public const string EpicReconciliation     = "Epic.Reconciliation";
    public const string EpicDebits             = "Epic.Debits";
    public const string EpicTrustee           = "Epic.Trustee";
    public const string EpicCreditsOverviews  = "Epic.CreditsOverviews";

    public const string Member       = "Member";
    public const string Admin        = "Admin";

    public const string Authentication = "Authentication";
    public const string Clients        = "Clients";
    public const string Itineraries    = "Itineraries";
    public const string Bookings       = "Bookings";
    public const string Suppliers      = "Suppliers";
    public const string Debits         = "Debits";
    public const string Reconciliation = "Reconciliation";
    public const string CsvImport      = "CsvImport";
}
