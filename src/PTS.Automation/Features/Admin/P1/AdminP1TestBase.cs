using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Home;

namespace PTS.Automation.Features.Admin.P1;

/// <summary>Shared setup for Admin <b>P1</b> acceptance tests.</summary>
[Category(Categories.Admin)]
[Category(Categories.P1)]
public abstract class AdminP1TestBase : AdminTest
{
    /// <summary>Lands on Sales/Enquiries so the desktop admin nav (<see cref="Pages.Admin.Shell.AdminNavBar"/>) is available.</summary>
    protected async Task LandOnAdminEnquiriesAsync()
    {
        var home = new AdminEnquiriesPage(Page, Settings.Applications.Admin);
        await home.GotoAsync();
        await home.WaitForReadyAsync();
    }
}
