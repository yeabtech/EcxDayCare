/**
 * Auth API service — login, register, refresh, logout, current user
 */
import { apiClient } from '@services/api'
import type { AuthResponse, LoginRequest, RegisterRequest, RefreshTokenRequest, CurrentUser } from '../types/auth.types'
import type { Staff } from '@features/staff/types'

export interface ParentProfile {
  id: string
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  address: string
  status: string
}

const AUTH_TOKEN_KEY = 'auth_token'
const REFRESH_TOKEN_KEY = 'auth_refresh_token'
const USER_KEY = 'auth_user'

export const authStorage = {
  getToken: () => localStorage.getItem(AUTH_TOKEN_KEY),
  setToken: (token: string) => localStorage.setItem(AUTH_TOKEN_KEY, token),
  getRefreshToken: () => localStorage.getItem(REFRESH_TOKEN_KEY),
  setRefreshToken: (token: string) => localStorage.setItem(REFRESH_TOKEN_KEY, token),
  getUser: (): CurrentUser | null => {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? (JSON.parse(raw) as CurrentUser) : null
  },
  setUser: (user: CurrentUser) => localStorage.setItem(USER_KEY, JSON.stringify(user)),
  clear: () => {
    localStorage.removeItem(AUTH_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  },
}

export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/login', data)
    return response.data
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/register', data)
    return response.data
  },

  refresh: async (data: RefreshTokenRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/refresh-token', data)
    return response.data
  },

  logout: async (refreshToken?: string): Promise<void> => {
    try {
      await apiClient.post('/auth/logout', refreshToken ? { RefreshToken: refreshToken } : {})
    } catch {
      // Swallow errors on logout — clear local state regardless
    }
  },

  getCurrentUser: async (): Promise<CurrentUser> => {
    const response = await apiClient.get<CurrentUser>('/auth/me')
    return response.data
  },

  getStaffProfile: async (): Promise<Staff> => {
    const response = await apiClient.get<any>('/auth/me/staff-profile')
    return response.data?.data ?? response.data
  },

  updateStaffProfile: async (data: Pick<Staff, 'firstName' | 'lastName' | 'phone' | 'email' | 'certifications'>): Promise<Staff> => {
    const response = await apiClient.put<any>('/auth/me/staff-profile', data)
    return response.data?.data ?? response.data
  },

  changeStaffPassword: async (data: { currentPassword: string; newPassword: string; confirmPassword: string }): Promise<void> => {
    await apiClient.post('/auth/me/change-password', data)
  },

  getParentProfile: async (): Promise<ParentProfile> => {
    const response = await apiClient.get<any>('/auth/me/parent-profile')
    const data = response.data?.data ?? response.data?.Data ?? response.data
    return {
      id: data.Id ?? data.id,
      firstName: data.FirstName ?? data.firstName,
      lastName: data.LastName ?? data.lastName,
      email: data.Email ?? data.email,
      phoneNumber: data.PhoneNumber ?? data.phoneNumber,
      address: data.Address ?? data.address,
      status: data.Status ?? data.status,
    }
  },

  updateParentProfile: async (data: Omit<ParentProfile, 'id' | 'status'>): Promise<ParentProfile> => {
    const response = await apiClient.put<any>('/auth/me/parent-profile', data)
    const updated = response.data?.data ?? response.data?.Data ?? response.data
    return {
      id: updated.Id ?? updated.id,
      firstName: updated.FirstName ?? updated.firstName,
      lastName: updated.LastName ?? updated.lastName,
      email: updated.Email ?? updated.email,
      phoneNumber: updated.PhoneNumber ?? updated.phoneNumber,
      address: updated.Address ?? updated.address,
      status: updated.Status ?? updated.status,
    }
  },

  changeParentPassword: async (data: { currentPassword: string; newPassword: string; confirmPassword: string }): Promise<void> => {
    await apiClient.post('/auth/me/change-password', data)
  },
}
