using System.Globalization;

namespace PTS.Automation.Pages.Shared.UiComponents;

/// <summary>
/// The PTS WebUI date inputs are typically bound to the format
/// <c>dd-MMM-yyyy</c> (e.g. <c>15-Dec-2026</c>) and are editable via plain
/// text entry plus blur. Rather than driving the calendar widget (which is
/// brittle), we fill the input directly.
///
/// For date inputs that rely on JS to format their value on change, call
/// <see cref="FillAsync"/> which sends an explicit change + blur event.
/// </summary>
public sealed class PtsDatePicker
{
    public const string AppDateFormat = "dd-MMM-yyyy";

    private readonly ILocator _input;

    public PtsDatePicker(IPage page, string selector) : this(page.Locator(selector)) { }
    public PtsDatePicker(ILocator input) { _input = input; }

    /// <summary>Formats <paramref name="date"/> to <c>dd-MMM-yyyy</c> and fills the input.</summary>
    public Task FillAsync(DateTime date) =>
        FillAsync(date.ToString(AppDateFormat, CultureInfo.InvariantCulture));

    /// <summary>
    /// Fills the input with a pre-formatted string. Caller is responsible for
    /// matching the app's expected format.
    /// </summary>
    public async Task FillAsync(string value)
    {
        await _input.FillAsync(value);
        // Some of the app's date inputs rely on blur to trigger their own
        // validation / formatting. Dispatch both change and blur explicitly.
        await _input.DispatchEventAsync("change");
        await _input.DispatchEventAsync("blur");
    }

    public Task<string> GetValueAsync() => _input.InputValueAsync();
}
