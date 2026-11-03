/**
 * Children API service
 * All API calls for children management go through here
 */
import { apiClient } from '@services/api'
import {
  Child,
  ChildListDto,
  ChildFormInput,
  ChildrenListParams,
  ParentLink,
} from '../types'

const BASE_URL = '/children'
const STORAGE_URL = '/storage'

/**
 * Converts a date-only string "YYYY-MM-DD" to a full ISO datetime "YYYY-MM-DDT00:00:00"
 * required by the backend's DateTime fields.
 */
function toDateTime(dateStr: string): string {
  if (!dateStr) return dateStr
  // Already has UTC or timezone component
  if (dateStr.endsWith('Z')) return dateStr
  if (dateStr.includes('T')) return `${dateStr}Z`
  return `${dateStr}T00:00:00.000Z`
}

/**
 * Strips UI-only fields and ensures DateTime fields are properly formatted
 * before sending to the backend CreateChildDto / UpdateChildDto.
 */
function buildChildPayload(data: ChildFormInput) {
  return {
    FirstName: data.FirstName,
    LastName: data.LastName,
    DateOfBirth: toDateTime(data.DateOfBirth),
    Gender: data.Gender,
    EnrollmentDate: toDateTime(data.EnrollmentDate),
    PhotoUrl: data.PhotoUrl || undefined,
    Allergies: data.Allergies || undefined,
    MedicalNotes: data.MedicalNotes || undefined,
    // Only include Status when the caller provided one, so that an update
    // without the field leaves the existing status untouched on the backend.
    ...(data.status ? { Status: data.status } : {}),
    // Only include Parents when the caller provided them, so that an update
    // without the field leaves existing links untouched on the backend.
    ...(data.Parents !== undefined && {
      Parents: data.Parents.map((p) => ({ ParentId: p.ParentId, Role: p.Role })),
    }),
  }
}

export const childrenApi = {
  /**
   * Get paginated list of children.
   * Backend accepts: skip (0-based offset) and take (page size).
   * Backend returns: ChildListDto { Children[], Total, Skip, Take, ... }
   */
  async getChildren(params?: ChildrenListParams): Promise<ChildListDto> {
    const page = params?.page || 1
    const pageSize = params?.pageSize || 10
    const skip = (page - 1) * pageSize
    const response = await apiClient.get<ChildListDto>(BASE_URL, {
      params: {
        skip,
        take: pageSize,
        ...(params?.search && { search: params.search }),
        ...(params?.status && { status: params.status }),
      },
    })
    return response.data
  },

  /**
   * Get a single child by ID.
   * Backend returns: ChildDto directly.
   */
  async getChild(id: string): Promise<Child> {
    const response = await apiClient.get<Child>(`${BASE_URL}/${id}`)
    return response.data
  },

  /**
   * Create a new child.
   * Strips UI-only fields and ensures DateTime format before posting.
   * Backend returns: ChildDto of the created child.
   */
  async createChild(data: ChildFormInput): Promise<Child> {
    const payload = buildChildPayload(data)
    const response = await apiClient.post<Child>(BASE_URL, payload)
    return response.data
  },

  /**
   * Update an existing child.
   * Strips UI-only fields and ensures DateTime format before putting.
   * Backend returns: ChildDto of the updated child.
   */
  async updateChild(id: string, data: ChildFormInput): Promise<Child> {
    const payload = buildChildPayload(data)
    const response = await apiClient.put<Child>(`${BASE_URL}/${id}`, payload)
    return response.data
  },

  /**
   * Delete a child (returns 204 No Content).
   */
  async deleteChild(id: string): Promise<void> {
    await apiClient.delete(`${BASE_URL}/${id}`)
  },

  /**
   * Get every parent linked to a child, including the stored role.
   * Backend returns: ParentLinkDto[] directly.
   */
  async getChildParents(childId: string): Promise<ParentLink[]> {
    const response = await apiClient.get<ParentLink[]>(`${BASE_URL}/${childId}/parents`)
    const payload = (response.data as any)?.data ?? response.data
    return Array.isArray(payload) ? payload : []
  },

  /**
   * Upload an image (e.g. a child photo) to UploadThing and return its public URL.
   * The backend proxies the upload and wraps the result in the standard ApiResponse envelope.
   */
  async uploadPhoto(file: File): Promise<string> {
    const formData = new FormData()
    formData.append('file', file)
    const response = await apiClient.post<any>(`${STORAGE_URL}/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    const payload = response.data?.data ?? response.data
    return payload?.url ?? payload?.Url ?? ''
  },
}

