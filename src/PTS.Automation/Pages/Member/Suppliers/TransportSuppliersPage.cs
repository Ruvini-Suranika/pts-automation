using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Suppliers;

/// <summary>
/// Suppliers → Transport suppliers page.
/// Controller action: <c>MemberController.SuppliersTransport</c>.
///
/// SKELETON.
/// </summary>
public sealed class TransportSuppliersPage : MemberPage
{
    public TransportSuppliersPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.Transport;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
