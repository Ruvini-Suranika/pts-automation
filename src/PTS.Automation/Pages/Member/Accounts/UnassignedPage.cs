using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Accounts → Unassigned. Credits that have not been assigned to a booking.
/// Controller action: <c>FinancialController.Unassigned</c>.
///
/// SKELETON.
/// </summary>
public sealed class UnassignedPage : MemberPage
{
    public UnassignedPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Unassigned;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
