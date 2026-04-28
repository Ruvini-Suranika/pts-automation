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

    /// <summary>Pay action links — Bootstrap tooltips may move <c>title</c> to <c>data-bs-original-title</c>.</summary>
    public ILocator PayLinks => Page.Locator(
        "#debitgrouping tbody a.ForPaymentPauseStatus[title='Pay'], #debitgrouping tbody a.ForPaymentPauseStatus[data-bs-original-title='Pay']");

    /// <summary>Currency action links (same tooltip caveat as <see cref="PayLinks"/>).</summary>
    public ILocator CurrencyLinks => Page.Locator(
        "#debitgrouping tbody a[title='Currency'], #debitgrouping tbody a[data-bs-original-title='Currency']");

    public Task<bool> IsGroupingTableVisibleAsync() => Page.Locator("table#debitgrouping").IsVisibleAsync();
}
