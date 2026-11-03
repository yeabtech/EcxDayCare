/**
 * useChildren hook
 * Manages children data fetching, caching, and mutations
 */
import { useState, useCallback } from 'react'
import { childrenApi } from '../services/childrenApi'
import { Child, ChildListDto, ChildFormInput, ChildrenListParams } from '../types'
import { handleApiError } from '@utils/index'

interface UseChildrenState {
  children: ChildListDto | null
  child: Child | null
  isLoading: boolean
  isCreating: boolean
  isUpdating: boolean
  isDeleting: boolean
  error: string | null
  success: string | null
}

const initialState: UseChildrenState = {
  children: null,
  child: null,
  isLoading: false,
  isCreating: false,
  isUpdating: false,
  isDeleting: false,
  error: null,
  success: null,
}

export const useChildren = () => {
  const [state, setState] = useState<UseChildrenState>(initialState)

  /**
   * Fetch list of children
   */
  const fetchChildren = useCallback(async (params?: ChildrenListParams) => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }))
    try {
      const data = await childrenApi.getChildren(params)
      setState((prev) => ({
        ...prev,
        children: data,
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
   * Fetch single child
   */
  const fetchChild = useCallback(async (id: string) => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }))
    try {
      const data = await childrenApi.getChild(id)
      setState((prev) => ({
        ...prev,
        child: data,
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
   * Create new child
   */
  const createChild = useCallback(async (data: ChildFormInput) => {
    setState((prev) => ({ ...prev, isCreating: true, error: null, success: null }))
    try {
      const newChild = await childrenApi.createChild(data)
      setState((prev) => ({
        ...prev,
        child: newChild,
        isCreating: false,
        success: 'Child created successfully',
        error: null,
      }))
      return newChild
    } catch (err) {
      const error = handleApiError(err)
      setState((prev) => ({
        ...prev,
        isCreating: false,
        error: error.message,
      }))
      throw error
    }
  }, [])

  /**
   * Update child
   */
  const updateChild = useCallback(async (id: string, data: ChildFormInput) => {
    setState((prev) => ({ ...prev, isUpdating: true, error: null, success: null }))
    try {
      const updated = await childrenApi.updateChild(id, data)
      setState((prev) => ({
        ...prev,
        child: updated,
        isUpdating: false,
        success: 'Child updated successfully',
        error: null,
      }))
      return updated
    } catch (err) {
      const error = handleApiError(err)
      setState((prev) => ({
        ...prev,
        isUpdating: false,
        error: error.message,
      }))
      throw error
    }
  }, [])

  /**
   * Delete child
   */
  const deleteChild = useCallback(async (id: string) => {
    setState((prev) => ({ ...prev, isDeleting: true, error: null, success: null }))
    try {
      await childrenApi.deleteChild(id)
      setState((prev) => ({
        ...prev,
        child: null,
        isDeleting: false,
        success: 'Child deleted successfully',
        error: null,
      }))
    } catch (err) {
      const error = handleApiError(err)
      setState((prev) => ({
        ...prev,
        isDeleting: false,
        error: error.message,
      }))
      throw error
    }
  }, [])

  /**
   * Clear error message
   */
  const clearError = useCallback(() => {
    setState((prev) => ({ ...prev, error: null }))
  }, [])

  /**
   * Clear success message
   */
  const clearSuccess = useCallback(() => {
    setState((prev) => ({ ...prev, success: null }))
  }, [])

  return {
    // State
    children: state.children,
    child: state.child,
    isLoading: state.isLoading,
    isCreating: state.isCreating,
    isUpdating: state.isUpdating,
    isDeleting: state.isDeleting,
    error: state.error,
    success: state.success,
    // Methods
    fetchChildren,
    fetchChild,
    createChild,
    updateChild,
    deleteChild,
    clearError,
    clearSuccess,
  }
}
