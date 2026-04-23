using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Suppliers;

/// <summary>
/// Suppliers → Accommodation search landing page.
/// Controller action: <c>AccommodationController.SearchAccommodation</c>.
///
/// SKELETON.
/// </summary>
public sealed class AccommodationSearchPage : MemberPage
{
    public AccommodationSearchPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Accommodation;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
