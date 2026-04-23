using PTS.Automation.Data.Models;

namespace PTS.Automation.Data.Builders;

/// <summary>
/// Fluent builder for <see cref="Client"/>. Produces realistic, unique,
/// randomised test data so parallel test runs don't collide.
///
/// Defaults are safe for parallel runs:
///   - Email is suffixed with a UTC timestamp + random 4-digit salt
///   - Phone uses the Ofcom-reserved UK test range (07700 900000–900999,
///     expressed in E.164 as +447700900xxx) which will never reach a real device
///
/// Usage:
/// <code>
///   var client = new ClientBuilder().WithDefaults().Build();
///   var custom = new ClientBuilder().WithDefaults().WithEmail("fixed@x.com").Build();
/// </code>
/// </summary>
public sealed class ClientBuilder
{
    private static readonly string[] Titles     = { "Mr", "Mrs", "Miss", "Ms", "Mx", "Dr" };
    private static readonly string[] FirstNames = { "Alex", "Jordan", "Taylor", "Sam", "Morgan", "Robin", "Charlie", "Drew" };
    private static readonly string[] LastNames  = { "Smith", "Harper", "Brooks", "Flynn", "Patel", "Nolan", "Ward", "Reid" };
    private static readonly Random Rng = new();

    private string  _title        = "Mr";
    private string  _firstName    = "";
    private string  _lastName     = "";
    private string  _email        = "";
    private string  _phone        = "";
    private string? _enquiryType;
    private string? _assignedUser;

    /// <summary>Fills every field with realistic, unique random data.</summary>
    public ClientBuilder WithDefaults()
    {
        var first  = Pick(FirstNames);
        var last   = Pick(LastNames);
        var stamp  = DateTime.UtcNow.ToString("yyMMddHHmmssfff");
        var suffix = Rng.Next(1000, 9999);

        _title     = Pick(Titles);
        _firstName = first;
        _lastName  = last;
        _email     = $"{first}.{last}.{stamp}{suffix}@pts-automation.test".ToLowerInvariant();

        // UK Ofcom-reserved test range 07700 900000–900999 — never rings a real
        // device. Format is DIGITS ONLY (local form): the dev app's client-side
        // validator enforces /^[0-9 ]+$/, and the intlTelInput plugin then
        // converts local → E.164 before the POST to /Client/AddClientDetail.
        _phone = $"07700900{Rng.Next(0, 1000):D3}";
        return this;
    }

    public ClientBuilder WithTitle(string value)        { _title        = value; return this; }
    public ClientBuilder WithFirstName(string value)    { _firstName    = value; return this; }
    public ClientBuilder WithLastName(string value)     { _lastName     = value; return this; }
    public ClientBuilder WithEmail(string value)        { _email        = value; return this; }
    public ClientBuilder WithPhone(string value)        { _phone        = value; return this; }
    public ClientBuilder WithEnquiryType(string? value) { _enquiryType  = value; return this; }
    public ClientBuilder WithAssignedUser(string? value){ _assignedUser = value; return this; }

    public Client Build() => new()
    {
        Title        = _title,
        FirstName    = _firstName,
        LastName     = _lastName,
        Email        = _email,
        Phone        = _phone,
        EnquiryType  = _enquiryType,
        AssignedUser = _assignedUser
    };

    private static string Pick(string[] pool) => pool[Rng.Next(pool.Length)];
}
