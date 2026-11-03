/**
 * Dashboard Hooks Index
 * Exports all dashboard-related custom hooks
 */
export { useDashboardData } from './useDashboardData'
export type { DashboardStats, ClassroomSummary, ChildAttendanceStatus } from './useDashboardData'

export { useAdminDashboard } from './useAdminDashboard'
export type {
  DashboardStat,
  AttendanceDay,
  ClassroomUtilization,
  RecentIncident,
  QuickStat,
} from './useAdminDashboard'

export { useList } from './useList'
export type { UseListOptions } from './useList'

export { useApiList } from './useApiList'
export type { UseApiListOptions, ApiListState } from './useApiList'

export { useDashboardPreferences } from './useDashboardPreferences'
export type { UserDashboardPreferences, DashboardWidget } from './useDashboardPreferences'
export { getDefaultPreferences } from './useDashboardPreferences'

export { useStaffDashboard } from './useStaffDashboard'
export type {
  StaffClassroomData,
  ChildCheckInStatus,
  DailyTask,
} from './useStaffDashboard'

export { useParentDashboard } from './useParentDashboard'
export type {
  ChildData,
  ActivityLog,
  DailyReportSummary,
  Message,
} from './useParentDashboard'
