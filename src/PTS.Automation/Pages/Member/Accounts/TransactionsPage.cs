using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Accounts → Transactions. Client-level transactions listing.
/// Controller action: <c>ClientController.Transactions</c>.
///
/// SKELETON.
/// </summary>
public sealed class TransactionsPage : MemberPage
{
    public TransactionsPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Transactions;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
