using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Member → Accounts → Unclaimed (<c>Financial/GetAllUnclaimed</c>).
/// View: <c>Views/Financial/Unclaimed.cshtml</c>. Refine: <c>#refineUnclaimed</c> input on <c>tbody#unclaimedResults tr</c>.
/// </summary>
public sealed class UnclaimedPage : MemberPage
{
    public UnclaimedPage(IPage page, AppUrl app) : base(page, app)
    {
        // Rows live inside repeated <tbody id="unclaimedResults"> — scope rows, not a single tbody id.
        DataRows = Page.Locator("tbody#unclaimedResults tr");
    }

    public override string RelativePath => MemberRoutes.Unclaimed;

    /// <summary>All data rows across every <c>tbody#unclaimedResults</c> block.</summary>
    public ILocator DataRows { get; }

    protected override ILocator ReadinessIndicator => Page.Locator("#refineUnclaimed");

    private ILocator RefineSearchInput => Page.Locator("#refineUnclaimed");

    private ILocator AssignToBookingTriggers =>
        Page.Locator("a[data-attr=\"assignToBooking\"]");

    private ILocator SplitIncomingTriggers =>
        Page.Locator("a[data-attr=\"split\"][data-bs-target=\"#splitIncomingCredit\"]");

    /// <summary>Client-side refine (<c>input</c> event per <c>unclaimed.js</c>).</summary>
    public async Task SetRefineSearchAsync(string text)
    {
        await RefineSearchInput.FillAsync(text);
        await RefineSearchInput.DispatchEventAsync("input");
    }

    public Task<int> DataRowCountAsync() => DataRows.CountAsync();

    /// <summary>Visible credit rows (excludes the client-side <c>#emptyRows</c> placeholder).</summary>
    public Task<int> VisibleDataRowCountAsync() =>
        Page.Locator("tbody#unclaimedResults tr:not(#emptyRows):visible").CountAsync();

    public Task<int> AssignToBookingTriggerCountAsync() => AssignToBookingTriggers.CountAsync();
    public Task<int> SplitTriggerCountAsync()           => SplitIncomingTriggers.CountAsync();

    public Task OpenFirstAssignToBookingModalAsync() =>
        AssignToBookingTriggers.First.ClickAsync(new LocatorClickOptions { Force = true });

    public Task OpenFirstSplitModalAsync() =>
        SplitIncomingTriggers.First.ClickAsync(new LocatorClickOptions { Force = true });

    public Task<bool> IsAssignToBookingModalVisibleAsync() =>
        Page.Locator("#assignToBooking.show").IsVisibleAsync();

    public Task<bool> IsSplitIncomingModalVisibleAsync() =>
        Page.Locator("#splitIncomingCredit.show").IsVisibleAsync();
}
