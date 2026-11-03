/**
 * List management utilities
 * Common functions for filtering, sorting, and pagination
 */

export interface ListFilter {
  key: string
  value: string | string[] | number | null
  operator?: 'equals' | 'contains' | 'gt' | 'lt' | 'gte' | 'lte' | 'in'
}

export interface ListSort {
  key: string
  direction: 'asc' | 'desc'
}

export interface ListPagination {
  page: number
  pageSize: number
  total: number
  totalPages: number
}

export interface ListState<T> {
  items: T[]
  filters: ListFilter[]
  sort: ListSort | null
  pagination: ListPagination
  isLoading: boolean
  error: string | null
  selectedIds: Set<string>
}

/**
 * Apply filters to array of items
 */
export const filterItems = <T extends Record<string, any>>(
  items: T[],
  filters: ListFilter[]
): T[] => {
  if (!filters.length) return items

  return items.filter((item) => {
    return filters.every((filter) => {
      const value = item[filter.key]
      const operator = filter.operator || 'equals'

      switch (operator) {
        case 'equals':
          return value === filter.value
        case 'contains':
          return String(value).toLowerCase().includes(String(filter.value).toLowerCase())
        case 'in':
          return Array.isArray(filter.value) && filter.value.includes(value)
        case 'gt':
          return typeof value === 'number' && value > (filter.value as number)
        case 'lt':
          return typeof value === 'number' && value < (filter.value as number)
        case 'gte':
          return typeof value === 'number' && value >= (filter.value as number)
        case 'lte':
          return typeof value === 'number' && value <= (filter.value as number)
        default:
          return true
      }
    })
  })
}

/**
 * Sort array of items
 */
export const sortItems = <T extends Record<string, any>>(
  items: T[],
  sort: ListSort | null
): T[] => {
  if (!sort) return items

  const sorted = [...items].sort((a, b) => {
    const aVal = a[sort.key]
    const bVal = b[sort.key]

    // Handle null/undefined
    if (aVal == null && bVal == null) return 0
    if (aVal == null) return sort.direction === 'asc' ? 1 : -1
    if (bVal == null) return sort.direction === 'asc' ? -1 : 1

    // Handle different types
    if (typeof aVal === 'string' && typeof bVal === 'string') {
      const comparison = aVal.localeCompare(bVal)
      return sort.direction === 'asc' ? comparison : -comparison
    }

    if (typeof aVal === 'number' && typeof bVal === 'number') {
      return sort.direction === 'asc' ? aVal - bVal : bVal - aVal
    }

    // Default numeric comparison
    return sort.direction === 'asc' ? (aVal > bVal ? 1 : -1) : aVal > bVal ? -1 : 1
  })

  return sorted
}

/**
 * Paginate array of items
 */
export const paginateItems = <T extends Record<string, any>>(
  items: T[],
  page: number,
  pageSize: number
): { items: T[]; total: number; totalPages: number } => {
  const total = items.length
  const totalPages = Math.ceil(total / pageSize)
  const startIndex = (page - 1) * pageSize
  const endIndex = startIndex + pageSize

  return {
    items: items.slice(startIndex, endIndex),
    total,
    totalPages,
  }
}

/**
 * Apply complete list transformations (filter, sort, paginate)
 */
export const applyListTransformations = <T extends Record<string, any>>(
  items: T[],
  filters: ListFilter[],
  sort: ListSort | null,
  page: number,
  pageSize: number
): {
  items: T[]
  total: number
  totalPages: number
  pagination: ListPagination
} => {
  // Apply filters
  let filtered = filterItems(items, filters)
  const filteredTotal = filtered.length

  // Apply sort
  filtered = sortItems(filtered, sort)

  // Apply pagination
  const { items: paginated, totalPages } = paginateItems(filtered, page, pageSize)

  return {
    items: paginated,
    total: filteredTotal,
    totalPages,
    pagination: {
      page,
      pageSize,
      total: filteredTotal,
      totalPages,
    },
  }
}

/**
 * Search items by multiple fields
 */
export const searchItems = <T extends Record<string, any>>(
  items: T[],
  query: string,
  searchFields: string[]
): T[] => {
  if (!query.trim()) return items

  const lowerQuery = query.toLowerCase()
  return items.filter((item) =>
    searchFields.some((field) =>
      String(item[field]).toLowerCase().includes(lowerQuery)
    )
  )
}

/**
 * Format list filter for API query string
 */
export const formatFilterForAPI = (filters: ListFilter[]): Record<string, any> => {
  return filters.reduce((acc, filter) => {
    acc[filter.key] = filter.value
    return acc
  }, {} as Record<string, any>)
}

/**
 * Format sort for API query string
 */
export const formatSortForAPI = (sort: ListSort | null): string => {
  if (!sort) return ''
  return `${sort.key}:${sort.direction}`
}

/**
 * Parse API error response for field-specific errors
 */
export const parseFieldErrors = (error: any): Record<string, string> => {
  if (error.response?.data?.errors) {
    return error.response.data.errors
  }
  return {}
}
