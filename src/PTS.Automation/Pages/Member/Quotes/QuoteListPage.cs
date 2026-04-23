using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Quotes;

/// <summary>
/// Member → Quotes landing page.
/// Source view: <c>Views/Quote/QuoteSearchView.cshtml</c>.
/// Controller action: <c>QuoteController.QuoteSearchView</c>.
///
/// SKELETON.
/// </summary>
public sealed class QuoteListPage : MemberPage
{
    public QuoteListPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Quotes;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
