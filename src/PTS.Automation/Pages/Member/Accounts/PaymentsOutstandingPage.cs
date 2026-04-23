using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Accounts → Payments (outstanding). Lists debits the member owes suppliers.
/// Controller action: <c>TravelMemberAccountController.PaymentsOutstanding</c>.
///
/// SKELETON.
/// </summary>
public sealed class PaymentsOutstandingPage : MemberPage
{
    public PaymentsOutstandingPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.PaymentsOutstanding;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
