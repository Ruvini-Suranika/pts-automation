namespace PTS.Automation.Pages.Member.Shell;

/// <summary>
/// Single source of truth for the Member portal's URL routes. Every page
/// class references one of these constants for its <c>RelativePath</c>, so
/// if the dev team renames a route we update ONE file, not dozens.
///
/// Routes are derived from <c>MemberMenu.cshtml</c>, <c>MemberMegaMenu.cshtml</c>,
/// and the controller action method names. See <c>docs/MEMBER-PAGE-MAP.md</c>
/// for the full table.
/// </summary>
public static class MemberRoutes
{
    public const string Dashboard = "Member/Index";

    // ── Clients group ──────────────────────────────────────────────────
    public const string Clients       = "Client/ClientSearchView";
    public const string Quotes        = "Quote/QuoteSearchView";
    public const string Bookings      = "Client/BookingSearchView";

    /// <summary>Booking overview (summary) — query: <c>Id</c>, <c>BookingRefId</c>.</summary>
    public const string BookingDetailsOverview = "Client/BookingDetails";

    /// <summary>Booking management / itinerary shell — query: <c>id</c>, <c>BookingRefId</c>.</summary>
    public const string ClientBookingDetailsManagement = "Client/ClientBookingDetails";

    /// <summary>Booking Money — query: <c>Id</c>, <c>BookingRefId</c>.</summary>
    public const string BookingMoney = "Client/Money";

    /// <summary>Total booking fees (APC etc.) — query: <c>Id</c> (client ref new).</summary>
    public const string BookingFee = "Client/BookingFee";

    public const string IssueTickets  = "Member/IssueTicket";

    // ── Calendar ───────────────────────────────────────────────────────
    public const string Calendar      = "Member/MemberCalender";

    // ── Suppliers group ────────────────────────────────────────────────
    public const string Accommodation = "Accommodation/SearchAccommodation";
    public const string Activities    = "Activities/ActivitySearch";
    public const string Flights       = "Flight/Flights";
    public const string Packages      = "Package/PackageSearch";
    public const string Transport     = "Member/SuppliersTransport";
    public const string Cruises       = "Cruise/SuppliersCruises";

    // ── Accounts group ─────────────────────────────────────────────────
    public const string AccountOverview     = "TravelMemberAccount/AccountOverview";
    public const string Transactions        = "Client/Transactions";
    public const string GpsPayments         = "TravelMemberAccount/GPSPayments";
    public const string PaymentsOutstanding = "TravelMemberAccount/PaymentsOutstanding";
    public const string PaymentDue          = "TravelMemberAccount/PaymentDue";
    public const string Unclaimed           = "Financial/GetAllUnclaimed";
    public const string Unassigned          = "Financial/Unassigned";
    public const string ProfitClaims        = "TravelMemberAccount/ProfitClaims";

    // ── Reporting group ────────────────────────────────────────────────
    public const string BusinessReporting       = "TravelReporting/BusinessReporting";
    public const string SupplierReporting       = "TravelReporting/SupplierReporting";
    public const string BookingsReporting       = "TravelReporting/ClientReporting";
    public const string AtolReporting           = "TravelReporting/AtolReporting";
    public const string SupplierDebitsReporting = "TravelReporting/AccountDebits";
    public const string UserCommissionReporting = "TravelReporting/GetSearchCommissionReporting";

    // ── Settings mega-menu ─────────────────────────────────────────────
    public const string ApiSupplierSettings   = "Member/APISupplierSettings";
    public const string ContractedSuppliers   = "Member/SearchContractedSuppliers";
    public const string Downloads             = "Member/PTSDownloads";
    public const string EmailSettings         = "Member/EmailSettings";
    public const string ItinerarySettings     = "Member/ItinerarySettings";
    public const string OrganisationSettings  = "Client/SettingOrganisation";
    public const string QuoteSettings         = "Member/QuoteSettings";
    public const string Users                 = "Member/Users";

    // ── Top-bar icons ──────────────────────────────────────────────────
    public const string Apps        = "Client/Apps";
    public const string UserProfile = "Member/UserDetails";

    // ── Authentication ─────────────────────────────────────────────────
    public const string Login          = "Account/Login";
    public const string Logout         = "Account/LogOut";
    public const string ForgotPassword = "Account/ForgotPassword";
}
