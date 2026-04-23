using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Reporting;

/// <summary>
/// Reporting → Supplier Debits. Debit-level reporting for supplier accounts.
/// Controller action: <c>TravelReportingController.AccountDebits</c>.
///
/// SKELETON.
/// </summary>
public sealed class SupplierDebitsReportingPage : MemberPage
{
    public SupplierDebitsReportingPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.SupplierDebitsReporting;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
