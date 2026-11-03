/**
 * HTTP client configuration and interceptors
 */
import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios'
import { config } from '../config'

class ApiClient {
  public client: AxiosInstance

  constructor() {
    this.client = axios.create({
      baseURL: config.apiBaseUrl,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    this.setupInterceptors()
  }

  private setupInterceptors(): void {
    // Request interceptor
    this.client.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        // Add auth token from localStorage if available
        const token = localStorage.getItem('auth_token')
        if (token) {
          config.headers.Authorization = `Bearer ${token}`
        }
        return config
      },
      (error: AxiosError) => {
        return Promise.reject(error)
      }
    )

    // Response interceptor
    this.client.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        if (error.response?.status === 401 && !error.config?.url?.includes('/auth/login')) {
          // Handle unauthorized - clear tokens and redirect to login
          localStorage.removeItem('auth_token')
          localStorage.removeItem('auth_refresh_token')
          localStorage.removeItem('auth_user')
          window.location.href = '/login'
        }
        return Promise.reject(error)
      }
    )
  }

  public get<T>(url: string, config?: any) {
    return this.client.get<T>(url, config)
  }

  public post<T>(url: string, data?: any, config?: any) {
    return this.client.post<T>(url, data, config)
  }

  public put<T>(url: string, data?: any, config?: any) {
    return this.client.put<T>(url, data, config)
  }

  public delete<T>(url: string, config?: any) {
    return this.client.delete<T>(url, config)
  }

  public patch<T>(url: string, data?: any, config?: any) {
    return this.client.patch<T>(url, data, config)
  }
}

export const apiClient = new ApiClient()
