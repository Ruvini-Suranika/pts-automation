using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// Member → Issue tickets page. Lists tickets that have been issued.
/// Source view: <c>Views/Member/IssueTicket.cshtml</c>.
/// Controller action: <c>MemberController.IssueTicket</c>.
///
/// SKELETON.
/// </summary>
public sealed class IssueTicketsPage : MemberPage
{
    public IssueTicketsPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.IssueTickets;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
