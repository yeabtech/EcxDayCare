# ECX Daycare Management System

> **Project documentation:** See [project.md](project.md) for the architecture, modules, setup, testing, operations, and current security considerations.

Full-stack daycare and childcare management software built with **ASP.NET Core 10**, **PostgreSQL / EF Core**, **MediatR**, and a **React 18 + TypeScript + Vite** frontend.

The application includes modules for family and staff management, classrooms,
enrollment, attendance, daily care, incident reports, notifications, and
role-specific dashboards. Feature presence does not by itself establish
production readiness.

---

## 🏛️ Architecture & Security Design (Phase 9)

This section summarizes the intended design. It is not a security certification;
see [project.md](project.md#security-controls-and-deployment-requirements) for
implementation details and deployment requirements.

### Backend Security
- **Identity Framework:** ASP.NET Core Identity (`IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`) with secure PBKDF2 password hashing.
- **Token Strategy:** Short-lived JWT Access Tokens (60 minutes) containing Claims (`NameIdentifier`, `Email`, `Name`, `Role`, `StaffId`, `ParentId`).
- **Refresh Token Rotation:** Cryptographically secure 64-byte random refresh tokens; only SHA-256 hashes are persisted.
- **Authorization Policies:**
  - `RequireAdmin` (`Admin` role)
  - `RequireStaffOrAdmin` (`Admin`, `Staff` roles)
  - `RequireParent` (`Parent` role)
  - `RequireAuthenticated` (valid JWT bearer token)
- **Role Seeder:** Creates the `Admin`, `Staff`, and `Parent` roles. Initial administrator provisioning is an explicit, one-time environment configuration.

### Frontend Security
- **Auth State:** React Context (`AuthContext` / `useAuth`) managing user session, automatic background token verification, and refresh rotation.
- **Route Guards:** `<ProtectedRoute>` guarding protected application views with optional role restriction.
- **Role-Based UI:** Dynamic sidebar navigation, header badges, and profile cards tailored to user role.
- **Axios Interceptor:** Automatic `Authorization: Bearer <token>` injection on outgoing requests and safe 401 handling.

---

## 📋 Attendance Module (Phase 11)

- **Daily Check-In & Check-Out:** Live attendance tracking with drop-off and pick-up guardian logging.
- **Statuses Supported:** `Present`, `Late`, `Absent`, `EarlyPickup`.
- **Domain Business Rules:**
  - Prevents duplicate check-ins on the same day.
  - Requires active check-in prior to check-out.
  - Automatic time-based status determination (e.g. late thresholding and early departure flags).
- **Attendance History & Analytics:** Per-child attendance history logs with date filtering and calculated attendance rates.
- **Modern React Dashboard:** Real-time KPI stat cards, attendance rate progress bar, status filtering, and fast modal workflows.
- **SPA Routing & Fallback:** ASP.NET Core configured with SPA fallback so both `http://localhost:5000/attendance` and Vite dev server `http://localhost:5173/attendance` work seamlessly.

---

## 📝 Daily Reports Module (Phase 12)

- **Daily Activity Tracking:** Child reports capturing Mood, Meals & Snacks, Nap & Rest, Activities, Diaper Changes, Milestones 🌟, and Concerns / Notes.
- **Authorization & Access Control:**
  - Staff and Admin can create, edit, delete (drafts), and dispatch daily reports to parents.
  - Parents are restricted to viewing only sent reports belonging specifically to their own children via verified parent-child identity invariants.
- **Domain Business Rules:**
  - One daily report per child per calendar day (enforced via DB unique index and application verification).
  - Date validation prevents future reporting and dates older than 60 days.
  - Sent status lifecycle prevents unauthorized mutations or deletion of already-distributed reports.
- **Rich Frontend Views:**
  - **Staff Daily Dashboard (`/reports`):** Calendar feed navigation, KPI stat chips (total, sent, draft), search, and 1-click "Send to Parent" dispatch.
  - **Staff Report Creator/Editor (`/reports/create`, `/reports/:id/edit`):** Interactive mood pills, multi-section text areas, with both "Save Draft" and "Save & Send" actions.
  - **Parent Report Viewer (`/reports/parent`, `/reports/child/:childId`):** Personalized read-only view with date-range filters, child switcher, and milestone highlights.

---

## 🛠️ Tech Stack

### Backend
- **Framework:** .NET 10 (ASP.NET Core Web API)
- **Database:** PostgreSQL (with Npgsql & Entity Framework Core 10)
- **Identity & Auth:** Microsoft.AspNetCore.Identity + Microsoft.AspNetCore.Authentication.JwtBearer
- **Patterns:** Clean Architecture, CQRS, MediatR, FluentValidation, Repository Pattern
- **API Documentation:** Swashbuckle / Microsoft.OpenApi 2.7.5

### Frontend
- **Framework:** React 18 + TypeScript + Vite
- **Styling:** Tailwind CSS + Lucide Icons
- **Routing:** React Router v7
- **HTTP Client:** Axios with JWT request/response interceptors

---

## 🏃 Getting Started

### 1. Database Setup
Ensure PostgreSQL is running on `localhost:5432` with database `Ecxdaycare` (or update connection strings in `ECX_Daycare/appsettings.json`):
```bash
dotnet ef database update --project Daycare.Infrastructure --startup-project ECX_Daycare
```

### 2. Run the Backend API
```bash
dotnet run --project ECX_Daycare
```
Swagger UI will be available at `http://localhost:5000/swagger`.

### 3. Run the Frontend
```bash
cd ECX_Daycare.Frontend
npm install
npm run dev
```
Access the application at `http://localhost:5173`.

### 4. Initial Administrator

The API does not create a default administrator account. To bootstrap a new
database, configure `BOOTSTRAP_ADMIN_EMAIL` and `BOOTSTRAP_ADMIN_PASSWORD`
securely for the first startup, then remove those values. See
[project.md](project.md#configure-the-database-and-secrets).

---

## 🧪 Running Tests

```bash
# Run unit tests (PowerShell, from the repository root)
dotnet test .\Daycare.Domain.Tests\Daycare.Domain.Tests.csproj --configuration Release
dotnet test .\Daycare.Application.Tests\Daycare.Application.Tests.csproj --configuration Release

# Integration tests may write to PostgreSQL; use an isolated test database.
dotnet test .\ECX_Daycare.IntegrationTests\ECX_Daycare.IntegrationTests.csproj --configuration Release
```
