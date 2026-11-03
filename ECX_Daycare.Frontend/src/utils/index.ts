/**
 * API response formatting utility
 */
import { ApiError } from '@/types/api'

export const handleApiError = (error: unknown): ApiError => {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const err = error as any
    const data = err.response?.data
    const message =
      data?.details ||
      data?.message ||
      data?.error ||
      err.message ||
      'An error occurred'
    return {
      status: err.response?.status || 500,
      message,
      code: data?.code || 'API_ERROR',
    }
  }

  if (error instanceof Error) {
    return {
      status: 500,
      message: error.message,
      code: 'UNKNOWN_ERROR',
    }
  }

  return {
    status: 500,
    message: 'An unknown error occurred',
    code: 'UNKNOWN_ERROR',
  }
}

export const formatFullName = (firstName?: string, lastName?: string): string => {
  return [firstName, lastName].filter(Boolean).join(' ') || 'Unknown'
}

export const formatDate = (date: string | Date): string => {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

export const formatTime = (date: string | Date): string => {
  return new Date(date).toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
  })
}

export * from './listUtils'
export * from './exportData'
export * from './cache'
export * from './performance'
