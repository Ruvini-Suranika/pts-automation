using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Reporting;

/// <summary>
/// Reporting → Bookings (rendered from <c>ClientReporting</c> action — MVC
/// strips the <c>Async</c> suffix).
/// Controller action: <c>TravelReportingController.ClientReportingAsync</c>.
///
/// SKELETON.
/// </summary>
public sealed class BookingsReportingPage : MemberPage
{
    public BookingsReportingPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.BookingsReporting;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
