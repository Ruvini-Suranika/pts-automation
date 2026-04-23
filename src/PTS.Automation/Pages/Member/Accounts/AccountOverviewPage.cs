using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Accounts → Overview. Summary of the member's financial position.
/// Controller action: <c>TravelMemberAccountController.AccountOverview</c>.
///
/// SKELETON.
/// </summary>
public sealed class AccountOverviewPage : MemberPage
{
    public AccountOverviewPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.AccountOverview;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
