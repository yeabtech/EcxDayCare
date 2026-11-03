/**
 * Custom hook for classrooms data management
 */
import { useState, useCallback } from 'react'
import { Classroom, ClassroomFormInput, ClassroomListParams } from '../types'
import { classroomsApi } from '../services/classroomsApi'

interface UseClassroomsReturn {
  classrooms: Classroom[]
  currentClassroom: Classroom | null
  loading: boolean
  error: string | null
  total: number
  page: number
  pageSize: number
  fetchClassrooms: (params?: ClassroomListParams) => Promise<void>
  fetchClassroom: (id: string) => Promise<void>
  createClassroom: (data: ClassroomFormInput) => Promise<Classroom>
  updateClassroom: (id: string, data: ClassroomFormInput) => Promise<void>
  deleteClassroom: (id: string) => Promise<void>
  enrollChild: (classroomId: string, childId: string) => Promise<void>
  removeChild: (classroomId: string, childId: string) => Promise<void>
  clearError: () => void
  clearCurrent: () => void
}

export const useClassrooms = (): UseClassroomsReturn => {
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [currentClassroom, setCurrentClassroom] = useState<Classroom | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const fetchClassrooms = useCallback(async (params?: ClassroomListParams) => {
    setLoading(true)
    setError(null)
    try {
      const data = await classroomsApi.getClassrooms(params)
      setClassrooms(Array.isArray(data?.items) ? data.items : [])
      setTotal(data?.total ?? 0)
      setPage(data?.page ?? 1)
      setPageSize(data?.pageSize ?? 10)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch classrooms')
      setClassrooms([])
    } finally {
      setLoading(false)
    }
  }, [])

  const fetchClassroom = useCallback(async (id: string) => {
    setLoading(true)
    setError(null)
    try {
      const data = await classroomsApi.getClassroom(id)
      setCurrentClassroom(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch classroom')
      setCurrentClassroom(null)
    } finally {
      setLoading(false)
    }
  }, [])

  const createClassroom = useCallback(async (data: ClassroomFormInput): Promise<Classroom> => {
    setLoading(true)
    setError(null)
    try {
      const result = await classroomsApi.createClassroom(data)
      return result
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Failed to create classroom'
      setError(errorMsg)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const updateClassroom = useCallback(async (id: string, data: ClassroomFormInput) => {
    setLoading(true)
    setError(null)
    try {
      await classroomsApi.updateClassroom(id, data)
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Failed to update classroom'
      setError(errorMsg)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const deleteClassroom = useCallback(async (id: string) => {
    setLoading(true)
    setError(null)
    try {
      await classroomsApi.deleteClassroom(id)
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Failed to delete classroom'
      setError(errorMsg)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const enrollChild = useCallback(async (classroomId: string, childId: string) => {
    setLoading(true)
    setError(null)
    try {
      await classroomsApi.enrollChild(classroomId, childId)
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Failed to enroll child'
      setError(errorMsg)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const removeChild = useCallback(async (classroomId: string, childId: string) => {
    setLoading(true)
    setError(null)
    try {
      await classroomsApi.removeChild(classroomId, childId)
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Failed to remove child'
      setError(errorMsg)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const clearError = useCallback(() => setError(null), [])
  const clearCurrent = useCallback(() => setCurrentClassroom(null), [])

  return {
    classrooms,
    currentClassroom,
    loading,
    error,
    total,
    page,
    pageSize,
    fetchClassrooms,
    fetchClassroom,
    createClassroom,
    updateClassroom,
    deleteClassroom,
    enrollChild,
    removeChild,
    clearError,
    clearCurrent,
  }
}
