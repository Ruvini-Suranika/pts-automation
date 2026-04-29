using Microsoft.Playwright;

using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;
using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>P1 critical coverage: Member → Accounts → Profit claims.</summary>
[TestFixture]
[AllureSuite(Categories.Member)]
[AllureFeature("Profit claims")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag(Categories.Member)]
[AllureTag("Accounts.ProfitClaims")]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category("Accounts.ProfitClaims")]
public sealed class ProfitClaimsP1Tests : MemberTest
{
    private async Task GotoProfitClaimsOrIgnoreIfUnauthorizedAsync(ProfitClaimsPage profit)
    {
        var url = new Uri(Settings.Applications.Member.BaseUri, MemberRoutes.ProfitClaims.TrimStart('/')).ToString();
        await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (Page.Url.Contains("Unauthorised", StringComparison.OrdinalIgnoreCase)
            || Page.Url.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
            Assert.Ignore("Member user does not have the 'Claiming Profit' claim — Profit Claims is not available.");
        await profit.WaitForReadyAsync();
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-869 — Booking Name Link on Available Remittance Profit Claims.")]
    public async Task DEV_TC_869_Booking_name_link_available_remittance()
    {
        var profit = new ProfitClaimsPage(Page, Settings.Applications.Member);

        await StepAsync("Open Profit claims", () => GotoProfitClaimsOrIgnoreIfUnauthorizedAsync(profit));

        if (await profit.AvailableBookingLinkRowCountAsync() == 0)
            Assert.Ignore("No available remittance rows with a booking link in QA for this member.");

        var href = await StepAsync("Read first available booking link href",
            () => profit.FirstAvailableBookingHrefAsync());

        await StepAsync("Assert booking link", async () =>
        {
            Assert.That(href, Is.Not.Null.And.Not.Empty);
            Assert.That(href, Does.Contain("/Client/BookingDetails").IgnoreCase);
            Assert.That(href, Does.Contain("BookingRefId=").IgnoreCase);
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-870 — Booking Name Link on Pending Remittance Profit Claims.")]
    public async Task DEV_TC_870_Booking_name_link_pending_remittance()
    {
        var profit = new ProfitClaimsPage(Page, Settings.Applications.Member);

        await StepAsync("Open Profit claims", () => GotoProfitClaimsOrIgnoreIfUnauthorizedAsync(profit));

        if (await profit.PendingBookingLinkRowCountAsync() == 0)
            Assert.Ignore("No pending remittance rows with a booking link in QA for this member.");

        var href = await StepAsync("Read first pending booking link href",
            () => profit.FirstPendingBookingHrefAsync());

        await StepAsync("Assert booking link", async () =>
        {
            Assert.That(href, Is.Not.Null.And.Not.Empty);
            Assert.That(href, Does.Contain("/Client/BookingDetails").IgnoreCase);
            Assert.That(href, Does.Contain("BookingRefId=").IgnoreCase);
        });
    }
}
