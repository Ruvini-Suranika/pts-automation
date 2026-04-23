using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Suppliers;

/// <summary>
/// Suppliers → Cruises page.
/// Controller action: <c>CruiseController.SuppliersCruises</c>.
///
/// SKELETON.
/// </summary>
public sealed class CruisesSuppliersPage : MemberPage
{
    public CruisesSuppliersPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Cruises;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
