using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Reporting;

/// <summary>
/// Reporting → Business. Business KPIs and trend reports.
/// Controller action: <c>TravelReportingController.BusinessReporting</c>.
///
/// SKELETON.
/// </summary>
public sealed class BusinessReportingPage : MemberPage
{
    public BusinessReportingPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.BusinessReporting;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
