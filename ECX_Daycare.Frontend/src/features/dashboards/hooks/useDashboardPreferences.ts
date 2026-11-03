/**
 * User Dashboard Preferences Hook
 * Manages user-specific dashboard layout, theme, and widget settings
 */
import { useState, useCallback, useEffect } from 'react'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'

export interface DashboardWidget {
  id: string
  name: string
  position: number
  visible: boolean
  size: 'small' | 'medium' | 'large'
}

export interface UserDashboardPreferences {
  userId: string
  theme: 'light' | 'dark'
  layout: 'grid' | 'list' | 'compact'
  widgets: DashboardWidget[]
  refreshInterval: number // milliseconds
  autoRefresh: boolean
  showNotifications: boolean
  compactMode: boolean
  defaultChartType: 'bar' | 'line' | 'pie'
  dateRange: 'today' | 'week' | 'month' | 'year' | 'custom'
  customStartDate?: string
  customEndDate?: string
}

export const useDashboardPreferences = () => {
  const [preferences, setPreferences] = useState<UserDashboardPreferences | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isSaving, setIsSaving] = useState(false)

  /**
   * Load preferences from API
   */
  const loadPreferences = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      const response = await apiClient.get('/users/preferences/dashboard')
      setPreferences(response.data as UserDashboardPreferences)
    } catch (err) {
      const apiError = handleApiError(err)
      setError(apiError.message)
      // Use default preferences if load fails
      setPreferences(getDefaultPreferences())
    } finally {
      setIsLoading(false)
    }
  }, [])

  /**
   * Save preferences to API
   */
  const savePreferences = useCallback(async (prefs: UserDashboardPreferences) => {
    setIsSaving(true)
    setError(null)

    try {
      await apiClient.put('/users/preferences/dashboard', prefs)
      setPreferences(prefs)
      return true
    } catch (err) {
      const apiError = handleApiError(err)
      setError(apiError.message)
      return false
    } finally {
      setIsSaving(false)
    }
  }, [])

  /**
   * Update a specific preference
   */
  const updatePreference = useCallback(
    <K extends keyof UserDashboardPreferences>(key: K, value: UserDashboardPreferences[K]) => {
      if (preferences) {
        const updated = { ...preferences, [key]: value }
        setPreferences(updated)
        return savePreferences(updated)
      }
    },
    [preferences, savePreferences]
  )

  /**
   * Update widget visibility
   */
  const toggleWidgetVisibility = useCallback(
    (widgetId: string) => {
      if (!preferences) return

      const updated = {
        ...preferences,
        widgets: preferences.widgets.map((w) =>
          w.id === widgetId ? { ...w, visible: !w.visible } : w
        ),
      }
      setPreferences(updated)
      return savePreferences(updated)
    },
    [preferences, savePreferences]
  )

  /**
   * Reorder widgets
   */
  const reorderWidgets = useCallback(
    (sourceId: string, targetId: string) => {
      if (!preferences) return

      const sourceIdx = preferences.widgets.findIndex((w) => w.id === sourceId)
      const targetIdx = preferences.widgets.findIndex((w) => w.id === targetId)

      if (sourceIdx === -1 || targetIdx === -1) return

      const updated = {
        ...preferences,
        widgets: [...preferences.widgets],
      }

      // Swap positions
      const temp = updated.widgets[sourceIdx]
      updated.widgets[sourceIdx] = updated.widgets[targetIdx]
      updated.widgets[targetIdx] = temp

      setPreferences(updated)
      return savePreferences(updated)
    },
    [preferences, savePreferences]
  )

  /**
   * Reset to default preferences
   */
  const resetToDefaults = useCallback(async () => {
    const defaults = getDefaultPreferences()
    const success = await savePreferences(defaults)
    if (success) {
      setPreferences(defaults)
    }
    return success
  }, [savePreferences])

  /**
   * Load on mount
   */
  useEffect(() => {
    loadPreferences()
  }, [loadPreferences])

  return {
    preferences,
    isLoading,
    isSaving,
    error,
    loadPreferences,
    savePreferences,
    updatePreference,
    toggleWidgetVisibility,
    reorderWidgets,
    resetToDefaults,
  }
}

/**
 * Get default dashboard preferences
 */
export const getDefaultPreferences = (): UserDashboardPreferences => ({
  userId: '',
  theme: 'light',
  layout: 'grid',
  widgets: [
    {
      id: 'overview-stats',
      name: 'Overview Statistics',
      position: 0,
      visible: true,
      size: 'large',
    },
    {
      id: 'attendance-trend',
      name: 'Attendance Trend',
      position: 1,
      visible: true,
      size: 'large',
    },
    {
      id: 'classroom-capacity',
      name: 'Classroom Capacity',
      position: 2,
      visible: true,
      size: 'medium',
    },
    {
      id: 'recent-incidents',
      name: 'Recent Incidents',
      position: 3,
      visible: true,
      size: 'medium',
    },
    {
      id: 'quick-actions',
      name: 'Quick Actions',
      position: 4,
      visible: true,
      size: 'small',
    },
    {
      id: 'upcoming-events',
      name: 'Upcoming Events',
      position: 5,
      visible: true,
      size: 'small',
    },
  ],
  refreshInterval: 5 * 60 * 1000, // 5 minutes
  autoRefresh: true,
  showNotifications: true,
  compactMode: false,
  defaultChartType: 'bar',
  dateRange: 'week',
})
