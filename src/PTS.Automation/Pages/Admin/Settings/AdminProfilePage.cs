namespace PTS.Automation.Pages.Admin.Settings;

/// <summary>Admin → View profile (<c>Admin/AdminProfile</c>).</summary>
public sealed class AdminProfilePage : AdminPage
{
    public AdminProfilePage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.AdminProfile;

    protected override ILocator ReadinessIndicator =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Your profile", Exact = true });

    public ILocator MembershipSectionsHeader =>
        Page.Locator(".change_password .profile_header span.h1", new() { HasText = "Membership Sections" }).First;

    public ILocator MembershipAccessPanel => Page.Locator("#panelMemberShipAccess");

    public ILocator ChangePasswordForm => Page.Locator("#changePasswordForm");

    public ILocator PermissionsSectionLabel =>
        Page.Locator("span.h1", new() { HasText = "Permissions" }).First;

    public ILocator TwoFactorHeading =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "2-Factor Authentication", Exact = true });

    /// <summary>TOTP setup block when 2FA is not yet enabled (<c>Views/Shared/Totp.cshtml</c>).</summary>
    public ILocator TotpSetupRoot => Page.Locator("#fa2_html");

    public ILocator RemoveTwoFactorButton => Page.Locator("#btnRemove2Factor");
}
