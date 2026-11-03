/**
 * Main Application Component
 */
import { BrowserRouter } from 'react-router-dom'
import { AppRoutes } from '@routes/index'
import { AuthProvider } from '@features/auth'
import '@/index.css'

export const App = () => {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  )
}
