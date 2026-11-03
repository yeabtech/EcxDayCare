/**
 * Custom hook for daily reports data management
 */
import { useState, useCallback } from 'react'
import { dailyReportsApi } from '../services/dailyReportsApi'
import type {
  DailyReport,
  ChildDailyReports,
  CreateDailyReportRequest,
  UpdateDailyReportRequest,
} from '../types'

interface UseDailyReportsState {
  dailyFeed: DailyReport[]
  childReports: ChildDailyReports | null
  currentReport: DailyReport | null
  loading: boolean
  error: string | null
}

export const useDailyReports = () => {
  const [state, setState] = useState<UseDailyReportsState>({
    dailyFeed: [],
    childReports: null,
    currentReport: null,
    loading: false,
    error: null,
  })

  const setLoading = (loading: boolean) => setState((s) => ({ ...s, loading, error: null }))
  const setError = (error: string) => setState((s) => ({ ...s, loading: false, error }))

  const fetchDailyFeed = useCallback(async (date?: string) => {
    setLoading(true)
    try {
      const data = await dailyReportsApi.getDailyFeed(date)
      setState((s) => ({ ...s, dailyFeed: data, loading: false }))
    } catch (err: any) {
      setError(err?.response?.data?.message ?? 'Failed to load daily feed.')
    }
  }, [])

  const fetchChildReports = useCallback(
    async (
      childId: string,
      params?: { startDate?: string; endDate?: string; page?: number; pageSize?: number },
    ) => {
      setLoading(true)
      try {
        const data = await dailyReportsApi.getByChild(childId, params)
        setState((s) => ({ ...s, childReports: data, loading: false }))
      } catch (err: any) {
        setError(err?.response?.data?.message ?? 'Failed to load child reports.')
      }
    },
    [],
  )

  const fetchById = useCallback(async (id: string) => {
    setLoading(true)
    try {
      const data = await dailyReportsApi.getById(id)
      setState((s) => ({ ...s, currentReport: data, loading: false }))
      return data
    } catch (err: any) {
      setError(err?.response?.data?.message ?? 'Failed to load report.')
      return null
    }
  }, [])

  const createReport = useCallback(async (data: CreateDailyReportRequest): Promise<DailyReport | null> => {
    setLoading(true)
    try {
      const report = await dailyReportsApi.create(data)
      setState((s) => ({
        ...s,
        dailyFeed: [report, ...s.dailyFeed],
        loading: false,
      }))
      return report
    } catch (err: any) {
      const msg = err?.response?.data?.message ?? 'Failed to create report.'
      setError(msg)
      return null
    }
  }, [])

  const updateReport = useCallback(
    async (id: string, data: UpdateDailyReportRequest): Promise<DailyReport | null> => {
      setLoading(true)
      try {
        const report = await dailyReportsApi.update(id, data)
        setState((s) => ({
          ...s,
          dailyFeed: s.dailyFeed.map((r) => (r.id === id ? report : r)),
          childReports: s.childReports
            ? {
                ...s.childReports,
                items: s.childReports.items.map((r) => (r.id === id ? report : r)),
              }
            : null,
          currentReport: s.currentReport?.id === id ? report : s.currentReport,
          loading: false,
        }))
        return report
      } catch (err: any) {
        const msg = err?.response?.data?.message ?? 'Failed to update report.'
        setError(msg)
        return null
      }
    },
    [],
  )

  const sendToParent = useCallback(async (id: string): Promise<DailyReport | null> => {
    setLoading(true)
    try {
      const report = await dailyReportsApi.sendToParent(id)
      setState((s) => ({
        ...s,
        dailyFeed: s.dailyFeed.map((r) => (r.id === id ? report : r)),
        childReports: s.childReports
          ? {
              ...s.childReports,
              items: s.childReports.items.map((r) => (r.id === id ? report : r)),
            }
          : null,
        currentReport: s.currentReport?.id === id ? report : s.currentReport,
        loading: false,
      }))
      return report
    } catch (err: any) {
      setError(err?.response?.data?.message ?? 'Failed to send report.')
      return null
    }
  }, [])

  const deleteReport = useCallback(async (id: string): Promise<boolean> => {
    setLoading(true)
    try {
      await dailyReportsApi.delete(id)
      setState((s) => ({
        ...s,
        dailyFeed: s.dailyFeed.filter((r) => r.id !== id),
        childReports: s.childReports
          ? {
              ...s.childReports,
              items: s.childReports.items.filter((r) => r.id !== id),
              totalCount: s.childReports.totalCount - 1,
            }
          : null,
        loading: false,
      }))
      return true
    } catch (err: any) {
      setError(err?.response?.data?.message ?? 'Failed to delete report.')
      return false
    }
  }, [])

  const clearError = useCallback(() => setState((s) => ({ ...s, error: null })), [])

  return {
    ...state,
    fetchDailyFeed,
    fetchChildReports,
    fetchById,
    createReport,
    updateReport,
    sendToParent,
    deleteReport,
    clearError,
  }
}
