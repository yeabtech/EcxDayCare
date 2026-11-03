/**
 * Staff API service
 */
import { apiClient } from '@services/api'
import {
  Staff,
  StaffFormInput,
  StaffListParams,
  StaffListResponse,
} from '../types'

const BASE_URL = '/staff'

function normalizeStaff(s: any): Staff {
  if (!s) return s
  return {
    id: s.Id ?? s.id ?? '',
    firstName: s.FirstName ?? s.firstName ?? '',
    lastName: s.LastName ?? s.lastName ?? '',
    role: s.Role ?? s.role ?? 'Teacher',
    classroomId: s.ClassroomId ?? s.classroomId ?? null,
    classroomName: s.ClassroomName ?? s.classroomName ?? null,
    phone: s.Phone ?? s.phone ?? '',
    email: s.Email ?? s.email ?? '',
    hireDate: s.HireDate ?? s.hireDate ?? '',
    certifications: s.Certifications ?? s.certifications ?? null,
    shiftStart: s.ShiftStart ?? s.shiftStart ?? '08:00',
    shiftEnd: s.ShiftEnd ?? s.shiftEnd ?? '17:00',
    createdAt: s.CreatedAt ?? s.createdAt ?? '',
    updatedAt: s.UpdatedAt ?? s.updatedAt ?? '',
  }
}

function normalizeStaffList(data: any): StaffListResponse {
  if (!data) {
    return { items: [], total: 0, page: 1, pageSize: 10 }
  }
  const rawItems = data.Items ?? data.items ?? (Array.isArray(data) ? data : [])
  return {
    items: Array.isArray(rawItems) ? rawItems.map(normalizeStaff) : [],
    total: data.Total ?? data.total ?? (Array.isArray(rawItems) ? rawItems.length : 0),
    page: data.Page ?? data.page ?? 1,
    pageSize: data.PageSize ?? data.pageSize ?? 10,
  }
}

export const staffApi = {
  async getStaff(params?: StaffListParams): Promise<StaffListResponse> {
    const isSearch = Boolean(params?.search && params.search.trim().length > 0)
    const url = isSearch ? `${BASE_URL}/search` : BASE_URL
    const queryParams: Record<string, any> = {
      page: params?.page || 1,
      pageSize: params?.pageSize || 10,
    }

    if (isSearch) {
      queryParams.q = params!.search!.trim()
    } else {
      if (params?.role) queryParams.role = params.role
      if (params?.classroomId) queryParams.classroomId = params.classroomId
    }

    const response = await apiClient.get<any>(url, { params: queryParams })
    const payload = response.data?.data ?? response.data
    return normalizeStaffList(payload)
  },

  async getStaffMember(id: string): Promise<Staff> {
    const response = await apiClient.get<any>(`${BASE_URL}/${id}`)
    const payload = response.data?.data ?? response.data
    return normalizeStaff(payload)
  },

  async getStaffByClassroom(classroomId: string): Promise<Staff[]> {
    const response = await apiClient.get<any>(`${BASE_URL}/classroom/${classroomId}`)
    const payload = response.data?.data ?? response.data
    const rawItems = Array.isArray(payload) ? payload : (payload?.items ?? [])
    return rawItems.map(normalizeStaff)
  },

  async createStaff(data: StaffFormInput): Promise<Staff> {
    const payload = {
      ...data,
      classroomId: data.classroomId ? data.classroomId : null,
      certifications: data.certifications ? data.certifications.trim() : null,
    }
    const response = await apiClient.post<any>(BASE_URL, payload)
    const resPayload = response.data?.data ?? response.data
    return normalizeStaff(resPayload)
  },

  async updateStaff(id: string, data: StaffFormInput): Promise<void> {
    const payload = {
      ...data,
      classroomId: data.classroomId ? data.classroomId : null,
      certifications: data.certifications ? data.certifications.trim() : null,
    }
    await apiClient.put(`${BASE_URL}/${id}`, payload)
  },

  async deleteStaff(id: string): Promise<void> {
    await apiClient.delete(`${BASE_URL}/${id}`)
  },

  async assignClassroom(id: string, classroomId: string | null): Promise<void> {
    await apiClient.put(`${BASE_URL}/${id}/assign-classroom`, {
      classroomId: classroomId || null,
    })
  },
}
