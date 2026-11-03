/**
 * Child domain types and interfaces
 * Aligned with backend ChildDto / ChildListDto (PascalCase — PropertyNamingPolicy = null)
 */

/** Parent role stored on the ParentChildren join table */
export type ParentRole = 'Guardian' | 'Mother' | 'Father'

/**
 * A parent assigned to a child, sent with create/update requests.
 * Mirrors the backend ParentAssignmentDto.
 */
export interface ParentAssignment {
  ParentId: string
  Role: ParentRole
}

/**
 * A parent link read back from the backend (ParentLinkDto) — includes the
 * parent's display details plus the role held on that link.
 */
export interface ParentLink {
  ParentId: string
  FirstName: string
  LastName: string
  Email: string
  PhoneNumber: string
  Role: string
}

/** Single child as returned by the backend ChildDto */
export interface Child {
  Id: string
  FirstName: string
  LastName: string
  DateOfBirth: string // ISO date
  Age: number
  Gender: 'Male' | 'Female' | 'Other'
  PhotoUrl?: string
  EnrollmentDate: string // ISO date
  Allergies?: string
  MedicalNotes?: string
  Status: string
  CreatedAt: string
  UpdatedAt: string
  // UI-only / client-side fields
  notes?: string
}

/** Backend ChildListDto shape */
export interface ChildListDto {
  Children: Child[]
  Total: number
  Skip: number
  Take: number
  TotalPages: number
  CurrentPage: number
  HasNextPage: boolean
}

/**
 * Form input for creating/editing a child.
 * Note: parentName/Email/Phone/Address/Notes are UI-only fields
 * stored locally and not sent to the backend child endpoint.
 */
export interface ChildFormInput {
  FirstName: string
  LastName: string
  DateOfBirth: string
  Gender: 'Male' | 'Female' | 'Other'
  EnrollmentDate: string
  PhotoUrl?: string
  Allergies?: string
  MedicalNotes?: string
  /** Parents to link to this child (written to the ParentChildren table). */
  Parents?: ParentAssignment[]
  status?: string
}

export interface ChildrenListParams {
  page?: number
  pageSize?: number
  search?: string
  status?: string
}

// Backward-compatible type aliases
export type ChildResponse = Child
export type ChildrenResponse = ChildListDto
export type ChildrenListResponse = ChildListDto
