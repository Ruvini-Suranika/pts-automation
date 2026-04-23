# Member Portal — Page Inventory

This is the authoritative list of pages the automation framework will model for the **Member** system (`https://qa.pts.cloud/`). It is derived directly from the dev source — nav menu + controller action methods + Razor views — so there is no guesswork.

**Source references**
- Nav menu: [`PTS.WebUI.Web.PTSWeb/Views/Shared/MemberMenu.cshtml`](../../Development/Protected%20Trust%20Services%20Projects/pts.cloud-master/pts.cloud-master/PTS.WebUI.Web.PTSWeb/Views/Shared/MemberMenu.cshtml)
- Top bar / profile / logout: [`_TravelMemberLayout.cshtml`](../../Development/Protected%20Trust%20Services%20Projects/pts.cloud-master/pts.cloud-master/PTS.WebUI.Web.PTSWeb/Views/Shared/_TravelMemberLayout.cshtml)
- Settings mega-menu: [`MemberMegaMenu.cshtml`](../../Development/Protected%20Trust%20Services%20Projects/pts.cloud-master/pts.cloud-master/PTS.WebUI.Web.PTSWeb/Views/Shared/MemberMegaMenu.cshtml)

**Legend**
- **Tier A** = shell / chrome (used by every test, build fully now)
- **Tier B** = primary feature landing (build skeleton now)
- **Tier C** = detail / nested / modal (skeleton only if reached from a Tier B test flow; leave out otherwise)

---

## Shell / chrome (Tier A)

These are **present on every authenticated page** via `_TravelMemberLayout.cshtml`. Build the POM for these now and prove them with a smoke test.

| Component | Source | Notes |
|-----------|--------|-------|
| Top-bar (logo → dashboard, apps, settings mega-menu, help, profile) | `_TravelMemberLayout.cshtml` | Present on every logged-in page |
| `MemberNavBar` (Clients / Calendar / Suppliers / Accounts / Reporting) | `MemberMenu.cshtml` | Primary navigation |
| `MemberSettingsMegaMenu` (API supplier, Contracted suppliers, Downloads, Email, Itinerary, Organisation, Quote, Users) | `MemberMegaMenu.cshtml` | Gear icon `#settingDropdown` |
| `ProfileMenu` (View Profile / Logout) | `_TravelMemberLayout.cshtml` ll.115–126 | Button `#dropdownMenuButton1`, logout link `#logout` |
| `DashboardPage` (post-login landing) | `Member/Index` · [`Views/Member/Index.cshtml`](../../Development/Protected%20Trust%20Services%20Projects/pts.cloud-master/pts.cloud-master/PTS.WebUI.Web.PTSWeb/Views/Member/Index.cshtml) | Post-login target; contains `section.mm-enquiries-head` + debit-note banner |
| `LogoutFlow` | `Account/LogOut` | AJAX POST; `#logout` click calls JS `logout()` |

---

## Clients group (Tier B)

Dropdown "Clients" in the nav.

| Page (POM class) | URL | Controller action | View |
|---|---|---|---|
| `ClientListPage`      | `/Client/ClientSearchView`  | `ClientController.ClientSearchView`   | `Views/Client/ClientSearchView.cshtml` |
| `QuoteListPage`       | `/Quote/QuoteSearchView`    | `QuoteController.QuoteSearchView`     | `Views/Quote/QuoteSearchView.cshtml` |
| `BookingListPage`     | `/Client/BookingSearchView` | `ClientController.BookingSearchView`  | `Views/Client/BookingSearchView.cshtml` |
| `IssueTicketsPage`    | `/Member/IssueTicket`       | `MemberController.IssueTicket`        | `Views/Member/IssueTicket.cshtml` |

### Client flow detail pages (Tier C)

| Page | URL | Controller action |
|------|-----|-------------------|
| `ClientDetailsPage`         | `/Client/ClientDetails?Id={ref}&BookingReferenceId=`  | `ClientController.ClientDetails` |
| `ClientBookingDetailsPage`  | `/Client/ClientBookingDetails?id={ref}&BookingRefId=` | `ClientController.ClientBookingDetails` |
| `BookingDetailsPage`        | `/Client/BookingDetails?Id={ref}&BookingRefId=`       | `ClientController.BookingDetails` |
| `GuestListPage`             | `/Client/GuestList?Id=&BookingRefId=`                 | `ClientController.GuestList` |
| `MoneyPage`                 | `/Client/Money?Id=&BookingRefId=`                     | `ClientController.Money` |
| `UploadFilesForClientPage`  | `/Client/UploadFilesForClient?Id=&BookingRefId=`      | `ClientController.UploadFilesForClient` |
| `RemittanceAdvicePage`      | `/Client/RemittanceAdvice`                            | `ClientController.RemittanceAdvice` |

---

## Calendar (Tier B)

Top-level link in the nav.

| Page | URL | Controller action |
|------|-----|-------------------|
| `MemberCalendarPage` | `/Member/MemberCalender` | `MemberController.MemberCalender` |

---

## Suppliers group (Tier B)

Dropdown "Suppliers" in the nav.

| Page | URL | Controller action |
|------|-----|-------------------|
| `AccommodationSearchPage`    | `/Accommodation/SearchAccommodation` | `AccommodationController.SearchAccommodation` |
| `ActivitiesSearchPage`       | `/Activities/ActivitySearch`         | `ActivitiesController.ActivitySearch` |
| `FlightsSearchPage`          | `/Flight/Flights`                    | `FlightController.Flights` |
| `PackagesSearchPage`         | `/Package/PackageSearch`             | `PackageController.PackageSearch` |
| `TransportSuppliersPage`     | `/Member/SuppliersTransport`         | `MemberController.SuppliersTransport` |
| `CruisesSuppliersPage`       | `/Cruise/SuppliersCruises`           | `CruiseController.SuppliersCruises` |

### Supplier detail pages (Tier C)

| Page | URL | Controller action |
|------|-----|-------------------|
| `AccommodationDetailsPage`   | `/Member/AccomodationDetails?supl={id}` | `MemberController.AccomodationDetails` |
| `PackageDetailsPage`         | `/Member/PackageDetails?supl={id}`      | `MemberController.PackageDetails` |
| `TransportDetailsPage`       | `/Member/TransportDetails?supl={id}`    | `MemberController.TransportDetails` |
| `ContractedSupplierDetailsPage` | `/Member/ContractedSupplierDetails?Id={id}` | `MemberController.ContractedSupplierDetails` |

---

## Accounts group (Tier B — financial heart of the Member portal)

Dropdown "Accounts" in the nav. **These are the pages that host debit / reconciliation / profit claim workflows — highest test value.**

| Page | URL | Controller action |
|------|-----|-------------------|
| `AccountOverviewPage`        | `/TravelMemberAccount/AccountOverview`     | `TravelMemberAccountController.AccountOverview` |
| `TransactionsPage`           | `/Client/Transactions`                     | `ClientController.Transactions` |
| `GpsPaymentsPage` *(role-gated)* | `/TravelMemberAccount/GPSPayments`     | `TravelMemberAccountController.GPSPayments` |
| `PaymentsOutstandingPage`    | `/TravelMemberAccount/PaymentsOutstanding` | `TravelMemberAccountController.PaymentsOutstanding` |
| `PaymentDuePage` *(hidden `d-none` in nav)* | `/TravelMemberAccount/PaymentDue` | `TravelMemberAccountController.PaymentDue` |
| `UnclaimedPage`              | `/Financial/GetAllUnclaimed`               | `FinancialController.GetAllUnclaimed` |
| `UnassignedPage`             | `/Financial/Unassigned`                    | `FinancialController.Unassigned` |
| `ProfitClaimsPage`           | `/TravelMemberAccount/ProfitClaims`        | `TravelMemberAccountController.ProfitClaims` |

---

## Reporting group (Tier B)

Dropdown "Reporting" in the nav.

| Page | URL | Controller action |
|------|-----|-------------------|
| `BusinessReportingPage`          | `/TravelReporting/BusinessReporting`    | `TravelReportingController.BusinessReporting` |
| `SupplierReportingPage`          | `/TravelReporting/SupplierReporting`    | `TravelReportingController.SupplierReporting` |
| `BookingsReportingPage`          | `/TravelReporting/ClientReporting`      | `TravelReportingController.ClientReporting` *(action suffix `Async` stripped by MVC)* |
| `AtolReportingPage`              | `/TravelReporting/AtolReporting`        | `TravelReportingController.AtolReporting` |
| `SupplierDebitsReportingPage`    | `/TravelReporting/AccountDebits`        | `TravelReportingController.AccountDebits` |
| `UserCommissionReportingPage`    | `/TravelReporting/GetSearchCommissionReporting` | `TravelReportingController.GetSearchCommissionReporting` *(action suffix `Async` stripped)* |

---

## Settings mega-menu (Tier B/C)

Gear icon in the top bar (`#settingDropdown`).

| Page | URL | Controller action | Tier |
|------|-----|-------------------|------|
| `ApiSupplierSettingsPage`     | `/Member/APISupplierSettings`        | `MemberController.APISupplierSettings`    | C |
| `ContractedSuppliersPage`     | `/Member/SearchContractedSuppliers`  | `MemberController.SearchContractedSuppliers` | B |
| `DownloadsPage`               | `/Member/PTSDownloads`               | `MemberController.PTSDownloads`           | C |
| `EmailSettingsPage`           | `/Member/EmailSettings`              | `MemberController.EmailSettings`          | C |
| `ItinerarySettingsPage`       | `/Member/ItinerarySettings`          | `MemberController.ItinerarySettings`      | C |
| `OrganisationSettingsPage`    | `/Client/SettingOrganisation`        | `ClientController.SettingOrganisation`    | C |
| `QuoteSettingsPage`           | `/Member/QuoteSettings`              | `MemberController.QuoteSettings` (not yet verified) | C |
| `UsersPage`                   | `/Member/Users`                      | `MemberController.Users`                  | B |

---

## Authentication pages (already implemented)

| Page | URL | Controller action | Status |
|------|-----|-------------------|--------|
| `LoginPage` (member)        | `/Account/Login`          | `AccountController.Login`          | ✅ built |
| `ForgotPasswordPage`        | `/Account/ForgotPassword` | `AccountController.ForgotPassword` | skeleton |
| `ResetPasswordPage`         | `/Account/SetPassword?UserName=…&Token=…` | `AccountController.SetPassword` | skeleton |

The Member login endpoint is shared with Admin; which landing page the user lands on after `POST /Account/LoginCheck` is role-dependent (Member → `/Member/Index`, Admin → `/Admin/...`).

---

## Top-bar icon destinations

| Element | Target |
|---------|--------|
| Logo (member) | `/Member/Index` (Dashboard) |
| Apps icon  | `/Client/Apps` |
| Gear icon  | opens `#settingDropdown` mega-menu (no navigation) |
| Help icon  | opens `#helpPopUp` modal (no navigation) |
| Profile → View Profile | `/Member/UserDetails` |
| Profile → Logout       | AJAX POST to `/Account/LogOut`, then redirect |

---

## What this gives the framework

1. **A named class for every page** — tests reference pages by symbol, not URL string.
2. **A fixed URL contract** — routes captured here become constants on the page classes.
3. **A shared nav helper** — any test can call `NavBar.GoToClientsAsync()` without knowing the URL.
4. **A stopping rule for the first POM pass** — build Tier A fully, Tier B as skeletons, skip Tier C until a test needs them.
