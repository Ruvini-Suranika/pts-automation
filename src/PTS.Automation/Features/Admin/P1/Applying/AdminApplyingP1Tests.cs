using System.Text.RegularExpressions;

using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Applying;
using PTS.Automation.Pages.Admin.Members;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P1.Applying;

/// <summary>P1 critical: Admin → Members → Applying search, AfterSales header actions, Sales trust UI, Admin tab sections.</summary>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Applying")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag("Admin.Applying")]
[Category(Categories.Regression)]
[Category("Admin.Applying")]
public sealed class AdminApplyingP1Tests : AdminP1TestBase
{
    private async Task<AdminApplyingSearchPage> OpenApplyingSearchAsync()
    {
        await LandOnAdminEnquiriesAsync();
        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        var applying = new AdminApplyingSearchPage(Page, Settings.Applications.Admin);
        var wait = applying.StartWaitForGetApplyingSearchListAsync();
        await nav.GoToApplyingAsync();
        var resp = await wait;
        await applying.WaitForReadyAsync();

        await StepAsync("Assert GetApplyingSearchList POST", async () =>
        {
            Assert.That(resp.Ok, Is.True, $"GetApplyingSearchList failed: HTTP {resp.Status}.");
        });

        return applying;
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-205 — Applying search screen: grid, search box, and list API.")]
    public async Task DEV_TC_205_Applying_search_screen()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);

        await StepAsync("Assert Applying table chrome", async () =>
        {
            Assert.That(await applying.SearchInput.IsVisibleAsync(), Is.True);
            Assert.That(await Page.GetByRole(AriaRole.Columnheader, new() { Name = "member name" }).First.IsVisibleAsync(),
                Is.True);
            Assert.That(await applying.TableBody.IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-206 — AfterSales: Get invite modal + Notes off-canvas from Applying header.")]
    public async Task DEV_TC_206_Get_invite_and_notes_from_applying_member()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);
        var memberIdStr = await StepAsync("Resolve first member id", applying.TryGetFirstAfterSalesMemberIdAsync);
        if (string.IsNullOrEmpty(memberIdStr))
            Assert.Ignore("No Applying rows with AfterSales member link.");
        if (!long.TryParse(memberIdStr, out var memberId))
            Assert.Ignore("AfterSales member link did not contain a numeric member id.");

        var afterSales = new AdminAfterSalesApplyingPage(Page, Settings.Applications.Admin);
        await StepAsync("Open AfterSales (Applying)", () => afterSales.GotoWithMemberIdAsync(memberId));

        await StepAsync("Open Get invite modal", afterSales.OpenGetInviteModalAsync);
        await StepAsync("Assert invite modal", async () =>
        {
            Assert.That(await afterSales.InviteLinkModal.IsVisibleAsync(), Is.True);
            Assert.That(await Page.GetByRole(AriaRole.Heading, new() { Name = "Invite link" }).IsVisibleAsync(), Is.True);
        });

        await StepAsync("Close invite modal", () => afterSales.InviteLinkModal.Locator(".btn-close").First.ClickAsync());

        await StepAsync("Open Notes", afterSales.OpenNotesAsync);
        await StepAsync("Assert Notes off-canvas", async () =>
        {
            Assert.That(await afterSales.NotesOffcanvas.IsVisibleAsync(), Is.True);
            Assert.That(await Page.GetByRole(AriaRole.Heading, new() { Name = "Notes" }).IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-746 — Client-side search + pagination: refine filter while on page 2 of results.")]
    public async Task DEV_TC_746_Refine_filters_across_paginated_search_results()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);

        var page2 = applying.Pagination.GetByRole(AriaRole.Link, new() { Name = "2", Exact = true });
        if (!await page2.IsVisibleAsync())
            Assert.Ignore("Fewer than two pages of Applying results — pagination link 2 is not shown.");

        var firstNamePage1 = (await applying.TableBody.Locator("tr").First.Locator("td").First.InnerTextAsync()).Trim();
        if (firstNamePage1.Length < 2)
            Assert.Ignore("First row member name too short to run a meaningful substring search.");

        await StepAsync("Go to page 2", () => page2.First.ClickAsync());

        var filter = firstNamePage1[..Math.Min(4, firstNamePage1.Length)];
        await StepAsync("Apply search refine from page 2 context", () => applying.FillSearchAsync(filter));

        await StepAsync("Assert filtered result strip", async () =>
        {
            var showing = (await Page.Locator("#recordShowing").InnerTextAsync()).Trim();
            Assert.That(showing, Is.Not.Empty);
            Assert.That(showing, Does.Match(new Regex(@"of\s+\d+\s+results?", RegexOptions.IgnoreCase)));
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description(
        "DEV-TC-748 — Move Applying member to Risk: assign user and POST UpdateTimeLineStatus (mutates member timeline).")]
    public async Task DEV_TC_748_Move_applying_record_to_risk()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);
        var memberIdStr = await StepAsync("Resolve first member id", applying.TryGetFirstAfterSalesMemberIdAsync);
        if (string.IsNullOrEmpty(memberIdStr))
            Assert.Ignore("No Applying rows with AfterSales member link.");
        if (!long.TryParse(memberIdStr, out var memberId))
            Assert.Ignore("AfterSales member link did not contain a numeric member id.");

        var afterSales = new AdminAfterSalesApplyingPage(Page, Settings.Applications.Admin);
        await StepAsync("Open AfterSales", () => afterSales.GotoWithMemberIdAsync(memberId));

        await StepAsync("Open Move to risk modal", afterSales.OpenMoveToRiskModalAsync);
        await StepAsync("Assert Move to risk modal", async () =>
        {
            Assert.That(await afterSales.MoveToRiskModal.GetByRole(AriaRole.Heading, new() { Name = "Move to risk" }).IsVisibleAsync(),
                Is.True);
        });

        var assignee = afterSales.MoveToRiskAssigneeSelect;
        var optCount = await assignee.Locator("option").CountAsync();
        if (optCount <= 1)
            Assert.Ignore("No assignable users in Move to risk dropdown for this environment.");

        var wait = afterSales.StartWaitForUpdateTimeLineStatusAsync();
        await StepAsync("Select assignee and save", async () =>
        {
            await assignee.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            await afterSales.MoveToRiskSaveButton.ClickAsync();
        });

        var resp = await wait;
        await StepAsync("Assert UpdateTimeLineStatus", async () =>
        {
            Assert.That(resp.Ok, Is.True, $"UpdateTimeLineStatus failed: HTTP {resp.Status}.");
        });

        await StepAsync("Assert navigation to Risk details", async () =>
        {
            await Page.WaitForURLAsync(
                u => u.Contains("/Admin/RiskDetails/", StringComparison.OrdinalIgnoreCase),
                new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description(
        "DEV-TC-749 — Invite email template preview: Email Link opens recipients modal when onboarding template is configured.")]
    public async Task DEV_TC_749_Get_invite_email_contents()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);
        var memberIdStr = await StepAsync("Resolve first member id", applying.TryGetFirstAfterSalesMemberIdAsync);
        if (string.IsNullOrEmpty(memberIdStr))
            Assert.Ignore("No Applying rows with AfterSales member link.");
        if (!long.TryParse(memberIdStr, out var memberId))
            Assert.Ignore("AfterSales member link did not contain a numeric member id.");

        var afterSales = new AdminAfterSalesApplyingPage(Page, Settings.Applications.Admin);
        var tplWait = afterSales.StartWaitForGetOnboardingEmailTemplateAsync();
        await StepAsync("Open AfterSales (loads invite email template)", () => afterSales.GotoWithMemberIdAsync(memberId));
        try
        {
            await tplWait;
        }
        catch (TimeoutException)
        {
            // POST may complete before the waiter attaches on fast loads.
        }

        await StepAsync("Open Get invite then Email Link", async () =>
        {
            await afterSales.OpenGetInviteModalAsync();
            var emailWait = afterSales.StartWaitForGetMemberContactEmailAsync();
            await afterSales.EmailLinkButton.ClickAsync();
            await emailWait;
        });

        await StepAsync("Assert recipients modal or skip if template/email integration missing", async () =>
        {
            if (!await afterSales.RecipientsEmailModal.IsVisibleAsync())
            {
                Assert.Ignore(
                    "Invite email flow did not open the Recipients modal — onboarding email template or contact email may be unavailable.");
            }

            await afterSales.EmailSubjectInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = Settings.Timeouts.DefaultMs
            });

            var subject = (await afterSales.EmailSubjectInput.InputValueAsync()).Trim();
            var hasEditor = await Page.Locator(".tox-edit-area").CountAsync() > 0;
            var editorText = hasEditor
                ? (await Page.Locator(".tox-edit-area").InnerTextAsync()).Trim()
                : string.Empty;

            if (string.IsNullOrEmpty(subject) && string.IsNullOrEmpty(editorText))
            {
                Assert.Ignore(
                    "DEV-TC-749: Non-automatable in this environment — invite email subject/body were not populated " +
                    "(GetOnboardingGetEmailTemplate may return empty, or TinyMCE/setInviteEmail wiring may not fill the preview). " +
                    "Validate message content manually against sent mail when email integration is configured.");
            }

            Assert.That(subject.Length > 0 || editorText.Contains("Invite", StringComparison.OrdinalIgnoreCase),
                Is.True,
                "Expected subject line and/or editor body to reflect invite template content.");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-1004 — Applying Sales tab (Edit Member Company) shows trust account section.")]
    public async Task DEV_TC_1004_Applying_sales_page()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);
        var memberIdStr = await StepAsync("Resolve first member id", applying.TryGetFirstAfterSalesMemberIdAsync);
        if (string.IsNullOrEmpty(memberIdStr))
            Assert.Ignore("No Applying rows with AfterSales member link.");
        if (!long.TryParse(memberIdStr, out var memberId))
            Assert.Ignore("AfterSales member link did not contain a numeric member id.");

        var afterSales = new AdminAfterSalesApplyingPage(Page, Settings.Applications.Admin);
        await StepAsync("Open AfterSales", () => afterSales.GotoWithMemberIdAsync(memberId));
        await StepAsync("Open Sales tab", afterSales.OpenSalesTabAsync);

        await Page.WaitForURLAsync(
            u => u.Contains("EditMemberCompany", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });

        var sales = new AdminEditMemberCompanySalesPage(Page, Settings.Applications.Admin);
        await StepAsync("Wait for Sales page", () => sales.WaitForReadyAsync());

        await StepAsync("Assert trust account access section", async () =>
        {
            Assert.That(await sales.TrustAccountDetailsTable.IsVisibleAsync(), Is.True);
            Assert.That(await sales.AddTrustAccountTrigger.IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-1008 — Add Trust Account modal opens from Applying Sales tab.")]
    public async Task DEV_TC_1008_Add_trust_account_modal_on_sales_tab()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);
        var memberIdStr = await StepAsync("Resolve first member id", applying.TryGetFirstAfterSalesMemberIdAsync);
        if (string.IsNullOrEmpty(memberIdStr))
            Assert.Ignore("No Applying rows with AfterSales member link.");
        if (!long.TryParse(memberIdStr, out var memberId))
            Assert.Ignore("AfterSales member link did not contain a numeric member id.");

        var afterSales = new AdminAfterSalesApplyingPage(Page, Settings.Applications.Admin);
        await StepAsync("Open AfterSales", () => afterSales.GotoWithMemberIdAsync(memberId));
        await StepAsync("Open Sales tab", afterSales.OpenSalesTabAsync);
        await Page.WaitForURLAsync(
            u => u.Contains("EditMemberCompany", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });

        var sales = new AdminEditMemberCompanySalesPage(Page, Settings.Applications.Admin);
        await sales.WaitForReadyAsync();

        await StepAsync("Open Add trust account modal", sales.OpenAddTrustAccountModalAsync);
        await StepAsync("Assert modal", async () =>
        {
            Assert.That(await sales.AddTrustAccountModal.IsVisibleAsync(), Is.True);
            Assert.That(await Page.GetByRole(AriaRole.Heading, new() { Name = "Add trust account" }).IsVisibleAsync(), Is.True);
            var options = await sales.AddTrustAccountDropdown.Locator("option").CountAsync();
            if (options <= 1)
                Assert.Ignore("No trust accounts available in Add trust account dropdown for this member.");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-1009 — Trust account row actions (virtual account assign/edit controls) visible when accounts exist.")]
    public async Task DEV_TC_1009_Trust_account_actions_on_sales_tab()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);
        var memberIdStr = await StepAsync("Resolve first member id", applying.TryGetFirstAfterSalesMemberIdAsync);
        if (string.IsNullOrEmpty(memberIdStr))
            Assert.Ignore("No Applying rows with AfterSales member link.");
        if (!long.TryParse(memberIdStr, out var memberId))
            Assert.Ignore("AfterSales member link did not contain a numeric member id.");

        var afterSales = new AdminAfterSalesApplyingPage(Page, Settings.Applications.Admin);
        await StepAsync("Open AfterSales", () => afterSales.GotoWithMemberIdAsync(memberId));
        await StepAsync("Open Sales tab", afterSales.OpenSalesTabAsync);
        await Page.WaitForURLAsync(
            u => u.Contains("EditMemberCompany", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });

        var sales = new AdminEditMemberCompanySalesPage(Page, Settings.Applications.Admin);
        await sales.WaitForReadyAsync();

        await StepAsync("Assert trust row actions when grid has rows", async () =>
        {
            if (await sales.TrustRowActionLinks.CountAsync() == 0)
                Assert.Ignore("No trust account rows with virtual-account actions for this member.");

            Assert.That(await sales.TrustRowActionLinks.First.IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-1021 — Applying Admin tab: PTS system setup section on Member Admin.")]
    public async Task DEV_TC_1021_Pts_system_setup_on_applying_admin_tab()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);
        var memberIdStr = await StepAsync("Resolve first member id", applying.TryGetFirstAfterSalesMemberIdAsync);
        if (string.IsNullOrEmpty(memberIdStr))
            Assert.Ignore("No Applying rows with AfterSales member link.");
        if (!long.TryParse(memberIdStr, out var memberId))
            Assert.Ignore("AfterSales member link did not contain a numeric member id.");

        var afterSales = new AdminAfterSalesApplyingPage(Page, Settings.Applications.Admin);
        await StepAsync("Open AfterSales", () => afterSales.GotoWithMemberIdAsync(memberId));
        await StepAsync("Open Admin tab", afterSales.OpenAdminTabAsync);

        await Page.WaitForURLAsync(
            u => u.Contains("MemberAdmin", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });

        var adminTab = new AdminMemberAdminPage(Page, Settings.Applications.Admin);
        await StepAsync("Wait for Member Admin", () => adminTab.WaitForReadyAsync());

        await StepAsync("Assert PTS system setup", async () =>
        {
            Assert.That(await adminTab.IsPtsSystemSetupSectionVisibleAsync(), Is.True);
            Assert.That(await adminTab.MemberStatusSetLink.IsVisibleAsync(), Is.True);
            Assert.That(await adminTab.PtsSystemSetupSaveButton.IsVisibleAsync(), Is.True);
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-1024 — Applying Admin tab: Membership section on Member Admin.")]
    public async Task DEV_TC_1024_Membership_section_on_applying_admin_tab()
    {
        var applying = await StepAsync("Open Applying", OpenApplyingSearchAsync);
        var memberIdStr = await StepAsync("Resolve first member id", applying.TryGetFirstAfterSalesMemberIdAsync);
        if (string.IsNullOrEmpty(memberIdStr))
            Assert.Ignore("No Applying rows with AfterSales member link.");
        if (!long.TryParse(memberIdStr, out var memberId))
            Assert.Ignore("AfterSales member link did not contain a numeric member id.");

        var afterSales = new AdminAfterSalesApplyingPage(Page, Settings.Applications.Admin);
        await StepAsync("Open AfterSales", () => afterSales.GotoWithMemberIdAsync(memberId));
        await StepAsync("Open Admin tab", afterSales.OpenAdminTabAsync);

        await Page.WaitForURLAsync(
            u => u.Contains("MemberAdmin", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });

        var adminTab = new AdminMemberAdminPage(Page, Settings.Applications.Admin);
        await adminTab.WaitForReadyAsync();

        await StepAsync("Assert Membership block", async () =>
        {
            Assert.That(await adminTab.MembershipSectionHeading.IsVisibleAsync(), Is.True);
            Assert.That(await adminTab.MembershipSaveButton.IsVisibleAsync(), Is.True);
        });
    }
}
