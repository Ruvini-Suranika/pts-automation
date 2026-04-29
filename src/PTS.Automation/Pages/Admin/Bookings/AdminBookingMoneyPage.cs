using Microsoft.Playwright;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Pages.Admin.Bookings;

/// <summary>Admin → Money (<c>Admin/Money</c>). Refunds live in <c>Views/Admin/Money.cshtml</c>.</summary>
public sealed class AdminBookingMoneyPage : AdminPage
{
    public AdminBookingMoneyPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.Money;

    public override Task GotoAsync() =>
        throw new InvalidOperationException("Use GotoWithBookingAsync(clientNewId, bookingRefId).");

    protected override ILocator ReadinessIndicator =>
        Page.GetByText("Credits and debits", new() { Exact = true });

    public async Task GotoWithBookingAsync(string clientNewId, string bookingRefId)
    {
        var path =
            $"{AdminRoutes.Money}?Id={Uri.EscapeDataString(clientNewId)}&BookingReferenceId={Uri.EscapeDataString(bookingRefId)}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

    public Task<bool> IsCreditsAndDebitsHeadingVisibleAsync() =>
        Page.GetByText("Credits and debits", new() { Exact = true }).IsVisibleAsync();

    public Task<bool> IsRefundsHeadingVisibleAsync() =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Refunds" }).IsVisibleAsync();

    public ILocator AddRefundButton => Page.Locator("#AddRefundPopUpButton");

    public ILocator AddRefundModal => Page.Locator("#addRefund");

    public Task OpenAddRefundModalAsync() => AddRefundButton.ClickAsync();

    public Task<bool> IsAddRefundModalVisibleAsync() => Page.Locator("#addRefund.show").IsVisibleAsync();

    public Task CloseAddRefundModalViaCancelAsync() =>
        AddRefundModal.Locator("pts-button[type='cancel']").ClickAsync();

    /// <summary>Selectpicker-backed <c>#txtRefundType</c> — value <c>14</c> = Bank Refund.</summary>
    public Task SelectRefundTypeValueAsync(string optionValue) =>
        Page.EvaluateAsync(
            @"(v) => {
                const sel = document.querySelector('#txtRefundType');
                if (!sel) return;
                const jq = window['$'];
                if (jq && typeof jq(sel).selectpicker === 'function') {
                    jq(sel).selectpicker('val', v);
                } else {
                    sel.value = v;
                }
                sel.dispatchEvent(new Event('change', { bubbles: true }));
            }",
            optionValue);

    public ILocator RefundEditLinks =>
        Page.Locator("a.trust_account[onclick*='editRefund']");

    public Task<int> RefundEditLinkCountAsync() => RefundEditLinks.CountAsync();

    public Task OpenFirstRefundEditModalAsync() => RefundEditLinks.First.ClickAsync();

    public Task<string> AddRefundModalHeaderTextAsync() => Page.Locator("#addmodalHeader").InnerTextAsync();

    public Task<string> UpdateRefundButtonTextAsync() => Page.Locator("#UpdateRefund").InnerTextAsync();

    public Task FillRefundAccountNameAsync(string value) => Page.Locator("#txtAccountName").FillAsync(value);

    public Task FillRefundAccountNumberAsync(string value) => Page.Locator("#txtAccountNo").FillAsync(value);

    public Task FillRefundSortCodeAsync(string value) => Page.Locator("#txtSortCode").FillAsync(value);

    public Task FillRefundValueAsync(string value) => Page.Locator("#txtValue").FillAsync(value);
}
