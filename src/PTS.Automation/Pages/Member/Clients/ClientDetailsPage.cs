using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// Client details landing page — where the user lands after successfully
/// creating a client. URL: <c>/Client/ClientDetails/{clientRefNumber}</c>.
/// Controller action: <c>ClientController.ClientDetails</c>.
///
/// SKELETON (minimal): enough to verify the post-create redirect landed on a
/// valid details page. Full POM will be built when a feature explicitly tests
/// the details page.
/// </summary>
public sealed class ClientDetailsPage : MemberPage
{
    public ClientDetailsPage(IPage page, AppUrl app) : base(page, app) { }

    /// <summary>Not used for navigation — this page is reached via redirect only.</summary>
    public override string RelativePath => "Client/ClientDetails";

    protected override ILocator ReadinessIndicator => Page.Locator("article").First;

    /// <summary>Returns <c>true</c> if the current URL matches the ClientDetails route pattern.</summary>
    public bool IsCurrent() =>
        Page.Url.Contains("/Client/ClientDetails", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Extracts the new client's reference code from the URL, or <c>null</c>
    /// if not on this page. The reference is alphanumeric (typically
    /// <c>{memberId}A{sequence}</c>, e.g. <c>5305A2300117</c>), NOT a plain int.
    /// </summary>
    public string? ExtractClientRefFromUrl()
    {
        // Expected URL shape: .../Client/ClientDetails/{ref}
        //                or:  .../Client/ClientDetails?Id={ref}
        var url = new Uri(Page.Url);
        var segments = url.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length >= 3 &&
            segments[^3].Equals("Client", StringComparison.OrdinalIgnoreCase) &&
            segments[^2].Equals("ClientDetails", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(segments[^1]))
        {
            return segments[^1];
        }

        var query = System.Web.HttpUtility.ParseQueryString(url.Query);
        var idParam = query["Id"];
        return string.IsNullOrWhiteSpace(idParam) ? null : idParam;
    }
}
