namespace PTS.Automation.Pages.Admin.Applying;

/// <summary>Applying-stage member header + tabs (<c>Admin/AfterSales/{id}</c>, <c>_headerPartial.cshtml</c>).</summary>
public sealed class AdminAfterSalesApplyingPage : AdminPage
{
    public AdminAfterSalesApplyingPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath =>
        throw new InvalidOperationException("Use GotoWithMemberIdAsync.");

    protected override ILocator ReadinessIndicator => GetInviteButton;

    public ILocator GetInviteButton => Page.Locator("#GetInviteButton");

    public ILocator NotesButton => Page.Locator("a.notes").Filter(new() { HasText = "Notes" }).First;

    public ILocator MoveToRiskOpenButton => Page.Locator("#btn-headerPartial");

    public ILocator MoveToRiskModal => Page.Locator("#movetoadmin");

    public ILocator MoveToRiskAssigneeSelect => MoveToRiskModal.Locator("select#move-to-risk");

    public ILocator MoveToRiskSaveButton => MoveToRiskModal.Locator("#update-assign-user");

    public ILocator InviteLinkModal => Page.Locator("#getInvite");

    public ILocator InviteLinkTextBox => Page.Locator("#txtInviteUser");

    public ILocator EmailLinkButton => InviteLinkModal.GetByRole(AriaRole.Button, new() { Name = "Email Link" });

    public ILocator RecipientsEmailModal => Page.Locator("#senddebitsemail1");

    public ILocator EmailSubjectInput => RecipientsEmailModal.Locator("#Subject");

    public ILocator NotesOffcanvas => Page.Locator("#offcanvasRight");

    public async Task GotoWithMemberIdAsync(long memberId)
    {
        var path = $"{AdminRoutes.AfterSalesPathPrefix}/{memberId}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

    public Task OpenSalesTabAsync() => Page.Locator("#headerSalesLink").ClickAsync();

    public Task OpenAdminTabAsync() => Page.Locator("#headerAdminLink").ClickAsync();

    public Task OpenGetInviteModalAsync() => GetInviteButton.ClickAsync();

    public Task OpenNotesAsync() => NotesButton.ClickAsync();

    /// <summary>
    /// Opens the Move-to-Risk dialog. The header button targets <c>#MoveToAdmin</c> but markup uses <c>id="movetoadmin"</c>,
    /// so Bootstrap does not wire the click target; we always show <c>#movetoadmin</c> explicitly after the click.
    /// </summary>
    public async Task OpenMoveToRiskModalAsync()
    {
        await MoveToRiskOpenButton.ClickAsync();
        await Page.EvaluateAsync(
            "() => { const el = document.querySelector('#movetoadmin'); const b = window.bootstrap; if (el && b?.Modal) { b.Modal.getOrCreateInstance(el).show(); } }");
        await MoveToRiskModal.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = Settings.Timeouts.DefaultMs
        });
    }

    public Task<IResponse> StartWaitForUpdateTimeLineStatusAsync() =>
        Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/UpdateTimeLineStatus", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

    public Task<IResponse> StartWaitForGetOnboardingEmailTemplateAsync() =>
        Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/GetOnboardingGetEmailTemplate", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

    public Task<IResponse> StartWaitForGetMemberContactEmailAsync() =>
        Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/GetMemberContactEmail", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "GET", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });
}
