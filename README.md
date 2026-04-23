# PTS.Automation

Playwright test automation framework for the Protected Trust Services (PTS) platform. Covers the Member (travel agency) and Admin (PTS staff) surfaces of the PTS.WebUI application, with room to grow to two more systems without structural change.

## Requirements

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ (repo targets `net8.0`) |
| PowerShell | 7+ (for Playwright browser install script) |
| OS | Windows / Linux / macOS |

## First-time setup

```powershell
cd C:\Users\Laptop\source\PTS\Automation

dotnet restore
dotnet build

# Install Playwright browsers (Chromium + deps). Run once per dev machine.
pwsh src/PTS.Automation/bin/Debug/net8.0/playwright.ps1 install --with-deps
```

## Configure credentials (local dev)

Credentials are **never** committed. Use `dotnet user-secrets`:

```powershell
cd src\PTS.Automation

dotnet user-secrets set "Users:Member:Username"     "your.member.user@example.com"
dotnet user-secrets set "Users:Member:Password"     "…"
dotnet user-secrets set "Users:PtsAdmin:Username"   "your.admin.user@example.com"
dotnet user-secrets set "Users:PtsAdmin:Password"   "…"
```

In CI (Azure DevOps) set the corresponding secret pipeline variables using the `__` separator:

- `PTS_Users__Member__Username`, `PTS_Users__Member__Password`
- `PTS_Users__PtsAdmin__Username`, `PTS_Users__PtsAdmin__Password`

## Run tests

```powershell
# All tests (QA environment is the default)
dotnet test

# Smoke only
dotnet test --filter "Category=Smoke"

# Member regression only
dotnet test --filter "Category=Regression&Category=Member"

# A different environment (once additional appsettings.{env}.json files exist)
$env:TEST_ENV = "uat"; dotnet test

# Headed (watch the browser)
$env:HEADED = "1"; dotnet test --filter "Category=Smoke"

# One-shot script that verifies the Member login flow end-to-end and writes
# the full console output to TestResults/login-verification.txt.
.\build\verify-member-login.ps1 -Username "<qa user>" -Password "<qa pwd>"
```

## Environment variables cheat-sheet

| Variable | Purpose | Default |
|----------|---------|---------|
| `TEST_ENV`                         | Selects `appsettings.{env}.json` layer | `qa` |
| `HEADED`                           | `1` / `true` / `on` forces headed; `0` / `false` forces headless | from `Browser.Headless` in `appsettings` |
| `BROWSER`                          | `chromium` / `firefox` / `webkit` | `chromium` |
| `PWDEBUG`                          | `console` enables Playwright Inspector (headed + step) | off |
| `PTS_Users__Member__Username`      | Member login (CI secret) | user-secrets |
| `PTS_Users__Member__Password`      | Member login (CI secret) | user-secrets |
| `PTS_Users__PtsAdmin__Username`    | Admin login (CI secret)  | user-secrets |
| `PTS_Users__PtsAdmin__Password`    | Admin login (CI secret)  | user-secrets |

## Repo layout (top level)

```
Automation/
├── PTS.Automation.sln
├── Directory.Build.props          shared C# settings
├── Directory.Packages.props       central NuGet version management
├── .runsettings                   dotnet test settings (parallelism, env)
├── src/PTS.Automation/            the single test project
├── ci/azure-pipelines.yml         Azure DevOps pipeline
└── docs/ARCHITECTURE.md           fixtures, auth caching, adding pages/tests
```

See `docs/ARCHITECTURE.md` for internals and guidance on adding new pages and tests.
