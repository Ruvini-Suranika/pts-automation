using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Suppliers;

/// <summary>
/// Suppliers → Packages search page.
/// Controller action: <c>PackageController.PackageSearch</c>.
///
/// SKELETON.
/// </summary>
public sealed class PackagesSearchPage : MemberPage
{
    public PackagesSearchPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Packages;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
