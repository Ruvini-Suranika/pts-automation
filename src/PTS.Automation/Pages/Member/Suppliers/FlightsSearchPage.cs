using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Suppliers;

/// <summary>
/// Suppliers → Flights search page.
/// Controller action: <c>FlightController.Flights</c>.
///
/// SKELETON.
/// </summary>
public sealed class FlightsSearchPage : MemberPage
{
    public FlightsSearchPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Flights;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
