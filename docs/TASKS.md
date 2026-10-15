# S TOM'S — Project Task Tracker

This file tracks all engineering and testing tasks. All team members must keep task entries up to date.

Allowed Statuses:
- `Not Started`
- `In Progress`
- `Blocked`
- `In Review`
- `Verified`
- `Done` *(Only marked after verified evidence is provided in TEST_REPORT.md)*

---

## Task Backlog

### [x] `TASK-BOOT-001`: Multi-Developer Bootstrap & Standards Setup
- **Owner**: Saumya Chaudhari (Vision & Project Owner) / AI Assistant
- **Role**: Vision & Workflow Setup
- **Description**: Initialize Git repository, configure .gitignore, establish team standards (`TEAM_RULES.md`), project documentation (`README.md`), task tracking (`docs/TASKS.md`), test logs (`docs/TEST_REPORT.md`), and developer workspace (`.team/`).
- **Acceptance Criteria**:
  1. `.git` initialized with `main` branch.
  2. `.gitignore` created covering .NET/Visual Studio artifacts.
  3. `TEAM_RULES.md` created with 9-step workflow, clean architecture, role boundaries, and security rules.
  4. Build succeeds with zero warnings and zero errors.
- **Status**: Verified
- **Verification Evidence**: `dotnet build` succeeded with 0 warnings, 0 errors. Git repository initialized. Verified via automated terminal checks. Approved by Vision.
- **Blockers**: None.

---

### [ ] `TASK-BOOT-002`: Clean Architecture Layering
- **Owner**: Engine Developer / UI/UX Developer
- **Role**: Engineering Architecture
- **Description**: Establish Clean Architecture folder structure (`src/`, `App`, `Core`, `Engine`, `Infrastructure`) separating presentation from engine logic without breaking existing WinForms initialization.
- **Acceptance Criteria**:
  1. UI views/controls separated from business logic.
  2. Core interfaces and models created without UI dependencies.
  3. Solution builds cleanly with 0 warnings and 0 errors.
- **Status**: Not Started
- **Verification Evidence**: None yet.
- **Blockers**: Awaiting developer role onboarding & feature requirements.

---

### [x] `TASK-BOOT-003`: Automated Testing Project Setup
- **Owner**: Hardik (Full-Stack UI/UX & Engine Developer)
- **Role**: Engineering & Testing
- **Description**: Set up an automated unit test project under `tests/` (`PROJECT-SBV1.Tests`, xUnit) for validating domain validation and engine workflows.
- **Acceptance Criteria**:
  1. Test project created and referenced in solution.
  2. Sample domain tests run via `dotnet test` and pass.
  3. Test outputs logged in `docs/TEST_REPORT.md`.
- **Status**: Verified
- **Verification Evidence**: `dotnet test tests/PROJECT-SBV1.Tests/PROJECT-SBV1.Tests.csproj` executed with 7 passed tests, 0 failed. Evidence recorded in TR-20261014-002.
- **Blockers**: None.

---

### [x] `TASK-FEAT-001`: Sales & Billing Module (C# Desktop Software)
- **Owner**: Hardik (Full-Stack UI/UX & Engine Developer)
- **Role**: UI/UX Developer / Engine Developer
- **Description**: Full-featured Sales & Billing module with interactive Invoicing Dashboard, Invoice Creation screen, real-time bill calculations (GST, discounts, totals), multi-mode payment recording, PDF invoice generation (PdfSharp), persistent local SQLite database storage, customer history, and ThemeManager (Dark/Light) integration.
- **Acceptance Criteria**:
  1. Live sales metrics (Total Sales, Total Invoices, Paid/Unpaid, Outstanding Balance) from database.
  2. Complete invoice creation with customer selection, itemized services, live bill calculation.
  3. Payment management with partial/full settlements and automated status updates.
  4. PDF invoice export and print support.
  5. Persistent SQLite database storage with transactions and parameterized queries.
  6. Dark and Light mode theme compatibility.
  7. Automated unit test suite passing with 0 errors.
- **Status**: Verified
- **Verification Evidence**: `dotnet test` 16/16 passed (0 failed). `dotnet build` 0 warnings, 0 errors. All 8 metric cards dynamic with hover effects, multi-year charts, and live order table verified. Evidence in TR-20261015-001.
- **Blockers**: None.

---

### [ ] `TASK-TEST-001`: Temporary Onboarding Workflow Demonstration
- **Owner**: Temporary Developer (Test Role: PM/Tester)
- **Role**: PM/Tester
- **Description**: Verify feature branch isolation, task checklist update, and worklog synchronization on a test branch.
- **Acceptance Criteria**:
  1. Feature branch created and isolated from main without application code changes.
  2. Task checklist entry tracked with unique ID and In Progress status.
  3. Worklog entry synchronized in .team/WORKLOG.md for the onboarding session.
- **Status**: In Progress
- **Verification Evidence**: Branch test/onboarding-workflow verified isolated; task entry added to docs/TASKS.md; worklog entry synchronized in .team/WORKLOG.md.
- **Blockers**: None.

---

### [x] `TASK-SPRINT-001`: UI Presentation — Splash, Login & Dashboard Forms
- **Owner**: Hardik (Full-Stack UI/UX & Engine Developer)
- **Role**: UI/UX Developer
- **Description**: Design and implement WinForms presentation views, controls, and styling for Splash Screen, Login Screen, and Dashboard Screen. The developer guides the styling and Antigravity collaborates on the implementation without imposing direct code assumptions.
- **Acceptance Criteria**:
  1. Splash Form displays branding and initiates smooth startup transition.
  2. Login Form provides username/password controls, error labels, and submit buttons.
  3. Dashboard Form renders main application layout and placeholder panels matching user reference image.
  4. Completed on a feature branch (`feature/splash-login-dashboard`), verified with `dotnet build`.
- **Status**: Verified
- **Verification Evidence**: `dotnet build` succeeded with 0 warnings, 0 errors. SplashScreen, LoginForm, and DashboardForm implemented with custom GDI+ controls (DonutChartControl, BarChartControl, SplineChartControl, ThemeToggle). Evidence logged in TR-20261014-001.
- **Blockers**: None.

---

### [x] `TASK-SPRINT-002`: Engine Logic — Auth, Session & Navigation Services
- **Owner**: Hardik (Full-Stack UI/UX & Engine Developer)
- **Role**: Engine Developer
- **Description**: Implement Clean Architecture domain models, authentication validation, user session state, and navigation workflow for Splash ➔ Login ➔ Dashboard. Developer has complete freedom to write simple, robust logic with no unnecessary restrictions.
- **Acceptance Criteria**:
  1. Core models/interfaces for User, Credentials, and SessionState created under Clean Architecture.
  2. Engine service validates credentials and manages user session.
  3. Navigation transition logic coordinates state between screens.
  4. Completed on a feature branch (`feature/splash-login-dashboard`), verified with `dotnet build`.
- **Status**: Verified
- **Verification Evidence**: `dotnet build` succeeded with 0 warnings, 0 errors. AuthService, DashboardDataService, ThemeManager implemented. Program.cs sequences Splash -> Login -> Dashboard. Evidence logged in TR-20261014-001.
- **Blockers**: None.


---

### [ ] `TASK-SPRINT-003`: Verification & Test Cases — Splash, Login & Dashboard
- **Owner**: PM/Tester
- **Role**: PM/Tester
- **Description**: Define acceptance test scenarios and verify the integrated flow between Splash, Login, and Dashboard.
- **Acceptance Criteria**:
  1. Verify Splash-to-Login transition.
  2. Verify valid and invalid login handling.
  3. Verify Dashboard landing upon authentication.
  4. Test report logged in `docs/TEST_REPORT.md`.
- **Status**: Not Started
- **Verification Evidence**: None yet.
- **Blockers**: Pending completion of TASK-SPRINT-001 and TASK-SPRINT-002.

