/**
 * Common type definitions
 */

export interface User {
  id: string
  email: string
  name: string
  role: 'admin' | 'staff' | 'parent'
}

export interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
}
