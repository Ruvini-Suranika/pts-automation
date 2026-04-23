using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Reporting;

/// <summary>
/// Reporting → ATOL. ATOL compliance reporting.
/// Controller action: <c>TravelReportingController.AtolReporting</c>.
///
/// SKELETON.
/// </summary>
public sealed class AtolReportingPage : MemberPage
{
    public AtolReportingPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.AtolReporting;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
