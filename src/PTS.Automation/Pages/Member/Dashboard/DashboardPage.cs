using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Dashboard;

/// <summary>
/// Member-portal landing page after successful login. The member logo in the
/// top bar points here, and <c>AccountController.LoginCheck</c> redirects
/// Member-role users to this route.
///
/// Source: <c>Views/Member/Index.cshtml</c>, Controller:
/// <c>MemberController.Index(bool? justLoggedIn)</c>. The page uses the
/// <c>_TravelMemberLayout</c> shell so <see cref="MemberPage"/>'s nav /
/// profile / settings helpers are available.
/// </summary>
public sealed class DashboardPage : MemberPage
{
    public DashboardPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Dashboard;

    // ── Dashboard-specific locators ────────────────────────────────────
    // The "enquiries head" section is unique to the dashboard and renders
    // after all server + JS data is bound — a good readiness probe.
    private ILocator EnquiriesHead     => Page.Locator("section.mm-enquiries-head").First;
    private ILocator DebitNoteSection  => Page.Locator("section#debit_note_section").First;
    private ILocator SelectedBranchBtn => Page.Locator("button.dashboard-dropdown").First;
    private ILocator SelectedWeekLabel => Page.Locator("#startDate").First;
    private ILocator PageTitle         => Page.Locator("title#BrowserTitle");

    protected override ILocator ReadinessIndicator => EnquiriesHead;

    // ── Queries ────────────────────────────────────────────────────────
    public async Task<string> GetBrowserTitleAsync() =>
        (await PageTitle.InnerTextAsync()).Trim();

    public Task<bool> IsDebitNoteBannerVisibleAsync() => DebitNoteSection.IsVisibleAsync();
    public Task<bool> IsEnquiriesHeadVisibleAsync()   => EnquiriesHead.IsVisibleAsync();

    public override async Task WaitForReadyAsync()
    {
        await base.WaitForReadyAsync();
        await MemberShellOverlays.DismissCalendarNotesModalIfBlockingAsync(Page);
    }

    public async Task<string> GetSelectedWeekRangeAsync() =>
        (await SelectedWeekLabel.InnerTextAsync()).Trim();
}
