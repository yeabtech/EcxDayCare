/**
 * Role-Specific Sidebar Components
 * Customized navigation for each role
 */

import { Link, useLocation } from 'react-router-dom'
import { LogOut } from 'lucide-react'
import { useAuth } from '@features/auth'
import { useParentNotifications } from '@features/notifications/ParentNotificationContext'
import * as Icons from 'lucide-react'

interface RoleSidebarProps {
  open: boolean
  onClose: () => void
  navItems: Array<{
    id: string
    label: string
    href: string
    icon: string
  }>
}

const getIcon = (iconName: string) => {
  const IconComponent = Icons[iconName as keyof typeof Icons] as any
  return IconComponent ? <IconComponent size={18} /> : null
}

const SidebarNav = ({ open, onClose, navItems }: RoleSidebarProps) => {
  const location = useLocation()
  const { user, logout } = useAuth()
  const { unreadCount } = useParentNotifications()

  const isActive = (href: string) => {
    return location.pathname === href || location.pathname.startsWith(href + '/')
  }

  return (
    <>
      {/* Mobile overlay */}
      {open && (
        <div
          className="fixed inset-x-0 bottom-0 top-16 z-30 bg-black/50 lg:hidden"
          aria-hidden="true"
          onClick={onClose}
        />
      )}

      {/* Sidebar */}
      <aside
        id="app-sidebar"
        className={`fixed bottom-0 left-0 top-16 z-40 flex w-72 max-w-[calc(100vw-3rem)] flex-col border-r border-gray-200 bg-white transition-transform duration-300 lg:static lg:w-64 lg:max-w-none lg:transform-none ${
          open ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
        }`}
      >
        <nav className="flex h-full min-h-0 flex-col justify-between p-4">
          <div className="space-y-1">
            <div className="px-3 py-2 text-[11px] font-semibold uppercase tracking-wider text-gray-400">
              Navigation
            </div>
            <div className="min-h-0 overflow-y-auto">
              {navItems.map((item) => (
                <Link
                  key={item.id}
                  to={item.href}
                  onClick={onClose}
                  className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm transition-all ${
                    isActive(item.href)
                      ? 'bg-primary-50 text-primary-700 font-semibold shadow-xs'
                      : 'text-gray-600 hover:bg-gray-100/80 hover:text-gray-900 font-medium'
                  }`}
                >
                  <span className={isActive(item.href) ? 'text-primary-600' : 'text-gray-500'}>
                    {getIcon(item.icon)}
                  </span>
                  <span>{item.label}</span>
                  {item.id === 'notifications' && unreadCount > 0 && (
                    <span
                      className="ml-auto inline-flex min-w-5 items-center justify-center rounded-full bg-rose-100 px-1.5 py-0.5 text-[10px] font-bold leading-none text-rose-700"
                      aria-label={`${unreadCount} unread`}
                    >
                      {unreadCount > 9 ? '9+' : unreadCount}
                    </span>
                  )}
                </Link>
              ))}
            </div>
          </div>

          {/* User Profile Card & Sign Out */}
          <div className="pt-4 border-t border-gray-100 space-y-2">
            {user && (
              <div className="p-3 rounded-xl bg-gray-50 border border-gray-100">
                <div className="flex items-center justify-between mb-1">
                  <span className="text-xs font-semibold text-gray-800 truncate">
                    {user.FirstName} {user.LastName}
                  </span>
                  <span className="text-[10px] font-medium px-1.5 py-0.5 rounded bg-white text-gray-600 border border-gray-200">
                    {user.Role}
                  </span>
                </div>
                <div className="text-[11px] text-gray-400 truncate">{user.Email}</div>
              </div>
            )}

            <button
              onClick={() => logout()}
              className="flex items-center gap-2.5 px-3 py-2 text-sm text-gray-600 hover:text-red-600 hover:bg-red-50 rounded-xl transition-colors w-full font-medium"
            >
              <LogOut size={16} />
              <span>Sign Out</span>
            </button>
          </div>
        </nav>
      </aside>
    </>
  )
}

export { SidebarNav }
