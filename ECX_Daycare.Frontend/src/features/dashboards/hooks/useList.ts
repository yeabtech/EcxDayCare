/**
 * useList hook
 * Manages list state with filtering, sorting, and pagination
 */
import { useState, useCallback } from 'react'
import {
  ListFilter,
  ListSort,
  ListState,
  applyListTransformations,
  filterItems,
} from '@utils/listUtils'

export interface UseListOptions<T> {
  initialItems?: T[]
  initialPage?: number
  pageSize?: number
  sortKey?: string
  sortDirection?: 'asc' | 'desc'
}

export const useList = <T extends Record<string, any>>(options: UseListOptions<T> = {}) => {
  const {
    initialItems = [],
    initialPage = 1,
    pageSize = 10,
    sortKey,
    sortDirection = 'asc',
  } = options

  const [state, setState] = useState<ListState<T>>({
    items: initialItems,
    filters: [],
    sort: sortKey ? { key: sortKey, direction: sortDirection as 'asc' | 'desc' } : null,
    pagination: {
      page: initialPage,
      pageSize,
      total: initialItems.length,
      totalPages: Math.ceil(initialItems.length / pageSize),
    },
    isLoading: false,
    error: null,
    selectedIds: new Set(),
  })

  /**
   * Update all items and reset pagination
   */
  const setItems = useCallback((items: T[]) => {
    setState((prev) => {
      const total = items.length
      const totalPages = Math.ceil(total / prev.pagination.pageSize)
      return {
        ...prev,
        items,
        pagination: {
          ...prev.pagination,
          page: 1,
          total,
          totalPages,
        },
      }
    })
  }, [])

  /**
   * Apply filters
   */
  const setFilters = useCallback((filters: ListFilter[]) => {
    setState((prev) => {
      const filtered = filterItems(prev.items, filters)
      const total = filtered.length
      const totalPages = Math.ceil(total / prev.pagination.pageSize)
      return {
        ...prev,
        filters,
        pagination: {
          ...prev.pagination,
          page: 1,
          total,
          totalPages,
        },
      }
    })
  }, [])

  /**
   * Apply sorting
   */
  const setSort = useCallback((sort: ListSort | null) => {
    setState((prev) => ({
      ...prev,
      sort,
    }))
  }, [])

  /**
   * Change page
   */
  const setPage = useCallback((page: number) => {
    setState((prev) => ({
      ...prev,
      pagination: {
        ...prev.pagination,
        page: Math.max(1, Math.min(page, prev.pagination.totalPages)),
      },
    }))
  }, [])

  /**
   * Change page size
   */
  const setPageSize = useCallback((newPageSize: number) => {
    setState((prev) => {
      const totalPages = Math.ceil(prev.pagination.total / newPageSize)
      return {
        ...prev,
        pagination: {
          ...prev.pagination,
          pageSize: newPageSize,
          page: Math.min(prev.pagination.page, totalPages),
          totalPages,
        },
      }
    })
  }, [])

  /**
   * Set loading state
   */
  const setLoading = useCallback((isLoading: boolean) => {
    setState((prev) => ({ ...prev, isLoading }))
  }, [])

  /**
   * Set error state
   */
  const setError = useCallback((error: string | null) => {
    setState((prev) => ({ ...prev, error }))
  }, [])

  /**
   * Toggle item selection
   */
  const toggleSelection = useCallback((id: string) => {
    setState((prev) => {
      const newSelected = new Set(prev.selectedIds)
      if (newSelected.has(id)) {
        newSelected.delete(id)
      } else {
        newSelected.add(id)
      }
      return { ...prev, selectedIds: newSelected }
    })
  }, [])

  /**
   * Select all items on current page
   */
  const selectAll = useCallback(() => {
    setState((prev) => {
      const { items } = applyListTransformations(
        prev.items,
        prev.filters,
        prev.sort,
        prev.pagination.page,
        prev.pagination.pageSize
      )
      const newSelected = new Set(prev.selectedIds)
      items.forEach((item) => {
        if (item.id) newSelected.add(item.id)
      })
      return { ...prev, selectedIds: newSelected }
    })
  }, [])

  /**
   * Clear all selections
   */
  const clearSelections = useCallback(() => {
    setState((prev) => ({ ...prev, selectedIds: new Set() }))
  }, [])

  /**
   * Get transformed items (filtered, sorted, paginated)
   */
  const getDisplayItems = useCallback(() => {
    return applyListTransformations(
      state.items,
      state.filters,
      state.sort,
      state.pagination.page,
      state.pagination.pageSize
    )
  }, [state.items, state.filters, state.sort, state.pagination])

  return {
    // State
    ...state,
    displayItems: getDisplayItems().items,

    // Actions
    setItems,
    setFilters,
    setSort,
    setPage,
    setPageSize,
    setLoading,
    setError,
    toggleSelection,
    selectAll,
    clearSelections,

    // Helpers
    getDisplayItems,
    isSelected: (id: string) => state.selectedIds.has(id),
    selectionsCount: state.selectedIds.size,
    isAllSelected:
      state.items.length > 0 &&
      state.selectedIds.size === applyListTransformations(
        state.items,
        state.filters,
        state.sort,
        state.pagination.page,
        state.pagination.pageSize
      ).items.length,
  }
}
