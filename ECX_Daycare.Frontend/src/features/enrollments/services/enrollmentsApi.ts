/**
 * Enrollment API service
 */
import { apiClient } from '@services/api'
import {
  Enrollment,
  EnrollmentFormInput,
  EnrollmentListParams,
  EnrollmentListResponse,
  EndEnrollmentInput,
} from '../types'

const BASE_URL = '/enrollments'

function normalizeEnrollment(e: any): Enrollment {
  if (!e) return e
  return {
    id: e.Id ?? e.id ?? '',
    childId: e.ChildId ?? e.childId ?? '',
    childName: e.ChildName ?? e.childName ?? null,
    classroomId: e.ClassroomId ?? e.classroomId ?? null,
    classroomName: e.ClassroomName ?? e.classroomName ?? null,
    programType: e.ProgramType ?? e.programType ?? 'FullTime',
    daysOfWeek: e.DaysOfWeek ?? e.daysOfWeek ?? '',
    startDate: e.StartDate ?? e.startDate ?? '',
    endDate: e.EndDate ?? e.endDate ?? null,
    contractSigned: e.ContractSigned ?? e.contractSigned ?? false,
    immunizationRecord: e.ImmunizationRecord ?? e.immunizationRecord ?? false,
    birthCertificate: e.BirthCertificate ?? e.birthCertificate ?? false,
    medicalForm: e.MedicalForm ?? e.medicalForm ?? false,
    status: e.Status ?? e.status ?? 'Pending',
    endReason: e.EndReason ?? e.endReason ?? null,
    createdAt: e.CreatedAt ?? e.createdAt ?? '',
    updatedAt: e.UpdatedAt ?? e.updatedAt ?? '',
  }
}

function normalizeList(data: any): EnrollmentListResponse {
  if (!data) return { items: [], total: 0, page: 1, pageSize: 10 }
  const rawItems = data.Items ?? data.items ?? (Array.isArray(data) ? data : [])
  return {
    items: Array.isArray(rawItems) ? rawItems.map(normalizeEnrollment) : [],
    total: data.TotalCount ?? data.totalCount ?? data.Total ?? data.total ?? rawItems.length,
    page: data.Page ?? data.page ?? 1,
    pageSize: data.PageSize ?? data.pageSize ?? 10,
  }
}

export const enrollmentsApi = {
  async getEnrollments(params?: EnrollmentListParams): Promise<EnrollmentListResponse> {
    const queryParams: Record<string, any> = {
      page: params?.page || 1,
      pageSize: params?.pageSize || 10,
    }
    if (params?.status) queryParams.status = params.status
    if (params?.programType) queryParams.programType = params.programType
    if (params?.classroomId) queryParams.classroomId = params.classroomId

    const response = await apiClient.get<any>(BASE_URL, { params: queryParams })
    const payload = response.data?.data ?? response.data
    return normalizeList(payload)
  },

  async getEnrollment(id: string): Promise<Enrollment> {
    const response = await apiClient.get<any>(`${BASE_URL}/${id}`)
    const payload = response.data?.data ?? response.data
    return normalizeEnrollment(payload)
  },

  async getChildHistory(childId: string): Promise<Enrollment[]> {
    const response = await apiClient.get<any>(`${BASE_URL}/child/${childId}/history`)
    const payload = response.data?.data ?? response.data
    const rawItems = Array.isArray(payload) ? payload : (payload?.items ?? [])
    return rawItems.map(normalizeEnrollment)
  },

  async getChildActiveEnrollment(childId: string): Promise<Enrollment | null> {
    try {
      const response = await apiClient.get<any>(`${BASE_URL}/child/${childId}/active`)
      const payload = response.data?.data ?? response.data
      return normalizeEnrollment(payload)
    } catch {
      return null
    }
  },

  async createEnrollment(data: EnrollmentFormInput): Promise<Enrollment> {
    const payload = {
      childId: data.childId,
      programType: data.programType,
      daysOfWeek: data.daysOfWeek,
      startDate: data.startDate,
      endDate: data.endDate || null,
      contractSigned: data.contractSigned,
      immunizationRecord: data.immunizationRecord,
      birthCertificate: data.birthCertificate,
      medicalForm: data.medicalForm,
      classroomId: data.classroomId || null,
    }
    const response = await apiClient.post<any>(BASE_URL, payload)
    const resPayload = response.data?.data ?? response.data
    return normalizeEnrollment(resPayload)
  },

  async updateEnrollment(id: string, data: EnrollmentFormInput): Promise<Enrollment> {
    const payload = {
      programType: data.programType,
      daysOfWeek: data.daysOfWeek,
      startDate: data.startDate,
      endDate: data.endDate || null,
      contractSigned: data.contractSigned,
      immunizationRecord: data.immunizationRecord,
      birthCertificate: data.birthCertificate,
      medicalForm: data.medicalForm,
      classroomId: data.classroomId || null,
      status: data.status,
      endReason: data.endReason || null,
    }
    const response = await apiClient.put<any>(`${BASE_URL}/${id}`, payload)
    const resPayload = response.data?.data ?? response.data
    return normalizeEnrollment(resPayload)
  },

  async endEnrollment(id: string, data: EndEnrollmentInput): Promise<Enrollment> {
    const response = await apiClient.post<any>(`${BASE_URL}/${id}/end`, {
      endDate: data.endDate,
      reason: data.reason || null,
    })
    const resPayload = response.data?.data ?? response.data
    return normalizeEnrollment(resPayload)
  },
}
