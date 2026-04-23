namespace PTS.Automation.Pages.Shared.UiComponents;

/// <summary>
/// Helper for both native HTML <c>&lt;select&gt;</c> elements and the app's
/// custom "pts-dropdown" widget.
///
/// Prefer <see cref="SelectByLabelAsync"/> for readability in tests — it maps
/// directly to what a human sees.
/// </summary>
public sealed class PtsDropdown
{
    private readonly ILocator _root;

    public PtsDropdown(IPage page, string selector) : this(page.Locator(selector)) { }
    public PtsDropdown(ILocator root) { _root = root; }

    public ILocator Root => _root;

    // ── Native <select> ────────────────────────────────────────────────────
    public Task SelectByValueAsync(string value) =>
        _root.SelectOptionAsync(new SelectOptionValue { Value = value });

    public Task SelectByLabelAsync(string label) =>
        _root.SelectOptionAsync(new SelectOptionValue { Label = label });

    public Task SelectByIndexAsync(int index) =>
        _root.SelectOptionAsync(new SelectOptionValue { Index = index });

    public async Task<string> GetSelectedValueAsync() =>
        await _root.EvaluateAsync<string>("el => el.value") ?? "";

    public async Task<string> GetSelectedLabelAsync() =>
        await _root.EvaluateAsync<string>(
            "el => el.options[el.selectedIndex]?.textContent ?? ''") ?? "";

    public async Task<IReadOnlyList<string>> GetOptionLabelsAsync() =>
        await _root.Locator("option").AllInnerTextsAsync();

    public async Task<IReadOnlyList<string>> GetOptionValuesAsync() =>
        await _root.Locator("option").EvaluateAllAsync<string[]>(
            "opts => opts.map(o => o.value)");

    /// <summary>
    /// Selects the first option that is neither blank nor the common "0"
    /// placeholder. Handy when a dropdown's first real value isn't stable.
    /// </summary>
    public async Task SelectFirstRealOptionAsync()
    {
        var values = await GetOptionValuesAsync();
        var firstReal = values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v) && v != "0");
        if (firstReal is not null)
            await SelectByValueAsync(firstReal);
    }
}
