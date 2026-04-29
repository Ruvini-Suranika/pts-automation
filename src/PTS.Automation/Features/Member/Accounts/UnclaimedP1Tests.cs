using Microsoft.Playwright;

using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;
using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>P1 critical coverage: Member → Accounts → Unclaimed credits.</summary>
[TestFixture]
[AllureSuite(Categories.Member)]
[AllureFeature("Unclaimed credits")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag(Categories.Member)]
[AllureTag("Accounts.Unclaimed")]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category("Accounts.Unclaimed")]
public sealed class UnclaimedP1Tests : MemberTest
{
    private async Task GotoUnclaimedOrIgnoreAsync(UnclaimedPage page)
    {
        var url = new Uri(Settings.Applications.Member.BaseUri, MemberRoutes.Unclaimed.TrimStart('/')).ToString();
        await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (Page.Url.Contains("Unauthorised", StringComparison.OrdinalIgnoreCase)
            || Page.Url.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
            Assert.Ignore("Member user does not have the 'Unclaimed Credits' claim — Unclaimed is not available.");
        await page.WaitForReadyAsync();
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-653 — Assign to booking and Split under Actions column (modals open).")]
    public async Task DEV_TC_653_Assign_to_booking_and_split_actions()
    {
        var page = new UnclaimedPage(Page, Settings.Applications.Member);

        await StepAsync("Open Unclaimed", () => GotoUnclaimedOrIgnoreAsync(page));

        if (await page.AssignToBookingTriggerCountAsync() == 0)
            Assert.Ignore("No unclaimed rows with Assign to booking in QA for this member.");

        await StepAsync("Open Assign to booking modal", () => page.OpenFirstAssignToBookingModalAsync());
        await Page.WaitForTimeoutAsync(300);

        await StepAsync("Assert Assign to booking modal", async () =>
        {
            Assert.That(await page.IsAssignToBookingModalVisibleAsync(), Is.True);
        });

        await StepAsync("Close Assign modal", () => Page.Keyboard.PressAsync("Escape"));
        await Page.WaitForTimeoutAsync(300);

        if (await page.SplitTriggerCountAsync() == 0)
            Assert.Ignore("No unclaimed rows with Split in QA for this member.");

        await StepAsync("Open Split modal", () => page.OpenFirstSplitModalAsync());
        await Page.WaitForTimeoutAsync(300);

        await StepAsync("Assert Split modal", async () =>
        {
            Assert.That(await page.IsSplitIncomingModalVisibleAsync(), Is.True);
        });

        await StepAsync("Close Split modal", () => Page.Keyboard.PressAsync("Escape"));
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-757 — System should not allow assigning multiple credits with different currencies to a single booking.")]
    public async Task DEV_TC_757_Assign_rejects_cross_currency_to_booking()
    {
        Assert.Ignore(
            "DEV-TC-757 requires curated QA data (credit vs booking currency mismatch) or a stable way to " +
            "force Financial/UpdateUnclaimed Result=3 past client-side booking autocomplete validation. " +
            "Expected UX: SweetAlert error 'Cannot assign credit as credit currency is different from booking's currency'.");
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-872 — Refine search on Unclaimed page.")]
    public async Task DEV_TC_872_Refine_search_on_unclaimed()
    {
        var page = new UnclaimedPage(Page, Settings.Applications.Member);

        await StepAsync("Open Unclaimed", () => GotoUnclaimedOrIgnoreAsync(page));

        var rowsBefore = await StepAsync("Count unclaimed data rows", () => page.DataRowCountAsync());
        if (rowsBefore == 0)
            Assert.Ignore("No unclaimed rows — refine search has nothing to filter.");

        const string sentinel = "ZZZ_PTS_AUTOMATION_NO_MATCH_ZZZ";
        await StepAsync($"Apply refine '{sentinel}'", () => page.SetRefineSearchAsync(sentinel));
        await Page.WaitForTimeoutAsync(300);

        await StepAsync("Assert refine hides data (empty marker or no visible credit rows)", async () =>
        {
            // unclaimed.js appends tr#emptyRows when nothing matches the refine text.
            var emptyShown = await Page.Locator("tr#emptyRows").IsVisibleAsync();
            var visibleCredits = await page.VisibleDataRowCountAsync();
            Assert.That(emptyShown || visibleCredits == 0,
                $"Expected either the 'No records found' placeholder or zero visible credit rows. emptyRows={emptyShown}, visibleRows={visibleCredits}");
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-673 — Import Bank.csv with incorrect booking reference and split the booking.")]
    public async Task DEV_TC_673_Import_bank_csv_incorrect_booking_ref_and_split()
    {
        Assert.Ignore(
            "DEV-TC-673: No Member Unclaimed (<c>Financial/GetAllUnclaimed</c>) file-upload control for Bank.csv " +
            "was found in the WebUI view; the flow may live on another screen or require manual/seeded data. " +
            "Add a POM entry point when the import UI is identified.");
    }
}
