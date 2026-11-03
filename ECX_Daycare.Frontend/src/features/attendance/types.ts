/**
 * Attendance feature types
 */

export type AttendanceStatus = 'Present' | 'Late' | 'Absent' | 'EarlyPickup'

export interface AttendanceRecord {
  id: string
  childId: string
  childName?: string | null
  classroomId?: string | null
  classroomName?: string | null
  date: string
  checkInTime?: string | null
  checkOutTime?: string | null
  status: AttendanceStatus | string
  droppedOffBy?: string | null
  pickedUpBy?: string | null
  notes?: string | null
  staffId?: string | null
  checkOutStaffId?: string | null
  createdAt: string
  updatedAt: string
}

export interface DailyAttendanceSummary {
  date: string
  totalEnrolled: number
  totalPresent: number
  totalLate: number
  totalAbsent: number
  totalEarlyPickup: number
  totalCurrentlyCheckedIn: number
  items: AttendanceRecord[]
}

export interface ChildAttendanceHistory {
  childId: string
  childName?: string | null
  totalDays: number
  presentDays: number
  lateDays: number
  absentDays: number
  earlyPickupDays: number
  attendanceRate: number
  items: AttendanceRecord[]
  page: number
  pageSize: number
  totalCount: number
}

export interface CheckInRequest {
  childId: string
  checkInTime?: string | null
  classroomId?: string | null
  droppedOffBy?: string | null
  notes?: string | null
  status?: string | null
}

export interface CheckOutRequest {
  childId?: string | null
  checkOutTime?: string | null
  pickedUpBy?: string | null
  notes?: string | null
  isEarlyPickup?: boolean | null
}

export interface MarkAbsentRequest {
  childId: string
  date?: string | null
  notes?: string | null
}

export interface DailyAttendanceParams {
  date?: string
  classroomId?: string
  status?: string
}

export interface AttendanceHistoryParams {
  startDate?: string
  endDate?: string
  page?: number
  pageSize?: number
}
