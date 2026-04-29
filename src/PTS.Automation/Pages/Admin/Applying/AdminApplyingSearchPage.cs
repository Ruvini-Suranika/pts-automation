namespace PTS.Automation.Pages.Admin.Applying;

/// <summary>Admin → Members → Applying (<c>Admin/Applying</c>). Grid rows from <c>POST /Admin/GetApplyingSearchList</c>.</summary>
public sealed class AdminApplyingSearchPage : AdminPage
{
    public AdminApplyingSearchPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.Applying;

    protected override ILocator ReadinessIndicator => SearchInput;

    public ILocator SearchInput => Page.Locator("#search");

    public ILocator TableBody => Page.Locator("#tableApplyingBody");

    public ILocator Pagination => Page.Locator("#pagination");

    public ILocator AfterSalesMemberLinks => Page.Locator("#tableApplyingBody a[href*='/Admin/AfterSales/']");

    public Task<IResponse> StartWaitForGetApplyingSearchListAsync() =>
        Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/GetApplyingSearchList", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

    /// <summary>Parses member id from <c>/Admin/AfterSales/{id}</c> anchor href.</summary>
    public static string? TryParseMemberIdFromAfterSalesHref(string? href)
    {
        if (string.IsNullOrWhiteSpace(href)) return null;
        var marker = "/Admin/AfterSales/";
        var idx = href.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        var tail = href[(idx + marker.Length)..].TrimEnd('/');
        var slash = tail.IndexOf('/');
        var id = slash >= 0 ? tail[..slash] : tail;
        return string.IsNullOrWhiteSpace(id) ? null : id;
    }

    public async Task<string?> TryGetFirstAfterSalesMemberIdAsync()
    {
        if (await AfterSalesMemberLinks.CountAsync() == 0) return null;
        var href = await AfterSalesMemberLinks.First.GetAttributeAsync("href");
        return TryParseMemberIdFromAfterSalesHref(href);
    }

    public Task FillSearchAsync(string text) => SearchInput.FillAsync(text);
}
