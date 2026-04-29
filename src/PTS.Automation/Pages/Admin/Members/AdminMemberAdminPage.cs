using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Pages.Admin.Members;

/// <summary>Admin → Member admin (<c>Admin/MemberAdmin/{id}</c>). View: <c>Views/Admin/MemberAdmin.cshtml</c>.</summary>
public sealed class AdminMemberAdminPage : AdminPage
{
    public AdminMemberAdminPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath =>
        throw new InvalidOperationException("Use GotoWithMemberIdAsync.");

    protected override ILocator ReadinessIndicator =>
        Page.GetByText("PTS system setup", new() { Exact = false }).First;

    public async Task GotoWithMemberIdAsync(long memberId)
    {
        var path = $"{AdminRoutes.MemberAdminPrefix}/{memberId}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

    public Task<bool> IsPtsSystemSetupSectionVisibleAsync() =>
        Page.GetByText("PTS system setup", new() { Exact = false }).First.IsVisibleAsync();

    public ILocator PtsSystemSetupSaveButton => Page.Locator("#btnPtsSystem");

    public Task<bool> IsMainUserOnSystemChoiceVisibleAsync() =>
        Page.Locator("#mainUserYes").IsVisibleAsync();

    public ILocator MembershipSectionHeading =>
        Page.Locator("span.h1", new() { HasText = "Membership" }).First;

    public ILocator MembershipSaveButton => Page.Locator("#btnMemberShip");

    public ILocator MemberStatusSetLink => Page.Locator("#spnSetStatus");
}
