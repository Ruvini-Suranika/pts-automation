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

    public const string BookingSearch = "Admin/BookingSearch";

    public const string BookingDetails = "Admin/BookingDetails";

    public const string Money = "Admin/Money";

    public const string BookingFee = "Admin/BookingFee";

    public const string SearchClient = "Admin/SearchClient";

    public const string ClientDetails = "Admin/ClientDetails";

    public const string MemberAdminPrefix = "Admin/MemberAdmin";

    /// <summary>Client itinerary (same host as admin; <c>ItineraryController.ClientBookingsItinerary</c>).</summary>
    public const string ClientBookingsItineraryPathPrefix = "Itinerary/ClientBookingsItinerary";

    /// <summary>Settings (mega menu) → Users → Member users (<c>Admin/MemberUser</c>).</summary>
    public const string MemberUser = "Admin/MemberUser";

    public const string AdminUsers = "Admin/AdminUsers";

    public const string SupplierUsers = "Admin/SupplierUsers";

    /// <summary>Member user drill-down (<c>AdminController.MemberUserDetail</c> — <c>Id</c> query).</summary>
    public const string MemberUserDetail = "Admin/MemberUserDetail";

    /// <summary>Logged-in admin profile (<c>Admin/AdminProfile</c>).</summary>
    public const string AdminProfile = "Admin/AdminProfile";

    /// <summary>Members → Applying (<c>Admin/Applying</c>).</summary>
    public const string Applying = "Admin/Applying";

    /// <summary>Applying member workspace (<c>Admin/AfterSales/{id}</c>).</summary>
    public const string AfterSalesPathPrefix = "Admin/AfterSales";

    /// <summary>Sales tab for applying / onboarding (<c>Admin/EditMemberCompany/{id}</c>).</summary>
    public const string EditMemberCompanyPathPrefix = "Admin/EditMemberCompany";

    /// <summary>Risk stage (<c>Admin/RiskDetails/{id}</c>).</summary>
    public const string RiskDetailsPathPrefix = "Admin/RiskDetails";
}
