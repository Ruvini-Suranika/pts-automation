using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.Reconciliation;

/// <summary>
/// Bank → Reconciliation (previous travel). Source: <c>Views/Admin/PreviousTravelReconcilation.cshtml</c>,
/// filter script <c>BankReconcile.js</c> (<c>#search</c> keyup filters <c>#searchRecord tr</c>).
/// </summary>
public sealed class AdminTravelReconciliationPage : AdminPage
{
    public AdminTravelReconciliationPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.PreviousTravelReconciliationTravel;

    private ILocator SearchBox => Page.Locator("#search");

    protected override ILocator ReadinessIndicator => Page.Locator("#divResults").First;

    /// <summary>Client-side filter on the reconciliation table (<c>keyup</c> handler).</summary>
    public async Task FilterGridByKeywordAsync(string keyword)
    {
        await SearchBox.FillAsync("");
        if (keyword.Length > 0)
            await SearchBox.PressSequentiallyAsync(keyword);
    }
}
