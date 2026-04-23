namespace PTS.Automation.Data.Models;

/// <summary>
/// Lightweight DTO representing a client for automation purposes.
/// Mirrors the subset of fields the Add Client modal in the Member portal
/// actually renders today — many fields in the dev code are commented out
/// in the Razor view (travelDate, clientType, destination, …), so this record
/// only carries what the UI exposes.
///
/// Source: <c>Views/Client/ClientSearchView.cshtml</c> (modal id <c>#Addclient</c>)
/// + <c>Scripts/client/client.js</c> (<c>#addClientButton</c> submit handler).
/// </summary>
public sealed record Client
{
    public string Title     { get; init; } = "Mr";
    public string FirstName { get; init; } = "";
    public string LastName  { get; init; } = "";
    public string Email     { get; init; } = "";

    /// <summary>
    /// Phone number in DIGITS-ONLY local form (e.g. <c>07700900123</c> for UK).
    /// The dev app's validator enforces <c>/^[0-9 ]+$/</c>; intlTelInput then
    /// converts local → E.164 using the selected country flag before the POST.
    /// </summary>
    public string Phone { get; init; } = "";

    /// <summary>Label of the "Enquiry type" dropdown option. Null = pick first real option.</summary>
    public string? EnquiryType { get; init; }

    /// <summary>Label of the "Assigned user" dropdown option. Null = pick first real option.</summary>
    public string? AssignedUser { get; init; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
