/**
 * Custom hook for attendance data management
 */
import { useState, useCallback } from 'react'
import type {
  DailyAttendanceSummary,
  ChildAttendanceHistory,
  AttendanceRecord,
  CheckInRequest,
  CheckOutRequest,
  MarkAbsentRequest,
  DailyAttendanceParams,
  AttendanceHistoryParams,
} from '../types'
import { attendanceApi } from '../services/attendanceApi'

interface UseAttendanceReturn {
  summary: DailyAttendanceSummary | null
  history: ChildAttendanceHistory | null
  loading: boolean
  actionLoading: boolean
  error: string | null
  fetchTodayAttendance: (params?: DailyAttendanceParams) => Promise<void>
  fetchChildHistory: (childId: string, params?: AttendanceHistoryParams) => Promise<void>
  checkIn: (data: CheckInRequest) => Promise<AttendanceRecord>
  checkOut: (data: CheckOutRequest) => Promise<AttendanceRecord>
  checkOutById: (attendanceId: string, data: CheckOutRequest) => Promise<AttendanceRecord>
  markAbsent: (data: MarkAbsentRequest) => Promise<AttendanceRecord>
  clearError: () => void
}

export const useAttendance = (): UseAttendanceReturn => {
  const [summary, setSummary] = useState<DailyAttendanceSummary | null>(null)
  const [history, setHistory] = useState<ChildAttendanceHistory | null>(null)
  const [loading, setLoading] = useState(false)
  const [actionLoading, setActionLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const fetchTodayAttendance = useCallback(async (params?: DailyAttendanceParams) => {
    setLoading(true)
    setError(null)
    try {
      const data = await attendanceApi.getTodayAttendance(params)
      setSummary(data)
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to fetch attendance'
      setError(msg)
    } finally {
      setLoading(false)
    }
  }, [])

  const fetchChildHistory = useCallback(
    async (childId: string, params?: AttendanceHistoryParams) => {
      setLoading(true)
      setError(null)
      try {
        const data = await attendanceApi.getChildHistory(childId, params)
        setHistory(data)
      } catch (err: any) {
        const msg =
          err.response?.data?.message || err.message || 'Failed to fetch attendance history'
        setError(msg)
      } finally {
        setLoading(false)
      }
    },
    [],
  )

  const checkIn = useCallback(async (data: CheckInRequest): Promise<AttendanceRecord> => {
    setActionLoading(true)
    setError(null)
    try {
      const result = await attendanceApi.checkIn(data)
      setSummary((prev) =>
        prev
          ? {
              ...prev,
              totalPresent: prev.totalPresent + 1,
              totalCurrentlyCheckedIn: prev.totalCurrentlyCheckedIn + 1,
              items: [result, ...prev.items.filter((i) => i.childId !== result.childId)],
            }
          : prev,
      )
      return result
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Check-in failed'
      setError(msg)
      throw new Error(msg)
    } finally {
      setActionLoading(false)
    }
  }, [])

  const checkOut = useCallback(async (data: CheckOutRequest): Promise<AttendanceRecord> => {
    setActionLoading(true)
    setError(null)
    try {
      const result = await attendanceApi.checkOut(data)
      setSummary((prev) =>
        prev
          ? {
              ...prev,
              totalCurrentlyCheckedIn: Math.max(0, prev.totalCurrentlyCheckedIn - 1),
              items: prev.items.map((i) => (i.childId === result.childId ? result : i)),
            }
          : prev,
      )
      return result
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Check-out failed'
      setError(msg)
      throw new Error(msg)
    } finally {
      setActionLoading(false)
    }
  }, [])

  const checkOutById = useCallback(
    async (attendanceId: string, data: CheckOutRequest): Promise<AttendanceRecord> => {
      setActionLoading(true)
      setError(null)
      try {
        const result = await attendanceApi.checkOutById(attendanceId, data)
        setSummary((prev) =>
          prev
            ? {
                ...prev,
                totalCurrentlyCheckedIn: Math.max(0, prev.totalCurrentlyCheckedIn - 1),
                items: prev.items.map((i) => (i.id === attendanceId ? result : i)),
              }
            : prev,
        )
        return result
      } catch (err: any) {
        const msg = err.response?.data?.message || err.message || 'Check-out failed'
        setError(msg)
        throw new Error(msg)
      } finally {
        setActionLoading(false)
      }
    },
    [],
  )

  const markAbsent = useCallback(async (data: MarkAbsentRequest): Promise<AttendanceRecord> => {
    setActionLoading(true)
    setError(null)
    try {
      const result = await attendanceApi.markAbsent(data)
      setSummary((prev) =>
        prev
          ? {
              ...prev,
              totalAbsent: prev.totalAbsent + 1,
              items: [result, ...prev.items.filter((i) => i.childId !== result.childId)],
            }
          : prev,
      )
      return result
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to mark absent'
      setError(msg)
      throw new Error(msg)
    } finally {
      setActionLoading(false)
    }
  }, [])

  const clearError = useCallback(() => setError(null), [])

  return {
    summary,
    history,
    loading,
    actionLoading,
    error,
    fetchTodayAttendance,
    fetchChildHistory,
    checkIn,
    checkOut,
    checkOutById,
    markAbsent,
    clearError,
  }
}
