# ECX Daycare Frontend - Setup Instructions

## 📋 Quick Setup Guide

### 1. Install Dependencies
```bash
cd ECX_Daycare.Frontend
npm install
```

### 2. Configure Environment
The `.env` file is already created with default values. Update if needed:
```
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_ENV=development
```

### 3. Start Development Server
```bash
npm run dev
```

The app will open automatically at `http://localhost:5173`

### 4. Build for Production
```bash
npm run build
```

## 📁 Project Structure

```
ECX_Daycare.Frontend/
├── src/
│   ├── components/       # Reusable components (Header, Sidebar)
│   ├── layouts/         # Layout components (AppLayout)
│   ├── pages/           # Page components (Dashboard, NotFound)
│   ├── routes/          # Route definitions
│   ├── services/        # API client
│   ├── types/           # TypeScript definitions
│   ├── App.tsx          # Main app component
│   ├── main.tsx         # React entry point
│   └── index.css        # Global styles
├── index.html           # HTML entry point
├── package.json         # Dependencies
├── vite.config.ts       # Vite configuration
├── tsconfig.json        # TypeScript configuration
├── tailwind.config.ts   # Tailwind CSS configuration
└── README.md            # Full documentation
```

## 🎨 What's Included

✅ **Foundation Components**
- Header with responsive mobile menu
- Sidebar navigation with active state
- Professional card-based layout

✅ **Routing**
- React Router v7 setup
- Protected route structure
- Dashboard and 404 pages

✅ **API Integration**
- Axios client with interceptors
- Auth token handling
- Environment configuration

✅ **Styling**
- Tailwind CSS with custom theme
- Responsive mobile-first design
- Pre-defined utility classes

✅ **Development Tools**
- TypeScript strict mode
- Path aliases for clean imports
- Vite for fast development

## 🚀 Next Steps

1. **Backend Integration**: Update `VITE_API_BASE_URL` to point to your backend API
2. **Authentication**: Implement login/logout screens and auth flows
3. **Pages**: Build out child management, staff management, and other features
4. **State Management**: Add global state management if needed
5. **Testing**: Set up unit and integration tests

## 📚 File Highlights

- `src/services/api.ts` - Axios client with interceptors
- `src/routes/index.tsx` - Route definitions
- `src/layouts/AppLayout.tsx` - Main application shell
- `src/pages/DashboardPage.tsx` - Dashboard landing page
- `src/components/Header.tsx` - Top navigation
- `src/components/Sidebar.tsx` - Side navigation

## 🔗 Connecting Frontend to Backend

The frontend is configured to call APIs at `http://localhost:5000/api` by default.

When making API calls:
```typescript
import { apiClient } from '@services/api'

// Example: Get children list
const { data } = await apiClient.get('/children')

// Example: Create new child
const { data } = await apiClient.post('/children', childData)
```

Auth tokens are automatically included in all requests if stored in localStorage:
```typescript
localStorage.setItem('auth_token', 'your-token-here')
```

## 💡 Tips

- Use `@/` prefix for imports from src directory
- Icons from Lucide React: `import { SettingsIcon } from 'lucide-react'`
- Tailwind classes are already configured
- TypeScript will help catch errors early

---

**Status**: Foundation complete. Ready for feature development.
