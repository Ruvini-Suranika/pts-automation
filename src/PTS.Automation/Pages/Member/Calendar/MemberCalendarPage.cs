using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Calendar;

/// <summary>
/// Member → Calendar. Shows the agency's event calendar and diary notes.
/// Source view: <c>Views/Member/MemberCalender.cshtml</c> (note: misspelling
/// preserved from dev).
/// Controller action: <c>MemberController.MemberCalender</c>.
///
/// SKELETON.
/// </summary>
public sealed class MemberCalendarPage : MemberPage
{
    public MemberCalendarPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Calendar;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
