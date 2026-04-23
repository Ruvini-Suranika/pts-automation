using PTS.Automation.Data.Models;
using PTS.Automation.Pages.Shared.UiComponents;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// The "Add client" modal hosted inside <see cref="ClientListPage"/>.
/// Backed by a <see cref="PtsModal"/> for open/close/title semantics; adds the
/// feature-specific form interactions on top.
///
/// Source: <c>Views/Client/ClientSearchView.cshtml</c> (modal <c>#Addclient</c>)
/// + <c>Scripts/client/client.js</c> (<c>#addClientButton</c> click handler
/// which POSTs to <c>/Client/AddClientDetail</c>).
///
/// Note on submit behaviour: the dev JS navigates the browser to
/// <c>/Client/ClientDetails/{newId}</c> on success — NOT a toast + grid refresh.
/// <see cref="SubmitAsync"/> therefore returns the URL we landed on.
/// </summary>
public sealed class AddClientModal
{
    private readonly IPage _page;
    private readonly TestSettings _settings;
    private readonly PtsModal _modal;

    public AddClientModal(IPage page)
    {
        _page = page;
        _settings = ConfigFactory.Settings;
        _modal = new PtsModal(page, "#Addclient");
    }

    // ── Locators ────────────────────────────────────────────────────────────
    private ILocator Root             => _page.Locator("#Addclient");
    private ILocator TitleSelect      => _page.Locator("#addClientTitle");
    private ILocator FirstNameInput   => _page.Locator("#addClientFirstName");
    private ILocator LastNameInput    => _page.Locator("#addClientLastName");
    private ILocator EmailInput       => _page.Locator("#addClientEmail");
    private ILocator PhoneInput       => _page.Locator("#countryCode-phone");
    private ILocator EnquiryTypeSel   => _page.Locator("#dropDownClientEnquiryType");
    private ILocator AssignedUserSel  => _page.Locator("#dropDownAddClientAssignUser");
    private ILocator AddButton        => _page.Locator("#addClientButton");
    private ILocator CancelButton     => Root.Locator("pts-button[type='cancel']");
    private ILocator ValidationErrors => Root.Locator("span.error, label.error");

    // ── Waits / queries ─────────────────────────────────────────────────────
    public Task WaitForOpenAsync()   => _modal.WaitForOpenAsync();
    public Task WaitForClosedAsync() => _modal.WaitForClosedAsync();
    public Task<bool> IsOpenAsync()  => _modal.IsOpenAsync();

    /// <summary>Count of inline validation error messages currently shown in the modal.</summary>
    public Task<int> ValidationErrorCountAsync() => ValidationErrors.CountAsync();

    // ── Actions ─────────────────────────────────────────────────────────────
    /// <summary>Fills every field on the modal. Null dropdown values pick the first real option.</summary>
    public async Task FillAsync(Client client)
    {
        await TitleSelect.SelectOptionAsync(new SelectOptionValue { Value = client.Title });
        await FirstNameInput.FillAsync(client.FirstName);
        await LastNameInput.FillAsync(client.LastName);
        await EmailInput.FillAsync(client.Email);
        await PhoneInput.FillAsync(client.Phone);

        await SelectDropdownAsync(EnquiryTypeSel,  client.EnquiryType);
        await SelectDropdownAsync(AssignedUserSel, client.AssignedUser);
    }

    public Task ClickAddAsync()    => AddButton.ClickAsync();
    public Task ClickCancelAsync() => CancelButton.ClickAsync();

    /// <summary>
    /// Fills the form and submits. Waits for either:
    ///   - the <c>/Client/AddClientDetail</c> XHR response (success path), or
    ///   - inline validation errors to appear (rejection path).
    /// Returns <c>true</c> if the server accepted the submission.
    /// </summary>
    public async Task<bool> SubmitAsync(Client client)
    {
        await FillAsync(client);

        var addPostTask = _page.WaitForResponseAsync(
            r => r.Url.Contains("/Client/AddClientDetail", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = _settings.Timeouts.DefaultMs });

        await ClickAddAsync();

        try
        {
            var resp = await addPostTask;
            return resp.Status >= 200 && resp.Status < 300;
        }
        catch (TimeoutException)
        {
            return false; // validation failed — no POST fired
        }
    }

    /// <summary>Clicks Add without filling the form. Used by validation tests.</summary>
    public async Task SubmitEmptyAsync() => await ClickAddAsync();

    // ── Helpers ────────────────────────────────────────────────────────────
    private static async Task SelectDropdownAsync(ILocator select, string? label)
    {
        if (!string.IsNullOrWhiteSpace(label))
        {
            await select.SelectOptionAsync(new SelectOptionValue { Label = label });
            return;
        }

        // Fall back to first real (non-blank, non-"0") option.
        var values = await select.Locator("option").EvaluateAllAsync<string[]>(
            "opts => opts.map(o => o.value)");
        var firstReal = values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v) && v != "0");
        if (firstReal is not null)
            await select.SelectOptionAsync(new SelectOptionValue { Value = firstReal });
    }
}
