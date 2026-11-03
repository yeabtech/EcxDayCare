/**
 * Parents API service
 */
import { apiClient } from '@services/api'
import {
  Parent,
  ParentFormInput,
  ParentListParams,
  ParentListResponse,
} from '../types'

const BASE_URL = '/parents'

function normalizeParent(p: any): Parent {
  if (!p) return p
  return {
    id: p.Id ?? p.id ?? '',
    firstName: p.FirstName ?? p.firstName ?? '',
    lastName: p.LastName ?? p.lastName ?? '',
    email: p.Email ?? p.email ?? '',
    phoneNumber: p.PhoneNumber ?? p.phoneNumber ?? '',
    address: p.Address ?? p.address ?? '',
    status: p.Status ?? p.status ?? 'Active',
    notes: p.Notes ?? p.notes ?? '',
    createdAt: p.CreatedAt ?? p.createdAt ?? '',
    updatedAt: p.UpdatedAt ?? p.updatedAt ?? '',
  }
}

function normalizeParentList(data: any): ParentListResponse {
  if (!data) {
    return { items: [], total: 0, page: 1, pageSize: 10 }
  }
  const rawItems = data.Items ?? data.items ?? (Array.isArray(data) ? data : [])
  return {
    items: Array.isArray(rawItems) ? rawItems.map(normalizeParent) : [],
    total: data.Total ?? data.total ?? (Array.isArray(rawItems) ? rawItems.length : 0),
    page: data.Page ?? data.page ?? 1,
    pageSize: data.PageSize ?? data.pageSize ?? 10,
  }
}

export const parentsApi = {
  async getParents(params?: ParentListParams): Promise<ParentListResponse> {
    const isSearch = Boolean(params?.search && params.search.trim().length > 0)
    const url = isSearch ? `${BASE_URL}/search` : BASE_URL
    const queryParams = isSearch
      ? {
          q: params!.search!.trim(),
          page: params?.page || 1,
          pageSize: params?.pageSize || 10,
        }
      : {
          page: params?.page || 1,
          pageSize: params?.pageSize || 10,
        }

    const response = await apiClient.get<any>(url, { params: queryParams })
    const payload = response.data?.data ?? response.data
    return normalizeParentList(payload)
  },

  async getParent(id: string): Promise<Parent> {
    const response = await apiClient.get<any>(`${BASE_URL}/${id}`)
    const payload = response.data?.data ?? response.data
    return normalizeParent(payload)
  },

  async createParent(data: ParentFormInput): Promise<Parent> {
    const response = await apiClient.post<any>(BASE_URL, data)
    const payload = response.data?.data ?? response.data
    return normalizeParent(payload)
  },

  async updateParent(id: string, data: ParentFormInput): Promise<void> {
    await apiClient.put(`${BASE_URL}/${id}`, data)
  },

  async deleteParent(id: string): Promise<void> {
    await apiClient.delete(`${BASE_URL}/${id}`)
  },

  async linkChild(parentId: string, childId: string): Promise<void> {
    await apiClient.post(`${BASE_URL}/${parentId}/children/${childId}`, {})
  },

  async unlinkChild(parentId: string, childId: string): Promise<void> {
    await apiClient.delete(`${BASE_URL}/${parentId}/children/${childId}`)
  },
}

