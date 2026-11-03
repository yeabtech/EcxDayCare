/**
 * Dashboard Page - Role-based routing
 * Routes to appropriate dashboard based on user role
 */
import { useAuth } from '@features/auth'
import { AdminDashboard } from './dashboards/AdminDashboard'
import { StaffDashboard } from './dashboards/StaffDashboard'
import { ParentDashboard } from './dashboards/ParentDashboard'

export const DashboardPage = () => {
  const { user } = useAuth()

  // Route to appropriate dashboard based on role
  const role = user?.Role || 'Parent'

  switch (role) {
    case 'Admin':
      return <AdminDashboard />
    case 'Staff':
      return <StaffDashboard />
    case 'Parent':
      return <ParentDashboard />
    default:
      return <ParentDashboard />
  }
}
