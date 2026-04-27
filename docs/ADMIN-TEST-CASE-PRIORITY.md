# Admin system — prioritized test scenarios

This document scripts the requested scenarios in **priority order** for the PTS **Admin** surface. Use it for backlog ordering, risk-based testing, and future Playwright automation (`AdminTest` + `Pages/Admin/...`).

---

## How to read this document

| Priority | Meaning | When to automate / execute |
|----------|---------|---------------------------|
| **P0** | Smoke / access / navigation | First in every release; minimal data; proves the area is reachable. |
| **P1** | Core financial workflows | End-to-end money movement, imports, authorisation; needs controlled QA data. |
| **P2** | Supporting UX and governance | Search, refine, notes, menus, split flows; may share P1 data setup. |
| **P3** | Edge cases, narrow payment types, polish | After P0–P2 stable; often CSV/role-specific. |

**Comments** in each row explain *why* the priority was chosen, *dependencies* (e.g. seed bookings, CSV files), and *automation notes* (POM location, shared steps).

---

## Epic map (quick reference)

| Epic | Themes covered |
|------|----------------|
| A | Trust Accounts, transaction reporting, booking links |
| B | Reconciliation (travel), Bank & batches, CSV import/export |
| C | Credit Data, Claimed Credits, Unclaimed / Unassigned overviews |
| D | Debit Data, Debit Set Up, Debits Unauthorised / Authorised / Grouping |
| E | Trustee’s Authorisation, currency purchasing, remittance flows |
| F | InterTransfer of Funds, refunds, SAFI / SFI / FX multiple / Trustee fees |
| G | Beneficiary links, view breakdowns, “money on system” breakdown |

---

# P0 — Smoke, navigation, screen presence

These prove **role access**, **routing**, and **critical shell** before deeper tests. Several overlap with existing smoke (`AdminLoginSmokeTests`, `AdminShellSmokeTests`); extend rather than duplicate.

---

## P0-A1 — Trust Accounts: page navigation

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P0-A1 |
| **Priority** | P0 |
| **Epic** | A — Trust Accounts |

**Objective:** From authenticated Admin, user can reach the **Trust Accounts** area via primary navigation (mega menu / admin nav as per `_AdminLayout` / `AdminMenu`).

**Preconditions:** PtsAdmin (or equivalent) logged in; QA env stable.

**Steps (script):**

1. Open Admin home (`/Admin/Index` or landing after login).  
2. Use the documented nav path to **Trust Accounts** (exact menu label from QA — capture in POM comment when implemented).  
3. Assert URL path and page **readiness** (stable heading or table container — selector TBD from `Views/Admin/...`).

**Comments:**

- *Why P0:* If navigation breaks, all Trust Accounts reporting tests are blocked.  
- *Automation:* One `AdminTrustAccountsPage` with `RelativePath` + `ReadinessIndicator`; assert nav does not 404.  
- *Data:* None required for “page loads”.

---

## P0-A2 — Trust Accounts: Booking Reference link

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P0-A2 |
| **Priority** | P0 |
| **Epic** | A |

**Objective:** On Trust Accounts (or nested **transaction reporting** view), a **Booking Reference** value is a link; it navigates to the expected booking/member context without error.

**Steps:**

1. Navigate to Trust Accounts (same as P0-A1).  
2. Locate a row with a populated booking reference (seed data or known QA client).  
3. Click the link; assert destination URL/title or key control on target page.

**Comments:**

- *Why P0:* High-trust drill-down; regressions are common when routes change.  
- *Dependency:* At least one row with booking ref in QA.  
- *Automation:* `Expect(page).ToHaveURLAsync(...)` or new tab handling if `target="_blank"`.

---

## P0-B1 — Reconciliation: screen access + basic search

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P0-B1 |
| **Priority** | P0 |
| **Epic** | B — Reconciliation |

**Objective:** **Reconciliation** page loads; **search** control is visible and returns results (or empty state) without error.

**Steps:**

1. Navigate to Reconciliation (admin reconciliation route — confirm in WebUI `AdminController` / menu).  
2. Enter a known-safe search term (e.g. date range or member fragment from test data).  
3. Submit / apply search; assert grid updates or “no results” message.

**Comments:**

- *Split from backlog:* “Search functionality” alone is P0; advanced refine is P2.  
- *Automation:* Separate `AdminReconciliationPage` with `SearchAsync` + `Expect` on row count or skeleton hidden.

---

## P0-D1 — Debits Unauthorised: screen validation (light)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P0-D1 |
| **Priority** | P0 |
| **Epic** | D |

**Objective:** **Debits Unauthorised** page loads; table/grid and primary actions region visible.

**Steps:** Navigate via menu → assert readiness → optional: assert column headers present.

**Comments:** Listed backlog items “validate Debits Unauthorised screen / data” — **screen** first (P0), **full data assertions** later (P1/P2).

---

## P0-D2 — Debits Authorised: screen validation (light)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P0-D2 |
| **Priority** | P0 |
| **Epic** | D |

**Objective:** Same as P0-D1 for **Debits Authorised**.

**Comments:** Reconcile actions (P1) depend on this navigation being stable.

---

## P0-D3 — Debits Grouping: screen validation (light)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P0-D3 |
| **Priority** | P0 |
| **Epic** | D |

**Objective:** **Debits Grouping** page loads; Pay / Currency affordances (or disabled state) visible per layout.

**Comments:** Select-all / pay flows (P1) build on this.

---

## P0-E1 — Trustee’s Authorisation: screen validation (light)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P0-E1 |
| **Priority** | P0 |
| **Epic** | E |

**Objective:** **Trustee’s Authorisation** loads; list/table readiness.

**Comments:** Central gate for many payment types; keep smoke minimal then add P1 authorise flows per payment type.

---

## P0-C1 — Unassigned / Unclaimed overviews: screen access

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P0-C1 |
| **Priority** | P0 |
| **Epic** | C |

**Objective:** User can open **Unassigned Overview** and **Unclaimed Overview** without error.

**Steps:** Two short navigations + readiness assertions.

**Comments:** “Copy column / three dots / notes” are P2–P3; access first.

---

# P1 — Core business flows (money, import, authorise, reconcile)

---

## P1-B1 — Bank & batches screen validation

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-B1 |
| **Priority** | P1 |
| **Epic** | B |

**Objective:** Validate **Bank & batches** UI: filters, upload zone, grid, batch status columns as per spec.

**Comments:** Prerequisite CSV templates (`Bank.csv`, `Batch.csv`) in repo `TestData/` or docs; never commit secrets.

---

## P1-B2 — Validate `Bank.csv` before upload

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-B2 |
| **Priority** | P1 |
| **Epic** | B |

**Objective:** Client-side or server-side **pre-upload validation** messages appear for invalid file (wrong columns, empty, etc.).

**Comments:** Use golden **invalid** fixture files; assert toast/error text. Keeps P1 separate from “import success”.

---

## P1-B3 — Import `Batch.csv` with debit records

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-B3 |
| **Priority** | P1 |
| **Epic** | B |

**Objective:** Successful import creates expected unauthorised debit rows (count or IDs).

**Comments:** Needs **idempotent** QA data or cleanup step; coordinate with env reset policy.

---

## P1-B4 — Select transaction type while importing `Batch.csv`

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-B4 |
| **Priority** | P1 |
| **Epic** | B |

**Objective:** Import UI allows choosing **transaction type**; imported rows reflect selection.

**Comments:** Parameterised test (one per type) if types are enumerable in UI.

---

## P1-B5 — Import `Bank.csv` with incorrect booking reference → assign to booking

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-B5 |
| **Priority** | P1 |
| **Epic** | B |

**Objective:** Exception path: wrong ref flow completes by **assigning to correct booking**.

**Comments:** High value regression; depends on **Unassigned** or bank staging screen — link to P1-C2.

---

## P1-B6 — Travel reconciliation: view previous

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-B6 |
| **Priority** | P1 |
| **Epic** | B |

**Objective:** Open historical travel reconciliation; list/detail loads.

**Comments:** May need **read-only** QA period with existing reconciliations.

---

## P1-B7 — Travel reconciliation: start and edit

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-B7 |
| **Priority** | P1 |
| **Epic** | B |

**Objective:** Create draft reconciliation, save edits, state persists.

**Comments:** Prefer dedicated QA “draft” flag or cleanup job to avoid polluting production-like data.

---

## P1-B8 — Travel reconciliation: finalise

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-B8 |
| **Priority** | P1 |
| **Epic** | B |

**Objective:** Finalise changes status and locks editing (behaviour per product spec).

**Comments:** Often **last** in reconciliation chain after P1-B7; may use separate test user or rollback API if available.

---

## P1-A1 — Trust Accounts transaction reporting screen

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-A1 |
| **Priority** | P1 |
| **Epic** | A |

**Objective:** **Transaction reporting** under Trust Accounts validates filters + grid + export if applicable.

**Comments:** Align with MEMBER-PAGE-MAP style doc for Admin when routes are catalogued.

---

## P1-A2 — Validate Trust Accounts (functional)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-A2 |
| **Priority** | P1 |
| **Epic** | A |

**Objective:** Beyond P0: totals, period filter, or trust balance line matches seeded expectation (narrow assertion).

**Comments:** “Money showing on system” / breakdown (see P1-G1) may be same page — keep one **source of truth** assertion to avoid duplication.

---

## P1-D1 — Debits Authorised: reconcile debit records

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-D1 |
| **Priority** | P1 |
| **Epic** | D |

**Objective:** Select authorised debits and complete **reconcile** action; status updates.

**Comments:** Prerequisite: debits in **authorised** state from import or API seed.

---

## P1-D2 — Reconcile FX multiple debits + Currency debit together (Debits Authorised)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-D2 |
| **Priority** | P1 |
| **Epic** | D / F |

**Objective:** Multi-select combination allowed by product; post-condition matches spec.

**Comments:** Complex; single P1 scenario after P1-D1 passes; may be nightly only (`Category=Slow`).

---

## P1-D3 — Debit Set Up: authorise debits set up

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-D3 |
| **Priority** | P1 |
| **Epic** | D |

**Objective:** Member-supplier debits move from pending to authorised via admin **Authorise**.

**Comments:** Depends on **Member** having created set-ups — cross-system test; tag `Category=E2E` if slow.

---

## P1-D4 — Debit Set Up: remove already set up debits

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-D4 |
| **Priority** | P1 |
| **Epic** | D |

**Objective:** Remove action succeeds; row disappears or status updates.

**Comments:** Run after P1-D3 or use disposable supplier setup.

---

## P1-D5 — Debit Set Up: manage actions (aggregate)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-D5 |
| **Priority** | P1 |
| **Epic** | D |

**Objective:** Cover “manage actions” menu outcomes (subset of three-dot actions).

**Comments:** Decompose into P2 three-dot tests per action if menu is large.

---

## P1-E1 — Trustee’s: authorise bank / currency / refund / reimbursement / remittance

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-E1a … E1e |
| **Priority** | P1 |
| **Epic** | E |

**Objective:** One scripted case per **payment type** from Trustee’s screen (or parameterised).

**Comments:** Shared **Arrange**: seed debits in trustee queue. **Assert:** status + optional ledger side-effect.

---

## P1-E2 — Trustee’s: authorise Trustee fees / SAFI / SFI / Fx Multiple

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-E2 |
| **Priority** | P1 |
| **Epic** | F |

**Objective:** Fee-type rows authorise correctly (subset of P1-E1 with different row factory).

**Comments:** “SAFI records testing breakdown” ties here + P2-G1 view breakdown.

---

## P1-E3 — Remove debit record for all payment types (Trustee’s)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-E3 |
| **Priority** | P1 |
| **Epic** | E |

**Objective:** Remove action available and works per type (matrix or sample types).

**Comments:** Potentially **P2** if remove is rare; kept P1 if compliance-critical.

---

## P1-D6 — Debits Grouping: Group Bank payments; Group multiple banks + single debit

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-D6 |
| **Priority** | P1 |
| **Epic** | D |

**Objective:** Grouping actions produce expected grouped row / totals.

**Comments:** Visual regression risk — assert DOM attributes or API-backed numbers not screenshots only.

---

## P1-D7 — Debits Grouping: Pay / Currency buttons (Bank, Refund, Reimbursement, Remittance, Currency)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-D7 |
| **Priority** | P1 |
| **Epic** | D |

**Objective:** From grouped state, **Pay** and **Currency** paths complete (modal → confirm → success).

**Comments:** Split into two cases if modals differ; long flow → `[Category(Slow)]`.

---

## P1-D8 — Debits Grouping: Select All + Pay; Select Pay with refined results

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-D8 |
| **Priority** | P1 |
| **Epic** | D |

**Objective:** Bulk operations respect selection and refine filter.

**Comments:** Depends on P2 refine search (could implement refine asserts first).

---

## P1-C1 — Claimed Credits: Approve / Reject; Approve button; notes

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-C1 |
| **Priority** | P1 |
| **Epic** | C |

**Objective:** State transitions for claimed credit rows; **Add note** persists; **Approve** bulk works.

**Comments:** Merge related backlog lines into one **fixture** with ordered steps or separate tests with shared `[SetUp]`.

---

## P1-C2 — Approve / Reject split credits from Claimed Credits

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-C2 |
| **Priority** | P1 |
| **Epic** | C / D |

**Objective:** After **split debits** (P1-F1/P1-F2), splits appear on Claimed Credits and approve/reject works.

**Comments:** **Order:** split (remittance/reimbursement) → claimed credits — document in test `StepAsync` comments.

---

## P1-F1 — Split debits: Reimbursement transactions

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-F1 |
| **Priority** | P1 |
| **Epic** | F |

**Objective:** Split flow completes; child rows visible where expected.

---

## P1-F2 — Split debits: Remittance transactions

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-F2 |
| **Priority** | P1 |
| **Epic** | F |

**Objective:** Same as P1-F1 for remittance.

**Comments:** Parameterise if UI is identical.

---

## P1-F3 — InterTransfer of Funds: Approve / Reject (screen + actions)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-F3 |
| **Priority** | P1 |
| **Epic** | F |

**Objective:** ITF page loads; approve and reject change row state.

**Comments:** P0 could be “screen only”; P1 adds both actions with two rows or sequential with re-seed.

---

## P1-F4 — Currency purchasing: remove remittance → pay from Debits Grouping → see on Trustee’s

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-F4 |
| **Priority** | P1 |
| **Epic** | E |

**Objective:** **End-to-end** across three admin areas (longest scenario in backlog).

**Comments:** Single **E2E** test with many `StepAsync` comments; run nightly; optional manual companion checklist.

---

## P1-F5 — Refunds from Member + refund types as debits from Admin

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P1-F5 |
| **Priority** | P1 |
| **Epic** | F |

**Objective:** Cross-system refund visibility + admin debit-type creation.

**Comments:** Requires `MemberTest` setup or API; tag both `Member` and `Admin` if single pipeline, or split.

---

# P2 — Search, refine, notes, three-dot menus, view, refine-search results

---

## P2-B1 — Reconciliation: three-dot menu + View functionality

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-B1 |
| **Priority** | P2 |
| **Epic** | B |

**Comments:** Three-dot = action menu pattern — one `AdminRowActionMenu` helper if markup repeats across pages.

---

## P2-X1 — Three-dot on Credit Data, Debit Data, Debit Set Up, Trustee’s, Unassigned, Unclaimed

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-X1 (per page) |
| **Priority** | P2 |
| **Epic** | C / D / E |

**Objective:** For each page, open menu and invoke **one safe read-only** action (e.g. View) to prove wiring.

**Comments:** Avoid destructive first item in menu; map menu indices to names in POM constants.

---

## P2-D1 — Debits Authorised: Refine Search, Note, View

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-D1 |
| **Priority** | P2 |
| **Epic** | D |

**Comments:** Align with same patterns as Debit Set Up (P2-D2).

---

## P2-D2 — Debit Set Up: View debit; Refine Search + Search Results link; three dots; view breakdown (Remittance); advanced filter search

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-D2 |
| **Priority** | P2 |
| **Epic** | D |

**Comments:** Large epic — split into 4–6 Playwright tests to keep failure diagnosis clear.

---

## P2-D3 — Debits Unauthorised: validate batch debits (data-heavy)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-D3 |
| **Priority** | P2 |
| **Epic** | D |

**Comments:** “Validate data” = column assertions vs import fixture — after P1-B3.

---

## P2-C1 — Refined search: Select All Approve on results

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-C1 |
| **Priority** | P2 |
| **Epic** | C |

**Comments:** Depends on refine filter producing subset; assert only refined rows selected.

---

## P2-C2 — Unassigned: Refine Search; Move to Unclaimed; Remove; copy columns; three dots; full Notes

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-C2 |
| **Priority** | P2 |
| **Epic** | C |

**Comments:** “Copy” may need clipboard API assertions (Playwright `navigator.clipboard`) — browser-dependent; sometimes **manual** only.

---

## P2-C3 — Unclaimed: Assign credit; Remove; Add note; three dots; copy; payment type; GPS Refunds

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-C3 |
| **Priority** | P2 |
| **Epic** | C |

**Comments:** **GPS Refunds** (narrow payment type) — last within P2-C3 or bump to P3 if flaky availability.

---

## P2-F1 — InterTransfer: Refine + Links functionality

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-F1 |
| **Priority** | P2 |
| **Epic** | F |

---

## P2-G1 — View breakdown of debit notes; links in view breakdown; “Money showing on system” + breakdown

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-G1 |
| **Priority** | P2 |
| **Epic** | G |

**Comments:** Numeric assertions: use **tolerance** for rounding; compare to fixture totals if API exists.

---

## P2-G2 — Account Name column for all payment types; Account Paid for UPFX debits

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-G2 |
| **Priority** | P2 |
| **Epic** | G |

**Comments:** Good candidate for **table snapshot** or row-wise parameterisation from CSV metadata.

---

## P2-G3 — Beneficiary: select beneficiary name link → beneficiary details

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-G3 |
| **Priority** | P2 |
| **Epic** | G |

---

## P2-D4 — Debits Grouping: Add / Edit / Remove note; Remove debit from View breakdown; Refine search; links in View breakdown

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-D4 |
| **Priority** | P2 |
| **Epic** | D |

**Comments:** “Remove debit from view breakdown” interacts with grouping state — document pre-state in test name.

---

## P2-D5 — Debit Set Up: authorise refund / bank / currency / reimbursement / remittance payment (from set-up context)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P2-D5 |
| **Priority** | P2 |
| **Epic** | D |

**Comments:** Overlaps P1-D3 if “authorise” is single button; keep one test if product unifies actions.

---

# P3 — Booking fees, currency purchasing screen validation, SAFI breakdown polish, grouping “test case” variants

---

## P3-A1 — Validate Booking Fees section

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P3-A1 |
| **Priority** | P3 |
| **Epic** | A / F |

**Comments:** Lower if section is static text; raise if fees drive payouts.

---

## P3-A2 — Validate currency purchasing screen (static)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P3-A2 |
| **Priority** | P3 |
| **Epic** | E |

**Comments:** Full remittance removal flow is already **P1-F4**; this is **layout-only** pass.

---

## P3-D1 — Debits Grouping: Grouping + Select all (explicit test case variant)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P3-D1 |
| **Priority** | P3 |
| **Epic** | D |

**Comments:** Merged logically with P1-D8 unless QA insists separate case ID for audit.

---

## P3-F1 — SAFI records testing breakdown (extended)

| Field | Value |
|-------|--------|
| **ID** | ADMIN-P3-F1 |
| **Priority** | P3 |
| **Epic** | F |

**Comments:** Deep assertion matrix once P1-E2 and P2-G1 exist.

---

# Suggested implementation order (sprints)

1. **Sprint 1:** All **P0** IDs + extend `AdminShellSmokeTests` with menu navigation helpers.  
2. **Sprint 2:** **P1** Bank/Batch/Reconciliation/Trust Accounts reporting + Trustee authorise (bank + one non-bank).  
3. **Sprint 3:** **P1** Debits Authorised/Grouping/Claimed Credits + split debits.  
4. **Sprint 4:** **P2** Three-dot/view/refine/note patterns shared POM.  
5. **Sprint 5:** **P2** Unassigned/Unclaimed + **P3** polish.

---

# Traceability to your original wording

| Your phrase (abridged) | Primary ID |
|-------------------------|------------|
| Trust Accounts navigation | P0-A1 |
| Trust Accounts booking reference link | P0-A2 |
| Reconciliation search | P0-B1 |
| Reconciliation three-dot / view | P2-B1 |
| Credit / Debit Data three-dot | P2-X1 |
| Bank & batches | P1-B1 |
| Debit Data screen | P0 + P2 data |
| Trust Accounts transaction reporting | P1-A1 |
| Bank.csv before upload | P1-B2 |
| Validate Trust Accounts | P1-A2 |
| Travel reconciliation view / edit / finalise | P1-B6–B8 |
| Batch.csv import / types / debits | P1-B3, B4, P1-C2 chain |
| Bank.csv wrong ref assign | P1-B5 |
| Currency purchasing / remittance / grouping / Trustee | P1-F4 |
| Booking Fees | P3-A1 |
| Split debits reimbursement / remittance | P1-F1, F2 |
| Claimed Credits (all variants) | P1-C1, C2 |
| Debits unauthorised batch | P2-D3 |
| Select All Approve refined | P2-C1 |
| Debits Authorised (all) | P0-D2, P1-D1, D2, P2-D1 |
| SAFI breakdown | P3-F1 / P1-E2 |
| Debits Grouping (all) | P0-D3, P1-D6–D8, P2-D4, P3-D1 |
| Debit Set Up (all) | P2-D2, P1-D3–D5, P2-D5 |
| Refunds member/admin | P1-F5 |
| Trustee’s (all) | P0-E1, P1-E1–E3, P2-X1 |
| InterTransfer | P1-F3, P2-F1 |
| Unassigned / Unclaimed | P0-C1, P2-C2, C3 |
| View breakdown / beneficiary / money on system / columns | P2-G1–G3 |
| Booking fees | P3-A1 |

---

## File maintenance

- When a Playwright test exists, add its **fully qualified name** as a comment under the ID.  
- When QA routes change, update **one** routing table (future `docs/ADMIN-PAGE-MAP.md`) and link from here.
