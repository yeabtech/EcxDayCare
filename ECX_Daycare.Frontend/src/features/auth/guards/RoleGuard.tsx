/**
 * Role-Based Authorization Guards
 * Utilities for enforcing role-based access control at route and component level
 */
import React from 'react'
import { useAuth } from '../hooks/useAuth'
import { EmptyState } from '@components/shared'

export type UserRole = 'Admin' | 'Staff' | 'Parent'

export interface RoleGuardProps {
  /**
   * Required role(s) to access the component
   * Can be a single role or array of roles (acts as OR)
   */
  requiredRoles: UserRole | UserRole[]
  /**
   * Component to render if authorized
   */
  children: React.ReactNode
  /**
   * Fallback component if not authorized (default: empty state)
   */
  fallback?: React.ReactNode
  /**
   * If true, redirect to login instead of showing fallback
   */
  redirectToLogin?: boolean
}

/**
 * RoleGuard Component
 * Restricts component rendering based on user role
 *
 * Usage:
 * <RoleGuard requiredRoles="Admin">
 *   <AdminPanel />
 * </RoleGuard>
 *
 * Or multiple roles:
 * <RoleGuard requiredRoles={['Admin', 'Staff']}>
 *   <ManagementPanel />
 * </RoleGuard>
 */
export const RoleGuard: React.FC<RoleGuardProps> = ({
  requiredRoles,
  children,
  fallback,
  redirectToLogin,
}) => {
  const { user, isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    if (redirectToLogin) {
      window.location.href = '/login'
      return null
    }
    return (
      fallback || (
        <EmptyState
          icon="🔐"
          title="Authentication Required"
          description="You need to be logged in to access this page"
        />
      )
    )
  }

  const roles = Array.isArray(requiredRoles) ? requiredRoles : [requiredRoles]
  const hasAccess = roles.includes(user?.Role as UserRole)

  if (!hasAccess) {
    return (
      fallback || (
        <EmptyState
          icon="⛔"
          title="Access Denied"
          description="You don't have permission to access this resource"
        />
      )
    )
  }

  return <>{children}</>
}

/**
 * useCanAccess Hook
 * Check if current user has access to specific features
 *
 * Usage:
 * const canManageStaff = useCanAccess('Admin');
 * if (canManageStaff) { ... }
 */
export const useCanAccess = (requiredRoles: UserRole | UserRole[]): boolean => {
  const { user, isAuthenticated } = useAuth()

  if (!isAuthenticated || !user) return false

  const roles = Array.isArray(requiredRoles) ? requiredRoles : [requiredRoles]
  return roles.includes(user.Role as UserRole)
}

/**
 * withRoleGuard HOC
 * Wraps a component with role-based access control
 *
 * Usage:
 * export default withRoleGuard(AdminPanel, 'Admin');
 */
export const withRoleGuard =
  <P extends object>(
    Component: React.ComponentType<P>,
    requiredRoles: UserRole | UserRole[],
    fallback?: React.ReactNode
  ): React.FC<P> =>
  (props: P) =>
    (
      <RoleGuard requiredRoles={requiredRoles} fallback={fallback}>
        <Component {...props} />
      </RoleGuard>
    )

/**
 * Role-specific route patterns
 * Maps roles to their accessible routes
 */
export const ROLE_ROUTE_ACCESS: Record<UserRole, string[]> = {
  Admin: [
    '/dashboard',
    '/children',
    '/parents',
    '/staff',
    '/classrooms',
    '/enrollments',
    '/attendance',
    '/reports',
    '/health',
    '/incidents',
  ],
  Staff: [
    '/dashboard',
    '/attendance',
    '/reports',
    '/feeding',
    '/nap',
    '/activities',
    '/health',
    '/incidents',
  ],
  Parent: [
    '/dashboard',
    '/reports',
    '/messages',
  ],
}

/**
 * Check if a user can access a specific route
 */
export const canAccessRoute = (userRole: UserRole, route: string): boolean => {
  const allowedRoutes = ROLE_ROUTE_ACCESS[userRole] || []
  return allowedRoutes.some((allowedRoute) => route.startsWith(allowedRoute))
}
