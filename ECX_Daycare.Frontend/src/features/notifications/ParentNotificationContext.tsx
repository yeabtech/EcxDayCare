import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { apiClient } from '@services/api'
import { useAuth } from '@features/auth'

export type ParentNotification = {
  id: string
  dailyReportId: string
  childId: string
  childName?: string | null
  title: string
  message: string
  isRead: boolean
  createdAt: string
  readAt?: string | null
  reportDate: string
}

export type ParentIncidentNotification = {
  id: string
  incidentId: string
  childId: string
  childName: string
  incidentTime: string
  location: string
  description: string
  injuryType: string
  affectedBodyPart: string
  firstAidGiven?: string | null
  followUp?: string | null
  notes?: string | null
  isRead: boolean
  createdAt: string
  readAt?: string | null
}

interface ParentNotificationContextValue {
  notifications: ParentNotification[]
  incidentNotifications: ParentIncidentNotification[]
  unreadCount: number
  incidentUnreadCount: number
  refreshNotifications: () => Promise<ParentNotification[]>
  markAsRead: (dailyReportId: string) => void
  markIncidentAsRead: (incidentId: string) => void
}

const ParentNotificationContext = createContext<ParentNotificationContextValue | null>(null)

const unwrap = (response: any) => response?.data?.data ?? response?.data?.Data ?? response?.data
const value = (item: any, camelCase: string, pascalCase: string) => item?.[camelCase] ?? item?.[pascalCase]

const normalizeNotification = (item: any): ParentNotification => ({
  id: String(value(item, 'id', 'Id') ?? ''),
  dailyReportId: String(value(item, 'dailyReportId', 'DailyReportId') ?? ''),
  childId: String(value(item, 'childId', 'ChildId') ?? ''),
  childName: value(item, 'childName', 'ChildName'),
  title: value(item, 'title', 'Title') ?? 'Daycare update',
  message: value(item, 'message', 'Message') ?? '',
  isRead: value(item, 'isRead', 'IsRead') ?? false,
  createdAt: value(item, 'createdAt', 'CreatedAt') ?? '',
  readAt: value(item, 'readAt', 'ReadAt'),
  reportDate: value(item, 'reportDate', 'ReportDate') ?? '',
})

const normalizeIncidentNotification = (item: any): ParentIncidentNotification => ({
  id: String(value(item, 'id', 'Id') ?? ''),
  incidentId: String(value(item, 'incidentId', 'IncidentId') ?? ''),
  childId: String(value(item, 'childId', 'ChildId') ?? ''),
  childName: value(item, 'childName', 'ChildName') ?? 'Your child',
  incidentTime: value(item, 'incidentTime', 'IncidentTime') ?? '',
  location: value(item, 'location', 'Location') ?? '',
  description: value(item, 'description', 'Description') ?? '',
  injuryType: value(item, 'injuryType', 'InjuryType') ?? '',
  affectedBodyPart: value(item, 'affectedBodyPart', 'AffectedBodyPart') ?? '',
  firstAidGiven: value(item, 'firstAidGiven', 'FirstAidGiven'),
  followUp: value(item, 'followUp', 'FollowUp'),
  notes: value(item, 'notes', 'Notes'),
  isRead: value(item, 'isRead', 'IsRead') ?? false,
  createdAt: value(item, 'createdAt', 'CreatedAt') ?? '',
  readAt: value(item, 'readAt', 'ReadAt'),
})

export const ParentNotificationProvider = ({ children }: { children: ReactNode }) => {
  const { user } = useAuth()
  const [notifications, setNotifications] = useState<ParentNotification[]>([])
  const [incidentNotifications, setIncidentNotifications] = useState<ParentIncidentNotification[]>([])

  const refreshNotifications = useCallback(async () => {
    if (user?.Role?.toLowerCase() !== 'parent') {
      setNotifications([])
      setIncidentNotifications([])
      return []
    }

    const [response, incidentResponse] = await Promise.all([
      apiClient.get<any>('/parent-portal/notifications'),
      apiClient.get<any>('/parent-portal/incident-notifications'),
    ])
    const items: any[] = Array.isArray(unwrap(response)) ? unwrap(response) : []
    const incidentItems: any[] = Array.isArray(unwrap(incidentResponse)) ? unwrap(incidentResponse) : []
    const normalized = items.map(normalizeNotification)
    setNotifications(normalized)
    setIncidentNotifications(incidentItems.map(normalizeIncidentNotification))
    return normalized
  }, [user?.Role])

  const markAsRead = useCallback((dailyReportId: string) => {
    setNotifications(current => current.map(notification =>
      notification.dailyReportId === dailyReportId
        ? { ...notification, isRead: true, readAt: notification.readAt ?? new Date().toISOString() }
        : notification))
  }, [])

  const markIncidentAsRead = useCallback((incidentId: string) => {
    setIncidentNotifications(current => current.map(notification =>
      notification.incidentId === incidentId
        ? { ...notification, isRead: true, readAt: notification.readAt ?? new Date().toISOString() }
        : notification))
  }, [])

  useEffect(() => {
    if (user?.Role?.toLowerCase() !== 'parent') {
      setNotifications([])
      setIncidentNotifications([])
      return
    }

    const refresh = () => {
      void refreshNotifications().catch(error => {
        console.error('Could not refresh parent notifications.', error)
      })
    }

    refresh()
    const intervalId = window.setInterval(refresh, 60_000)
    window.addEventListener('focus', refresh)
    return () => {
      window.clearInterval(intervalId)
      window.removeEventListener('focus', refresh)
    }
  }, [refreshNotifications, user?.Role])

  const value = useMemo(() => ({
    notifications,
    incidentNotifications,
    incidentUnreadCount: incidentNotifications.filter(notification => !notification.isRead).length,
    unreadCount: notifications.filter(notification => !notification.isRead).length
      + incidentNotifications.filter(notification => !notification.isRead).length,
    refreshNotifications,
    markAsRead,
    markIncidentAsRead,
  }), [notifications, incidentNotifications, refreshNotifications, markAsRead, markIncidentAsRead])

  return (
    <ParentNotificationContext.Provider value={value}>
      {children}
    </ParentNotificationContext.Provider>
  )
}

export const useParentNotifications = () => {
  const context = useContext(ParentNotificationContext)
  if (!context)
    throw new Error('useParentNotifications must be used within ParentNotificationProvider.')
  return context
}
