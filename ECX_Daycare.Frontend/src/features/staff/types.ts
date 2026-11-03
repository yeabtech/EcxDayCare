/**
 * Staff types and interfaces
 */

export type StaffRole =
  | 'LeadTeacher'
  | 'Teacher'
  | 'Assistant'
  | 'Aide'
  | 'Director'
  | 'Admin'

export interface Staff {
  id: string
  firstName: string
  lastName: string
  role: StaffRole | string
  classroomId?: string | null
  classroomName?: string | null
  phone: string
  email: string
  hireDate: string
  certifications?: string | null
  shiftStart: string
  shiftEnd: string
  createdAt: string
  updatedAt: string
}

export interface StaffFormInput {
  firstName: string
  lastName: string
  role: StaffRole | string
  classroomId?: string | null
  phone: string
  email: string
  hireDate: string
  certifications?: string
  shiftStart: string
  shiftEnd: string
}

export interface StaffListResponse {
  items: Staff[]
  total: number
  page: number
  pageSize: number
}

export interface StaffListParams {
  page?: number
  pageSize?: number
  search?: string
  role?: string
  classroomId?: string
}

export interface AssignClassroomInput {
  classroomId?: string | null
}
