using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.Debits;

/// <summary>
/// Admin → Debits → Debits unauthorised. Source: <c>Views/Admin/DebitsUnAuthorised.cshtml</c>.
/// </summary>
public sealed class AdminDebitsUnauthorisedPage : AdminPage
{
    public AdminDebitsUnauthorisedPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.DebitsUnauthorised;

    protected override ILocator ReadinessIndicator => Page.Locator("#searchUnauth");

    public Task<bool> IsGridVisibleAsync() => Page.Locator("table#unauthorised").IsVisibleAsync();
}
