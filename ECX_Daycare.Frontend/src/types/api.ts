/**
 * Type definitions for API responses
 */

export interface ApiResponse<T = unknown> {
  data: T
  success: boolean
  message?: string
}

export interface PaginatedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface ApiError {
  status: number
  message: string
  code?: string
}
