using Microsoft.Playwright;

using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;
using PTS.Automation.Pages.Member.Dashboard;
using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>P1 critical coverage: Member → Accounts → Unassigned credits.</summary>
[TestFixture]
[AllureSuite(Categories.Member)]
[AllureFeature("Unassigned credits")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag(Categories.Member)]
[AllureTag("Accounts.Unassigned")]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category("Accounts.Unassigned")]
public sealed class UnassignedP1Tests : MemberTest
{
    private async Task GotoUnassignedAsync(UnassignedPage page)
    {
        var url = new Uri(Settings.Applications.Member.BaseUri, MemberRoutes.Unassigned.TrimStart('/')).ToString();
        await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForReadyAsync();
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-758 — Verify Unassigned Credits (page structure and key columns).")]
    public async Task DEV_TC_758_Verify_unassigned_credits()
    {
        var page = new UnassignedPage(Page, Settings.Applications.Member);

        await StepAsync("Open Unassigned credits", () => GotoUnassignedAsync(page));

        var headers = await StepAsync("Read grid headers", () => page.GetColumnHeadersAsync());
        Logger.Information("Headers: {Headers}", string.Join(" | ", headers));

        await StepAsync("Assert core columns", async () =>
        {
            Assert.That(await page.ColumnIndexOfAsync("transaction ref"), Is.GreaterThan(0));
            Assert.That(await page.ColumnIndexOfAsync("actions"), Is.GreaterThan(0));
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-585 — Assign to booking and Split under Actions column (modals open).")]
    public async Task DEV_TC_585_Assign_to_booking_and_split_actions()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new UnassignedPage(Page, Settings.Applications.Member);

        await StepAsync("Navigate via Accounts menu to Unassigned", async () =>
        {
            await dashboard.GotoAsync();
            await dashboard.NavBar.GoToUnassignedAsync();
            await page.WaitForReadyAsync();
        });

        if (await page.AssignToBookingTriggerCountAsync() == 0)
            Assert.Ignore("No unassigned rows with an Assign to booking action in QA for this member.");

        await StepAsync("Open Assign to booking modal", () => page.OpenFirstAssignToBookingModalAsync());
        await Page.WaitForTimeoutAsync(300);

        await StepAsync("Assert Assign to booking modal is visible", async () =>
        {
            Assert.That(await page.IsAssignToBookingModalVisibleAsync(), Is.True,
                "Assign to booking modal (#assignToBooking) should be visible.");
        });

        await StepAsync("Close Assign to booking modal", () => Page.Keyboard.PressAsync("Escape"));
        await Page.WaitForTimeoutAsync(300);

        if (await page.SplitTriggerCountAsync() == 0)
            Assert.Ignore("No unassigned rows with a Split action in QA for this member.");

        await StepAsync("Open Split incoming credit modal", () => page.OpenFirstSplitModalAsync());
        await Page.WaitForTimeoutAsync(300);

        await StepAsync("Assert Split modal is visible", async () =>
        {
            Assert.That(await page.IsSplitIncomingModalVisibleAsync(), Is.True,
                "Split incoming credit modal (#splitIncomingCredit) should be visible.");
        });

        await StepAsync("Close Split modal", () => Page.Keyboard.PressAsync("Escape"));
    }
}
