namespace PTS.Automation.Pages.Admin.Settings;

/// <summary>Admin → Settings → Users → Member users (<c>Admin/MemberUser</c>). Grid HTML from <c>POST /Admin/MemberUserPagination</c>.</summary>
public sealed class AdminMemberUsersListPage : AdminPage
{
    public AdminMemberUsersListPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.MemberUser;

    protected override ILocator ReadinessIndicator => Page.Locator("#linkMemberUserActive");

    public ILocator MemberUserTable => Page.Locator("#tableMemberUser");

    public ILocator TwoFaColumnHeader =>
        Page.Locator("#tableMemberUser thead th", new() { HasText = "2FA ACTIVATED" }).First;

    public ILocator MemberUserDetailLinks => Page.Locator("#tb_MemberUser a[href*='MemberUserDetail']");

    public Task<IResponse> StartWaitForMemberUserPaginationAsync() =>
        Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/MemberUserPagination", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

    public Task ClickAdminUsersTabAsync() => Page.Locator("#linkAdminUserActive").ClickAsync();

    public Task ClickSupplierUsersTabAsync() => Page.Locator("#linkSupplierUserActive").ClickAsync();

    public static string? TryParseMemberUserDetailIdFromHref(string? href)
    {
        if (string.IsNullOrWhiteSpace(href)) return null;
        if (!href.Contains("MemberUserDetail", StringComparison.OrdinalIgnoreCase)) return null;

        if (href.Contains('?', StringComparison.Ordinal))
        {
            var query = href.Split('?', 2)[1];
            foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2
                    && kv[0].Equals("Id", StringComparison.OrdinalIgnoreCase))
                    return Uri.UnescapeDataString(kv[1]);
            }
        }

        var trimmed = href.TrimEnd('/');
        var segments = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < segments.Length; i++)
        {
            if (segments[i].Equals("MemberUserDetail", StringComparison.OrdinalIgnoreCase)
                && i + 1 < segments.Length)
                return Uri.UnescapeDataString(segments[i + 1]);
        }

        return null;
    }

    public async Task<string?> TryGetFirstMemberUserDetailIdAsync()
    {
        if (await MemberUserDetailLinks.CountAsync() == 0) return null;
        var href = await MemberUserDetailLinks.First.GetAttributeAsync("href");
        return TryParseMemberUserDetailIdFromHref(href);
    }
}
