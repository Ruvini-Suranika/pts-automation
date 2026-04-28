using PTS.Automation.Pages.Member.Shell;
using PTS.Automation.Pages.Shared.UiComponents;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Member → Accounts → Unassigned credits (<c>Financial/Unassigned</c>).
/// View: <c>Views/Financial/Unassigned.cshtml</c>. Client refine: <c>#searchId</c> keyup on <c>#unassignedResults tr</c>.
/// </summary>
public sealed class UnassignedPage : MemberPage
{
    public UnassignedPage(IPage page, AppUrl app) : base(page, app)
    {
        Grid = new PtsTable(page, "#unassignedResults");
    }

    public override string RelativePath => MemberRoutes.Unassigned;

    public PtsTable Grid { get; }

    protected override ILocator ReadinessIndicator => Page.Locator("#tblUnassigned thead");

    private ILocator SearchFilterInput => Page.Locator("#searchId");

    private ILocator AssignToBookingTriggers =>
        Page.Locator("a[data-attr=\"assignToBooking\"][data-bs-target=\"#assignToBooking\"]");

    private ILocator SplitIncomingTriggers =>
        Page.Locator("a[data-attr=\"split\"][data-bs-target=\"#splitIncomingCredit\"]");

    /// <summary>Client-side refine search (keyup on <c>#searchId</c>).</summary>
    public async Task SetRefineSearchAsync(string text)
    {
        await SearchFilterInput.FillAsync(text);
        await SearchFilterInput.DispatchEventAsync("keyup");
    }

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

    public async Task<IReadOnlyList<string>> GetColumnHeadersAsync()
    {
        var cells = Page.Locator("#tblUnassigned thead tr th");
        return (await cells.AllInnerTextsAsync())
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }

    public async Task<int> ColumnIndexOfAsync(string headerText)
    {
        var headers = await Page.Locator("#tblUnassigned thead tr th").AllInnerTextsAsync();
        for (var i = 0; i < headers.Count; i++)
        {
            if (string.Equals(headers[i].Trim(), headerText, StringComparison.OrdinalIgnoreCase))
                return i + 1;
        }
        return -1;
    }
}
