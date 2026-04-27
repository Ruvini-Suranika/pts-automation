using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.Credits;

/// <summary>
/// Admin → Credits → Unassigned overview. Source: <c>Views/Admin/UnassignedCredits.cshtml</c>.
/// </summary>
public sealed class AdminUnassignedOverviewPage : AdminPage
{
    public AdminUnassignedOverviewPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.UnassignedCredits;

    protected override ILocator ReadinessIndicator => Page.Locator("#searchtext").First;

    public Task<bool> IsRemoveButtonPresentAsync() => Page.Locator("#deleteBtn").IsVisibleAsync();
}
