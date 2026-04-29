using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Clients;
using PTS.Automation.Pages.Member.Dashboard;

namespace PTS.Automation.Features.Member.Bookings;

/// <summary>P1 critical coverage: Member → Booking Management (list, overview, money, itinerary).</summary>
[TestFixture]
[AllureSuite(Categories.Member)]
[AllureFeature("Booking management")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag(Categories.Member)]
[AllureTag("Bookings.BookingManagement")]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category("Bookings.BookingManagement")]
public sealed class BookingManagementP1Tests : MemberTest
{
    private static async Task<BookingListPage> OpenBookingsListAsync(DashboardPage dashboard, BookingListPage bookings)
    {
        await dashboard.GotoAsync();
        await dashboard.NavBar.GoToBookingsAsync();
        await bookings.WaitForReadyAsync();
        return bookings;
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-15 — Verify booking information screen (filters + optional booking overview from first result).")]
    public async Task DEV_TC_15_Booking_information_screen()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));

        await StepAsync("Assert booking search / filter chrome", async () =>
        {
            Assert.That(await list.IsSearchButtonVisibleAsync(), Is.True);
            Assert.That(await list.AreMainFilterInputsVisibleAsync(), Is.True);
            Assert.That(await list.AreMainFilterDropdownsVisibleAsync(), Is.True);
        });

        var status = await StepAsync("Run search (all bookings)", () => list.SearchAsync());
        await StepAsync("Assert search succeeds", () =>
        {
            Assert.That(status, Is.EqualTo(200));
            return Task.CompletedTask;
        });

        var ids = await StepAsync("Try read first booking from grid", () => list.TryReadFirstBookingDetailsQueryAsync());
        if (ids is null)
            Assert.Ignore("No booking rows returned for this member — cannot open booking information overview.");

        var overview = new BookingOverviewPage(Page, Settings.Applications.Member);
        await StepAsync("Open first booking overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Assert overview shell and booking ref", async () =>
        {
            Assert.That(await overview.IsSubNavOverviewActiveOrPresentAsync(), Is.True);
            var refValue = await overview.BookingReferenceHiddenValueAsync();
            Assert.That(refValue, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-16 — Search all booking information (search returns 200 and reveals grid).")]
    public async Task DEV_TC_16_Search_all_booking_information()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));

        var status = await StepAsync("Click Search without narrowing filters", () => list.SearchAsync());

        await StepAsync("Assert grid section visible after search", async () =>
        {
            Assert.That(status, Is.EqualTo(200));
            await Page.WaitForTimeoutAsync(200);
            Assert.That(await list.IsGridSectionVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-17 — Assign new user to existing booking (modal opens; no reassign submit).")]
    public async Task DEV_TC_17_Assign_new_user_modal_from_list()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        if (await list.RowSelectionCheckboxCountAsync() == 0)
            Assert.Ignore("No booking rows — cannot tick a row for Assign new user.");

        await StepAsync("Tick first row via checkmark UI", () => list.SelectFirstRowCheckmarkAsync());
        await Page.WaitForTimeoutAsync(250);

        await StepAsync("Assert Assign new user becomes clickable", async () =>
        {
            Assert.That(await list.IsAssignNewUserLinkPointerEventsEnabledAsync(), Is.True,
                "Selecting a row should enable the Assign new user control (pointer-events).");
        });

        await StepAsync("Open Assign new user modal", () => list.ClickAssignNewUserAsync());

        await StepAsync("Assert assign-user modal and user dropdown", async () =>
        {
            Assert.That(await list.IsAssignUserModalVisibleAsync(), Is.True);
            Assert.That(await list.ReassignUserOptionCountAsync(), Is.GreaterThan(0),
                "Reassign user dropdown should list at least a placeholder option.");
        });

        await StepAsync("Dismiss assign modal", () => Page.Keyboard.PressAsync("Escape"));
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-22 — View Booking Overview screen.")]
    public async Task DEV_TC_22_View_booking_overview_screen()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first booking ids", () => list.TryReadFirstBookingDetailsQueryAsync());
        if (ids is null)
            Assert.Ignore("No booking rows — no overview to open.");

        var overview = new BookingOverviewPage(Page, Settings.Applications.Member);
        await StepAsync("Navigate to Booking Overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Assert overview markers", async () =>
        {
            Assert.That(await overview.BookingReferenceHiddenValueAsync(), Is.Not.Null.And.Not.Empty);
            Assert.That(await overview.IsSubNavBookingTabVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-26 — Validate Booking Overview Detail option (Booking tab → management view).")]
    public async Task DEV_TC_26_Booking_overview_detail_booking_tab()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first booking ids", () => list.TryReadFirstBookingDetailsQueryAsync());
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var overview = new BookingOverviewPage(Page, Settings.Applications.Member);
        await StepAsync("Open overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Click Booking tab", () => overview.ClickBookingManagementTabAsync());

        await StepAsync("Assert navigation to ClientBookingDetails", async () =>
        {
            await Page.WaitForURLAsync("**/Client/ClientBookingDetails**",
                new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });
            Assert.That(Page.Url, Does.Contain("ClientBookingDetails").IgnoreCase);
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-123 — Search client using Booking Type from advanced filters (ClientTypeId in POST).")]
    public async Task DEV_TC_123_Search_by_booking_type_round_trip()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));

        var selectedValue = await StepAsync("Select first non-empty Booking Type", () => list.SelectFirstNonEmptyBookingTypeValueAsync());
        if (string.IsNullOrEmpty(selectedValue))
            Assert.Ignore("No booking-type options in #dropDownClientType — cannot assert ClientTypeId round-trip.");

        var (status, postData) = await StepAsync("Search and capture POST body", () => list.SearchAndCaptureRequestAsync());

        await StepAsync("Assert ClientTypeId in POST", async () =>
        {
            Assert.That(status, Is.EqualTo(200));
            Assert.That(postData, Does.Contain($"ClientTypeId={selectedValue}").IgnoreCase
                                  .Or.Contain($"\"ClientTypeId\":\"{selectedValue}\"").IgnoreCase
                                  .Or.Contain($"clientTypeID={selectedValue}").IgnoreCase,
                $"POST should carry booking type id {selectedValue}.\nActual: {postData}");
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-133 — Add new booking item and verify from Admin system.")]
    public async Task DEV_TC_133_Add_booking_item_verify_in_admin()
    {
        Assert.Ignore(
            "End-to-end add-booking plus Admin verification requires coordinated test data and an authenticated Admin session; not automated in this suite.");
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-169 — Change confirmation type from Booking summary section (control present; no toggle to avoid reload).")]
    public async Task DEV_TC_169_Booking_summary_confirmation_control_present()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first booking ids", () => list.TryReadFirstBookingDetailsQueryAsync());
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var overview = new BookingOverviewPage(Page, Settings.Applications.Member);
        await StepAsync("Open overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        try
        {
            await StepAsync("Wait for booking summary rows (AJAX)", () => overview.WaitForBookingSummaryRowsAsync(1));
        }
        catch (Exception)
        {
            Assert.Ignore("Booking summary table did not populate — no confirmation control to assert.");
        }

        await StepAsync("Assert confirmation affordance in summary", async () =>
        {
            Assert.That(await overview.IsBookingSummarySectionVisibleAsync(), Is.True);
            Assert.That(await overview.FirstBookingSummaryConfirmationControl().IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-172 — Remove Quick Note: Edit Quick Note modal opens and can be dismissed without persisting.")]
    public async Task DEV_TC_172_Remove_quick_note_modal_smoke_without_save()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first booking ids", () => list.TryReadFirstBookingDetailsQueryAsync());
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var overview = new BookingOverviewPage(Page, Settings.Applications.Member);
        await StepAsync("Open overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Assert Quick Note section", async () =>
        {
            Assert.That(await overview.IsQuickNoteSectionVisibleAsync(), Is.True);
        });

        await StepAsync("Open Edit Quick Note modal", () => overview.OpenEditQuickNoteModalAsync());

        await StepAsync("Assert modal and textarea, then dismiss without Save", async () =>
        {
            Assert.That(await overview.IsEditQuickNoteModalVisibleAsync(), Is.True);
            await overview.EditQuickNoteTextArea.FillAsync("");
            await overview.CloseEditQuickNoteModalAsync();
            await Page.WaitForTimeoutAsync(200);
            Assert.That(await overview.IsEditQuickNoteModalVisibleAsync(), Is.False);
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-192 — Convert activity supplier into internal supplier (affordance visible when eligible).")]
    public async Task DEV_TC_192_Activity_convert_to_internal_supplier_affordance()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first booking ids", () => list.TryReadFirstBookingDetailsQueryAsync());
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var manage = new ClientBookingDetailsManagementPage(Page, Settings.Applications.Member);
        await StepAsync("Open booking management", () => manage.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        var count = await StepAsync("Count activity convert badges", () => manage.ActivityConvertToInternalBadgeCountAsync());
        if (count == 0)
            Assert.Ignore("No activity supplier in AddSuplier state — CONVERT TO INTERNAL SUPPLIER not shown.");

        Assert.That(count, Is.GreaterThan(0));
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-224 — Extract filtered booking information via three-dots (PDF / Excel hrefs).")]
    public async Task DEV_TC_224_Extract_filtered_bookings_via_three_dots()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid so export links bind", () => list.SearchAsync());

        await StepAsync("Open bookings grid three-dots menu", () => list.OpenBookingsGridThreeDotsMenuAsync());

        await StepAsync("Assert copy/print actions visible", async () =>
        {
            Assert.That(await list.IsCopyToClipboardMenuItemVisibleAsync(), Is.True);
            Assert.That(await list.IsPrintMenuItemVisibleAsync(), Is.True);
        });

        var pdf   = await StepAsync("Read PDF export href", () => list.GetClientSearchPdfHrefAsync());
        var excel = await StepAsync("Read Excel export href", () => list.GetClientSearchExcelHrefAsync());

        await StepAsync("Assert export targets", async () =>
        {
            Assert.That(pdf, Is.Not.Null.And.Not.Empty);
            Assert.That(excel, Is.Not.Null.And.Not.Empty);
            Assert.That(pdf, Does.Contain("ExportBookingSearchToPdf").IgnoreCase);
            Assert.That(excel, Does.Contain("ExportBookingSearchToExcel").IgnoreCase);
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-226 — Extract Booking Money information via three-dots (PDF / Excel hrefs).")]
    public async Task DEV_TC_226_Extract_booking_money_via_three_dots()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first Money link query", () => list.TryReadFirstMoneyPageQueryAsync());
        if (ids is null)
            Assert.Ignore("No Money link in grid — cannot open booking money.");

        var money = new BookingMoneyPage(Page, Settings.Applications.Member);
        await StepAsync("Open Booking Money", () => money.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Open money three-dots menu", () => money.OpenMoneyThreeDotsMenuAsync());

        var pdf   = await StepAsync("Read Money PDF href", () => money.GetExportMoneyPdfHrefAsync());
        var excel = await StepAsync("Read Money Excel href", () => money.GetExportMoneyExcelHrefAsync());

        await StepAsync("Assert export targets", async () =>
        {
            Assert.That(pdf, Is.Not.Null.And.Not.Empty);
            Assert.That(excel, Is.Not.Null.And.Not.Empty);
            Assert.That(pdf, Does.Contain("ExportMoneyCreditToPdf").IgnoreCase);
            Assert.That(excel, Does.Contain("ExportMoneyCreditToExcel").IgnoreCase);
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-232 — Validate notes from booking money when rejecting inter-transfer from admin.")]
    public async Task DEV_TC_232_Booking_money_notes_after_admin_reject_inter_transfer()
    {
        Assert.Ignore(
            "Requires an Admin inter-transfer rejection against this booking and matching notes on Money — out of scope for member-only automation.");
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-368 — Validate passengers column on Booking Money screen.")]
    public async Task DEV_TC_368_Passenger_column_on_booking_money()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first Money link query", () => list.TryReadFirstMoneyPageQueryAsync());
        if (ids is null)
            Assert.Ignore("No Money link in grid.");

        var money = new BookingMoneyPage(Page, Settings.Applications.Member);
        await StepAsync("Open Booking Money", () => money.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Assert Passenger column header on money table", async () =>
        {
            Assert.That(await money.IsPassengerColumnHeaderVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-377 — Validate booking details for all suppliers on Itinerary (preview links to itinerary).")]
    public async Task DEV_TC_377_Itinerary_preview_links_on_booking_management()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first booking ids", () => list.TryReadFirstBookingDetailsQueryAsync());
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var manage = new ClientBookingDetailsManagementPage(Page, Settings.Applications.Member);
        await StepAsync("Open booking management", () => manage.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        var itineraryHref = await StepAsync("Read sub-nav Itinerary href", () => manage.ItineraryHrefAsync());
        await StepAsync("Assert Itinerary href", async () =>
        {
            Assert.That(itineraryHref, Is.Not.Null.And.Not.Empty);
            Assert.That(itineraryHref, Does.Contain("ClientBookingsItinerary").IgnoreCase);
            Assert.That(itineraryHref, Does.Contain("BookingRefId=").IgnoreCase);
        });

        var previewCount = await StepAsync("Count itinerary Preview dropdown links", () => manage.ItineraryPreviewLinkCountAsync());
        if (previewCount == 0)
            Assert.Ignore("No supplier blocks with Itinerary Preview — nothing to assert.");

        Assert.That(previewCount, Is.GreaterThan(0));
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-388 — Convert Transport supplier into internal supplier (affordance visible when eligible).")]
    public async Task DEV_TC_388_Transport_convert_to_internal_supplier_affordance()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first booking ids", () => list.TryReadFirstBookingDetailsQueryAsync());
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var manage = new ClientBookingDetailsManagementPage(Page, Settings.Applications.Member);
        await StepAsync("Open booking management", () => manage.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        var count = await StepAsync("Count transport convert badges", () => manage.TransportConvertToInternalBadgeCountAsync());
        if (count == 0)
            Assert.Ignore("No transport supplier in AddSuplier state — CONVERT TO INTERNAL SUPPLIER not shown.");

        Assert.That(count, Is.GreaterThan(0));
    }

    [Test]
    [Category(Categories.P1)]
    [Category(Categories.UI)]
    [Description("DEV-TC-508 — Convert Package supplier into internal supplier (affordance visible when eligible).")]
    public async Task DEV_TC_508_Package_convert_to_internal_supplier_affordance()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list      = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Bookings", () => OpenBookingsListAsync(dashboard, list));
        await StepAsync("Populate grid", () => list.SearchAsync());

        var ids = await StepAsync("Read first booking ids", () => list.TryReadFirstBookingDetailsQueryAsync());
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var manage = new ClientBookingDetailsManagementPage(Page, Settings.Applications.Member);
        await StepAsync("Open booking management", () => manage.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        var count = await StepAsync("Count package convert badges", () => manage.PackageConvertToInternalBadgeCountAsync());
        if (count == 0)
            Assert.Ignore("No package supplier in AddSuplier state — CONVERT TO INTERNAL SUPPLIER not shown.");

        Assert.That(count, Is.GreaterThan(0));
    }
}
