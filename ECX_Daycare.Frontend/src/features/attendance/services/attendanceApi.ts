/**
 * Attendance API service — thin wrapper over the REST API
 */
import { apiClient } from '@services/api'
import type {
  AttendanceRecord,
  CheckInRequest,
  CheckOutRequest,
  MarkAbsentRequest,
  DailyAttendanceSummary,
  ChildAttendanceHistory,
  DailyAttendanceParams,
  AttendanceHistoryParams,
} from '../types'

const BASE = '/attendance'

function normalizeRecord(r: any): AttendanceRecord {
  if (!r) return r
  return {
    id: r.Id ?? r.id ?? '',
    childId: r.ChildId ?? r.childId ?? '',
    childName: r.ChildName ?? r.childName ?? null,
    classroomId: r.ClassroomId ?? r.classroomId ?? null,
    classroomName: r.ClassroomName ?? r.classroomName ?? null,
    date: r.Date ?? r.date ?? '',
    checkInTime: r.CheckInTime ?? r.checkInTime ?? null,
    checkOutTime: r.CheckOutTime ?? r.checkOutTime ?? null,
    status: r.Status ?? r.status ?? 'Absent',
    droppedOffBy: r.DroppedOffBy ?? r.droppedOffBy ?? null,
    pickedUpBy: r.PickedUpBy ?? r.pickedUpBy ?? null,
    notes: r.Notes ?? r.notes ?? null,
    staffId: r.StaffId ?? r.staffId ?? null,
    checkOutStaffId: r.CheckOutStaffId ?? r.checkOutStaffId ?? null,
    createdAt: r.CreatedAt ?? r.createdAt ?? '',
    updatedAt: r.UpdatedAt ?? r.updatedAt ?? '',
  }
}

function normalizeSummary(d: any): DailyAttendanceSummary {
  return {
    date: d.Date ?? d.date ?? '',
    totalEnrolled: d.TotalEnrolled ?? d.totalEnrolled ?? 0,
    totalPresent: d.TotalPresent ?? d.totalPresent ?? 0,
    totalLate: d.TotalLate ?? d.totalLate ?? 0,
    totalAbsent: d.TotalAbsent ?? d.totalAbsent ?? 0,
    totalEarlyPickup: d.TotalEarlyPickup ?? d.totalEarlyPickup ?? 0,
    totalCurrentlyCheckedIn: d.TotalCurrentlyCheckedIn ?? d.totalCurrentlyCheckedIn ?? 0,
    items: (d.Items ?? d.items ?? []).map(normalizeRecord),
  }
}

function normalizeHistory(d: any): ChildAttendanceHistory {
  return {
    childId: d.ChildId ?? d.childId ?? '',
    childName: d.ChildName ?? d.childName ?? null,
    totalDays: d.TotalDays ?? d.totalDays ?? 0,
    presentDays: d.PresentDays ?? d.presentDays ?? 0,
    lateDays: d.LateDays ?? d.lateDays ?? 0,
    absentDays: d.AbsentDays ?? d.absentDays ?? 0,
    earlyPickupDays: d.EarlyPickupDays ?? d.earlyPickupDays ?? 0,
    attendanceRate: d.AttendanceRate ?? d.attendanceRate ?? 0,
    items: (d.Items ?? d.items ?? []).map(normalizeRecord),
    page: d.Page ?? d.page ?? 1,
    pageSize: d.PageSize ?? d.pageSize ?? 50,
    totalCount: d.TotalCount ?? d.totalCount ?? 0,
  }
}

export const attendanceApi = {
  /** GET /api/attendance/today */
  async getTodayAttendance(params?: DailyAttendanceParams): Promise<DailyAttendanceSummary> {
    const query: Record<string, any> = {}
    if (params?.date) query.date = params.date
    if (params?.classroomId) query.classroomId = params.classroomId
    if (params?.status) query.status = params.status
    const res = await apiClient.get<any>(`${BASE}/today`, { params: query })
    const payload = res.data?.data ?? res.data
    return normalizeSummary(payload)
  },

  /** GET /api/attendance/child/{childId}/history */
  async getChildHistory(
    childId: string,
    params?: AttendanceHistoryParams,
  ): Promise<ChildAttendanceHistory> {
    const query: Record<string, any> = {
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 50,
    }
    if (params?.startDate) query.startDate = params.startDate
    if (params?.endDate) query.endDate = params.endDate
    const res = await apiClient.get<any>(`${BASE}/child/${childId}/history`, { params: query })
    const payload = res.data?.data ?? res.data
    return normalizeHistory(payload)
  },

  /** POST /api/attendance/checkin */
  async checkIn(data: CheckInRequest): Promise<AttendanceRecord> {
    const res = await apiClient.post<any>(`${BASE}/checkin`, {
      childId: data.childId,
      checkInTime: data.checkInTime ?? null,
      classroomId: data.classroomId ?? null,
      droppedOffBy: data.droppedOffBy ?? null,
      notes: data.notes ?? null,
      status: data.status ?? null,
    })
    const payload = res.data?.data ?? res.data
    return normalizeRecord(payload)
  },

  /** POST /api/attendance/checkout */
  async checkOut(data: CheckOutRequest): Promise<AttendanceRecord> {
    const res = await apiClient.post<any>(`${BASE}/checkout`, {
      childId: data.childId ?? null,
      checkOutTime: data.checkOutTime ?? null,
      pickedUpBy: data.pickedUpBy ?? null,
      notes: data.notes ?? null,
      isEarlyPickup: data.isEarlyPickup ?? null,
    })
    const payload = res.data?.data ?? res.data
    return normalizeRecord(payload)
  },

  /** POST /api/attendance/{id}/checkout */
  async checkOutById(attendanceId: string, data: CheckOutRequest): Promise<AttendanceRecord> {
    const res = await apiClient.post<any>(`${BASE}/${attendanceId}/checkout`, {
      childId: data.childId ?? null,
      checkOutTime: data.checkOutTime ?? null,
      pickedUpBy: data.pickedUpBy ?? null,
      notes: data.notes ?? null,
      isEarlyPickup: data.isEarlyPickup ?? null,
    })
    const payload = res.data?.data ?? res.data
    return normalizeRecord(payload)
  },

  /** POST /api/attendance/absent */
  async markAbsent(data: MarkAbsentRequest): Promise<AttendanceRecord> {
    const res = await apiClient.post<any>(`${BASE}/absent`, {
      childId: data.childId,
      date: data.date ?? null,
      notes: data.notes ?? null,
    })
    const payload = res.data?.data ?? res.data
    return normalizeRecord(payload)
  },
}
