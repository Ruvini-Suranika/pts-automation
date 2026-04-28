using Microsoft.Playwright;
using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Member → Accounts → Profit claims (<c>TravelMemberAccount/ProfitClaims</c>).
/// View: <c>Views/TravelMemberAccount/ProfitClaims.cshtml</c>.
/// </summary>
public sealed class ProfitClaimsPage : MemberPage
{
    public ProfitClaimsPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.ProfitClaims;

    protected override ILocator ReadinessIndicator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Available remittances" });

    private ILocator AccountsSection => Page.Locator("section.your_profile.accountsPage");

    /// <summary>First profit table block ("Available remittances").</summary>
    private ILocator AvailableSection => AccountsSection.Locator(".accProfit").First;

    /// <summary>Second profit table block ("Pending remittances").</summary>
    private ILocator PendingSection => AccountsSection.Locator(".accProfit").Nth(1);

    /// <summary>First data row in Available remittances that exposes a booking name link.</summary>
    private ILocator FirstAvailableBookingNameLink =>
        AvailableSection.Locator("tbody tr:has(a[href*=\"BookingDetails\"])").First
            .Locator("a[href*=\"BookingDetails\"]").First;

    /// <summary>First data row in Pending remittances that exposes a booking name link.</summary>
    private ILocator FirstPendingBookingNameLink =>
        PendingSection.Locator("tbody tr:has(a[href*=\"BookingDetails\"])").First
            .Locator("a[href*=\"BookingDetails\"]").First;

    public Task<int> AvailableBookingLinkRowCountAsync() =>
        AvailableSection.Locator("tbody tr:has(a[href*=\"BookingDetails\"])").CountAsync();

    public Task<int> PendingBookingLinkRowCountAsync() =>
        PendingSection.Locator("tbody tr:has(a[href*=\"BookingDetails\"])").CountAsync();

    public async Task<string?> FirstAvailableBookingHrefAsync()
    {
        if (await AvailableBookingLinkRowCountAsync() == 0) return null;
        return await FirstAvailableBookingNameLink.GetAttributeAsync("href");
    }

    public async Task<string?> FirstPendingBookingHrefAsync()
    {
        if (await PendingBookingLinkRowCountAsync() == 0) return null;
        return await FirstPendingBookingNameLink.GetAttributeAsync("href");
    }

    /// <summary>First “Available remittances” block (see <c>ProfitClaims.cshtml</c>).</summary>
    private ILocator AvailableRemittancesBlock =>
        Page.Locator("section.your_profile.accountsPage div.accProfit").First;

    /// <summary>
    /// Rows where the claim checkbox is disabled and the Claim action is visually disabled —
    /// e.g. when <c>HasPendingPayments &amp;&amp; HasPendingCredits</c> is false (cannot claim yet).
    /// </summary>
    public Task<int> AvailableRemittancesDisabledClaimRowCountAsync() =>
        AvailableRemittancesBlock.Locator("tbody tr:has(input.claim[disabled]):has(a[name='Claim'].disabled)").CountAsync();
}
