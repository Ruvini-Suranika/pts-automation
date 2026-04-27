using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.Debits;

/// <summary>
/// Admin → Debits → Debits authorised. Source: <c>Views/Admin/GetDebitsAuthorised.cshtml</c>.
/// </summary>
public sealed class AdminDebitsAuthorisedPage : AdminPage
{
    public AdminDebitsAuthorisedPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.DebitsAuthorised;

    protected override ILocator ReadinessIndicator => Page.Locator("#searchtextauth");
}
