/**
 * Auth types matching the backend DTOs
 */

export interface CurrentUser {
  Id: string
  Email: string
  FirstName: string
  LastName: string
  Role: 'Admin' | 'Staff' | 'Parent'
  StaffId: string | null
  ParentId: string | null
}

export interface AuthResponse {
  AccessToken: string
  RefreshToken: string
  ExpiresIn: number
  User: CurrentUser
}

export interface LoginRequest {
  Email: string
  Password: string
}

export interface RegisterRequest {
  Email: string
  Password: string
  FirstName: string
  LastName: string
  Role?: 'Admin' | 'Staff' | 'Parent'
  StaffId?: string | null
  ParentId?: string | null
}

export interface RefreshTokenRequest {
  RefreshToken: string
}
