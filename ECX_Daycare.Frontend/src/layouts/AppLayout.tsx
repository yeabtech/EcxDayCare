/**
 * Main Application Layout Component
 * Adapts sidebar and header based on user role
 */
import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { Header } from '@components/Header'
import { AdminSidebar } from '@components/navigation/AdminSidebar'
import { StaffSidebar } from '@components/navigation/StaffSidebar'
import { ParentSidebar } from '@components/navigation/ParentSidebar'
import { useAuth } from '@features/auth'
import { ParentNotificationProvider } from '@features/notifications/ParentNotificationContext'
import type { UserRole } from '@/types/roles'

export const AppLayout = () => {
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const { user } = useAuth()

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen)
  }

  const renderSidebar = (role: UserRole) => {
    switch (role) {
      case 'Admin':
        return <AdminSidebar open={sidebarOpen} onClose={toggleSidebar} />
      case 'Staff':
        return <StaffSidebar open={sidebarOpen} onClose={toggleSidebar} />
      case 'Parent':
      default:
        return <ParentSidebar open={sidebarOpen} onClose={toggleSidebar} />
    }
  }

  return (
    <ParentNotificationProvider>
      <div className="flex h-dvh flex-col overflow-hidden bg-gray-50">
        <Header onToggleSidebar={toggleSidebar} sidebarOpen={sidebarOpen} userRole={user?.Role} />
        <div className="flex min-h-0 flex-1 overflow-hidden">
          {renderSidebar(user?.Role || 'Parent')}
          <main className="min-w-0 flex-1 overflow-auto">
            <div className="p-3 sm:p-4 lg:p-6">
              <Outlet />
            </div>
          </main>
        </div>
      </div>
    </ParentNotificationProvider>
  )
}
