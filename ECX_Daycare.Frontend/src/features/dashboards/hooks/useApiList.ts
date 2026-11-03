/**
 * useApiList hook
 * Manages list state with API calls for fetching, filtering, sorting, and pagination
 */
import { useState, useCallback, useEffect } from 'react'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'
import type { ListFilter, ListSort } from '@utils/listUtils'

export interface UseApiListOptions {
  endpoint: string
  initialPage?: number
  pageSize?: number
  sortKey?: string
  sortDirection?: 'asc' | 'desc'
  autoFetch?: boolean
}

export interface ApiListState<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
  isLoading: boolean
  isRefreshing: boolean
  error: string | null
  filters: ListFilter[]
  sort: ListSort | null
}

export const useApiList = <T extends Record<string, any>>(options: UseApiListOptions) => {
  const {
    endpoint,
    initialPage = 1,
    pageSize = 10,
    sortKey,
    sortDirection = 'asc',
    autoFetch = true,
  } = options

  const [state, setState] = useState<ApiListState<T>>({
    items: [],
    total: 0,
    page: initialPage,
    pageSize,
    totalPages: 0,
    isLoading: false,
    isRefreshing: false,
    error: null,
    filters: [],
    sort: sortKey ? { key: sortKey, direction: sortDirection as 'asc' | 'desc' } : null,
  })

  /**
   * Fetch data from API
   */
  const fetchData = useCallback(
    async (
      pageNumber: number = state.page,
      filters: ListFilter[] = state.filters,
      sort: ListSort | null = state.sort
    ) => {
      setState((prev) => ({ ...prev, isLoading: true, error: null }))

      try {
        // Build query params
        const params = new URLSearchParams()
        params.append('page', String(pageNumber))
        params.append('pageSize', String(state.pageSize))
        // skip/take style backends (e.g. Daycare GetAllChildrenQuery)
        params.append('skip', String((pageNumber - 1) * state.pageSize))
        params.append('take', String(state.pageSize))

        // Add filters
        filters.forEach((filter) => {
          params.append(`filters[${filter.key}]`, String(filter.value))
        })

        // Add sort
        if (sort) {
          params.append('sortBy', sort.key)
          params.append('sortDirection', sort.direction)
        }

        // Fetch from API
        const response = await apiClient.get(`${endpoint}?${params.toString()}`)
        const raw = response.data as any

        // Support multiple response shapes:
        // { data, total, page, pageSize }        (generic, camelCase)
        // { Children, Total, Take, CurrentPage } (children endpoint, PascalCase)
        const payload =
          raw?.data ?? raw?.children ?? raw?.Children ?? raw?.items ?? raw?.Items ?? raw ?? []
        const data: any[] = Array.isArray(payload)
          ? payload
          : (payload?.children ?? payload?.Children ?? payload?.items ?? payload?.Items ?? [])
        const total =
          Number(raw?.total ?? raw?.Total ?? raw?.totalCount ?? raw?.TotalCount ?? data.length) ||
          0
        const currentPage = Number(raw?.page ?? raw?.CurrentPage ?? raw?.currentPage) || pageNumber
        const returnedPageSize =
          Number(raw?.pageSize ?? raw?.PageSize ?? raw?.Take ?? raw?.take) || state.pageSize

        const totalPages = Math.max(1, Math.ceil(total / returnedPageSize))

        setState((prev) => ({
          ...prev,
          items: data,
          total,
          page: currentPage,
          pageSize: returnedPageSize,
          totalPages,
          isLoading: false,
          filters,
          sort,
        }))
      } catch (err) {
        const error = handleApiError(err)
        setState((prev) => ({
          ...prev,
          isLoading: false,
          error: error.message,
        }))
      }
    },
    [endpoint, state.pageSize]
  )

  /**
   * Refresh data (soft refresh with current filters/sort)
   */
  const refresh = useCallback(async () => {
    setState((prev) => ({ ...prev, isRefreshing: true }))
    try {
      await fetchData(state.page, state.filters, state.sort)
    } finally {
      setState((prev) => ({ ...prev, isRefreshing: false }))
    }
  }, [fetchData, state.page, state.filters, state.sort])

  /**
   * Set page and fetch
   */
  const setPage = useCallback(
    async (page: number) => {
      const newPage = Math.max(1, Math.min(page, state.totalPages))
      await fetchData(newPage, state.filters, state.sort)
    },
    [fetchData, state.filters, state.sort, state.totalPages]
  )

  /**
   * Set filters and reset to page 1
   */
  const setFilters = useCallback(
    async (filters: ListFilter[]) => {
      await fetchData(1, filters, state.sort)
    },
    [fetchData, state.sort]
  )

  /**
   * Set sort and keep current page
   */
  const setSort = useCallback(
    async (sort: ListSort | null) => {
      await fetchData(state.page, state.filters, sort)
    },
    [fetchData, state.page, state.filters]
  )

  /**
   * Set page size and reset to page 1
   */
  const setPageSize = useCallback(async (newPageSize: number) => {
    setState((prev) => ({ ...prev, pageSize: newPageSize }))
    await fetchData(1, state.filters, state.sort)
  }, [fetchData, state.filters, state.sort])

  /**
   * Load data on mount
   */
  useEffect(() => {
    if (autoFetch) {
      fetchData()
    }
  }, [autoFetch, endpoint])

  return {
    // State
    ...state,

    // Actions
    fetchData,
    refresh,
    setPage,
    setFilters,
    setSort,
    setPageSize,

    // Helpers
    isEmpty: state.items.length === 0,
    hasError: state.error !== null,
    isSearching: state.isLoading || state.isRefreshing,
  }
}
