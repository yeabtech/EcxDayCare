/**
 * Header/Top Navigation Component
 * Displays user profile, role badge, and quick logout
 */
import { Menu, X, LogOut, Shield, Bell, AlertTriangle } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useAuth } from '@features/auth'
import { useParentNotifications } from '@features/notifications/ParentNotificationContext'
import ecxLogo from '@/logo/EcxLogo.png'

interface HeaderProps {
  onToggleSidebar: () => void
  sidebarOpen: boolean
  userRole?: string
}

export const Header = ({ onToggleSidebar, sidebarOpen }: HeaderProps) => {
  const { user, logout } = useAuth()
  const { unreadCount, incidentUnreadCount } = useParentNotifications()

  const getInitials = () => {
    if (!user) return '?'
    const first = user.FirstName?.[0] || ''
    const last = user.LastName?.[0] || ''
    return (first + last).toUpperCase() || 'U'
  }

  const getRoleBadgeColor = () => {
    switch (user?.Role?.toLowerCase()) {
      case 'admin':
        return 'bg-primary-50 text-primary-700 border-primary-200'
      case 'staff':
        return 'bg-info-50 text-info-700 border-info-200'
      case 'parent':
        return 'bg-accent-50 text-accent-700 border-accent-200'
      default:
        return 'bg-gray-50 text-gray-700 border-gray-200'
    }
  }

  const getAvatarBgColor = () => {
    switch (user?.Role?.toLowerCase()) {
      case 'admin':
        return 'bg-gradient-to-tr from-primary-600 to-primary-500'
      case 'staff':
        return 'bg-gradient-to-tr from-info-600 to-info-500'
      case 'parent':
        return 'bg-gradient-to-tr from-accent-600 to-accent-500'
      default:
        return 'bg-gradient-to-tr from-primary-600 to-primary-500'
    }
  }

  return (
    <header className="relative z-50 h-16 shrink-0 bg-white border-b border-gray-200 shadow-sm">
      <div className="flex h-full items-center justify-between gap-2 px-3 sm:px-5 lg:px-6">
        <div className="flex min-w-0 items-center gap-2 sm:gap-3">
          <button
            onClick={onToggleSidebar}
            className="lg:hidden flex h-10 w-10 shrink-0 items-center justify-center rounded-lg text-gray-600 transition-colors hover:bg-gray-100"
            aria-label="Toggle navigation"
            aria-expanded={sidebarOpen}
            aria-controls="app-sidebar"
          >
            {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
          <div className="flex min-w-0 items-center gap-2 sm:gap-2.5">
            <img
              src={ecxLogo}
              alt="ECX Daycare"
              className="h-9 w-9 shrink-0 object-contain shadow-md sm:h-10 sm:w-10"
            />
            <h1 className="truncate text-base font-bold tracking-tight text-gray-900 sm:text-lg">ECX Daycare</h1>
          </div>
        </div>

        {/* User Info & Actions */}
        <div className="flex shrink-0 items-center gap-2 sm:gap-3">
          {user?.Role?.toLowerCase() === 'parent' && (
            <Link
              to="/notifications"
              className="relative flex h-10 w-10 items-center justify-center rounded-xl text-gray-600 transition-colors hover:bg-amber-50 hover:text-amber-700"
              aria-label={unreadCount > 0 ? `Notifications, ${unreadCount} unread` : 'Notifications'}
              title={unreadCount > 0 ? `${unreadCount} unread notification${unreadCount === 1 ? '' : 's'}` : 'Notifications'}
            >
              {incidentUnreadCount > 0
                ? <AlertTriangle size={20} className="text-rose-600" />
                : <Bell size={20} />}
              {unreadCount > 0 && (
                <span className="absolute right-1 top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-rose-600 px-1 text-[10px] font-bold leading-none text-white ring-2 ring-white">
                  {unreadCount > 9 ? '9+' : unreadCount}
                </span>
              )}
            </Link>
          )}
          {user && (
            <div className="flex items-center gap-2 sm:gap-3">
              {/* User details */}
              <div className="hidden md:flex flex-col items-end">
                <span className="text-sm font-semibold text-gray-900 leading-tight">
                  {user.FirstName} {user.LastName}
                </span>
                <span
                  className={`text-[11px] font-medium px-2.5 py-1 rounded-full border mt-1 inline-flex items-center gap-1.5 ${getRoleBadgeColor()}`}
                >
                  <Shield size={11} />
                  {user.Role}
                </span>
              </div>

              {/* Avatar */}
              <div
                className={`flex h-9 w-9 items-center justify-center rounded-full text-xs font-bold text-white shadow-sm sm:h-10 sm:w-10 ${getAvatarBgColor()}`}
                title={`${user.FirstName} ${user.LastName} (${user.Email})`}
              >
                {getInitials()}
              </div>

              {/* Logout Button */}
              <button
                onClick={() => logout()}
                className="flex h-9 w-9 items-center justify-center rounded-lg border border-gray-200 text-gray-600 transition-colors hover:border-danger-200 hover:bg-danger-50 hover:text-danger-600 sm:h-auto sm:w-auto sm:gap-1.5 sm:px-3 sm:py-2 sm:text-xs sm:ml-1"
                aria-label="Sign out"
                title="Sign out"
              >
                <LogOut size={14} />
                <span className="hidden md:inline">Sign Out</span>
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  )
}
