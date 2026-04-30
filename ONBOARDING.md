# PTS.Automation — onboarding (~30 minutes)

Get the Playwright test suite running locally, then optionally generate an Allure HTML report. For deeper architecture and config layering, see [README.md](README.md) and [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

---

## Prerequisites

| Requirement | Notes |
|-------------|--------|
| **.NET SDK** | **9.x** (the test project targets `net9.0`). Verify: `dotnet --version`. |
| **Git** | To clone the repo. |
| **PowerShell** | **7+** recommended (`pwsh`) for running the Playwright install script cross-platform. Windows PowerShell 5.1 often works for the same script on Windows. |
| **Playwright browsers** | Installed via the **.NET** Playwright CLI script after build (see below). **Node.js is not required** for this repo (no `package.json`). |
| **Allure** (optional) | [Allure 2](https://github.com/allure-framework/allure2/releases) CLI on your `PATH` if you want `allure generate` / `allure open`. Not required to run tests. |
| **Network** | Access to the PTS environment you target (default **QA** URLs in `appsettings.qa.json`). VPN or corporate network rules may apply. |

---

## 1. Clone and open the solution (5 min)

```powershell
git clone <repository-url>
cd <repo-root>/Automation
```

Open `PTS.Automation.sln` in your IDE, or stay in the terminal for the steps below.

---

## 2. Restore, build, install browsers (10 min)

From the **`Automation`** folder (the directory that contains `PTS.Automation.sln`):

```powershell
dotnet restore
dotnet build
```

Install Chromium (and OS dependencies) **once per machine**. The script path matches your **configuration** and **target framework**:

```powershell
# After a Debug build (default)
pwsh .\src\PTS.Automation\bin\Debug\net9.0\playwright.ps1 install --with-deps
```

If you build **Release**, use `bin\Release\net9.0\` instead.

**Verify:** `dotnet test --filter "FullyQualifiedName~AdminLoginSmokeTests"` (or any small smoke filter) completes without “Executable doesn’t exist” browser errors.

---

## 3. Configure local credentials (10 min)

Passwords and tokens **must not** be committed. Use **.NET user secrets** tied to this project.

**UserSecretsId** (from `src/PTS.Automation/PTS.Automation.csproj`):  
`pts-automation-4f2a9c01-7d6b-4a8e-9f1c-5b3d8e2a6c0f`

```powershell
cd .\src\PTS.Automation

dotnet user-secrets set "Users:Member:Username"    "<your-member-email-or-username>"
dotnet user-secrets set "Users:Member:Password"    "<your-member-password>"
dotnet user-secrets set "Users:PtsAdmin:Username"  "<your-pts-admin-username>"
dotnet user-secrets set "Users:PtsAdmin:Password"  "<your-pts-admin-password>"
```

**Check:** `dotnet user-secrets list` (values are masked in some tooling).

**Optional overrides** (non-secret preferences):

- Copy `appsettings.local.json.sample` → `appsettings.local.json` next to `appsettings.json` (same folder). This file is **gitignored** — do not commit it.
- Prefer user secrets for **any** secret; use `appsettings.local.json` for things like `Browser:Headless: false` if you wish.

**Environment selection:** By default **`TEST_ENV`** is **`qa`**, which loads `appsettings.qa.json`. To change later, set the environment variable `TEST_ENV` before `dotnet test`, and add a matching `appsettings.<env>.json` if it does not exist yet.

---

## 4. Run tests (5 min)

From **`Automation`**:

```powershell
# Full suite (can take a long time)
dotnet test

# Smoke only (faster sanity check)
dotnet test --filter "Category=Smoke"

# With repo runsettings (TEST_ENV, workers, etc.)
dotnet test --settings .\.runsettings

# Headed browser (see also headed.runsettings)
$env:HEADED = "1"
dotnet test --filter "Category=Smoke"
```

**Menu runner (Windows):** `RunTests.bat` — interactive filters, sets headed + slowmo, runs `dotnet test`, then Allure if installed.

**Artifacts:** Traces, videos, screenshots, and logs go under `src/PTS.Automation/TestResults/` (relative to the test process working directory / output layout). Cached login state uses `.auth/` (gitignored).

---

## 5. View the Allure report (5 min, optional)

1. Run tests so JSON results are produced under:

   `src\PTS.Automation\bin\Debug\net9.0\allure-results`

   (Same **Configuration** / **TFM** as your build; `RunTests.bat` uses `CONFIG=Debug`, `TFM=net9.0`.)

2. Ensure **`allure`** is on your PATH.

3. From **`Automation`**:

```powershell
allure generate ".\src\PTS.Automation\bin\Debug\net9.0\allure-results" --clean -o ".\allure-report"
allure open ".\allure-report"
```

If `allure generate` says there are no results, the results path does not match your last build (wrong **Debug/Release** or **net9.0** folder), or tests did not run successfully enough to emit Allure files.

---

## 6. Common troubleshooting

| Symptom | What to check |
|---------|----------------|
| **“Credentials not configured” / tests ignored** | Run `dotnet user-secrets list` from `src\PTS.Automation`. Set `Users:Member:*` and/or `Users:PtsAdmin:*`. In CI, use `PTS_Users__Member__Username` style vars (double underscore = nested key). |
| **Browser / executable errors** | Re-run `playwright.ps1 install --with-deps` from the correct **`bin\<Configuration>\net9.0`**` folder after `dotnet build`. |
| **`Could not find a part of the path` for config** | Run tests via `dotnet test` so the working directory and copied `appsettings*.json` in output are consistent. |
| **Wrong environment / URL** | `TEST_ENV` selects `appsettings.{TEST_ENV}.json`. Default is `qa`. |
| **Allure folder empty or report missing** | Results are under **`bin\<Configuration>\net9.0\allure-results`**, not `bin\allure-results`. Align `allure generate` with that path. |
| **`allure` command not found** | Install Allure 2 and add its `bin` to PATH, or open only TRX/HTML from `dotnet test --logger trx` if you skip Allure. |
| **Headless vs headed confusion** | `HEADED=1` forces headed. `Browser:Headless` in JSON is bridged in `GlobalTestHooks` unless `HEADED` is already set — see [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md). |
| **README says `net8.0` but build is `net9.0`** | Trust **`PTS.Automation.csproj`** `TargetFramework` for Playwright script and Allure paths. |
| **401 / login failures** | Wrong QA user, expired password, or environment not reachable (VPN). |

---

## Quick reference

| Item | Value / location |
|------|-------------------|
| Solution | `Automation/PTS.Automation.sln` |
| Test project | `Automation/src/PTS.Automation/PTS.Automation.csproj` |
| Default env | `TEST_ENV=qa` → `appsettings.qa.json` |
| User secrets ID | `pts-automation-4f2a9c01-7d6b-4a8e-9f1c-5b3d8e2a6c0f` |
| Config layering | `README.md` + `docs/ARCHITECTURE.md` |

Welcome to the team — if anything in this doc drifts from the repo (TFM, paths), update **this file** in the same PR as the change.
