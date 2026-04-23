using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Reporting;

/// <summary>
/// Reporting → Suppliers.
/// Controller action: <c>TravelReportingController.SupplierReporting</c>.
///
/// SKELETON.
/// </summary>
public sealed class SupplierReportingPage : MemberPage
{
    public SupplierReportingPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.SupplierReporting;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
