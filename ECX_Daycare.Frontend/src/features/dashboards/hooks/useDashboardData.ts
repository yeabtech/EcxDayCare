/**
 * useDashboardData hook
 * Manages dashboard data fetching for all roles
 */
import { useState, useCallback } from 'react'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'

export interface DashboardStats {
  totalChildren: number
  presentToday: number
  absentToday: number
  activeStaff: number
  totalClassrooms: number
  attendanceRate: number
}

export interface ClassroomSummary {
  id: string
  name: string
  childrenCount: number
  presentCount: number
  absentCount: number
}

export interface ChildAttendanceStatus {
  childId: string
  childName: string
  status: 'present' | 'absent' | 'pending'
  checkInTime?: string
  checkOutTime?: string
}

interface UseDashboardDataState {
  stats: DashboardStats | null
  classrooms: ClassroomSummary[]
  attendance: ChildAttendanceStatus[]
  isLoading: boolean
  error: string | null
}

const initialState: UseDashboardDataState = {
  stats: null,
  classrooms: [],
  attendance: [],
  isLoading: false,
  error: null,
}

export const useDashboardData = () => {
  const [state, setState] = useState<UseDashboardDataState>(initialState)

  /**
   * Fetch dashboard statistics
   */
  const fetchDashboardStats = useCallback(
    async (role: 'Admin' | 'Staff' | 'Parent' = 'Admin', classroomId?: string) => {
      setState((prev) => ({ ...prev, isLoading: true, error: null }))
      try {
        const endpoint =
          role === 'Staff' && classroomId
            ? `/dashboards/staff/${classroomId}/stats`
            : `/dashboards/${role.toLowerCase()}/stats`

        const response = await apiClient.get<DashboardStats>(endpoint)
        setState((prev) => ({
          ...prev,
          stats: response.data,
          isLoading: false,
          error: null,
        }))
      } catch (err) {
        const error = handleApiError(err)
        setState((prev) => ({
          ...prev,
          isLoading: false,
          error: error.message,
        }))
      }
    },
    []
  )

  /**
   * Fetch classroom summaries
   */
  const fetchClassrooms = useCallback(async () => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }))
    try {
      const response = await apiClient.get<ClassroomSummary[]>(
        '/dashboards/admin/classrooms'
      )
      setState((prev) => ({
        ...prev,
        classrooms: response.data,
        isLoading: false,
        error: null,
      }))
    } catch (err) {
      const error = handleApiError(err)
      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: error.message,
      }))
    }
  }, [])

  /**
   * Fetch today's attendance
   */
  const fetchTodayAttendance = useCallback(async (classroomId?: string) => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }))
    try {
      const url = classroomId
        ? `/dashboards/attendance?classroomId=${classroomId}`
        : '/dashboards/attendance'

      const response = await apiClient.get<ChildAttendanceStatus[]>(url)
      setState((prev) => ({
        ...prev,
        attendance: response.data,
        isLoading: false,
        error: null,
      }))
    } catch (err) {
      const error = handleApiError(err)
      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: error.message,
      }))
    }
  }, [])

  /**
   * Refresh all dashboard data
   */
  const refreshDashboard = useCallback(
    async (role: 'Admin' | 'Staff' | 'Parent' = 'Admin', classroomId?: string) => {
      await Promise.all([
        fetchDashboardStats(role, classroomId),
        role === 'Admin' ? fetchClassrooms() : Promise.resolve(),
        fetchTodayAttendance(classroomId),
      ])
    },
    [fetchDashboardStats, fetchClassrooms, fetchTodayAttendance]
  )

  return {
    ...state,
    fetchDashboardStats,
    fetchClassrooms,
    fetchTodayAttendance,
    refreshDashboard,
  }
}
