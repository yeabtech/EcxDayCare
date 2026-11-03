# Frontend Project Structure

This document outlines the directory structure and organization of the ECX Daycare Frontend.

## Root Level Files

```
ECX_Daycare.Frontend/
├── .env                    # Environment variables (local)
├── .env.example            # Environment variables template
├── .eslintrc.cjs          # ESLint configuration
├── .gitignore             # Git ignore rules
├── index.html             # HTML entry point
├── package.json           # Node dependencies and scripts
├── postcss.config.js      # PostCSS configuration for Tailwind
├── README.md              # Full project documentation
├── SETUP.md               # Quick setup guide
├── tailwind.config.ts     # Tailwind CSS configuration
├── tsconfig.json          # TypeScript configuration
├── tsconfig.node.json     # TypeScript config for Node files
└── vite.config.ts         # Vite build tool configuration
```

## Source Directory Structure

### `/src` - Main Application Code

```
src/
├── components/            # Reusable UI components
│   ├── Header.tsx        # Top navigation bar
│   └── Sidebar.tsx       # Left sidebar navigation
│
├── layouts/              # Layout wrapper components
│   └── AppLayout.tsx     # Main app layout with header/sidebar
│
├── pages/                # Full-page components (routes)
│   ├── DashboardPage.tsx # Dashboard landing page
│   └── NotFoundPage.tsx  # 404 error page
│
├── features/             # Feature modules (organizational)
│   └── [feature-name]/   # Each feature gets its own folder
│       ├── components/   # Feature-specific components
│       ├── pages/        # Feature-specific pages
│       ├── services/     # Feature-specific API calls
│       └── hooks/        # Feature-specific hooks
│
├── services/             # API and external services
│   └── api.ts           # Axios HTTP client with interceptors
│
├── hooks/                # Custom React hooks
│   └── useAuth.ts       # Authentication hook (placeholder)
│
├── types/                # TypeScript type definitions
│   ├── api.ts           # API response types
│   └── index.ts         # Common types (User, AuthState, etc.)
│
├── utils/                # Utility functions and helpers
│   └── index.ts         # Error handling, formatting functions
│
├── routes/               # Routing configuration
│   ├── constants.ts      # Route path constants
│   └── index.tsx         # Route definitions and setup
│
├── config.ts             # Environment configuration
├── App.tsx               # Main app component
├── main.tsx              # React entry point
└── index.css             # Global styles (Tailwind)
```

## Directory Purposes

### `/components`
Reusable UI components used across multiple pages. Examples:
- Navigation components (Header, Sidebar)
- Form components (Input, Select, etc.)
- Dialog and Modal components
- Cards and layout primitives

### `/layouts`
Layout wrapper components that define page structure. Currently contains:
- `AppLayout.tsx` - Main layout with header and sidebar

### `/pages`
Full-page components that correspond to routes. Each typically:
- Uses `AppLayout` as wrapper
- Has its own component file
- May use multiple components

### `/features`
Feature-specific code organized by business domain:
```
features/
├── children/
│   ├── components/ChildForm.tsx
│   ├── components/ChildCard.tsx
│   ├── pages/ChildrenListPage.tsx
│   ├── hooks/useChildren.ts
│   └── services/childrenApi.ts
├── staff/
│   ├── components/...
│   ├── pages/...
│   └── services/...
└── reports/
	└── ...
```

### `/services`
API client and external service integrations:
- `api.ts` - Axios instance with configured interceptors
- Future: payment services, file uploads, etc.

### `/hooks`
Custom React hooks that encapsulate logic:
- `useAuth.ts` - Authentication state and methods
- Future: `useChildren.ts`, `useAttendance.ts`, etc.

### `/types`
TypeScript type definitions organized by concern:
- `api.ts` - API request/response types
- `index.ts` - Common application types

### `/utils`
Pure utility functions and helpers:
- Error handling
- Date/time formatting
- String manipulation
- Data transformation

### `/routes`
Routing configuration:
- `constants.ts` - Path constants to avoid string duplication
- `index.tsx` - Route definitions and protected routes

## Naming Conventions

- **Components**: PascalCase (e.g., `DashboardPage.tsx`)
- **Utilities/Hooks**: camelCase (e.g., `useAuth.ts`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `ROUTES`)
- **Types/Interfaces**: PascalCase (e.g., `User`, `ApiResponse`)

## Import Paths

The project uses path aliases for clean imports:

```typescript
// Instead of:
import { DashboardPage } from '../../../pages/DashboardPage'

// Use:
import { DashboardPage } from '@pages/DashboardPage'
```

Available aliases:
- `@/` → `src/`
- `@components/` → `src/components/`
- `@layouts/` → `src/layouts/`
- `@pages/` → `src/pages/`
- `@services/` → `src/services/`
- `@hooks/` → `src/hooks/`
- `@types/` → `src/types/`
- `@utils/` → `src/utils/`
- `@routes/` → `src/routes/`

## Adding New Features

When adding a new feature (e.g., "Attendance Tracking"):

1. Create a folder in `/features`:
   ```
   src/features/attendance/
   ```

2. Add subfolders as needed:
   ```
   src/features/attendance/
   ├── components/
   ├── pages/
   ├── services/
   ├── hooks/
   └── types.ts
   ```

3. Export from feature's index:
   ```typescript
   // src/features/attendance/index.ts
   export * from './components'
   export * from './pages'
   export * from './services'
   ```

4. Add routes in `/routes/index.tsx`:
   ```typescript
   import { AttendancePage } from '@features/attendance'

   <Route path="/attendance" element={<AttendancePage />} />
   ```

## File Limits

- Keep components under 300 lines
- Keep pages under 400 lines
- Keep services under 200 lines
- If larger, consider breaking into smaller pieces

## Documentation

- Each file should have a JSDoc comment explaining its purpose
- Complex functions should have inline comments
- README files in feature folders explain domain-specific logic
