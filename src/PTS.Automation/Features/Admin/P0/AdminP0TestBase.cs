using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Home;

namespace PTS.Automation.Features.Admin.P0;

/// <summary>
/// Shared setup for Admin <b>P0</b> acceptance tests (<c>docs/ADMIN-TEST-CASE-PRIORITY.md</c>).
/// Concrete fixtures inherit <see cref="AdminTest"/> via this type and reuse the same landing step.
/// </summary>
[Category(Categories.Admin)]
[Category(Categories.P0)]
public abstract class AdminP0TestBase : AdminTest
{
    /// <summary>Lands on Sales/Enquiries so the desktop admin nav (<see cref="Pages.Admin.Shell.AdminNavBar"/>) is available.</summary>
    protected async Task LandOnAdminEnquiriesAsync()
    {
        var home = new AdminEnquiriesPage(Page, Settings.Applications.Admin);
        await home.GotoAsync();
        await home.WaitForReadyAsync();
    }
}
