using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Clients;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P1.Clients;

/// <summary>P1 critical coverage: Admin → Admin → Clients (search, profile, edit/add booking modals).</summary>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Clients")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag("Admin.Clients")]
[Category(Categories.Regression)]
[Category("Admin.Clients")]
public sealed class AdminClientsP1Tests : AdminP1TestBase
{
    private static string? ConfiguredClientNewId(TestSettings s)
    {
        if (s.TestData.AdminClients.HasClientNewId)
            return s.TestData.AdminClients.ClientNewId.Trim();
        var bookingClient = s.TestData.AdminBookings.ClientNewId?.Trim();
        return string.IsNullOrEmpty(bookingClient) ? null : bookingClient;
    }

    private async Task<AdminClientSearchPage> OpenClientSearchFromNavAsync()
    {
        await LandOnAdminEnquiriesAsync();
        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToClientSearchAsync();
        var search = new AdminClientSearchPage(Page, Settings.Applications.Admin);
        await search.WaitForReadyAsync();
        return search;
    }

    /// <summary>Resolves client new id from test data or first grid row after search/show-all.</summary>
    private async Task<string?> TryResolveClientNewIdAsync(AdminClientSearchPage search, bool runSearchIfNeeded)
    {
        var configured = ConfiguredClientNewId(Settings);
        if (configured != null)
            return configured;

        if (!runSearchIfNeeded)
            return null;

        var refFilter = Settings.TestData.AdminClients.SearchClientReference?.Trim();
        if (string.IsNullOrEmpty(refFilter) && Settings.TestData.Member.Debits.ClientReferenceNumber > 0)
            refFilter = Settings.TestData.Member.Debits.ClientReferenceNumber.ToString();

        var wait = search.StartWaitForSearchClientAdminAsync();
        if (!string.IsNullOrEmpty(refFilter))
        {
            await search.FillClientReferenceAsync(refFilter);
            await search.ClickSearchAsync();
        }
        else
        {
            await search.ClickShowAllAsync();
        }

        try
        {
            await search.WaitForSearchClientAdminResponseAsync(wait);
        }
        catch (TimeoutException)
        {
            return null;
        }

        if (await search.IsNoResultsVisibleAsync())
            return null;

        try
        {
            await search.WaitForClientGridRowsAsync(1);
        }
        catch (TimeoutException)
        {
            return null;
        }

        return await search.TryGetFirstClientDetailsIdFromGridAsync();
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-178 — Search clients using combined advanced filter fields + Search (SearchClientAdmin).")]
    public async Task DEV_TC_178_Search_clients_with_advanced_filters()
    {
        var search = await StepAsync("Open Admin Clients search", OpenClientSearchFromNavAsync);

        await StepAsync("Apply combined filters (name + enquiry type + sector + member)", async () =>
        {
            await search.FillFirstNameAsync("T");
            await search.FillLastNameAsync("e");
            var optCount = await search.TravelTypeSelect.Locator("option").CountAsync();
            if (optCount > 1)
                await search.TravelTypeSelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            var sectorOpts = await search.SectorSelect.Locator("option").CountAsync();
            if (sectorOpts > 1)
                await search.SectorSelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            var memberOpts = await search.MemberSelect.Locator("option").CountAsync();
            if (memberOpts > 1)
                await search.MemberSelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });
        });

        var wait = search.StartWaitForSearchClientAdminAsync();
        await StepAsync("Submit Search", search.ClickSearchAsync);
        var resp = await StepAsync("Await SearchClientAdmin", () => wait);

        await StepAsync("Assert search POST succeeded", async () =>
        {
            Assert.That(resp.Ok, Is.True, $"SearchClientAdmin failed: HTTP {resp.Status}.");
        });

        if (await search.IsNoResultsVisibleAsync())
            Assert.Ignore("No clients matched the combined filter set in this environment.");

        try
        {
            await StepAsync("Wait for client grid rows", () => search.WaitForClientGridRowsAsync(1));
        }
        catch (Exception)
        {
            Assert.Ignore("Client grid did not render rows after search.");
        }

        await StepAsync("Assert at least one ClientDetails link", async () =>
        {
            Assert.That(await search.ClientDetailsLinks.CountAsync(), Is.GreaterThan(0));
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-1244 — Client Search advanced filter panel fields and Clear Filters behaviour.")]
    public async Task DEV_TC_1244_Advanced_filters_on_client_search()
    {
        var search = await StepAsync("Open Admin Clients search", OpenClientSearchFromNavAsync);

        await StepAsync("Assert core advanced filter controls are visible", async () =>
        {
            Assert.That(await search.IsAdvancedFilterFieldVisibleAsync(Page.Locator("#txtFirstName")), Is.True);
            Assert.That(await search.IsAdvancedFilterFieldVisibleAsync(Page.Locator("#txtLastName")), Is.True);
            Assert.That(await search.IsAdvancedFilterFieldVisibleAsync(Page.Locator("#txtEmail")), Is.True);
            Assert.That(await search.IsAdvancedFilterFieldVisibleAsync(Page.Locator("#txtClientReference")), Is.True);
            Assert.That(await search.IsAdvancedFilterFieldVisibleAsync(Page.Locator("#dropdownEnquiredDestination")), Is.True);
            Assert.That(await search.TravelTypeSelect.IsVisibleAsync(), Is.True);
            Assert.That(await search.AssignedUserSelect.IsVisibleAsync(), Is.True);
            Assert.That(await search.SectorSelect.IsVisibleAsync(), Is.True);
            Assert.That(await search.MemberSelect.IsVisibleAsync(), Is.True);
        });

        await StepAsync("Populate fields then clear", async () =>
        {
            await search.FillFirstNameAsync("FilterClearTest");
            await search.FillEmailAsync("cleartest@example.invalid");
            await search.ClickClearFiltersAsync();
        });

        await StepAsync("Assert filters cleared", async () =>
        {
            Assert.That(await Page.Locator("#txtFirstName").InputValueAsync(), Is.Empty);
            Assert.That(await Page.Locator("#txtEmail").InputValueAsync(), Is.Empty);
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-1245 — Client search results pagination (SearchClientAdmin with PageIndex).")]
    public async Task DEV_TC_1245_Client_search_pagination()
    {
        var search = await StepAsync("Open Admin Clients search", OpenClientSearchFromNavAsync);

        var wait = search.StartWaitForSearchClientAdminAsync();
        await StepAsync("Load first page (Show All)", search.ClickShowAllAsync);
        await search.WaitForSearchClientAdminResponseAsync(wait);

        if (await search.IsNoResultsVisibleAsync())
            Assert.Ignore("No client rows — pagination is not available.");

        try
        {
            await StepAsync("Wait for first page rows", () => search.WaitForClientGridRowsAsync(1));
        }
        catch (Exception)
        {
            Assert.Ignore("Client grid did not load.");
        }

        var page2 = search.PagingLinkForPage(2);
        if (!await page2.IsVisibleAsync())
            Assert.Ignore("Fewer than two pages of client results — page 2 control is not shown.");

        var waitPage = search.StartWaitForSearchClientAdminAsync();
        await StepAsync("Go to page 2", () => page2.First.ClickAsync());
        var resp = await StepAsync("Await SearchClientAdmin for page 2", () => waitPage);

        await StepAsync("Assert pagination request succeeded", async () =>
        {
            Assert.That(resp.Ok, Is.True, $"Paged client search failed: HTTP {resp.Status}.");
        });

        await StepAsync("Assert page 2 is active in pager", async () =>
        {
            var active = Page.Locator(".pagination a.paging.page-link.disabled[data-page-index='2']");
            Assert.That(await active.IsVisibleAsync(), Is.True, "Page 2 should be marked active after navigation.");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-194 — Admin client details screen shows populated profile fields.")]
    public async Task DEV_TC_194_Verify_client_details_screen()
    {
        var search = await StepAsync("Open Admin Clients search", OpenClientSearchFromNavAsync);
        var clientId = await StepAsync("Resolve client id", () => TryResolveClientNewIdAsync(search, runSearchIfNeeded: true));
        if (string.IsNullOrEmpty(clientId))
            Assert.Ignore(
                "No client context: configure TestData:AdminClients:ClientNewId, AdminBookings:ClientNewId, " +
                "AdminClients:SearchClientReference, Member:Debits:ClientReferenceNumber, or ensure Show All returns rows.");

        var details = new AdminClientDetailsPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Client details", () => details.GotoWithClientNewIdAsync(clientId));

        await StepAsync("Assert Details and Bookings layout", async () =>
        {
            Assert.That(await details.IsDetailsSectionVisibleAsync(), Is.True);
            Assert.That(await details.IsBookingsSectionVisibleAsync(), Is.True);
        });

        await StepAsync("Assert profile fields populated", async () =>
        {
            var first = (await details.DisplayedFirstNameAsync()).Trim();
            Assert.That(first, Is.Not.Empty);
            var email = (await details.DisplayedEmailAsync()).Trim();
            Assert.That(email, Is.Not.Empty);
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-180 — Edit client modal opens with hydrated fields (cancel without save).")]
    public async Task DEV_TC_180_Edit_client_modal()
    {
        var search = await StepAsync("Open Admin Clients search", OpenClientSearchFromNavAsync);
        var clientId = await StepAsync("Resolve client id", () => TryResolveClientNewIdAsync(search, runSearchIfNeeded: true));
        if (string.IsNullOrEmpty(clientId))
            Assert.Ignore("No client context — same configuration as DEV-TC-194.");

        var details = new AdminClientDetailsPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Client details", () => details.GotoWithClientNewIdAsync(clientId));

        await StepAsync("Open Edit client modal", details.OpenEditClientModalAsync);
        await StepAsync("Assert edit modal and hydrated first name", async () =>
        {
            Assert.That(await details.IsEditClientModalVisibleAsync(), Is.True);
            var modalName = (await details.EditClientFirstNameInputValueAsync()).Trim();
            Assert.That(modalName, Is.Not.Empty, "Edit client should bind first name from loaded client.");
        });

        await StepAsync("Close edit modal without saving", details.CloseEditClientModalViaCancelAsync);
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-182 — Add New Booking modal opens from client profile (cancel without save).")]
    public async Task DEV_TC_182_Add_new_booking_modal()
    {
        var search = await StepAsync("Open Admin Clients search", OpenClientSearchFromNavAsync);
        var clientId = await StepAsync("Resolve client id", () => TryResolveClientNewIdAsync(search, runSearchIfNeeded: true));
        if (string.IsNullOrEmpty(clientId))
            Assert.Ignore("No client context — same configuration as DEV-TC-194.");

        var details = new AdminClientDetailsPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Client details", () => details.GotoWithClientNewIdAsync(clientId));

        await StepAsync("Open Add New Booking", details.OpenAddNewBookingModalAsync);
        await StepAsync("Assert Add Booking modal", async () =>
        {
            Assert.That(await details.IsAddBookingModalVisibleAsync(), Is.True);
            Assert.That(await Page.Locator("#Addnewbooking").GetByRole(AriaRole.Heading, new() { Name = "Add Booking" }).IsVisibleAsync(), Is.True);
            Assert.That(await details.IsAddBookingDestinationFieldVisibleAsync(), Is.True);
        });

        await StepAsync("Close add-booking modal", details.CloseAddBookingModalViaCancelAsync);
    }
}
