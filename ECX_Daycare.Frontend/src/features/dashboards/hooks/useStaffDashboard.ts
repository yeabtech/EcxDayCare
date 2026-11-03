/**
 * useStaffDashboard hook
 * Specialized data fetching for staff dashboard
 */
import { useState, useCallback, useEffect } from 'react'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'

export interface StaffClassroomData {
  classroomId: string
  classroomName: string
  totalChildren: number
  presentCount: number
  absentCount: number
  capacity: number
}

export interface ChildCheckInStatus {
  childId: string
  childName: string
  status: 'present' | 'absent' | 'pending'
  checkInTime?: string
  checkOutTime?: string
  parentName: string
}

export interface DailyTask {
  id: string
  type: 'meal' | 'nap' | 'activity' | 'health' | 'report'
  childId: string
  childName: string
  scheduledTime: string
  completed: boolean
  notes?: string
}

interface UseStaffDashboardState {
  classroom: StaffClassroomData | null
  children: ChildCheckInStatus[]
  tasks: DailyTask[]
  isLoading: boolean
  error: string | null
}

const initialState: UseStaffDashboardState = {
  classroom: null,
  children: [],
  tasks: [],
  isLoading: false,
  error: null,
}

export const useStaffDashboard = (classroomId?: string) => {
  const [state, setState] = useState<UseStaffDashboardState>(initialState)

  /**
   * Fetch staff's classroom data
   */
  const fetchClassroomData = useCallback(async (id?: string) => {
    if (!id) return

    setState((prev) => ({ ...prev, isLoading: true, error: null }))
    try {
      const response = await apiClient.get<StaffClassroomData>(
        `/dashboards/staff/classroom/${id}`
      )
      setState((prev) => ({
        ...prev,
        classroom: response.data,
        isLoading: false,
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
   * Fetch classroom children with check-in status
   */
  const fetchChildrenStatus = useCallback(async (id?: string) => {
    if (!id) return

    setState((prev) => ({ ...prev, isLoading: true, error: null }))
    try {
      const response = await apiClient.get<ChildCheckInStatus[]>(
        `/dashboards/staff/classroom/${id}/children`
      )
      setState((prev) => ({
        ...prev,
        children: response.data,
        isLoading: false,
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
   * Fetch today's tasks
   */
  const fetchDailyTasks = useCallback(async (id?: string) => {
    if (!id) return

    try {
      const response = await apiClient.get<DailyTask[]>(
        `/dashboards/staff/classroom/${id}/tasks`
      )
      setState((prev) => ({
        ...prev,
        tasks: response.data,
      }))
    } catch (err) {
      // Tasks are optional, don't set error
    }
  }, [])

  /**
   * Check in a child
   */
  const checkInChild = useCallback(async (childId: string) => {
    try {
      await apiClient.post(`/attendance/checkin`, { childId })
      // Refresh children status
      if (classroomId) {
        await fetchChildrenStatus(classroomId)
      }
    } catch (err) {
      const error = handleApiError(err)
      setState((prev) => ({
        ...prev,
        error: error.message,
      }))
    }
  }, [classroomId, fetchChildrenStatus])

  /**
   * Check out a child
   */
  const checkOutChild = useCallback(async (childId: string) => {
    try {
      await apiClient.post(`/attendance/checkout`, { childId })
      // Refresh children status
      if (classroomId) {
        await fetchChildrenStatus(classroomId)
      }
    } catch (err) {
      const error = handleApiError(err)
      setState((prev) => ({
        ...prev,
        error: error.message,
      }))
    }
  }, [classroomId, fetchChildrenStatus])

  /**
   * Mark task as complete
   */
  const completeTask = useCallback(async (taskId: string) => {
    try {
      await apiClient.patch(`/dashboards/staff/tasks/${taskId}`, {
        completed: true,
      })
      // Update local state
      setState((prev) => ({
        ...prev,
        tasks: prev.tasks.map((task) =>
          task.id === taskId ? { ...task, completed: true } : task
        ),
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
   * Refresh all staff dashboard data
   */
  const refreshDashboard = useCallback(async (id?: string) => {
    const target = id || classroomId
    if (!target) return

    await Promise.all([
      fetchClassroomData(target),
      fetchChildrenStatus(target),
      fetchDailyTasks(target),
    ])
  }, [classroomId, fetchClassroomData, fetchChildrenStatus, fetchDailyTasks])

  // Auto-load on mount if classroomId provided
  useEffect(() => {
    if (classroomId) {
      refreshDashboard(classroomId)
    }
  }, [classroomId, refreshDashboard])

  return {
    ...state,
    fetchClassroomData,
    fetchChildrenStatus,
    fetchDailyTasks,
    checkInChild,
    checkOutChild,
    completeTask,
    refreshDashboard,
  }
}
