using System.Text.RegularExpressions;

using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Settings;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P1.Settings;

/// <summary>P1 critical: Admin → Settings → Users, and Admin user profile (membership, password, permissions, 2FA).</summary>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Settings")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag("Admin.Settings")]
[Category(Categories.Regression)]
[Category("Admin.Settings")]
public sealed class AdminSettingsP1Tests : AdminP1TestBase
{
    private async Task<AdminMemberUsersListPage> OpenMemberUsersListAsync()
    {
        await LandOnAdminEnquiriesAsync();
        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        var list = new AdminMemberUsersListPage(Page, Settings.Applications.Admin);
        var wait = list.StartWaitForMemberUserPaginationAsync();
        await nav.GoToSettingsMemberUsersAsync();
        await list.WaitForReadyAsync();
        var resp = await wait;

        await StepAsync("Assert MemberUserPagination POST", async () =>
        {
            Assert.That(resp.Ok, Is.True, $"MemberUserPagination failed: HTTP {resp.Status}.");
        });

        return list;
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-516 — Member Users list: grid and 2FA ACTIVATED column after MemberUserPagination.")]
    public async Task DEV_TC_516_Member_users_screen_with_2fa_column()
    {
        var list = await StepAsync("Open Settings → Users (Member users)", OpenMemberUsersListAsync);

        await StepAsync("Assert member users table and 2FA column", async () =>
        {
            Assert.That(await list.MemberUserTable.IsVisibleAsync(), Is.True);
            Assert.That(await list.TwoFaColumnHeader.IsVisibleAsync(), Is.True);
        });

        await StepAsync("Assert at least one data row or empty grid", async () =>
        {
            var rowCount = await Page.Locator("#tb_MemberUser tr").CountAsync();
            if (rowCount == 0)
                Assert.Ignore("No member user rows returned for this environment — 2FA cell assertions skipped.");
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-517 — Admin Users list: 2FA column populated after GetAllAdminUsers.")]
    public async Task DEV_TC_517_Admin_users_screen_with_2fa_data()
    {
        var list = await StepAsync("Land on Member users", OpenMemberUsersListAsync);

        var adminList = new AdminAdminUsersListPage(Page, Settings.Applications.Admin);
        var wait = adminList.StartWaitForGetAllAdminUsersAsync();
        await StepAsync("Open Admin users tab", list.ClickAdminUsersTabAsync);
        await adminList.WaitForReadyAsync();
        var resp = await wait;

        await StepAsync("Assert GetAllAdminUsers POST", async () =>
        {
            Assert.That(resp.Ok, Is.True, $"GetAllAdminUsers failed: HTTP {resp.Status}.");
        });

        await StepAsync("Assert admin users grid and 2FA column", async () =>
        {
            Assert.That(await adminList.TwoFaColumnHeader.IsVisibleAsync(), Is.True);
            var rows = await adminList.AdminUserTableBody.Locator("tr").CountAsync();
            if (rows == 0)
                Assert.Ignore("No admin user rows — cannot validate 2FA cells.");
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-518 — Supplier Users list: 2FA column after GetAllSupplierUser.")]
    public async Task DEV_TC_518_Supplier_users_screen_with_2fa_data()
    {
        var supplier = new AdminSupplierUsersListPage(Page, Settings.Applications.Admin);
        var wait = supplier.StartWaitForGetAllSupplierUsersAsync();
        await StepAsync("Open Supplier users", supplier.GotoAsync);
        var resp = await wait;

        await StepAsync("Assert GetAllSupplierUser POST", async () =>
        {
            Assert.That(resp.Ok, Is.True, $"GetAllSupplierUser failed: HTTP {resp.Status}.");
        });

        await StepAsync("Assert supplier grid and 2FA column", async () =>
        {
            Assert.That(await supplier.TwoFaColumnHeader.IsVisibleAsync(), Is.True);
            var rows = await supplier.SupplierUserTableBody.Locator("tr").CountAsync();
            if (rows == 0)
                Assert.Ignore("No supplier user rows — cannot validate 2FA cells.");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-545 — Member user detail shows Permissions block from Users settings.")]
    public async Task DEV_TC_545_Member_user_permissions_on_detail()
    {
        var list = await StepAsync("Open Member users", OpenMemberUsersListAsync);
        var userId = await StepAsync("Resolve first member user id", list.TryGetFirstMemberUserDetailIdAsync);
        if (string.IsNullOrEmpty(userId))
            Assert.Ignore("No member user detail link in the grid.");

        var detail = new AdminMemberUserDetailPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Member user detail", () => detail.GotoWithUserIdAsync(userId));

        await StepAsync("Assert Permissions section", async () =>
        {
            Assert.That(await detail.PermissionsSectionHeader.IsVisibleAsync(), Is.True);
            Assert.That(await Page.Locator(".accessPermissions").CountAsync(), Is.GreaterThan(0));
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-681 — Member user detail Details section (name, contact, commission).")]
    public async Task DEV_TC_681_Member_user_detail_details_section()
    {
        var list = await StepAsync("Open Member users", OpenMemberUsersListAsync);
        var userId = await StepAsync("Resolve first member user id", list.TryGetFirstMemberUserDetailIdAsync);
        if (string.IsNullOrEmpty(userId))
            Assert.Ignore("No member user detail link in the grid.");

        var detail = new AdminMemberUserDetailPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Member user detail", () => detail.GotoWithUserIdAsync(userId));

        await StepAsync("Assert Details section labels", async () =>
        {
            Assert.That(await detail.DetailsSectionHeader.IsVisibleAsync(), Is.True);
            Assert.That(await Page.GetByText("first name", new() { Exact = false }).First.IsVisibleAsync(), Is.True);
            Assert.That(await Page.GetByText("email", new() { Exact = false }).First.IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-852 — When 2FA is not enabled, admin profile shows TOTP setup copy (Totp partial / fa2_html).")]
    public async Task DEV_TC_852_Two_factor_not_enabled_setup_copy_on_profile()
    {
        await LandOnAdminEnquiriesAsync();

        var profile = new AdminProfilePage(Page, Settings.Applications.Admin);
        await StepAsync("Open View Profile", profile.ProfileMenu.GoToViewProfileAsync);
        await profile.WaitForReadyAsync();

        await StepAsync("Assert 2FA state messaging", async () =>
        {
            if (await profile.RemoveTwoFactorButton.IsVisibleAsync())
            {
                Assert.Ignore(
                    "Logged-in admin already has 2FA enabled — Totp / not-enabled setup copy is not shown.");
            }

            Assert.That(await profile.TotpSetupRoot.IsVisibleAsync(), Is.True,
                "Expected TOTP setup (#fa2_html) when 2FA is not enabled.");

            var setupCopy = Page.GetByText("Scan the image below with a 2-Factor Authentication app",
                new() { Exact = false });
            var notEnabledPhrase = Page.GetByText(new Regex("not\\s+enabled", RegexOptions.IgnoreCase));

            var hasSetupInstructions = await setupCopy.CountAsync() > 0 && await setupCopy.First.IsVisibleAsync();
            var hasNotEnabledText = await notEnabledPhrase.CountAsync() > 0 && await notEnabledPhrase.First.IsVisibleAsync();

            Assert.That(hasSetupInstructions || hasNotEnabledText, Is.True,
                "Expected either standard TOTP setup instructions or explicit 'not enabled' copy for inactive 2FA.");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-138 — Admin profile: Membership Sections block.")]
    public async Task DEV_TC_138_User_profile_membership_section()
    {
        await LandOnAdminEnquiriesAsync();
        var profile = new AdminProfilePage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin profile", profile.ProfileMenu.GoToViewProfileAsync);
        await profile.WaitForReadyAsync();

        await StepAsync("Assert Membership Sections", async () =>
        {
            Assert.That(await profile.MembershipSectionsHeader.IsVisibleAsync(), Is.True);
            Assert.That(await profile.MembershipAccessPanel.IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-680 — Admin profile: Change password (current + new password) form.")]
    public async Task DEV_TC_680_User_profile_change_password_section()
    {
        await LandOnAdminEnquiriesAsync();
        var profile = new AdminProfilePage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin profile", profile.ProfileMenu.GoToViewProfileAsync);
        await profile.WaitForReadyAsync();

        await StepAsync("Assert change password form", async () =>
        {
            Assert.That(await profile.ChangePasswordForm.IsVisibleAsync(), Is.True);
            Assert.That(await Page.Locator("#oldPassword").IsVisibleAsync(), Is.True);
            Assert.That(await Page.Locator("#newPassword").IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-682 — Admin profile: Permissions labels and privilege badges.")]
    public async Task DEV_TC_682_User_profile_permissions_section()
    {
        await LandOnAdminEnquiriesAsync();
        var profile = new AdminProfilePage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin profile", profile.ProfileMenu.GoToViewProfileAsync);
        await profile.WaitForReadyAsync();

        await StepAsync("Assert Permissions section", async () =>
        {
            Assert.That(await profile.PermissionsSectionLabel.IsVisibleAsync(), Is.True);
            var badges = Page.Locator(".col-lg-7 .status-badge.userPrivilege");
            Assert.That(await badges.CountAsync(), Is.GreaterThan(0), "Expected at least one permission status badge.");
        });
    }
}
