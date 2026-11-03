import { apiClient } from '@services/api'
import type { ChildOption, HealthLog } from '../types'

const unwrap = (response: any) => response?.data?.data ?? response?.data?.Data ?? response?.data

const normalizeHealthLog = (item: any): HealthLog => ({
  id: item.Id ?? item.id ?? '',
  childId: item.ChildId ?? item.childId ?? '',
  childName: item.ChildName ?? item.childName ?? null,
  time: item.Time ?? item.time ?? '',
  eventType: Number(item.EventType ?? item.eventType ?? 1),
  eventTypeName: item.EventTypeName ?? item.eventTypeName ?? '',
  description: item.Description ?? item.description ?? null,
  staffId: item.StaffId ?? item.staffId ?? '',
  notes: item.Notes ?? item.notes ?? null,
  createdAt: item.CreatedAt ?? item.createdAt ?? '',
  updatedAt: item.UpdatedAt ?? item.updatedAt ?? null,
})

export const healthRecordsApi = {
  async getChildren(parentView: boolean, parentId?: string | null): Promise<ChildOption[]> {
    if (parentView) {
      if (!parentId) throw new Error('Your parent profile is not linked to an account.')
      const response = await apiClient.get<any>('/healthlogs/children')
      const rows = unwrap(response)
      return (Array.isArray(rows) ? rows : []).map((child: any) => ({
        id: child.Id ?? child.id ?? '',
        name: child.FullName ?? child.fullName ?? `${child.FirstName ?? child.firstName ?? ''} ${child.LastName ?? child.lastName ?? ''}`.trim(),
      })).filter((child: ChildOption) => child.id && child.name)
    }

    const results: ChildOption[] = []
    let skip = 0
    let total = Number.POSITIVE_INFINITY
    while (skip < total) {
      const response = await apiClient.get<any>('/children', { params: { skip, take: 100 } })
      const payload = unwrap(response)
      const rows = Array.isArray(payload) ? payload : payload?.Children ?? payload?.children ?? []
      const page = rows.map((child: any) => ({
        id: child.Id ?? child.id ?? '',
        name: `${child.FirstName ?? child.firstName ?? ''} ${child.LastName ?? child.lastName ?? ''}`.trim(),
      })).filter((child: ChildOption) => child.id && child.name)
      results.push(...page)
      total = Number(payload?.Total ?? payload?.total ?? page.length)
      if (!page.length) break
      skip += page.length
    }
    return results
  },

  async getByChild(childId: string): Promise<HealthLog[]> {
    const response = await apiClient.get<any>(`/healthlogs/child/${childId}`)
    const rows = unwrap(response)
    return (Array.isArray(rows) ? rows : []).map(normalizeHealthLog)
  },

  async create(data: Record<string, unknown>): Promise<HealthLog> {
    const response = await apiClient.post<any>('/healthlogs', data)
    return normalizeHealthLog(unwrap(response))
  },

  async update(id: string, data: Record<string, unknown>): Promise<HealthLog> {
    const response = await apiClient.put<any>(`/healthlogs/${id}`, data)
    return normalizeHealthLog(unwrap(response))
  },
}
