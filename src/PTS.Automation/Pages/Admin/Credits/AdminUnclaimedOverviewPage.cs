using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.Credits;

/// <summary>
/// Admin → Credits → Unclaimed overview. Source: <c>Views/Admin/UnClaimedCredits.cshtml</c>.
/// </summary>
public sealed class AdminUnclaimedOverviewPage : AdminPage
{
    public AdminUnclaimedOverviewPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.UnclaimedCredits;

    private ILocator ActiveTab => Page.Locator("section.table_header a.active");

    protected override ILocator ReadinessIndicator => Page.Locator("#searchtext").First;

    public Task<string> ActiveTabTextAsync() => ActiveTab.InnerTextAsync();
}
