# Architecture

## Layers

```
┌──────────────────────────────────────────────────────────────┐
│  Features/                                                   │   ← tests (business slices)
│     Member/Authentication, Member/Debits, Admin/Debits, …    │
└──────────────────────────────────────────────────────────────┘
                 │ uses
                 ▼
┌──────────────────────────────────────────────────────────────┐
│  Pages/                                                      │   ← Page Object Model
│     Member/Auth/LoginPage, Admin/Debits/DebitApprovalPage,…  │
│     BasePage (shared helpers)                                │
│     Shared/UiComponents/ (modal, toast, table, dropdown,     │
│                          date picker, spinner)               │
└──────────────────────────────────────────────────────────────┘
                 │ uses
                 ▼
┌──────────────────────────────────────────────────────────────┐
│  Infrastructure/                                             │   ← framework plumbing
│     Config      → TestSettings, ConfigFactory                │
│     Fixtures    → BaseTest → MemberTest / AdminTest          │
│     Playwright  → ContextOptionsFactory,                     │
│                   BrowserLaunchOptionsFactory,               │
│                   AuthStateCache, TraceArtifactHook          │
│     Reporting   → Log (Serilog), Allure hook                 │
│     GlobalTestHooks (assembly-level run banner + cleanup)    │
└──────────────────────────────────────────────────────────────┘
```

## Fixture hierarchy

| Fixture | Role | Context |
|---------|------|---------|
| `PageTest` (Playwright.NUnit) | — | Fresh `IBrowserContext` + `IPage` per test, shared browser per worker |
| `BaseTest : PageTest` | none | Adds config, logging, tracing, screenshots |
| `MemberTest : BaseTest` | Member | Pre-loads Member `storageState` — tests start logged in |
| `AdminTest  : BaseTest` | PtsAdmin | Pre-loads Admin `storageState` — tests start logged in |

A test opts into a role by inheriting the right fixture:

```csharp
public class CreateCurrencyDebitTests : MemberTest { … }     // logged-in Member
public class ApproveDebitTests       : AdminTest  { … }     // logged-in PtsAdmin
public class LoginPageSmokeTests     : BaseTest   { … }     // no auth
```

## Auth state caching (why tests are fast)

Traditional POM frameworks log in on every test → 5-second tax × N tests. This framework logs in once per role per test run:

1. First test in a `MemberTest` fixture triggers `[OneTimeSetUp] PrimeMemberAuthState`.
2. It opens a throwaway browser context, performs a UI login, and writes Playwright's `storageState` to `.auth/member.storage-state.json`.
3. Every subsequent test (of any `MemberTest` subclass, in any parallel worker) creates its context with `StorageStatePath = .auth/member.storage-state.json` — the session cookies are loaded and the test starts already authenticated.
4. `.auth/` is gitignored and rebuilt each run.

## Configuration

Layered, later wins:

| Source | Purpose | Commit? |
|--------|---------|---------|
| `appsettings.json` | shared defaults (browser, timeouts) | yes |
| `appsettings.qa.json` | QA URLs, user-key placeholders | yes |
| `appsettings.local.json` | developer overrides (copy from `appsettings.local.json.sample`) | **no** (gitignored) |
| `dotnet user-secrets` | local dev credentials | no (per-machine) |
| env vars `PTS_*` | CI secrets (double-underscore = nesting) | no |

`TEST_ENV` env var selects which `appsettings.{env}.json` layer is loaded (default `qa`).

## Runtime overrides

Certain settings can be flipped at run time without editing any file:

| Env var | Effect | Wins over |
|---------|--------|-----------|
| `HEADED=1` / `HEADED=0` | Forces visible / hidden browser window | `Browser.Headless` in config |
| `BROWSER=firefox` | Selects browser engine | `Browser.Name` in config |
| `PWDEBUG=console` | Launches Playwright Inspector (step-through debugging) | all others |

**Why not `BrowserOptions()` override?** Playwright-NUnit manages browser launch per worker via its own `BrowserService`, and exposes no supported override on `PageTest`. Launch is therefore controlled via env vars. To make `Browser.Headless` from `appsettings` meaningful, `GlobalTestHooks.BeforeAllTests()` calls `BrowserLaunchOptionsFactory.ApplyConfigToEnv(...)`, which sets `HEADED=1` when the config asks for headed and no explicit env var was passed. The env var always wins.

## Adding a new page

1. Create `src/PTS.Automation/Pages/{System}/{Feature}/{Name}Page.cs`.
2. Inherit `BasePage`.
3. Implement `RelativePath` and `ReadinessIndicator`.
4. Expose locators as `private ILocator`, actions as `public async Task …`, queries as `public async Task<…> …`.
5. **Never put selectors in tests.**

Example skeleton:

```csharp
public sealed class CurrencyDebitPage : BasePage
{
    public CurrencyDebitPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => "member/financial/debits/currency/new";

    private ILocator SaveButton => Page.GetByRole(AriaRole.Button, new() { Name = "Save" });
    protected override ILocator ReadinessIndicator => SaveButton;

    public async Task FillAsync(CurrencyDebit d) { … }
    public async Task SubmitAsync() => await SaveButton.ClickAsync();
}
```

## Adding a new test

1. Create `src/PTS.Automation/Features/{System}/{Feature}/{Name}Tests.cs`.
2. Inherit the appropriate fixture (`MemberTest`, `AdminTest`, or `BaseTest`).
3. Tag with `[Category(Categories.Xxx)]` for CI filtering.
4. Use Playwright's `Expect()` for web assertions and NUnit `Assert.That()` for domain assertions.
5. No `Thread.Sleep`. No `WaitForTimeout`. Only explicit waits.
6. Wrap logical sub-steps with `StepAsync("Open add-client modal", …)` for readable logs.

## Reusable UI components

Under `Pages/Shared/UiComponents/`:

| Component | Purpose |
|-----------|---------|
| `PtsSpinner` | Waits for the `.loading` overlay to disappear (already wired into `BasePage.WaitForReadyAsync`). |
| `PtsModal` | Generic Bootstrap modal wrapper — title, close, wait-open/close, click named button. |
| `PtsToast` | Success / error toast detection + text extraction. |
| `PtsTable` | Row / cell queries + "find row containing text". |
| `PtsDropdown` | Native `<select>` helpers (by label/value/index, first-real-option). |
| `PtsDatePicker` | Fills date inputs in the app's `dd-MMM-yyyy` format and dispatches `change`/`blur`. |

Page objects should compose these rather than re-implement them. Example:

```csharp
public sealed class MyPage : BasePage
{
    private readonly PtsTable _results;
    private readonly PtsDropdown _typeFilter;

    public MyPage(IPage page, AppUrl app) : base(page, app)
    {
        _results    = new PtsTable(page, "#resultsTableBody");
        _typeFilter = new PtsDropdown(page, "#dropDownType");
    }

    public Task FilterByTypeAsync(string label) => _typeFilter.SelectByLabelAsync(label);
    public Task<int> ResultCountAsync()         => _results.RowCountAsync();
}
```

## Artifacts on failure

When a test fails, the framework automatically:

1. Takes a full-page screenshot → `TestResults/screenshots/{TestFullName}.png`
2. Saves the Playwright trace → `TestResults/traces/{TestFullName}.zip`
3. Retains the video (if `Browser.Video` is `on` or `retain-on-failure`)
4. Attaches all of the above to `TestContext` so Azure DevOps pipeline artifacts pick them up

Replay a trace locally:

```powershell
pwsh src/PTS.Automation/bin/Debug/net8.0/playwright.ps1 show-trace TestResults\traces\MyTest.zip
```

## Adding a new system (System 3, System 4)

The layout is designed for this:

1. Add `Applications:{NewSystem}` entry to each `appsettings.{env}.json`.
2. Add a new property to `ApplicationsSettings` in `TestSettings.cs`.
3. Create `Pages/{NewSystem}/…` and `Features/{NewSystem}/…` folders.
4. If the new system has its own login/role, add a `{NewSystem}Test.cs` fixture next to `MemberTest.cs` and a matching role in `UsersSettings`.
5. No changes to existing tests, pages, or config required.
