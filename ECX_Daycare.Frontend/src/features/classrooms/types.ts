/**
 * Classroom types and interfaces
 */

export interface Classroom {
  id: string
  name: string
  minAgeMonths: number
  maxAgeMonths: number
  maxCapacity: number
  currentEnrollment: number
  availableSpots: number
  status: 'Active' | 'Inactive'
  leadTeacherId?: string
  description?: string
  createdAt: string
  updatedAt: string
}

export interface ClassroomFormInput {
  name: string
  minAgeMonths: number
  maxAgeMonths: number
  maxCapacity: number
  status?: 'Active' | 'Inactive'
  leadTeacherId?: string
  description?: string
}

export interface ClassroomListResponse {
  items: Classroom[]
  total: number
  page: number
  pageSize: number
}

export interface ClassroomListParams {
  page?: number
  pageSize?: number
  search?: string
  status?: string
}
