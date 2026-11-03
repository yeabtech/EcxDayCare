/**
 * Custom hook for enrollment data management
 */
import { useState, useCallback } from 'react'
import { Enrollment, EnrollmentFormInput, EnrollmentListParams, EndEnrollmentInput } from '../types'
import { enrollmentsApi } from '../services/enrollmentsApi'

interface UseEnrollmentsReturn {
  enrollments: Enrollment[]
  currentEnrollment: Enrollment | null
  loading: boolean
  error: string | null
  total: number
  page: number
  pageSize: number
  fetchEnrollments: (params?: EnrollmentListParams) => Promise<void>
  fetchEnrollment: (id: string) => Promise<void>
  createEnrollment: (data: EnrollmentFormInput) => Promise<Enrollment>
  updateEnrollment: (id: string, data: EnrollmentFormInput) => Promise<Enrollment>
  endEnrollment: (id: string, data: EndEnrollmentInput) => Promise<void>
  clearError: () => void
  clearCurrent: () => void
}

export const useEnrollments = (): UseEnrollmentsReturn => {
  const [enrollments, setEnrollments] = useState<Enrollment[]>([])
  const [currentEnrollment, setCurrentEnrollment] = useState<Enrollment | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const fetchEnrollments = useCallback(async (params?: EnrollmentListParams) => {
    setLoading(true)
    setError(null)
    try {
      const data = await enrollmentsApi.getEnrollments(params)
      setEnrollments(Array.isArray(data?.items) ? data.items : [])
      setTotal(data?.total ?? 0)
      setPage(data?.page ?? 1)
      setPageSize(data?.pageSize ?? 10)
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to fetch enrollments'
      setError(msg)
      setEnrollments([])
    } finally {
      setLoading(false)
    }
  }, [])

  const fetchEnrollment = useCallback(async (id: string) => {
    setLoading(true)
    setError(null)
    try {
      const data = await enrollmentsApi.getEnrollment(id)
      setCurrentEnrollment(data)
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to fetch enrollment'
      setError(msg)
      setCurrentEnrollment(null)
    } finally {
      setLoading(false)
    }
  }, [])

  const createEnrollment = useCallback(async (data: EnrollmentFormInput): Promise<Enrollment> => {
    setLoading(true)
    setError(null)
    try {
      const created = await enrollmentsApi.createEnrollment(data)
      setEnrollments((prev) => [created, ...prev])
      setTotal((prev) => prev + 1)
      return created
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to create enrollment'
      setError(msg)
      throw new Error(msg)
    } finally {
      setLoading(false)
    }
  }, [])

  const updateEnrollment = useCallback(
    async (id: string, data: EnrollmentFormInput): Promise<Enrollment> => {
      setLoading(true)
      setError(null)
      try {
        const updated = await enrollmentsApi.updateEnrollment(id, data)
        setEnrollments((prev) => prev.map((e) => (e.id === id ? updated : e)))
        if (currentEnrollment?.id === id) {
          setCurrentEnrollment(updated)
        }
        return updated
      } catch (err: any) {
        const msg = err.response?.data?.message || err.message || 'Failed to update enrollment'
        setError(msg)
        throw new Error(msg)
      } finally {
        setLoading(false)
      }
    },
    [currentEnrollment]
  )

  const endEnrollment = useCallback(
    async (id: string, data: EndEnrollmentInput): Promise<void> => {
      setLoading(true)
      setError(null)
      try {
        const ended = await enrollmentsApi.endEnrollment(id, data)
        setEnrollments((prev) => prev.map((e) => (e.id === id ? ended : e)))
        if (currentEnrollment?.id === id) {
          setCurrentEnrollment(ended)
        }
      } catch (err: any) {
        const msg = err.response?.data?.message || err.message || 'Failed to end enrollment'
        setError(msg)
        throw new Error(msg)
      } finally {
        setLoading(false)
      }
    },
    [currentEnrollment]
  )

  const clearError = useCallback(() => setError(null), [])
  const clearCurrent = useCallback(() => setCurrentEnrollment(null), [])

  return {
    enrollments,
    currentEnrollment,
    loading,
    error,
    total,
    page,
    pageSize,
    fetchEnrollments,
    fetchEnrollment,
    createEnrollment,
    updateEnrollment,
    endEnrollment,
    clearError,
    clearCurrent,
  }
}
