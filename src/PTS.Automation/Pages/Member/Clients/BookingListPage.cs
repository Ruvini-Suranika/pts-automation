using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// Member → Bookings landing page. Shows the bookings grid with filters.
/// Source view: <c>Views/Client/BookingSearchView.cshtml</c>.
/// Controller action: <c>ClientController.BookingSearchView</c>.
///
/// SKELETON.
/// </summary>
public sealed class BookingListPage : MemberPage
{
    public BookingListPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Bookings;

    // TODO(scripting): narrow to the bookings grid's stable anchor.
    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
