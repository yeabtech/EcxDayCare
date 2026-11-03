/**
 * useParentDashboard hook
 * Specialized data fetching for parent dashboard
 */
import { useState, useCallback, useEffect } from 'react'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'

export interface ChildData {
  childId: string
  childName: string
  dateOfBirth: string
  classroomId: string
  classroomName: string
  primaryTeacherId: string
  primaryTeacherName: string
  pickupTime: string
}

export interface ChildCheckInStatus {
  childId: string
  status: 'present' | 'absent' | 'pending'
  checkInTime?: string
  checkOutTime?: string
}

export interface ActivityLog {
  id: string
  type: 'meal' | 'nap' | 'activity' | 'health'
  description: string
  timestamp: string
  notes?: string
}

export interface DailyReportSummary {
  date: string
  childId: string
  mealCount: number
  napDuration: number
  activitiesCount: number
  mood: string
  notes?: string
}

export interface Message {
  id: string
  fromName: string
  fromRole: 'staff' | 'admin'
  message: string
  timestamp: string
  childId?: string
}

interface UseParentDashboardState {
  children: ChildData[]
  selectedChild: ChildData | null
  checkInStatus: ChildCheckInStatus | null
  activities: ActivityLog[]
  dailyReport: DailyReportSummary | null
  messages: Message[]
  isLoading: boolean
  error: string | null
}

const initialState: UseParentDashboardState = {
  children: [],
  selectedChild: null,
  checkInStatus: null,
  activities: [],
  dailyReport: null,
  messages: [],
  isLoading: false,
  error: null,
}

export const useParentDashboard = () => {
  const [state, setState] = useState<UseParentDashboardState>(initialState)

  /**
   * Fetch parent's children
   */
  const fetchChildren = useCallback(async () => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }))
    try {
      const response = await apiClient.get<ChildData[]>('/dashboards/parent/children')
      setState((prev) => ({
        ...prev,
        children: response.data,
        isLoading: false,
        // Auto-select first child
        selectedChild: response.data[0] || null,
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
   * Fetch child's check-in status
   */
  const fetchChildCheckInStatus = useCallback(async (childId: string) => {
    try {
      const response = await apiClient.get<ChildCheckInStatus>(
        `/dashboards/parent/child/${childId}/check-in`
      )
      setState((prev) => ({
        ...prev,
        checkInStatus: response.data,
      }))
    } catch (err) {
      // Status is optional
    }
  }, [])

  /**
   * Fetch child's activity log for today
   */
  const fetchActivityLog = useCallback(async (childId: string) => {
    try {
      const response = await apiClient.get<ActivityLog[]>(
        `/dashboards/parent/child/${childId}/activities?date=today`
      )
      setState((prev) => ({
        ...prev,
        activities: response.data,
      }))
    } catch (err) {
      // Activities are optional
    }
  }, [])

  /**
   * Fetch today's daily report
   */
  const fetchDailyReport = useCallback(async (childId: string) => {
    try {
      const response = await apiClient.get<DailyReportSummary>(
        `/dashboards/parent/child/${childId}/report?date=today`
      )
      setState((prev) => ({
        ...prev,
        dailyReport: response.data,
      }))
    } catch (err) {
      // Report is optional
    }
  }, [])

  /**
   * Fetch messages for child or all
   */
  const fetchMessages = useCallback(async (childId?: string) => {
    try {
      const url = childId
        ? `/dashboards/parent/messages?childId=${childId}`
        : '/dashboards/parent/messages'
      const response = await apiClient.get<Message[]>(url)
      setState((prev) => ({
        ...prev,
        messages: response.data,
      }))
    } catch (err) {
      // Messages are optional
    }
  }, [])

  /**
   * Set selected child and load their data
   */
  const selectChild = useCallback(
    async (childId: string) => {
      const child = state.children.find((c) => c.childId === childId)
      if (!child) return

      setState((prev) => ({
        ...prev,
        selectedChild: child,
      }))

      // Load child-specific data
      await Promise.all([
        fetchChildCheckInStatus(childId),
        fetchActivityLog(childId),
        fetchDailyReport(childId),
        fetchMessages(childId),
      ])
    },
    [state.children, fetchChildCheckInStatus, fetchActivityLog, fetchDailyReport, fetchMessages]
  )

  /**
   * Refresh all parent dashboard data
   */
  const refreshDashboard = useCallback(async () => {
    await fetchChildren()
    if (state.selectedChild) {
      await selectChild(state.selectedChild.childId)
    }
  }, [fetchChildren, state.selectedChild, selectChild])

  // Auto-load children on mount
  useEffect(() => {
    fetchChildren()
  }, [fetchChildren])

  return {
    ...state,
    fetchChildren,
    fetchChildCheckInStatus,
    fetchActivityLog,
    fetchDailyReport,
    fetchMessages,
    selectChild,
    refreshDashboard,
  }
}
