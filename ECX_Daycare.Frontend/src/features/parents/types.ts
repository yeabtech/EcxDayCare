/**
 * Parent types and interfaces
 */

export interface Parent {
  id: string
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  address: string
  status: 'Active' | 'Inactive'
  notes?: string
  createdAt: string
  updatedAt: string
}

export interface ParentFormInput {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  address: string
  status: 'Active' | 'Inactive'
  notes?: string
}

export interface ParentListResponse {
  items: Parent[]
  total: number
  page: number
  pageSize: number
}

export interface ParentListParams {
  page?: number
  pageSize?: number
  search?: string
  status?: string
}
