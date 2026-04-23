using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Settings;

/// <summary>
/// Settings mega-menu → Users. Manage the member's internal users.
/// Controller action: <c>MemberController.Users</c>.
///
/// SKELETON.
/// </summary>
public sealed class UsersPage : MemberPage
{
    public UsersPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Users;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
