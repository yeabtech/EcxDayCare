# ECX Daycare Frontend

A modern React + TypeScript + Vite frontend for the ECX Daycare Management System.

## 🏗️ Project Structure

```
src/
├── components/          # Reusable UI components (Header, Sidebar, etc.)
├── layouts/            # Layout components (AppLayout)
├── pages/              # Page components (Dashboard, NotFound, etc.)
├── features/           # Feature-specific logic and components
├── services/           # API services and external integrations
├── hooks/              # Custom React hooks
├── types/              # TypeScript type definitions
├── utils/              # Utility functions and helpers
├── routes/             # Route definitions and constants
├── App.tsx             # Main application component
├── main.tsx            # React entry point
├── index.css           # Global styles with Tailwind
└── config.ts           # Environment configuration
```

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ 
- npm or yarn

### Installation

1. Install dependencies:
```bash
npm install
```

2. Create `.env` file (copy from `.env.example`):
```bash
cp .env.example .env
```

3. Update the API base URL in `.env`:
```
VITE_API_BASE_URL=http://localhost:5000/api
```

### Development

Start the development server:
```bash
npm run dev
```

The application will open at `http://localhost:5173`

### Building

Build for production:
```bash
npm run build
```

The output will be in the `dist/` directory.

### Preview Production Build

Preview the production build locally:
```bash
npm run preview
```

## 🛠️ Tech Stack

- **React 18** - UI library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool and dev server
- **React Router v7** - Client-side routing
- **Axios** - HTTP client
- **Tailwind CSS** - Utility-first CSS framework
- **Lucide React** - Icon library

## 📝 Features

### Implemented
- ✅ Application layout with responsive header and sidebar
- ✅ Responsive mobile-first design with Tailwind CSS
- ✅ Route configuration with protected routes & role-based route guards
- ✅ API client with Axios interceptors & automatic JWT bearer token injection
- ✅ Environment configuration & TypeScript strict mode
- ✅ Dashboard overview page
- ✅ Child management screens (CRUD, search, status management)
- ✅ Parent management screens (CRUD, relationships)
- ✅ Classroom management screens (CRUD, age group validation, capacity tracking)
- ✅ **Phase 8: Staff Module** (Staff CRUD, search, pagination, Lead/Assistant classroom assignments)
- ✅ **Phase 9: Authentication & Authorization** (Login page, AuthContext, JWT access tokens, refresh token rotation, role-based navigation & UI for Admin/Staff/Parent)
- ✅ **Phase 10: Enrollment Module** (Create, Update, View, End, History, capacity enforcement, domain invariants)

### To Be Implemented
- 🚧 Attendance tracking
- 🚧 Daily activity logging & Schedule management
- 🚧 Reports and analytics
- 🚧 Notifications system
- 🚧 System settings and preferences

## 🔧 Configuration

### Environment Variables

Create a `.env` file in the root directory:

```
# API Configuration
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_ENV=development
```

### Tailwind CSS

Tailwind CSS is pre-configured with:
- Responsive design utilities
- Custom color scheme (primary colors in sky blue)
- Custom component classes (`.btn-primary`, `.card`, etc.)

## 📚 API Integration

The API client is configured in `src/services/api.ts` and provides:
- Automatic token injection from localStorage
- Request/response interceptors
- Built-in error handling
- Logout on 401 responses

### Usage

```typescript
import { apiClient } from '@services/api'

// GET request
const { data } = await apiClient.get('/endpoint')

// POST request
const { data } = await apiClient.post('/endpoint', { /* payload */ })

// PUT request
const { data } = await apiClient.put('/endpoint', { /* payload */ })

// DELETE request
const { data } = await apiClient.delete('/endpoint')
```

## 🔒 Security Considerations

- Auth tokens are stored in localStorage (consider using httpOnly cookies for sensitive apps)
- CORS must be configured on the backend
- API requests include Authorization header when token is available
- Automatically logs out on 401 responses

## 📦 Dependencies

See `package.json` for complete list. Key packages:
- `react@^18.3.1` - UI library
- `react-dom@^18.3.1` - React DOM rendering
- `react-router-dom@^7.0.0` - Routing
- `axios@^1.6.7` - HTTP client
- `tailwindcss@^3.4.1` - CSS framework
- `lucide-react@^0.408.0` - Icons

## 🚦 Development Guidelines

### Component Structure
- Keep components small and focused
- Use TypeScript for type safety
- Export components as named exports
- Place styles in Tailwind CSS classes

### State Management
- Use React hooks for component state
- Consider Context API or state management library for global state
- Keep API calls in services

### Naming Conventions
- Components: PascalCase (e.g., `DashboardPage`)
- Files: PascalCase for components, camelCase for utilities
- Functions: camelCase
- Constants: UPPER_SNAKE_CASE

## 🐛 Troubleshooting

### Port Already in Use
If port 5173 is already in use:
```bash
npm run dev -- --port 5174
```

### TypeScript Errors
Ensure TypeScript is properly installed:
```bash
npm install --save-dev typescript
```

### Module Not Found
Clear node_modules and reinstall:
```bash
rm -rf node_modules package-lock.json
npm install
```

## 📖 Next Steps
 
1. Implement attendance tracking module
2. Build daily activity logging and schedules
3. Add notifications and messaging
4. Add reports and export utilities
5. Add end-to-end integration test suites for UI workflows

## 📞 Support

For issues or questions, refer to the documentation or backend API specification.
