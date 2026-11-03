# ECX Daycare Management System

## Executive Overview

ECX Daycare is a web application for coordinating day-to-day operations across
daycare administrators, staff, and parents. Its modules cover family and staff
records, classrooms, enrollment, attendance, daily care, health and incident
records, and parent communications.

The system consists of an ASP.NET Core Web API, a PostgreSQL database managed
through Entity Framework Core, and a React and TypeScript web client. Billing
and payment processing are outside the current scope.

> **Deployment note:** This guide describes the repository implementation. It
> is not a security certification or a guarantee of production readiness.
> Review [Security and deployment](#security-and-deployment) before using the
> system with real user or child data.

## Product Capabilities

1. **Identity and accounts** — Sign-in, account registration and status, profile and password management, JWT access tokens, and refresh-token rotation.
2. **Children and families** — Child records, parent profiles, parent-child relationships, and parent-facing child information.
3. **Staff and classrooms** — Staff records, staff search, classroom administration, and classroom assignments.
4. **Enrollment** — Enrollment lifecycle and child enrollment history.
5. **Attendance** — Check-in, check-out, absence recording, and attendance history.
6. **Daily care** — Daily reports, feeding, nap, activity, and health records.
7. **Incidents** — Incident reports and staff-triggered parent notifications.
8. **Parent portal** — Linked children, daily reports, incident notifications, and read-state actions.
9. **Administration** — Role-specific dashboards, account settings, and system metrics.

### Incident notification lifecycle

Saving an incident creates the report and parent in-app alerts. Email is sent
only after an authorized staff member selects the parent-notification action
(`POST /api/incidents/{id}/notify-parent`). Parents can then view incident
details in the portal and mark the notification as read.

## Technology Stack

1. **API** — ASP.NET Core 10, C#, and controller-based Web API.
2. **Application** — MediatR, CQRS, and FluentValidation.
3. **Domain** — C# domain entities, value objects, and business rules.
4. **Persistence** — PostgreSQL, Entity Framework Core 10, Npgsql, and EF Core migrations.
5. **Authentication** — ASP.NET Core Identity and JWT bearer authentication.
6. **Web client** — React 18, TypeScript, Vite, React Router, and Axios.
7. **User interface** — Tailwind CSS and Lucide icons.
8. **Email** — MailKit and Gmail SMTP for incident notifications.
9. **File storage** — UploadThing-backed storage service.

## Architecture

The solution separates domain behavior, application use cases, infrastructure,
HTTP endpoints, and the browser client.

```text
React / TypeScript client
          │ HTTP + JWT
          ▼
ASP.NET Core API
          │
          ▼
Application layer ─────── Domain layer
          │
          ▼
Infrastructure layer
          │
          ▼
      PostgreSQL
```

### Solution structure

1. **`Daycare.Domain/`** — Domain entities, value objects, results, and repository contracts.
2. **`Daycare.Application/`** — Use cases, CQRS requests and handlers, DTOs, and validators.
3. **`Daycare.Infrastructure/`** — EF Core context and mappings, migrations, repositories, Identity, and external services.
4. **`ECX_Daycare/`** — ASP.NET Core API, controllers, middleware, configuration, and API services.
5. **`ECX_Daycare.Frontend/`** — React application, feature modules, routes, and API client.
6. **`Daycare.Domain.Tests/`** — Domain unit tests.
7. **`Daycare.Application.Tests/`** — Application and handler tests.
8. **`ECX_Daycare.IntegrationTests/`** — API integration and database-oriented tests.

The required .NET SDK version is pinned in `global.json`. Frontend feature
modules are organized under `ECX_Daycare.Frontend/src/features/`, including
attendance, authentication, children, classrooms, daily reports, dashboards,
enrollment, health, notifications, parents, and staff.

## Data and Persistence

`Daycare.Infrastructure/Data/DaycareDbContext.cs` defines the application
database context. Entity configurations and relational constraints are applied
from the Infrastructure assembly. Schema changes are tracked under
`Daycare.Infrastructure/Migrations/`.

The data model covers:

- Children, parents, and parent-child relationships.
- Staff, classrooms, and enrollment records.
- Attendance and daily reports.
- Feeding, nap, activity, health, and incident records.
- Parent notifications for daily reports and incidents.
- ASP.NET Identity users and roles, and refresh-token records.

Use separate PostgreSQL databases for development, testing, and production.
Integration tests that write to the database must run only against a dedicated,
disposable test database.

## Authentication and Authorization

ASP.NET Core Identity manages user accounts and password hashing. API requests
are authenticated with JWT bearer tokens. The configured authorization
policies are:

1. **`RequireAdmin`** — Administrators.
2. **`RequireStaffOrAdmin`** — Staff and administrators.
3. **`RequireParent`** — Parents.
4. **`RequireAuthenticated`** — Any authenticated user.

The frontend also provides role-aware routes and navigation. These client-side
controls support the user experience; authorization must always be enforced by
the API.

## API Reference

The API uses attribute-routed ASP.NET Core controllers. Swagger is enabled in
Development, and `/health` is mapped as a health-check endpoint.

1. **Authentication — `/api/auth`** — Sign-in, registration, token refresh, logout, current user, and profile management.
2. **Children — `/api/children`** — Create, list, view, update, and lifecycle actions.
3. **Parents — `/api/parents`** — Search, manage parents, and manage child relationships.
4. **Staff — `/api/staff`** — Search, manage staff, and assign classrooms.
5. **Classrooms — `/api/classrooms`** — Manage classrooms and classroom membership.
6. **Enrollment — `/api/enrollments`** — Create, update, end, and retrieve enrollment history.
7. **Attendance — `/api/attendance`** — Check-in, check-out, absence, and history.
8. **Daily reports — `/api/dailyreports`** — Create, update, send, and retrieve reports.
9. **Incidents — `/api/incidents`** — Manage incident reports and notify parents.
10. **Parent portal — `/api/parent-portal`** — Retrieve linked children and notifications; mark notifications read.
11. **Dashboards — `/api/dashboard`** — Administrator, staff, and parent dashboard data.
12. **Health — `/health`** — Application health check.

The running API's Swagger UI provides the complete request and response
contracts. Review each route together with its authorization policy; the route
list alone does not establish access control.

## Local Development

### Prerequisites

- .NET SDK `10.0.401` (as specified by `global.json`).
- Node.js and npm compatible with the frontend toolchain.
- PostgreSQL.
- An isolated local development database.

### Configuration and secrets

Provide API settings through ASP.NET Core configuration, user secrets, or
environment variables. Do not commit credentials.

1. **`ConnectionStrings__DefaultConnection`** — Required PostgreSQL connection string; provide it through user secrets or an environment variable.
2. **`JWT_SECRET_KEY`** — Required high-entropy JWT signing key of at least 32 bytes.
3. **`Jwt__Issuer`** — JWT issuer.
4. **`Jwt__Audience`** — JWT audience.
5. **`BOOTSTRAP_ADMIN_EMAIL`** — Optional initial administrator email; must be supplied with the password.
6. **`BOOTSTRAP_ADMIN_PASSWORD`** — Optional initial administrator password; remove after provisioning.
7. **`INCIDENT_EMAIL_ADDRESS`** — Gmail sender account for incident email.
8. **`INCIDENT_EMAIL_APP_PASSWORD`** — Gmail app password for SMTP authentication.
9. **`UPLOADTHING_TOKEN`** — Base64-encoded UploadThing credential payload.
10. **`VITE_API_BASE_URL`** — Frontend API base URL; defaults to `http://localhost:5000/api`.
11. **`DAYCARE_TEST_CONNECTION_STRING`** — Dedicated PostgreSQL test database used only by real-database integration tests; its database name must contain `test`.

Generate a new signing key locally rather than reusing a checked-in or shared
value. For example, in PowerShell:

```powershell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
```

The API loads `.env` files from the API and frontend locations. Keep these files
out of source control and use separate credentials for each environment. The
frontend `.env.example` contains frontend settings; the API `.env.example`
documents API settings.

### Start the API

From the repository root in PowerShell:

```powershell
dotnet restore .\ECX_Daycare.slnx
dotnet run --project .\ECX_Daycare\ECX_Daycare.csproj
```

The local HTTP profile listens on `http://localhost:5000`. Swagger is available
at `/swagger` in Development.

At startup, the API applies pending EF Core migrations and creates missing
Identity roles. If both bootstrap administrator settings are provided, it also
creates that account if necessary. Remove the bootstrap settings after
provisioning. The refresh-token security migration invalidates existing
refresh-token sessions; affected users must sign in again. Production
deployments should coordinate schema migrations rather than rely on concurrent
application instances to apply them.

### Start the web client

In a second PowerShell window:

```powershell
Set-Location .\ECX_Daycare.Frontend
npm ci
npm run dev
```

Vite serves the client at `http://localhost:5173`. To create a production
build, run:

```powershell
npm run build
```

The API CORS policy currently permits local Vite origins on ports 5173 and
5174. Configure an explicit allow-list of trusted origins for deployment.

## Testing and Quality Checks

Run the unit-test projects individually from the repository root:

```powershell
dotnet test .\Daycare.Domain.Tests\Daycare.Domain.Tests.csproj --configuration Release
dotnet test .\Daycare.Application.Tests\Daycare.Application.Tests.csproj --configuration Release
```

The integration-test project includes database-writing tests. Run it only
against a dedicated, disposable database after reviewing its setup:

```powershell
dotnet test .\ECX_Daycare.IntegrationTests\ECX_Daycare.IntegrationTests.csproj --configuration Release
```

The real-database tests require `DAYCARE_TEST_CONNECTION_STRING` and refuse
database names that do not contain `test`. They insert and remove test rows;
never point this setting at a production or operational database.

Build the .NET solution:

```powershell
dotnet build .\ECX_Daycare.slnx --configuration Release
```

The frontend defines `build` and `lint` scripts in
`ECX_Daycare.Frontend/package.json`:

```powershell
npm run build
npm run lint
```

The frontend build runs TypeScript checks before bundling. At the time this
guide was prepared, lint was blocked by a missing configured ESLint plugin.
Confirm the current dependency installation and lint configuration before
relying on that check. Test results are revision- and environment-specific;
the presence of test files does not establish that the tests pass.

## Operations and Deployment

- Keep development, test, and production databases separate.
- Back up PostgreSQL and periodically verify restoration procedures.
- Coordinate EF Core migrations as part of deployment, particularly when
  running multiple API instances.
- Configure centralized logging, monitoring, and alerting for API and email
  delivery failures.
- Keep SMTP and file-storage credentials on the server; never expose them in
  the frontend bundle.
- Require HTTPS in deployed environments and restrict CORS to trusted client
  origins.
- Define retention and access controls appropriate for sensitive child,
  attendance, health, and incident records.

## Security and Deployment

The source-level security findings previously documented have been addressed:

- Staff management requires the administrator authorization policy.
- Staff dashboard requests verify staff-ID ownership; administrators may access
  dashboards across staff accounts.
- JWT signing requires an externally supplied key of at least 32 bytes; no
  hard-coded fallback key is used.
- Unhandled exceptions return generic Problem Details rather than exception
  messages.
- Refresh-token values are stored as SHA-256 hashes.
- A default administrator account is not created automatically. Initial
  administrator provisioning is an explicit, optional bootstrap operation.

Before deploying:

1. Configure a unique, high-entropy JWT signing key and rotate any key that may
   have been used previously.
2. Apply the refresh-token invalidation migration; existing sessions will need
   to authenticate again.
3. Remove bootstrap administrator settings after the initial account is
   provisioned.
4. Verify authorization behavior and secret configuration in the deployed
   environment.

These source changes and regression tests are not a penetration test or a
live-deployment assessment.

## Scope and Limitations

Billing, invoicing, payment processing, and financial dashboards are outside
the current scope. This guide describes the repository's implementation and
development workflows; it does not certify production behavior, database
state, regulatory compliance, or deployment security.
