/**
 * Application Routes Configuration
 */
import { Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from '@layouts/AppLayout'
import { DashboardPage } from '@pages/DashboardPage'
import { NotFoundPage } from '@pages/NotFoundPage'
import { ChildDetailsPage, ChildCreatePage, ChildEditPage } from '@features/children'
import { ChildrenManagementPage } from '@pages/management/ChildrenManagementPage'
import { StaffChildrenPage } from '@pages/management/StaffChildrenPage'
import { StaffProfilePage } from '@features/staff/pages/StaffProfilePage'
import {
  ParentsListPage,
  ParentDetailPage,
  ParentCreatePage,
  ParentEditPage,
} from '@features/parents'
import {
  ClassroomsListPage,
  ClassroomDetailPage,
  ClassroomCreatePage,
  ClassroomEditPage,
} from '@features/classrooms'
import {
  StaffListPage,
  StaffDetailPage,
  StaffCreatePage,
  StaffEditPage,
} from '@features/staff'
import { LoginPage, ProtectedRoute, RoleBasedRoute } from '@features/auth'
import {
  EnrollmentListPage,
  EnrollmentCreatePage,
  EnrollmentDetailPage,
  EnrollmentEditPage,
} from '@features/enrollments'
import { AttendanceDashboardPage, ChildAttendanceHistoryPage } from '@features/attendance'
import { AttendanceManagementPage } from '@pages/management/AttendanceManagementPage'
import { UsersAccessPage } from '@pages/management/UsersAccessPage'
import { AdminPreferencesPage } from '@pages/admin/AdminPreferencesPage'
import { HealthPage, IncidentsPage } from '@features/health'
import { useAuth } from '@features/auth/hooks/useAuth'
import {
  DailyReportsDashboardPage,
  StaffReportFormPage,
  ParentReportViewPage,
} from '@features/dailyReports'
import { DailyLogsPage } from '@features/dailyReports/pages/DailyLogsPage'
import { ParentPortalPage } from '@pages/dashboards/ParentPortalPage'
import { ParentProfilePage } from '@features/parents/pages/ParentProfilePage'
import { ROUTES } from './constants'

const AttendanceHome = () => {
  const { isAdmin, isStaff } = useAuth()
  if (isAdmin) return <AttendanceManagementPage />
  if (isStaff) return <AttendanceManagementPage staffMode />
  return <AttendanceDashboardPage />
}

const ReportsHome = () => {
  const { isParent } = useAuth()
  return isParent ? <ParentPortalPage view="reports" /> : <DailyReportsDashboardPage />
}

const ProfileHome = () => {
  const { isParent } = useAuth()
  return isParent ? <ParentProfilePage /> : <StaffProfilePage />
}

export const AppRoutes = () => {
  return (
    <Routes>
      {/* Public Login Route */}
      <Route path={ROUTES.LOGIN} element={<LoginPage />} />

      {/* Protected routes with layout */}
      <Route
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        {/* Redirect root to dashboard */}
        <Route path={ROUTES.HOME} element={<Navigate to={ROUTES.DASHBOARD} replace />} />
        <Route path={ROUTES.DASHBOARD} element={<DashboardPage />} />

        <Route
          path="/staff/children"
          element={
            <RoleBasedRoute requiredRoles="Staff">
              <StaffChildrenPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/profile"
          element={
            <RoleBasedRoute requiredRoles={['Staff', 'Parent']}>
              <ProfileHome />
            </RoleBasedRoute>
          }
        />

        {/* Children routes - Admin and Staff */}
        <Route
          path="/children"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff']}>
              <ChildrenManagementPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/children/create"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <ChildCreatePage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/children/:id"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff']}>
              <ChildDetailsPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/children/:id/edit"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff']}>
              <ChildEditPage />
            </RoleBasedRoute>
          }
        />

        {/* Parents routes - Admin only */}
        <Route
          path="/parents"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <ParentsListPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/parents/create"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <ParentCreatePage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/parents/:id"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <ParentDetailPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/parents/:id/edit"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <ParentEditPage />
            </RoleBasedRoute>
          }
        />

        {/* Classrooms routes - Admin only */}
        <Route
          path="/classrooms"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <ClassroomsListPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/classrooms/create"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <ClassroomCreatePage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/classrooms/:id"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <ClassroomDetailPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/classrooms/:id/edit"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <ClassroomEditPage />
            </RoleBasedRoute>
          }
        />

        {/* Staff routes - Admin only */}
        <Route
          path="/staff"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <StaffListPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/staff/create"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <StaffCreatePage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/staff/:id"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <StaffDetailPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/staff/:id/edit"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <StaffEditPage />
            </RoleBasedRoute>
          }
        />

        {/* User accounts and access - Admin only */}
        <Route
          path="/users-access"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <UsersAccessPage />
            </RoleBasedRoute>
          }
        />

        <Route
          path="/settings"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <AdminPreferencesPage />
            </RoleBasedRoute>
          }
        />

        {/* Enrollment routes - Admin only */}
        <Route
          path="/enrollments"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <EnrollmentListPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/enrollments/create"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <EnrollmentCreatePage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/enrollments/:id"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <EnrollmentDetailPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/enrollments/:id/edit"
          element={
            <RoleBasedRoute requiredRoles="Admin">
              <EnrollmentEditPage />
            </RoleBasedRoute>
          }
        />

        {/* Attendance routes - Admin and Staff */}
        <Route
          path="/attendance"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff']}>
              <AttendanceHome />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/attendance/child/:childId"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff']}>
              <ChildAttendanceHistoryPage />
            </RoleBasedRoute>
          }
        />

        {/* Health and incident records */}
        <Route
          path="/health"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff']}>
              <HealthPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/incidents"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff']}>
              <IncidentsPage />
            </RoleBasedRoute>
          }
        />
        <Route path="/parent/health" element={<Navigate to="/notifications" replace />} />
        <Route path="/parent/children" element={<RoleBasedRoute requiredRoles="Parent"><ParentPortalPage view="children" /></RoleBasedRoute>} />
        <Route path="/parent/classroom" element={<RoleBasedRoute requiredRoles="Parent"><ParentPortalPage view="classroom" /></RoleBasedRoute>} />
        <Route path="/parent/today" element={<Navigate to="/notifications" replace />} />
        <Route path="/parent/attendance" element={<Navigate to="/notifications" replace />} />
        <Route path="/parent/reports" element={<Navigate to="/notifications" replace />} />
        <Route path="/parent/meals" element={<Navigate to="/notifications" replace />} />
        <Route path="/parent/nap" element={<Navigate to="/notifications" replace />} />
        <Route path="/parent/activities" element={<Navigate to="/notifications" replace />} />
        <Route path="/parent/incidents" element={<Navigate to="/notifications" replace />} />
        <Route path="/notifications" element={<RoleBasedRoute requiredRoles="Parent"><ParentPortalPage view="notifications" /></RoleBasedRoute>} />

        {/* Daily Reports routes - Role specific */}
        <Route
          path="/reports"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff', 'Parent']}>
              <ReportsHome />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/reports/create"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff']}>
              <StaffReportFormPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/reports/:id/edit"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff']}>
              <StaffReportFormPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/reports/child/:childId"
          element={
            <RoleBasedRoute requiredRoles={['Admin', 'Staff', 'Parent']}>
              <ParentReportViewPage />
            </RoleBasedRoute>
          }
        />
        <Route
          path="/reports/parent"
          element={
            <RoleBasedRoute requiredRoles="Parent">
              <ParentReportViewPage />
            </RoleBasedRoute>
          }
        />

        <Route path="/activities" element={<RoleBasedRoute requiredRoles={['Admin', 'Staff']}><DailyLogsPage kind="activities" /></RoleBasedRoute>} />
        <Route path="/feeding" element={<RoleBasedRoute requiredRoles={['Admin', 'Staff']}><DailyLogsPage kind="feeding" /></RoleBasedRoute>} />
        <Route path="/nap" element={<RoleBasedRoute requiredRoles={['Admin', 'Staff']}><DailyLogsPage kind="nap" /></RoleBasedRoute>} />
      </Route>

      {/* Catch-all 404 */}
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}


