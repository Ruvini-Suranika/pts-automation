using PTS.Automation.Data.Builders;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Clients;
using PTS.Automation.Pages.Member.Dashboard;

namespace PTS.Automation.Features.Member.Clients;

/// <summary>
/// Vertical slice: creating a client via the Add Client modal on the
/// Member → Clients page. These tests prove the full stack end-to-end for
/// a "create feature":
///   login → dashboard → nav → page readiness → modal → form fill → submit
///   → server response → post-create redirect.
/// </summary>
[TestFixture]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category(Categories.Clients)]
public class CreateClientTests : MemberTest
{
    [Test]
    [Category(Categories.UI)]
    [Description("Creating a client with valid details POSTs successfully and " +
                 "the browser is redirected to the new client's details page.")]
    public async Task Member_creates_client_with_valid_details()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var clients   = new ClientListPage(Page, Settings.Applications.Member);
        var details   = new ClientDetailsPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard (auth state already primed)", () => dashboard.GotoAsync());

        await StepAsync("Navigate to Clients via nav bar", async () =>
        {
            await dashboard.NavBar.GoToClientsAsync();
            await clients.WaitForReadyAsync();
        });

        var modal = await StepAsync("Open Add Client modal",
            () => clients.OpenAddClientAsync());

        var client = new ClientBuilder().WithDefaults().Build();
        Logger.Information("Generated test client: {First} {Last} <{Email}>, {Phone}",
            client.FirstName, client.LastName, client.Email, client.Phone);

        var accepted = await StepAsync("Fill form + submit",
            () => modal.SubmitAsync(client));

        Assert.That(accepted, Is.True,
            "Server rejected the Add Client POST. Likely cause: a required field is empty " +
            "or the phone format doesn't satisfy intlTelInput's validator.");

        await StepAsync("Verify redirect to client details page", async () =>
        {
            await Page.WaitForURLAsync(
                url => url.Contains("/Client/ClientDetails", StringComparison.OrdinalIgnoreCase),
                new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });

            Assert.That(details.IsCurrent(), Is.True,
                $"After successful create we must land on /Client/ClientDetails. Actual URL: {Page.Url}");

            var newRef = details.ExtractClientRefFromUrl();
            Assert.That(newRef, Is.Not.Null.And.Not.Empty,
                $"Could not parse new client reference from URL: {Page.Url}");
            Logger.Information("New client ref: {Ref}", newRef);
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("Clicking Add on an empty form does NOT submit — " +
                 "the modal stays open, the client-side validator shows errors, " +
                 "and no /Client/AddClientDetail POST fires.")]
    public async Task Empty_form_submission_is_rejected_client_side()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var clients   = new ClientListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard", () => dashboard.GotoAsync());

        await StepAsync("Navigate to Clients", async () =>
        {
            await dashboard.NavBar.GoToClientsAsync();
            await clients.WaitForReadyAsync();
        });

        var modal = await StepAsync("Open Add Client modal",
            () => clients.OpenAddClientAsync());

        // Watch for the POST — it must NOT fire when required fields are empty.
        var addPostFired = false;
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" &&
                request.Url.Contains("/Client/AddClientDetail", StringComparison.OrdinalIgnoreCase))
            {
                addPostFired = true;
            }
        };

        await StepAsync("Click Add with empty form", () => modal.SubmitEmptyAsync());

        // Give the client-side validator a moment to render error elements.
        await Page.WaitForTimeoutAsync(300);

        await StepAsync("Assert validation behaviour", async () =>
        {
            Assert.That(await modal.IsOpenAsync(), Is.True,
                "Modal should remain open after an invalid submission.");

            Assert.That(addPostFired, Is.False,
                "No POST to /Client/AddClientDetail should fire when the form is invalid.");

            var errorCount = await modal.ValidationErrorCountAsync();
            Assert.That(errorCount, Is.GreaterThan(0),
                "Expected at least one inline validation error to be rendered.");
            Logger.Information("Inline validation errors shown: {Count}", errorCount);
        });
    }
}
