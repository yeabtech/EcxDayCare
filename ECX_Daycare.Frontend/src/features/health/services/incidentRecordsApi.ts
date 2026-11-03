import { apiClient } from '@services/api'
import type { IncidentRecord } from '../types'

const unwrap = (response: any) => response?.data?.data ?? response?.data?.Data ?? response?.data

const normalizeIncident = (item: any): IncidentRecord => ({
  id: item.Id ?? item.id ?? '',
  childId: item.ChildId ?? item.childId ?? '',
  childName: item.ChildName ?? item.childName ?? null,
  incidentTime: item.IncidentTime ?? item.incidentTime ?? '',
  location: item.Location ?? item.location ?? '',
  description: item.Description ?? item.description ?? '',
  injuryType: Number(item.InjuryType ?? item.injuryType ?? 1),
  injuryTypeName: item.InjuryTypeName ?? item.injuryTypeName ?? '',
  affectedBodyPart: Number(item.AffectedBodyPart ?? item.affectedBodyPart ?? 18),
  affectedBodyPartName: item.AffectedBodyPartName ?? item.affectedBodyPartName ?? '',
  firstAidGiven: item.FirstAidGiven ?? item.firstAidGiven ?? null,
  staffId: item.StaffId ?? item.staffId ?? '',
  parentNotified: item.ParentNotified ?? item.parentNotified ?? false,
  parentSignature: item.ParentSignature ?? item.parentSignature ?? null,
  followUp: item.FollowUp ?? item.followUp ?? null,
  notes: item.Notes ?? item.notes ?? null,
  createdAt: item.CreatedAt ?? item.createdAt ?? '',
  updatedAt: item.UpdatedAt ?? item.updatedAt ?? null,
})

export const incidentRecordsApi = {
  async getByChild(childId: string): Promise<IncidentRecord[]> {
    const response = await apiClient.get<any>(`/incidents/child/${childId}`)
    const rows = unwrap(response)
    return (Array.isArray(rows) ? rows : []).map(normalizeIncident)
  },

  async create(data: Record<string, unknown>): Promise<string> {
    const response = await apiClient.post<any>('/incidents', data)
    return response?.data?.message ?? response?.data?.Message ?? 'Incident report saved.'
  },

  async update(id: string, data: Record<string, unknown>): Promise<IncidentRecord> {
    const response = await apiClient.put<any>(`/incidents/${id}`, data)
    return normalizeIncident(unwrap(response))
  },

  async markParentNotified(id: string): Promise<void> {
    await apiClient.post(`/incidents/${id}/notify-parent`)
  },
}
