namespace PTS.Automation.Infrastructure;

/// <summary>NUnit <c>[Category("…")]</c> constants. Keep them here so filters stay consistent.</summary>
public static class Categories
{
    public const string Smoke        = "Smoke";
    public const string Regression   = "Regression";
    public const string E2E          = "E2E";
    public const string Negative     = "Negative";
    public const string Slow         = "Slow";

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
