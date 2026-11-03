/**
 * useAdminDashboard hook
 * Complete admin dashboard data integration with API calls
 */
import { useState, useCallback, useEffect } from 'react'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'

export interface DashboardStat {
  label: string
  value: number | string
  change?: number // percentage change
  changeType?: 'up' | 'down' | 'neutral'
  trend?: string // e.g., "vs last month"
}

export interface AttendanceDay {
  date: string
  presentCount: number
  absentCount: number
  totalCount: number
  rate: number
}

export interface ClassroomUtilization {
  classroomId: string
  classroomName: string
  capacity: number
  enrolled: number
  present: number
  utilization: number
  occupancyRate: number
}

export interface RecentIncident {
  id: string
  childId: string
  childName: string
  classroomId: string
  classroomName: string
  type: 'injury' | 'behavior' | 'health' | 'allergy' | 'other'
  description: string
  severity: 'low' | 'medium' | 'high'
  reportedAt: string
  reportedBy: string
  status: 'open' | 'closed'
}

export interface QuickStat {
  label: string
  value: number
  icon?: string
  color?: 'blue' | 'green' | 'red' | 'yellow' | 'purple'
  href?: string
}

interface UseAdminDashboardState {
  // Overview stats
  totalChildren: number
  presentToday: number
  absentToday: number
  activeStaff: number
  totalClassrooms: number
  attendanceRate: number

  // Detailed data
  attendanceTrend: AttendanceDay[]
  classroomUtilization: ClassroomUtilization[]
  recentIncidents: RecentIncident[]
  quickStats: QuickStat[]

  // State management
  isLoading: boolean
  isRefreshing: boolean
  error: string | null
  lastUpdated: Date | null
}

const initialState: UseAdminDashboardState = {
  totalChildren: 0,
  presentToday: 0,
  absentToday: 0,
  activeStaff: 0,
  totalClassrooms: 0,
  attendanceRate: 0,
  attendanceTrend: [],
  classroomUtilization: [],
  recentIncidents: [],
  quickStats: [],
  isLoading: false,
  isRefreshing: false,
  error: null,
  lastUpdated: null,
}

export const useAdminDashboard = () => {
  const [state, setState] = useState<UseAdminDashboardState>(initialState)

  /**
   * Fetch overview statistics
   */
  const fetchOverviewStats = useCallback(async () => {
    try {
      const response = await apiClient.get('/dashboards/admin/overview')
      const data = response.data as any
      setState((prev) => ({
        ...prev,
        totalChildren: data.TotalChildren || data.totalChildren || 0,
        presentToday: data.TodaysPresentCount || data.presentToday || 0,
        absentToday: data.TodaysAbsentCount || data.absentToday || 0,
        activeStaff: data.TotalStaff || data.activeStaff || 0,
        totalClassrooms: data.TotalClassrooms || data.totalClassrooms || 0,
        attendanceRate:
          data.attendanceRate ||
          (data.TotalChildren > 0
            ? Math.round((data.TodaysPresentCount / data.TotalChildren) * 100)
            : 0),
      }))
    } catch (err) {
      const error = handleApiError(err)
      setState((prev) => ({
        ...prev,
        error: error.message,
      }))
    }
  }, [])

  /**
   * Fetch attendance trend (last 7 days)
   */
  const fetchAttendanceTrend = useCallback(async (days: number = 7) => {
    try {
      const response = await apiClient.get<AttendanceDay[]>(
        `/dashboards/admin/attendance/trend?days=${days}`
      )
      setState((prev) => ({
        ...prev,
        attendanceTrend: response.data,
      }))
    } catch (err) {
      // Trend is optional, don't fail dashboard
    }
  }, [])

  /**
   * Fetch classroom utilization metrics
   */
  const fetchClassroomUtilization = useCallback(async () => {
    try {
      const response = await apiClient.get<ClassroomUtilization[]>(
        '/dashboards/admin/classrooms/utilization'
      )
      setState((prev) => ({
        ...prev,
        classroomUtilization: (response.data || []).map((c: any) => ({
          classroomId: c.classroomId || c.ClassroomId,
          classroomName: c.classroomName || c.ClassroomName,
          capacity: c.capacity || 20,
          enrolled: c.enrolled ?? c.ChildCount ?? 0,
          present: c.present ?? c.StaffCount ?? 0,
          utilization: c.utilization ?? c.ChildCount ?? 0,
          occupancyRate: c.occupancyRate ?? c.ChildCount ?? 0,
        })),
      }))
    } catch (err) {
      // Utilization is optional, don't fail dashboard
    }
  }, [])

  /**
   * Fetch recent incidents/alerts
   */
  const fetchRecentIncidents = useCallback(async (limit: number = 5) => {
    try {
      const response = await apiClient.get<RecentIncident[]>(
        `/dashboards/admin/incidents/recent?limit=${limit}`
      )
      setState((prev) => ({
        ...prev,
        recentIncidents: (response.data || []).map((i: any) => ({
          id: i.id || i.Id,
          childId: i.childId || i.ChildId,
          childName: i.childName || i.ChildName,
          classroomId: i.classroomId || null,
          classroomName: i.classroomName || null,
          type: i.type || 'other',
          description: i.description || i.Description,
          severity: i.severity || 'medium',
          reportedAt: i.reportedAt || i.IncidentTime,
          reportedBy: i.reportedBy || 'System',
          status: i.status || 'open',
        })),
      }))
    } catch (err) {
      // Incidents are optional, don't fail dashboard
    }
  }, [])

  /**
   * Build quick stats for dashboard cards
   */
  const buildQuickStats = useCallback(() => {
    setState((prev) => ({
      ...prev,
      quickStats: [
        {
          label: 'Total Children',
          value: prev.totalChildren,
          color: 'blue',
          href: '/children',
        },
        {
          label: 'Present Today',
          value: prev.presentToday,
          color: 'green',
          href: '/attendance',
        },
        {
          label: 'Absent Today',
          value: prev.absentToday,
          color: 'red',
          href: '/attendance',
        },
        {
          label: 'Active Staff',
          value: prev.activeStaff,
          color: 'purple',
          href: '/staff',
        },
        {
          label: 'Classrooms',
          value: prev.totalClassrooms,
          color: 'yellow',
          href: '/classrooms',
        },
      ],
    }))
  }, [])

  /**
   * Refresh all dashboard data
   */
  const refreshDashboard = useCallback(async () => {
    setState((prev) => ({ ...prev, isRefreshing: true, error: null }))
    try {
      await Promise.all([
        fetchOverviewStats(),
        fetchAttendanceTrend(7),
        fetchClassroomUtilization(),
        fetchRecentIncidents(10),
      ])
      setState((prev) => ({
        ...prev,
        isRefreshing: false,
        lastUpdated: new Date(),
      }))
    } catch (err) {
      const error = handleApiError(err)
      setState((prev) => ({
        ...prev,
        isRefreshing: false,
        error: error.message,
      }))
    }
  }, [fetchOverviewStats, fetchAttendanceTrend, fetchClassroomUtilization, fetchRecentIncidents])

  /**
   * Load dashboard on mount
   */
  useEffect(() => {
    setState((prev) => ({ ...prev, isLoading: true }))
    refreshDashboard().then(() => {
      setState((prev) => ({ ...prev, isLoading: false }))
    })
  }, [refreshDashboard])

  /**
   * Update quick stats when overview changes
   */
  useEffect(() => {
    buildQuickStats()
  }, [state.totalChildren, state.presentToday, state.absentToday, buildQuickStats])

  /**
   * Setup auto-refresh interval (every 5 minutes)
   */
  useEffect(() => {
    const interval = setInterval(() => {
      refreshDashboard()
    }, 5 * 60 * 1000)

    return () => clearInterval(interval)
  }, [refreshDashboard])

  return {
    ...state,
    refreshDashboard,
    fetchOverviewStats,
    fetchAttendanceTrend,
    fetchClassroomUtilization,
    fetchRecentIncidents,
  }
}
