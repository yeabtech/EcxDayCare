/**
 * Classrooms API service
 */
import { apiClient } from '@services/api'
import {
  Classroom,
  ClassroomFormInput,
  ClassroomListParams,
  ClassroomListResponse,
} from '../types'
import type { Child } from '@features/children/types'

const BASE_URL = '/classrooms'

function normalizeClassroom(c: any): Classroom {
  if (!c) return c
  return {
    id: c.Id ?? c.id ?? '',
    name: c.Name ?? c.name ?? '',
    minAgeMonths: c.MinAgeMonths ?? c.minAgeMonths ?? 0,
    maxAgeMonths: c.MaxAgeMonths ?? c.maxAgeMonths ?? 0,
    maxCapacity: c.MaxCapacity ?? c.maxCapacity ?? 0,
    currentEnrollment: c.CurrentEnrollment ?? c.currentEnrollment ?? 0,
    availableSpots: c.AvailableSpots ?? c.availableSpots ?? 0,
    status: c.Status ?? c.status ?? 'Active',
    leadTeacherId: c.LeadTeacherId ?? c.leadTeacherId,
    description: c.Description ?? c.description,
    createdAt: c.CreatedAt ?? c.createdAt ?? '',
    updatedAt: c.UpdatedAt ?? c.updatedAt ?? '',
  }
}

function normalizeClassroomList(data: any): ClassroomListResponse {
  if (!data) {
    return { items: [], total: 0, page: 1, pageSize: 10 }
  }
  const rawItems = data.Items ?? data.items ?? (Array.isArray(data) ? data : [])
  return {
    items: Array.isArray(rawItems) ? rawItems.map(normalizeClassroom) : [],
    total: data.Total ?? data.total ?? (Array.isArray(rawItems) ? rawItems.length : 0),
    page: data.Page ?? data.page ?? 1,
    pageSize: data.PageSize ?? data.pageSize ?? 10,
  }
}

export const classroomsApi = {
  async getClassrooms(params?: ClassroomListParams): Promise<ClassroomListResponse> {
    const response = await apiClient.get<any>(BASE_URL, {
      params: {
        page: params?.page || 1,
        pageSize: params?.pageSize || 10,
        ...(params?.search && { search: params.search }),
        ...(params?.status && { status: params.status }),
      },
    })
    const payload = response.data?.data ?? response.data
    return normalizeClassroomList(payload)
  },

  async getClassroom(id: string): Promise<Classroom> {
    const response = await apiClient.get<any>(`${BASE_URL}/${id}`)
    const payload = response.data?.data ?? response.data
    return normalizeClassroom(payload)
  },

  async getClassroomChildren(id: string): Promise<Child[]> {
    const response = await apiClient.get<any>(`${BASE_URL}/${id}/children`)
    const payload = response.data?.data ?? response.data?.Data ?? response.data
    return Array.isArray(payload) ? payload : []
  },

  async createClassroom(data: ClassroomFormInput): Promise<Classroom> {
    const payload = {
      ...data,
      leadTeacherId: data.leadTeacherId && data.leadTeacherId.trim() ? data.leadTeacherId.trim() : null,
      description: data.description && data.description.trim() ? data.description.trim() : null,
    }
    const response = await apiClient.post<any>(BASE_URL, payload)
    const payloadRes = response.data?.data ?? response.data
    return normalizeClassroom(payloadRes)
  },

  async updateClassroom(id: string, data: ClassroomFormInput): Promise<void> {
    const payload = {
      ...data,
      leadTeacherId: data.leadTeacherId && data.leadTeacherId.trim() ? data.leadTeacherId.trim() : null,
      description: data.description && data.description.trim() ? data.description.trim() : null,
    }
    await apiClient.put(`${BASE_URL}/${id}`, payload)
  },

  async deleteClassroom(id: string): Promise<void> {
    await apiClient.delete(`${BASE_URL}/${id}`)
  },

  async enrollChild(classroomId: string, childId: string): Promise<void> {
    await apiClient.post(`${BASE_URL}/${classroomId}/children/${childId}`, {})
  },

  async removeChild(classroomId: string, childId: string): Promise<void> {
    await apiClient.delete(`${BASE_URL}/${classroomId}/children/${childId}`)
  },
}

