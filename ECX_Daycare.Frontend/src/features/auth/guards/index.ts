/**
 * Auth Guards and Authorization Index
 */
export { RoleGuard, useCanAccess, withRoleGuard, canAccessRoute, ROLE_ROUTE_ACCESS } from './RoleGuard'
export type { UserRole, RoleGuardProps } from './RoleGuard'

export { RoleBasedRoute, ROUTE_ACCESS_CONFIG } from './RoleBasedRoute'
export type { RoleBasedRouteProps } from './RoleBasedRoute'
