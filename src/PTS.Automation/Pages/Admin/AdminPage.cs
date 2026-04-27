using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Pages.Admin;

/// <summary>
/// Base for authenticated Admin (<c>_AdminLayout.cshtml</c>) pages. Adds shell
/// helpers that exist on every admin view.
/// </summary>
public abstract class AdminPage : BasePage
{
    protected AdminPage(IPage page, AppUrl app) : base(page, app)
    {
        ProfileMenu = new AdminProfileMenu(page, app);
    }

    public AdminProfileMenu ProfileMenu { get; }
}
