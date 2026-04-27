using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.Debits;

/// <summary>
/// Admin → Debits → Trustee's authorisation. Source: <c>Views/Admin/DebitsTrustees.cshtml</c>.
/// </summary>
public sealed class AdminTrusteesAuthorisationPage : AdminPage
{
    public AdminTrusteesAuthorisationPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.DebitsTrustees;

    protected override ILocator ReadinessIndicator => Page.Locator("#searchId");

    public Task<bool> IsAuthoriseBulkButtonVisibleAsync() => Page.Locator("#BulkAuth").IsVisibleAsync();
}
