/**
 * Enrollment types and interfaces
 */

export type ProgramType =
  | 'FullTime'
  | 'PartTime'
  | 'HalfDay'
  | 'Occasional'

export type EnrollmentStatus =
  | 'Active'
  | 'Pending'
  | 'Ended'
  | 'Cancelled'

export interface Enrollment {
  id: string
  childId: string
  childName?: string | null
  classroomId?: string | null
  classroomName?: string | null
  programType: ProgramType | string
  daysOfWeek: string
  startDate: string
  endDate?: string | null
  contractSigned: boolean
  immunizationRecord: boolean
  birthCertificate: boolean
  medicalForm: boolean
  status: EnrollmentStatus | string
  endReason?: string | null
  createdAt: string
  updatedAt: string
}

export interface EnrollmentFormInput {
  childId: string
  classroomId?: string | null
  programType: ProgramType | string
  daysOfWeek: string
  startDate: string
  endDate?: string | null
  contractSigned: boolean
  immunizationRecord: boolean
  birthCertificate: boolean
  medicalForm: boolean
  status?: EnrollmentStatus | string
  endReason?: string | null
}

export interface EndEnrollmentInput {
  endDate: string
  reason?: string | null
}

export interface EnrollmentListResponse {
  items: Enrollment[]
  total: number
  page: number
  pageSize: number
}

export interface EnrollmentListParams {
  page?: number
  pageSize?: number
  status?: string
  programType?: string
  classroomId?: string
}
