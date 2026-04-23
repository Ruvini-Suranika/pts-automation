using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Accounts → Profit claims. Profit claim workflow landing page.
/// Controller action: <c>TravelMemberAccountController.ProfitClaims</c>.
///
/// SKELETON.
/// </summary>
public sealed class ProfitClaimsPage : MemberPage
{
    public ProfitClaimsPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.ProfitClaims;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
