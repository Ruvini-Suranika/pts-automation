using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Pages.Admin.Clients;

/// <summary>Admin → Clients list (<c>Admin/SearchClient</c>). Results via <c>/Admin/SearchClientAdmin</c>.</summary>
public sealed class AdminClientSearchPage : AdminPage
{
    public AdminClientSearchPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.SearchClient;

    protected override ILocator ReadinessIndicator => Page.Locator("#advanceFilter");

    private ILocator SearchButton => Page.Locator("pts-button[type='search']");

    private ILocator ClearFiltersButton => Page.Locator("pts-button[type='clear-filters']");

    public ILocator ClientDetailsLinks =>
        Page.Locator("#divSearchResults a[href*='/Admin/ClientDetails']");

    public Task ClickSearchAsync() => SearchButton.ClickAsync();

    public Task ClickClearFiltersAsync() => ClearFiltersButton.ClickAsync();

    public Task ClickShowAllAsync() => Page.Locator("#btnSearchMember").ClickAsync();

    public Task FillFirstNameAsync(string value) => Page.Locator("#txtFirstName").FillAsync(value);

    public Task FillLastNameAsync(string value) => Page.Locator("#txtLastName").FillAsync(value);

    public Task FillEmailAsync(string value) => Page.Locator("#txtEmail").FillAsync(value);

    public Task FillPhoneAsync(string value) => Page.Locator("#txtTelephone").FillAsync(value);

    public Task FillClientReferenceAsync(string value) => Page.Locator("#txtClientReference").FillAsync(value);

    public Task FillDestinationAsync(string value) => Page.Locator("#dropdownEnquiredDestination").FillAsync(value);

    public Task<bool> IsAdvancedFilterFieldVisibleAsync(ILocator field) => field.IsVisibleAsync();

    public ILocator TravelTypeSelect => Page.Locator("#TravelTypeId");

    public ILocator AssignedUserSelect => Page.Locator("#dropdownUser");

    public ILocator SectorSelect => Page.Locator("#dropdownSector");

    public ILocator MemberSelect => Page.Locator("#dropdownMember");

    public Task<bool> IsNoResultsVisibleAsync() =>
        Page.GetByText("No Result Found", new() { Exact = false }).IsVisibleAsync();

    public async Task WaitForSearchClientAdminResponseAsync(Task<IResponse> responseTask)
    {
        await responseTask;
    }

    public Task<IResponse> StartWaitForSearchClientAdminAsync() =>
        Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/SearchClientAdmin", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

    public Task<IResponse> StartWaitForPagedNewClientPartialAsync() =>
        Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/PagedNewClientPartial", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

    public async Task WaitForClientGridRowsAsync(int minimumRows = 1)
    {
        await Page.WaitForFunctionAsync(
            "min => document.querySelectorAll(\"#tableClientList tbody tr\").length >= min",
            minimumRows,
            new PageWaitForFunctionOptions { Timeout = Settings.Timeouts.NavigationMs });
    }

    public Task<int> ClientGridRowCountAsync() =>
        Page.Locator("#tableClientList tbody tr").CountAsync();

    /// <summary>Pagination link for a 1-based page index (e.g. 2 for second page).</summary>
    public ILocator PagingLinkForPage(int pageIndex) =>
        Page.Locator($".pagination a.paging.page-link[data-page-index=\"{pageIndex}\"]:not(.disabled)");

    public async Task<string?> TryGetFirstClientDetailsIdFromGridAsync()
    {
        if (await ClientDetailsLinks.CountAsync() == 0) return null;
        var href = await ClientDetailsLinks.First.GetAttributeAsync("href");
        return TryParseClientDetailsIdFromHref(href);
    }

    public static string? TryParseClientDetailsIdFromHref(string? href)
    {
        if (string.IsNullOrWhiteSpace(href)) return null;
        if (!href.Contains("ClientDetails", StringComparison.OrdinalIgnoreCase)) return null;

        if (href.Contains('?', StringComparison.Ordinal))
        {
            var query = href.Split('?', 2)[1];
            foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2
                    && kv[0].Equals("id", StringComparison.OrdinalIgnoreCase))
                    return Uri.UnescapeDataString(kv[1]);
            }
        }

        var trimmed = href.TrimEnd('/');
        var segments = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < segments.Length; i++)
        {
            if (segments[i].Equals("ClientDetails", StringComparison.OrdinalIgnoreCase)
                && i + 1 < segments.Length)
                return Uri.UnescapeDataString(segments[i + 1]);
        }

        return null;
    }
}
