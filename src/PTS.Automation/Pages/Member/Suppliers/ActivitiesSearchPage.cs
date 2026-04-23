using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Suppliers;

/// <summary>
/// Suppliers → Activities search page.
/// Controller action: <c>ActivitiesController.ActivitySearch</c>.
///
/// SKELETON.
/// </summary>
public sealed class ActivitiesSearchPage : MemberPage
{
    public ActivitiesSearchPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Activities;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
