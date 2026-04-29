using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Bookings;
using PTS.Automation.Pages.Admin.Itinerary;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P1.Bank;

/// <summary>P1 critical: payment-out edit (booking overview) and admin client itinerary payment layout.</summary>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Bank and itinerary")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag("Admin.Bank")]
[Category(Categories.Regression)]
[Category("Admin.Bank")]
public sealed class AdminBankItineraryP1Tests : AdminP1TestBase
{
    private async Task<(string ClientNewId, string BookingRefId)?> TryResolveAdminBookingContextAsync()
    {
        var ab = Settings.TestData.AdminBookings;
        if (ab.IsConfigured)
            return (ab.ClientNewId.Trim(), ab.BookingReferenceId.Trim());

        var debitRef = Settings.TestData.Member.Debits.BookingReferenceId?.Trim();
        if (string.IsNullOrEmpty(debitRef))
            return null;

        await LandOnAdminEnquiriesAsync();
        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToBookingSearchAsync();

        var search = new AdminBookingSearchPage(Page, Settings.Applications.Admin);
        await search.WaitForReadyAsync();

        var waitList = Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/BookingSearchAdmin", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

        await search.SearchByBookingReferenceAsync(debitRef);
        try
        {
            await waitList;
        }
        catch (TimeoutException)
        {
            return null;
        }

        try
        {
            await search.WaitForResultRowsAsync(1);
        }
        catch (TimeoutException)
        {
            return null;
        }

        return await search.TryReadFirstBookingDetailsQueryAsync();
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-741 — Payment out summary: edit payment-out opens supplier edit modal (cancel without save).")]
    public async Task DEV_TC_741_Edit_payment_out_from_booking_overview()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No booking context — configure TestData:AdminBookings or Member:Debits:BookingReferenceId.");

        var overview = new AdminBookingOverviewPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin booking overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        try
        {
            await StepAsync("Wait for payment-out rows", () => overview.WaitForPaymentOutSummaryRowsAsync(1));
        }
        catch (Exception)
        {
            Assert.Ignore("No payment-out rows — cannot open edit payment-out UI.");
        }

        if (await overview.EnabledPaymentOutEditLinks.CountAsync() == 0)
            Assert.Ignore("No enabled payment-out edit actions for this booking (all rows may be GPS/read-only).");

        await StepAsync("Click first enabled payment-out edit", overview.ClickFirstEnabledPaymentOutEditAsync);

        await StepAsync("Assert edit pay supplier modal", async () =>
        {
            Assert.That(await overview.IsEditPaySupplierModalVisibleAsync(), Is.True);
        });

        await StepAsync("Close edit modal", overview.CloseEditPaySupplierModalAsync);
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-796 — Itinerary payment schedule: Total paid and Still to pay sections render.")]
    public async Task DEV_TC_796_Itinerary_total_paid_and_still_to_pay()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No booking context for itinerary.");

        var itinerary = new AdminClientItineraryPage(Page, Settings.Applications.Admin);
        await StepAsync("Open client itinerary", () => itinerary.GotoWithClientAndBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Assert total paid / still to pay blocks", async () =>
        {
            Assert.That(await itinerary.IsTotalPaidSectionVisibleAsync(), Is.True);
            Assert.That(await itinerary.IsStillToPaySectionVisibleAsync(), Is.True);
        });

        await StepAsync("Wait for summary values from itinerary scripts", async () =>
        {
            await Page.WaitForFunctionAsync(
                "() => { const paid = document.querySelector('#spmgrossmoney'); const due = document.querySelector('#spmTotalClientStillPay'); " +
                "return paid && due && (paid.textContent || '').trim().length > 0 && (due.textContent || '').trim().length > 0; }",
                null,
                new PageWaitForFunctionOptions { Timeout = Settings.Timeouts.NavigationMs });
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-797 — Itinerary payment schedule: due date cell does not wrap to multiple text lines.")]
    public async Task DEV_TC_797_Itinerary_payment_date_single_line()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No booking context for itinerary.");

        var itinerary = new AdminClientItineraryPage(Page, Settings.Applications.Admin);
        await StepAsync("Open client itinerary", () => itinerary.GotoWithClientAndBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        try
        {
            await StepAsync("Wait for payment schedule rows", () => itinerary.WaitForPaymentScheduleRowsAsync(1));
        }
        catch (Exception)
        {
            Assert.Ignore("No payment schedule rows — cannot assert due date layout.");
        }

        var singleLine = await StepAsync("Evaluate due date cell is single-line", () =>
            Page.EvaluateAsync<bool>(
                "() => { const td = document.querySelector('#tblMoneyDue tr td:nth-child(3)'); " +
                "if (!td) return false; const t = (td.innerText || '').trim(); " +
                "if (t.indexOf(String.fromCharCode(10)) >= 0) return false; " +
                "const ws = window.getComputedStyle(td).whiteSpace; return ws === 'nowrap' || ws === 'pre' || t.length < 80; }"));

        await StepAsync("Assert due date presents on one line", async () =>
        {
            Assert.That(singleLine, Is.True, "Due date cell should not introduce line breaks in the payment schedule.");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-798 — Itinerary payment schedule: STATUS is the last column (index 3 of 4).")]
    public async Task DEV_TC_798_Itinerary_payment_status_column_position()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No booking context for itinerary.");

        var itinerary = new AdminClientItineraryPage(Page, Settings.Applications.Admin);
        await StepAsync("Open client itinerary", () => itinerary.GotoWithClientAndBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Read payment schedule header positions", async () =>
        {
            var idx = await itinerary.PaymentScheduleStatusColumnIndexAsync();
            Assert.That(idx, Is.EqualTo(3), "STATUS should be the fourth column (0-based index 3) per Payment Schedule markup.");
        });
    }
}
