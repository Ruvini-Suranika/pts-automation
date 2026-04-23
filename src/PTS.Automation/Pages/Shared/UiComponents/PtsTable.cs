namespace PTS.Automation.Pages.Shared.UiComponents;

/// <summary>
/// Generic helper for HTML tables / grids rendered by the PTS WebUI. The app
/// uses a mix of static server-rendered tables and DataTables-style AJAX
/// grids; both share the &lt;tr&gt; / &lt;td&gt; structure this helper relies on.
///
/// Build with either:
///   - a CSS selector string (e.g. <c>"#clientsTableBody"</c>), or
///   - an <see cref="ILocator"/> for pre-scoped flexibility.
///
/// <para>
/// <b>Not</b> a full DataTables API — intentionally small; add to it only when
/// a test needs something the current helpers don't cover.
/// </para>
/// </summary>
public sealed class PtsTable
{
    private readonly ILocator _body;

    public PtsTable(IPage page, string bodySelector) : this(page.Locator(bodySelector)) { }

    public PtsTable(ILocator tableBody)
    {
        _body = tableBody;
    }

    /// <summary>All visible body rows.</summary>
    public ILocator Rows => _body.Locator("tr");

    /// <summary>Return the nth row (1-based to match CSS nth-of-type).</summary>
    public ILocator Row(int indexOneBased) => Rows.Nth(indexOneBased - 1);

    /// <summary>Count of body rows currently rendered.</summary>
    public Task<int> RowCountAsync() => Rows.CountAsync();

    /// <summary>
    /// Rows containing the supplied text anywhere inside them.
    /// Useful for <c>RowsContaining("alice@example.com").First</c>.
    /// </summary>
    public ILocator RowsContaining(string text) =>
        Rows.Filter(new LocatorFilterOptions { HasText = text });

    /// <summary>
    /// Returns the inner text of the <paramref name="columnIndex"/>-th cell of
    /// <paramref name="rowIndex"/> (both 1-based). Throws if out of range.
    /// </summary>
    public async Task<string> CellTextAsync(int rowIndex, int columnIndex)
    {
        var cell = Rows.Nth(rowIndex - 1).Locator("td").Nth(columnIndex - 1);
        return (await cell.InnerTextAsync()).Trim();
    }

    /// <summary>
    /// Waits until at least one row is rendered (use after an AJAX fetch).
    /// Throws if no rows appear within the timeout.
    /// </summary>
    public async Task WaitForAnyRowAsync(int timeoutMs = 30_000)
    {
        await Rows.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
    }
}
