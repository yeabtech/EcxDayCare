/**
 * Route-Level Authorization
 * Protects routes from unauthorized access
 */
import React from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { LoadingSpinner } from '@components/shared'
import type { UserRole } from './RoleGuard'

export interface RoleBasedRouteProps {
  /**
   * Component to render if authorized
   */
  children: React.ReactNode
  /**
   * Required role(s)
   */
  requiredRoles?: UserRole | UserRole[]
  /**
   * Path to redirect if not authorized
   */
  redirectTo?: string
}

/**
 * RoleBasedRoute Component
 * Wraps route components to enforce role-based access control
 *
 * Usage in routes:
 * <Route
 *   path="/admin/*"
 *   element={
 *     <RoleBasedRoute requiredRoles="Admin" redirectTo="/dashboard">
 *       <AdminPanel />
 *     </RoleBasedRoute>
 *   }
 * />
 */
export const RoleBasedRoute: React.FC<RoleBasedRouteProps> = ({
  children,
  requiredRoles,
  redirectTo = '/dashboard',
}) => {
  const { user, isAuthenticated, isLoading } = useAuth()

  // Show loading state while auth is being checked
  if (isLoading) {
    return <LoadingSpinner message="Checking authorization..." />
  }

  // Redirect if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  // Check role if required
  if (requiredRoles) {
    const roles = Array.isArray(requiredRoles) ? requiredRoles : [requiredRoles]
    const hasAccess = roles.includes(user?.Role as UserRole)

    if (!hasAccess) {
      return <Navigate to={redirectTo} replace />
    }
  }

  return <>{children}</>
}

/**
 * Route access configuration
 * Defines which roles can access which route patterns
 */
export const ROUTE_ACCESS_CONFIG: Record<
  string,
  {
    requiredRoles: UserRole[]
    redirectTo?: string
  }
> = {
  '/admin': {
    requiredRoles: ['Admin'],
    redirectTo: '/dashboard',
  },
  '/staff': {
    requiredRoles: ['Staff'],
    redirectTo: '/dashboard',
  },
  '/parent': {
    requiredRoles: ['Parent'],
    redirectTo: '/dashboard',
  },
  '/children': {
    requiredRoles: ['Admin', 'Staff'],
    redirectTo: '/dashboard',
  },
  '/parents': {
    requiredRoles: ['Admin'],
    redirectTo: '/dashboard',
  },
  '/staff-list': {
    requiredRoles: ['Admin'],
    redirectTo: '/dashboard',
  },
  '/classrooms': {
    requiredRoles: ['Admin'],
    redirectTo: '/dashboard',
  },
  '/reports': {
    requiredRoles: ['Admin', 'Staff', 'Parent'],
  },
  '/attendance': {
    requiredRoles: ['Admin', 'Staff'],
    redirectTo: '/dashboard',
  },
}
