/**
 * Auth Context and Provider
 */
import React, { createContext, useState, useEffect, useCallback, useMemo } from 'react'
import { authApi, authStorage } from '../api/authApi'
import type { CurrentUser, LoginRequest } from '../types/auth.types'

export interface AuthContextType {
  user: CurrentUser | null
  token: string | null
  isAuthenticated: boolean
  isLoading: boolean
  isAdmin: boolean
  isStaff: boolean
  isParent: boolean
  login: (data: LoginRequest) => Promise<void>
  updateUser: (updates: Partial<CurrentUser>) => void
  logout: () => Promise<void>
  hasRole: (roles: string | string[]) => boolean
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<CurrentUser | null>(() => authStorage.getUser())
  const [token, setToken] = useState<string | null>(() => authStorage.getToken())
  const [isLoading, setIsLoading] = useState<boolean>(true)

  // Validate or restore authentication on mount
  useEffect(() => {
    let isMounted = true

    const initAuth = async () => {
      const storedToken = authStorage.getToken()
      const storedUser = authStorage.getUser()

      if (!storedToken) {
        if (isMounted) {
          setUser(null)
          setToken(null)
          setIsLoading(false)
        }
        return
      }

      // If we already have stored user, set it immediately to prevent flickering
      if (storedUser && isMounted) {
        setUser(storedUser)
        setToken(storedToken)
      }

      try {
        // Fetch current user from server to verify token
        const currentUser = await authApi.getCurrentUser()
        if (isMounted) {
          setUser(currentUser)
          authStorage.setUser(currentUser)
        }
      } catch (error) {
        // Token might be expired, try refresh
        const refreshToken = authStorage.getRefreshToken()
        if (refreshToken) {
          try {
            const refreshRes = await authApi.refresh({ RefreshToken: refreshToken })
            authStorage.setToken(refreshRes.AccessToken)
            authStorage.setRefreshToken(refreshRes.RefreshToken)
            authStorage.setUser(refreshRes.User)
            if (isMounted) {
              setToken(refreshRes.AccessToken)
              setUser(refreshRes.User)
            }
          } catch {
            // Refresh failed — clear auth
            authStorage.clear()
            if (isMounted) {
              setUser(null)
              setToken(null)
            }
          }
        } else {
          authStorage.clear()
          if (isMounted) {
            setUser(null)
            setToken(null)
          }
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    initAuth()

    return () => {
      isMounted = false
    }
  }, [])

  const login = useCallback(async (data: LoginRequest) => {
    setIsLoading(true)
    try {
      const response = await authApi.login(data)
      authStorage.setToken(response.AccessToken)
      authStorage.setRefreshToken(response.RefreshToken)
      authStorage.setUser(response.User)
      setToken(response.AccessToken)
      setUser(response.User)
    } finally {
      setIsLoading(false)
    }
  }, [])

  const logout = useCallback(async () => {
    const refreshToken = authStorage.getRefreshToken()
    try {
      if (refreshToken) {
        await authApi.logout(refreshToken)
      }
    } finally {
      authStorage.clear()
      setUser(null)
      setToken(null)
    }
  }, [])

  const updateUser = useCallback((updates: Partial<CurrentUser>) => {
    setUser((current) => {
      if (!current) return current
      const updated = { ...current, ...updates }
      authStorage.setUser(updated)
      return updated
    })
  }, [])

  const hasRole = useCallback(
    (roles: string | string[]) => {
      if (!user || !user.Role) return false
      const allowed = Array.isArray(roles) ? roles : [roles]
      return allowed.some((r) => r.toLowerCase() === user.Role.toLowerCase())
    },
    [user]
  )

  const isAdmin = useMemo(() => user?.Role?.toLowerCase() === 'admin', [user])
  const isStaff = useMemo(() => user?.Role?.toLowerCase() === 'staff', [user])
  const isParent = useMemo(() => user?.Role?.toLowerCase() === 'parent', [user])
  const isAuthenticated = Boolean(token && user)

  const value = useMemo(
    () => ({
      user,
      token,
      isAuthenticated,
      isLoading,
      isAdmin,
      isStaff,
      isParent,
      login,
      updateUser,
      logout,
      hasRole,
    }),
    [user, token, isAuthenticated, isLoading, isAdmin, isStaff, isParent, login, updateUser, logout, hasRole]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
