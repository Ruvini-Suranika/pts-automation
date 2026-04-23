using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Settings;

/// <summary>
/// Settings mega-menu → Contracted Suppliers. Searches suppliers that the
/// member (or PTS) has contracted with.
/// Controller action: <c>MemberController.SearchContractedSuppliers</c>.
///
/// SKELETON.
/// </summary>
public sealed class ContractedSuppliersPage : MemberPage
{
    public ContractedSuppliersPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.ContractedSuppliers;

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;
}
