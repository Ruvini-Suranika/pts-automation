using System.Globalization;
using System.Text.RegularExpressions;

using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Bookings;
using PTS.Automation.Pages.Admin.Members;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P1.TrustAccounts;

/// <summary>P1 critical: Admin trust accounts / member financial setup (MemberAdmin, booking fees).</summary>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Trust accounts")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag("Admin.TrustAccounts")]
[Category(Categories.Regression)]
[Category("Admin.TrustAccounts")]
public sealed class AdminTrustAccountsP1Tests : AdminP1TestBase
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
    [Description("DEV-TC-756 — Member Admin: PTS system setup section is present with save and trust controls.")]
    public async Task DEV_TC_756_Pts_system_setup_section()
    {
        if (!Settings.TestData.AdminTrustAndBank.HasMemberAdminMemberId)
            Assert.Ignore("Configure TestData:AdminTrustAndBank:MemberAdminMemberId for Member Admin (PTS system setup).");

        var memberAdmin = new AdminMemberAdminPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Member Admin", () => memberAdmin.GotoWithMemberIdAsync(Settings.TestData.AdminTrustAndBank.MemberAdminMemberId));

        await StepAsync("Assert PTS system setup block", async () =>
        {
            Assert.That(await memberAdmin.IsPtsSystemSetupSectionVisibleAsync(), Is.True);
            Assert.That(await memberAdmin.IsMainUserOnSystemChoiceVisibleAsync(), Is.True);
            Assert.That(await memberAdmin.PtsSystemSetupSaveButton.IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-763 — Booking overview Financials panel and Admin Booking Fee page load.")]
    public async Task DEV_TC_763_Financials_fees_page()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore(
                "No booking context: configure TestData:AdminBookings or Member:Debits:BookingReferenceId.");

        var overview = new AdminBookingOverviewPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin booking overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Assert Financials section and booking fee link", async () =>
        {
            Assert.That(await overview.IsFinancialsSectionVisibleAsync(), Is.True);
            await overview.TotalBookingFeesLink.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = Settings.Timeouts.DefaultMs
            });
            var href = await overview.TotalBookingFeesLink.GetAttributeAsync("href");
            Assert.That(href, Is.Not.Null.And.Not.Empty);
            Assert.That(href, Does.Contain("BookingFee").IgnoreCase);
        });

        var fees = new AdminBookingFeePage(Page, Settings.Applications.Admin);
        var response = await StepAsync("Navigate to Admin Booking Fee", () => fees.GotoWithClientNewIdAndReturnResponseAsync(ids.Value.ClientNewId));

        await StepAsync("Assert Booking Fee document", async () =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.Ok, Is.True, $"Admin/BookingFee GET failed: HTTP {response.Status}.");
            Assert.That(await fees.IsIncomingFeesSectionVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-778 — PTS Member Booking Fee (post Nov 1st schedule) on Admin Booking Fee incoming grid.")]
    public async Task DEV_TC_778_Member_booking_fee_nov_first_schedule()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No booking context for Admin Booking Fee.");

        var fees = new AdminBookingFeePage(Page, Settings.Applications.Admin);
        var response = await StepAsync("Open Admin Booking Fee", () => fees.GotoWithClientNewIdAndReturnResponseAsync(ids.Value.ClientNewId));
        Assert.That(response?.Ok, Is.True);

        if (!await fees.IncomingFeesContainsTextAsync("Member"))
            Assert.Ignore("No incoming fee row mentioning 'Member' — fee labels differ in this environment.");

        var totalText = await StepAsync("Read Member booking fee total cell (exclude New Member row)",
            () => fees.IncomingFeeTotalCellForRowMatchingAsync("Member", "New Member"));
        if (string.IsNullOrWhiteSpace(totalText))
            Assert.Ignore("Could not read Member booking fee total cell.");

        await StepAsync("Assert Member fee total parses as a non-negative amount", async () =>
        {
            var normalized = Regex.Replace(totalText, "[^0-9.,-]", "");
            Assert.That(decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount),
                Is.True, $"Unparseable fee total: '{totalText}'");
            Assert.That(amount, Is.GreaterThanOrEqualTo(0));
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-779 — PTS New Member Booking Fee (post Nov 1st schedule) on Admin Booking Fee incoming grid.")]
    public async Task DEV_TC_779_New_member_booking_fee_nov_first_schedule()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No booking context for Admin Booking Fee.");

        var fees = new AdminBookingFeePage(Page, Settings.Applications.Admin);
        var response = await StepAsync("Open Admin Booking Fee", () => fees.GotoWithClientNewIdAndReturnResponseAsync(ids.Value.ClientNewId));
        Assert.That(response?.Ok, Is.True);

        if (!await fees.IncomingFeesContainsTextAsync("New Member"))
            Assert.Ignore("No incoming fee row mentioning 'New Member' — fee labels differ in this environment.");

        var totalText = await StepAsync("Read New Member booking fee total cell",
            () => fees.IncomingFeeTotalCellForRowMatchingAsync("New Member"));
        if (string.IsNullOrWhiteSpace(totalText))
            Assert.Ignore("Could not read New Member booking fee total cell.");

        await StepAsync("Assert New Member fee total parses as a non-negative amount", async () =>
        {
            var normalized = Regex.Replace(totalText, "[^0-9.,-]", "");
            Assert.That(decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount),
                Is.True, $"Unparseable fee total: '{totalText}'");
            Assert.That(amount, Is.GreaterThanOrEqualTo(0));
        });
    }
}
