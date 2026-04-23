using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// Member → Clients landing page (the searchable clients grid).
/// Source view: <c>Views/Client/ClientSearchView.cshtml</c>.
/// Controller action: <c>ClientController.ClientSearchView</c>.
///
/// SKELETON: URL and readiness only. Locators and actions will be added in
/// the scripting phase when the first test exercises this page.
/// </summary>
public sealed class ClientListPage : MemberPage
{
    public ClientListPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Clients;

    // TODO(scripting): replace with a locator unique to the Clients grid
    // (e.g. the "Add Client" button or the #clientsTableBody element).
    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
