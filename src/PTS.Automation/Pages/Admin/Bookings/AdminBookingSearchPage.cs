using Microsoft.Playwright;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Pages.Admin.Bookings;

/// <summary>Admin → Bookings list (<c>Admin/BookingSearch</c>). Results load via <c>/Admin/BookingSearchAdmin</c>.</summary>
public sealed class AdminBookingSearchPage : AdminPage
{
    public AdminBookingSearchPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.BookingSearch;

    protected override ILocator ReadinessIndicator => Page.Locator("#txtClientReference");

    private ILocator SearchButton => Page.Locator("pts-button[type='search']");

    public ILocator BookingDetailsLinks =>
        Page.Locator("#divBookingSearchResults a[href*='/Admin/BookingDetails'], #tableClientList a[href*='/Admin/BookingDetails']");

    public async Task SearchByBookingReferenceAsync(string bookingReference)
    {
        await Page.Locator("#txtClientReference").FillAsync(bookingReference);
        await SearchButton.ClickAsync();
    }

    public async Task WaitForResultRowsAsync(int minimumRows = 1)
    {
        await Page.WaitForFunctionAsync(
            "min => document.querySelectorAll(\"#tableClientList tbody tr\").length >= min",
            minimumRows,
            new PageWaitForFunctionOptions { Timeout = Settings.Timeouts.NavigationMs });
    }

    public async Task<(string ClientNewId, string BookingRefId)?> TryReadFirstBookingDetailsQueryAsync()
    {
        var n = await BookingDetailsLinks.CountAsync();
        if (n == 0) return null;
        var href = await BookingDetailsLinks.First.GetAttributeAsync("href");
        return TryParseAdminBookingDetailsQuery(href);
    }

    /// <summary>Parses <c>/Admin/BookingDetails?id=…&amp;bookingReferenceId=…</c> (query names are case-insensitive).</summary>
    public static (string ClientNewId, string BookingRefId)? TryParseAdminBookingDetailsQuery(string? href)
    {
        if (string.IsNullOrWhiteSpace(href)) return null;
        if (!href.Contains("BookingDetails", StringComparison.OrdinalIgnoreCase)) return null;

        var query = "";
        if (Uri.TryCreate(href, UriKind.Absolute, out var abs))
            query = abs.Query.TrimStart('?');
        else
        {
            var q = href.IndexOf('?', StringComparison.Ordinal);
            query = q >= 0 ? href[(q + 1)..] : "";
        }

        var dict = ParseQueryString(query);
        if (!dict.TryGetValue("id", out var id) || string.IsNullOrWhiteSpace(id))
            return null;

        if (!dict.TryGetValue("bookingreferenceid", out var br) || string.IsNullOrWhiteSpace(br))
            return null;

        return (id, br);
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            var key = Uri.UnescapeDataString(kv[0]);
            var val = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : "";
            dict[key] = val;
        }

        return dict;
    }
}
