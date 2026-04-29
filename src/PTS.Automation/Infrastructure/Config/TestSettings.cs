namespace PTS.Automation.Infrastructure.Config;

/// <summary>
/// Strongly-typed root of the configuration hierarchy. Populated by
/// <see cref="ConfigFactory"/> from appsettings.json, appsettings.{env}.json,
/// user-secrets, and environment variables (PTS_ prefix).
/// </summary>
public sealed class TestSettings
{
    public string Environment { get; set; } = "qa";
    public ApplicationsSettings Applications { get; set; } = new();
    public UsersSettings Users { get; set; } = new();
    public BrowserSettings Browser { get; set; } = new();
    public TimeoutSettings Timeouts { get; set; } = new();
    public PathsSettings Paths { get; set; } = new();
    public TestDataSettings TestData { get; set; } = new();
}

/// <summary>
/// Pre-seeded test data shared across tests. Some tests (e.g. debit creation)
/// depend on a booking with suppliers already existing in the target env —
/// building one end-to-end is its own slice of work. Provide those references
/// here and those tests opt in; absent values → tests skip with a clear message.
/// </summary>
public sealed class TestDataSettings
{
    public MemberTestData Member { get; set; } = new();

    /// <summary>Optional explicit Admin booking context for deep links when search is not used.</summary>
    public AdminBookingsTestData AdminBookings { get; set; } = new();

    /// <summary>Optional Admin client search / <c>ClientDetails</c> identifiers.</summary>
    public AdminClientsTestData AdminClients { get; set; } = new();

    /// <summary>Trust accounts / bank / itinerary P1 tests (member admin id, fee assertions).</summary>
    public AdminTrustAndBankTestData AdminTrustAndBank { get; set; } = new();
}

/// <summary>Admin trust/bank P1 optional seeds.</summary>
public sealed class AdminTrustAndBankTestData
{
    /// <summary>Member id for <c>/Admin/MemberAdmin/{id}</c> (PTS system setup, membership fees).</summary>
    public long MemberAdminMemberId { get; set; }

    public bool HasMemberAdminMemberId => MemberAdminMemberId > 0;
}

/// <summary>Admin <c>SearchClient</c> / <c>ClientDetails</c> test data.</summary>
public sealed class AdminClientsTestData
{
    /// <summary>New client id for <c>/Admin/ClientDetails?id=…</c>.</summary>
    public string ClientNewId { get; set; } = "";

    /// <summary>Value for the Client Reference filter (<c>#txtClientReference</c>).</summary>
    public string SearchClientReference { get; set; } = "";

    public bool HasClientNewId => !string.IsNullOrWhiteSpace(ClientNewId);
}

/// <summary>Admin <c>BookingDetails</c> / <c>Money</c> URL parameters (<c>Id</c> + booking reference).</summary>
public sealed class AdminBookingsTestData
{
    /// <summary>New client id string (<c>ViewBag.ClientRefNew</c>) used as the <c>Id</c> query parameter.</summary>
    public string ClientNewId { get; set; } = "";

    public string BookingReferenceId { get; set; } = "";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientNewId) && !string.IsNullOrWhiteSpace(BookingReferenceId);
}

public sealed class MemberTestData
{
    public DebitSeedData Debits { get; set; } = new();
}

public sealed class DebitSeedData
{
    /// <summary>Client reference number that owns the seed booking. 0 = not configured.</summary>
    public long ClientReferenceNumber { get; set; }

    /// <summary>BookingRefId (string) of the seed booking.</summary>
    public string BookingReferenceId { get; set; } = "";

    /// <summary>Numeric BookingId used by the debit modal's JS trigger.</summary>
    public long BookingId { get; set; }

    /// <summary>Name of a supplier attached to the seed booking.</summary>
    public string SupplierName { get; set; } = "";

    /// <summary>Supplier type id (matches <c>SupplierTypeEnum</c> in dev code).</summary>
    public int SupplierTypeId { get; set; } = 1; // Accommodation by default

    public bool IsConfigured =>
        ClientReferenceNumber > 0
        && !string.IsNullOrWhiteSpace(BookingReferenceId)
        && BookingId > 0
        && !string.IsNullOrWhiteSpace(SupplierName);
}

public sealed class ApplicationsSettings
{
    public AppUrl Member { get; set; } = new();
    public AppUrl Admin { get; set; } = new();
}

public sealed class AppUrl
{
    public string BaseUrl { get; set; } = "";
    public string LoginPath { get; set; } = "Account/Login";
    public string PostLoginPath { get; set; } = "";

    public Uri LoginUri => new(new Uri(BaseUrl), LoginPath);
    public Uri BaseUri  => new(BaseUrl);
}

public sealed class UsersSettings
{
    public UserCredentials Member { get; set; } = new();
    public UserCredentials PtsAdmin { get; set; } = new();
    public UserCredentials Supervisor { get; set; } = new();
}

public sealed class UserCredentials
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}

public sealed class TimeoutSettings
{
    public int DefaultMs { get; set; } = 30_000;
    public int NavigationMs { get; set; } = 60_000;
    public int ExpectMs { get; set; } = 10_000;
}

public sealed class PathsSettings
{
    public string ArtifactsRoot { get; set; } = "TestResults";
    public string AuthStateDir { get; set; } = ".auth";
    public string TraceDir { get; set; } = "TestResults/traces";
    public string VideoDir { get; set; } = "TestResults/videos";
    public string ScreenshotDir { get; set; } = "TestResults/screenshots";
}
