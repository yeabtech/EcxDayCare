/**
 * Custom hook for parents data management
 */
import { useState, useCallback } from 'react'
import { Parent, ParentFormInput, ParentListParams } from '../types'
import { parentsApi } from '../services/parentsApi'

interface UseParentsReturn {
  parents: Parent[]
  currentParent: Parent | null
  loading: boolean
  error: string | null
  total: number
  page: number
  pageSize: number
  fetchParents: (params?: ParentListParams) => Promise<void>
  fetchParent: (id: string) => Promise<void>
  createParent: (data: ParentFormInput) => Promise<Parent>
  updateParent: (id: string, data: ParentFormInput) => Promise<void>
  deleteParent: (id: string) => Promise<void>
  clearError: () => void
  clearCurrent: () => void
}

export const useParents = (): UseParentsReturn => {
  const [parents, setParents] = useState<Parent[]>([])
  const [currentParent, setCurrentParent] = useState<Parent | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const fetchParents = useCallback(async (params?: ParentListParams) => {
    setLoading(true)
    setError(null)
    try {
      const data = await parentsApi.getParents(params)
      setParents(Array.isArray(data?.items) ? data.items : [])
      setTotal(data?.total ?? 0)
      setPage(data?.page ?? 1)
      setPageSize(data?.pageSize ?? 10)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch parents')
      setParents([])
    } finally {
      setLoading(false)
    }
  }, [])

  const fetchParent = useCallback(async (id: string) => {
    setLoading(true)
    setError(null)
    try {
      const data = await parentsApi.getParent(id)
      setCurrentParent(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch parent')
      setCurrentParent(null)
    } finally {
      setLoading(false)
    }
  }, [])

  const createParent = useCallback(async (data: ParentFormInput): Promise<Parent> => {
    setLoading(true)
    setError(null)
    try {
      const result = await parentsApi.createParent(data)
      return result
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Failed to create parent'
      setError(errorMsg)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const updateParent = useCallback(async (id: string, data: ParentFormInput) => {
    setLoading(true)
    setError(null)
    try {
      await parentsApi.updateParent(id, data)
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Failed to update parent'
      setError(errorMsg)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const deleteParent = useCallback(async (id: string) => {
    setLoading(true)
    setError(null)
    try {
      await parentsApi.deleteParent(id)
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Failed to delete parent'
      setError(errorMsg)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const clearError = useCallback(() => setError(null), [])
  const clearCurrent = useCallback(() => setCurrentParent(null), [])

  return {
    parents,
    currentParent,
    loading,
    error,
    total,
    page,
    pageSize,
    fetchParents,
    fetchParent,
    createParent,
    updateParent,
    deleteParent,
    clearError,
    clearCurrent,
  }
}
