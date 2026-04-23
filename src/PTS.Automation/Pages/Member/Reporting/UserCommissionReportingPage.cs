using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Reporting;

/// <summary>
/// Reporting → User Commission Breakdown. Per-user commission summary.
/// Controller action: <c>TravelReportingController.GetSearchCommissionReportingAsync</c>
/// (MVC strips the <c>Async</c> suffix for routing).
///
/// SKELETON.
/// </summary>
public sealed class UserCommissionReportingPage : MemberPage
{
    public UserCommissionReportingPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.UserCommissionReporting;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
