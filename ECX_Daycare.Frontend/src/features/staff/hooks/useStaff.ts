/**
 * Custom hook for staff data management
 */
import { useState, useCallback } from 'react'
import { Staff, StaffFormInput, StaffListParams } from '../types'
import { staffApi } from '../services/staffApi'

interface UseStaffReturn {
  staff: Staff[]
  currentStaff: Staff | null
  loading: boolean
  error: string | null
  total: number
  page: number
  pageSize: number
  fetchStaff: (params?: StaffListParams) => Promise<void>
  fetchStaffMember: (id: string) => Promise<void>
  createStaff: (data: StaffFormInput) => Promise<Staff>
  updateStaff: (id: string, data: StaffFormInput) => Promise<void>
  deleteStaff: (id: string) => Promise<void>
  assignClassroom: (id: string, classroomId: string | null) => Promise<void>
  clearError: () => void
  clearCurrent: () => void
}

export const useStaff = (): UseStaffReturn => {
  const [staff, setStaff] = useState<Staff[]>([])
  const [currentStaff, setCurrentStaff] = useState<Staff | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const fetchStaff = useCallback(async (params?: StaffListParams) => {
    setLoading(true)
    setError(null)
    try {
      const data = await staffApi.getStaff(params)
      setStaff(Array.isArray(data?.items) ? data.items : [])
      setTotal(data?.total ?? 0)
      setPage(data?.page ?? 1)
      setPageSize(data?.pageSize ?? 10)
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to fetch staff members'
      setError(msg)
      setStaff([])
    } finally {
      setLoading(false)
    }
  }, [])

  const fetchStaffMember = useCallback(async (id: string) => {
    setLoading(true)
    setError(null)
    try {
      const data = await staffApi.getStaffMember(id)
      setCurrentStaff(data)
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to fetch staff member'
      setError(msg)
      setCurrentStaff(null)
    } finally {
      setLoading(false)
    }
  }, [])

  const createStaff = useCallback(async (data: StaffFormInput): Promise<Staff> => {
    setLoading(true)
    setError(null)
    try {
      const newStaff = await staffApi.createStaff(data)
      setStaff((prev) => [newStaff, ...prev])
      setTotal((prev) => prev + 1)
      return newStaff
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to create staff member'
      setError(msg)
      throw new Error(msg)
    } finally {
      setLoading(false)
    }
  }, [])

  const updateStaff = useCallback(
    async (id: string, data: StaffFormInput): Promise<void> => {
      setLoading(true)
      setError(null)
      try {
        await staffApi.updateStaff(id, data)
        setStaff((prev) =>
          prev.map((s) => (s.id === id ? { ...s, ...data, classroomId: data.classroomId || null } : s))
        )
        if (currentStaff?.id === id) {
          setCurrentStaff((prev) => (prev ? { ...prev, ...data, classroomId: data.classroomId || null } : null))
        }
      } catch (err: any) {
        const msg = err.response?.data?.message || err.message || 'Failed to update staff member'
        setError(msg)
        throw new Error(msg)
      } finally {
        setLoading(false)
      }
    },
    [currentStaff]
  )

  const deleteStaff = useCallback(async (id: string): Promise<void> => {
    setLoading(true)
    setError(null)
    try {
      await staffApi.deleteStaff(id)
      setStaff((prev) => prev.filter((s) => s.id !== id))
      setTotal((prev) => Math.max(0, prev - 1))
      if (currentStaff?.id === id) {
        setCurrentStaff(null)
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to delete staff member'
      setError(msg)
      throw new Error(msg)
    } finally {
      setLoading(false)
    }
  }, [currentStaff])

  const assignClassroom = useCallback(async (id: string, classroomId: string | null): Promise<void> => {
    setLoading(true)
    setError(null)
    try {
      await staffApi.assignClassroom(id, classroomId)
      setStaff((prev) =>
        prev.map((s) => (s.id === id ? { ...s, classroomId: classroomId || null } : s))
      )
      if (currentStaff?.id === id) {
        setCurrentStaff((prev) => (prev ? { ...prev, classroomId: classroomId || null } : null))
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to assign classroom'
      setError(msg)
      throw new Error(msg)
    } finally {
      setLoading(false)
    }
  }, [currentStaff])

  const clearError = useCallback(() => setError(null), [])
  const clearCurrent = useCallback(() => setCurrentStaff(null), [])

  return {
    staff,
    currentStaff,
    loading,
    error,
    total,
    page,
    pageSize,
    fetchStaff,
    fetchStaffMember,
    createStaff,
    updateStaff,
    deleteStaff,
    assignClassroom,
    clearError,
    clearCurrent,
  }
}
