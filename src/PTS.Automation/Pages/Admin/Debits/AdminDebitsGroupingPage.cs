using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.Debits;

/// <summary>
/// Admin → Debits → Debits grouping. Source: <c>Views/Admin/DebitsGrouping.cshtml</c>.
/// </summary>
public sealed class AdminDebitsGroupingPage : AdminPage
{
    public AdminDebitsGroupingPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.DebitsGrouping;

    private ILocator GroupButton => Page.Locator("#btnGroup");

    protected override ILocator ReadinessIndicator => GroupButton;

    /// <summary>Pay action links share <c>title="Pay"</c> (duplicate <c>id</c> in markup — never use CSS id).</summary>
    public ILocator PayLinks => Page.Locator("a.ForPaymentPauseStatus[title='Pay']");

    /// <summary>Currency action links (<c>title="Currency"</c>).</summary>
    public ILocator CurrencyLinks => Page.Locator("a[title='Currency']");

    public Task<bool> IsGroupingTableVisibleAsync() => Page.Locator("table#debitgrouping").IsVisibleAsync();
}
