namespace PTS.Automation.Pages.Admin.Settings;

/// <summary>Admin → Member user detail (<c>Admin/MemberUserDetail?Id=…</c>).</summary>
public sealed class AdminMemberUserDetailPage : AdminPage
{
    public AdminMemberUserDetailPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.MemberUserDetail;

    protected override ILocator ReadinessIndicator => Page.Locator("section.member_page_title").First;

    public ILocator DetailsSectionHeader =>
        Page.Locator(".your_profile .profile_detail .profile_header span.h1", new() { HasText = "Details" }).First;

    public ILocator PermissionsSectionHeader =>
        Page.Locator(".change_password .profile_header span.h1", new() { HasText = "Permissions" }).First;

    public async Task GotoWithUserIdAsync(string userId)
    {
        var path = $"{AdminRoutes.MemberUserDetail}?Id={Uri.EscapeDataString(userId)}";
        var url = new Uri(App.BaseUri, path.TrimStart('/')).ToString();
        await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }
}
