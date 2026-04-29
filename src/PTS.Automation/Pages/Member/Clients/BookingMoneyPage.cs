using Microsoft.Playwright;
using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// Member → Booking Money (<c>Client/Money</c>). View: <c>Views/Client/Money.cshtml</c>.
/// </summary>
public sealed class BookingMoneyPage : MemberPage
{
    public BookingMoneyPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.BookingMoney;

    public override Task GotoAsync() =>
        throw new InvalidOperationException("Use GotoWithBookingAsync(clientNewId, bookingRefId).");

    public async Task GotoWithBookingAsync(string clientNewId, string bookingRefId)
    {
        var path = $"{MemberRoutes.BookingMoney}?Id={Uri.EscapeDataString(clientNewId)}&BookingRefId={Uri.EscapeDataString(bookingRefId)}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

    protected override ILocator ReadinessIndicator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Credits and debits" });

    private ILocator MoneySectionThreeDots => Page.Locator(".booking_money_section .filter_dot #filterCorner");

    private ILocator MoneyTableHeaderRow => Page.Locator("#moneyTable thead tr");

    public Task OpenMoneyThreeDotsMenuAsync() => MoneySectionThreeDots.ClickAsync();

    public async Task<string?> GetExportMoneyPdfHrefAsync()
    {
        var link = Page.Locator(".booking_money_section a.dropdown-item").Filter(new LocatorFilterOptions
        {
            HasText = "Download PDF"
        }).First;
        return await link.GetAttributeAsync("href");
    }

    public async Task<string?> GetExportMoneyExcelHrefAsync()
    {
        var link = Page.Locator(".booking_money_section a.dropdown-item").Filter(new LocatorFilterOptions
        {
            HasText = "Download Excel"
        }).First;
        return await link.GetAttributeAsync("href");
    }

    /// <summary>Asserts the Passenger column exists on the money grid.</summary>
    public Task<bool> IsPassengerColumnHeaderVisibleAsync() =>
        MoneyTableHeaderRow.GetByText("Passenger", new() { Exact = false }).IsVisibleAsync();

    private ILocator MoneyDataRows => Page.Locator("#moneyTable > tbody > tr");

    public Task<int> MoneyGridDataRowCountAsync() => MoneyDataRows.CountAsync();

    private ILocator AddRefundButton => Page.Locator("#AddRefundPopUpButton");

    private ILocator AddRefundModal => Page.Locator("#addRefund");

    /// <summary>Transaction type is the 3rd column (index 2) in <c>#moneyTable</c>.</summary>
    public async Task<ILocator?> FirstDataRowWhereTransactionTypeContainsAsync(string substring)
    {
        var n = await MoneyDataRows.CountAsync();
        for (var i = 0; i < n; i++)
        {
            var row = MoneyDataRows.Nth(i);
            var typeText = (await row.Locator("td").Nth(2).InnerTextAsync()).Trim();
            if (typeText.Contains(substring, StringComparison.OrdinalIgnoreCase))
                return row;
        }
        return null;
    }

    /// <summary>First row whose transaction type mentions Bank but not Batch (e.g. bank credit vs batch credit).</summary>
    public async Task<ILocator?> FirstDataRowWhereBankCreditNotBatchAsync()
    {
        var n = await MoneyDataRows.CountAsync();
        for (var i = 0; i < n; i++)
        {
            var row      = MoneyDataRows.Nth(i);
            var typeText = (await row.Locator("td").Nth(2).InnerTextAsync()).Trim();
            if (typeText.Contains("bank", StringComparison.OrdinalIgnoreCase)
                && !typeText.Contains("batch", StringComparison.OrdinalIgnoreCase))
                return row;
        }
        return null;
    }

    public async Task<bool> IsAddRefundButtonEnabledAsync()
    {
        if (!await AddRefundButton.IsVisibleAsync()) return false;
        var cls = await AddRefundButton.GetAttributeAsync("class");
        return cls == null || !cls.Contains("disabled", StringComparison.OrdinalIgnoreCase);
    }

    public Task OpenAddRefundModalAsync() => AddRefundButton.ClickAsync();

    public Task<bool> IsAddRefundModalVisibleAsync() => Page.Locator("#addRefund.show").IsVisibleAsync();

    public Task CloseAddRefundModalAsync() => AddRefundModal.Locator(".btn-close").First.ClickAsync();

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
}

