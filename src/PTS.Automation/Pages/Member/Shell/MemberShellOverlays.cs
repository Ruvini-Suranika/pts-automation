namespace PTS.Automation.Pages.Member.Shell;

/// <summary>
/// Bootstrap modals that auto-open on the Member shell (for example calendar notes)
/// sit above the header and intercept clicks on profile/settings — dismiss them
/// before driving shell chrome.
/// </summary>
public static class MemberShellOverlays
{
    /// <summary>
    /// If the calendar-notes modal is shown, close it so header controls are clickable.
    /// </summary>
    public static async Task DismissCalendarNotesModalIfBlockingAsync(IPage page)
    {
        var modal = page.Locator("#calendar-notes-modal.show");
        if (await modal.CountAsync() == 0)
            return;
        if (!await modal.IsVisibleAsync())
            return;

        var closeBtn = modal.Locator("button.btn-close, button[data-bs-dismiss='modal']").First;
        try
        {
            if (await closeBtn.IsVisibleAsync())
                await closeBtn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
        }
        catch (TimeoutException)
        {
            // Fall through to Escape.
        }

        if (await modal.IsVisibleAsync())
        {
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForTimeoutAsync(250);
        }

        try
        {
            await modal.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = ConfigFactory.Settings.Timeouts.DefaultMs
            });
        }
        catch (TimeoutException)
        {
            // Best-effort — caller may still proceed if overlay cleared partially.
        }
    }
}
