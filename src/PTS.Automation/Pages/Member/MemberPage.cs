using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member;

/// <summary>
/// Base class for every authenticated Member-portal page. Wraps
/// <see cref="BasePage"/> with the shell components (<see cref="NavBar"/>,
/// <see cref="ProfileMenu"/>, <see cref="SettingsMegaMenu"/>) that are
/// present on every page by virtue of <c>_TravelMemberLayout.cshtml</c>.
///
/// Concrete pages derive from this instead of <see cref="BasePage"/> when
/// they are only reachable by an authenticated Member.
/// </summary>
public abstract class MemberPage : BasePage
{
    protected MemberPage(IPage page, AppUrl app) : base(page, app)
    {
        NavBar           = new MemberNavBar(page, app);
        ProfileMenu      = new MemberProfileMenu(page, app);
        SettingsMegaMenu = new MemberSettingsMegaMenu(page, app);
    }

    /// <summary>Top-level navigation (Clients / Calendar / Suppliers / Accounts / Reporting).</summary>
    public MemberNavBar NavBar { get; }

    /// <summary>User profile dropdown in the top-right (View Profile / Logout).</summary>
    public MemberProfileMenu ProfileMenu { get; }

    /// <summary>Gear-icon settings mega-menu (Contracted suppliers / Users / Email / …).</summary>
    public MemberSettingsMegaMenu SettingsMegaMenu { get; }
}
