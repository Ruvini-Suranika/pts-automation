using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Debits;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Debits;

/// <seealso cref="Pages.Admin.Debits.AdminTrusteesAuthorisationPage"/>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Trustees authorisation")]
[AllureTag(Categories.P0)]
[AllureTag(Categories.EpicTrustee)]
[AllureTag(Categories.Debits)]
[Category(Categories.EpicTrustee)]
[Category(Categories.Debits)]
public sealed class AdminTrusteesAuthorisationP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-E1 — Trustee's Authorisation list readiness.")]
    public async Task ADMIN_P0_E1_Trustees_authorisation_screen_validation()
    {
        await StepAsync("Land on Admin Enquiries", () => LandOnAdminEnquiriesAsync());

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await StepAsync("Open Trustees Authorisation", () => nav.GoToTrusteesAuthorisationAsync());

        var pageObj = new AdminTrusteesAuthorisationPage(Page, Settings.Applications.Admin);
        await StepAsync("Wait for Trustees Authorisation page", () => pageObj.WaitForReadyAsync());

        await StepAsync("Verify Debits Trustees URL", async () =>
        {
            Assert.That(Page.Url, Does.Contain("DebitsTrustees").IgnoreCase);
        });

        await StepAsync("Verify Authorise bulk button is visible", async () =>
        {
            Assert.That(await pageObj.IsAuthoriseBulkButtonVisibleAsync(), Is.True);
        });
    }
}
