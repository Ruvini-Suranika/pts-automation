using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Accounts → Unclaimed. Lists credits that have not yet been claimed.
/// Controller action: <c>FinancialController.GetAllUnclaimed</c>.
///
/// SKELETON.
/// </summary>
public sealed class UnclaimedPage : MemberPage
{
    public UnclaimedPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Unclaimed;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
