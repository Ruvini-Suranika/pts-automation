using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Pages.Admin.Clients;

/// <summary>Admin → Client profile (<c>Admin/ClientDetails</c>). View: <c>Views/Admin/ClientDetails.cshtml</c>.</summary>
public sealed class AdminClientDetailsPage : AdminPage
{
    public AdminClientDetailsPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.ClientDetails;

    public override Task GotoAsync() =>
        throw new InvalidOperationException("Use GotoWithClientNewIdAsync.");

    protected override ILocator ReadinessIndicator =>
        Page.Locator("section.page_back.clientPageBack");

    public async Task GotoWithClientNewIdAsync(string clientNewId)
    {
        var path = $"{AdminRoutes.ClientDetails}/{Uri.EscapeDataString(clientNewId)}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
        await WaitForDetailsPopulatedAsync();
    }

    /// <summary>Waits for travel client basic detail binding (<c>getMemberDetails</c> in <c>client-details.js</c>).</summary>
    public async Task WaitForDetailsPopulatedAsync()
    {
        await Page.WaitForFunctionAsync(
            @"() => {
                const el = document.querySelector('.clientList .spanFirstName');
                return el && (el.textContent || '').trim().length > 0;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = Settings.Timeouts.NavigationMs });
        await Spinner.WaitUntilHiddenAsync();
    }

    public Task<bool> IsDetailsSectionVisibleAsync() =>
        Page.Locator("section.clientList").GetByText("Details", new() { Exact = false }).First.IsVisibleAsync();

    public Task<string> DisplayedFirstNameAsync() =>
        Page.Locator("section.clientList .spanFirstName").First.InnerTextAsync();

    public Task<string> DisplayedEmailAsync() => Page.Locator("#spanEmail").InnerTextAsync();

    public Task<bool> IsBookingsSectionVisibleAsync() =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Bookings" }).IsVisibleAsync();

    public Task OpenEditClientModalAsync() =>
        Page.Locator("#profileContainer pts-button[type='edit']").ClickAsync();

    public Task<bool> IsEditClientModalVisibleAsync() =>
        Page.Locator("#editClient.show").IsVisibleAsync();

    public Task CloseEditClientModalViaCancelAsync() =>
        Page.Locator("#editClient").Locator("pts-button[type='cancel']").ClickAsync();

    public Task<string> EditClientFirstNameInputValueAsync() =>
        Page.Locator("#editClientFirstName").InputValueAsync();

    public Task OpenAddNewBookingModalAsync() =>
        Page.Locator("section.clientList a").Filter(new() { HasText = "Add New Booking" }).ClickAsync();

    public Task<bool> IsAddBookingModalVisibleAsync() =>
        Page.Locator("#Addnewbooking.show").IsVisibleAsync();

    public Task CloseAddBookingModalViaCancelAsync() =>
        Page.Locator("#Addnewbooking").Locator("pts-button[type='cancel']").ClickAsync();

    public Task<bool> IsAddBookingDestinationFieldVisibleAsync() =>
        Page.Locator("#addBookingDestination").IsVisibleAsync();
}
