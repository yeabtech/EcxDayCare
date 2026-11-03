/**
 * Admin Sidebar - Structured navigation with collapse toggle
 * Responsive: slides in on mobile, collapses to icons on desktop
 */

import { useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { LogOut, ChevronLeft } from 'lucide-react'
import { useAuth } from '@features/auth'
import { ADMIN_NAV_ITEMS } from '@/types/roles'
import * as Icons from 'lucide-react'


interface AdminSidebarProps {
  open: boolean
  onClose: () => void
}

const getIcon = (iconName: string): React.ReactNode => {
  const IconComponent = Icons[iconName as keyof typeof Icons] as any
  return IconComponent ? <IconComponent size={18} /> : null
}

export const AdminSidebar = ({ open, onClose }: AdminSidebarProps) => {
  const location = useLocation()
  const { user, logout } = useAuth()
  const [collapsed, setCollapsed] = useState(false)

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
        className={`fixed bottom-0 left-0 top-16 z-40 flex w-72 max-w-[calc(100vw-3rem)] flex-col border-r border-gray-200 bg-white transition-all duration-300 ease-out lg:static lg:max-w-none lg:transform-none ${
          open ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
        } ${collapsed ? 'lg:w-16' : 'lg:w-56'}`}
      >
        <nav className="flex h-full min-h-0 flex-col p-2">
          {/* Toggle */}
          <div className="flex items-center justify-between mb-2">
            
            <button
              onClick={() => setCollapsed(!collapsed)}
              className="hidden lg:flex p-2 hover:bg-gray-100 rounded-lg transition-colors text-gray-600 ml-auto"
              aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
            >
              <ChevronLeft
                size={18}
                className={`transition-transform duration-200 ${collapsed ? 'rotate-180' : ''}`}
              />
            </button>
          </div>

          {/* Navigation */}
          <div className="min-h-0 flex-1 overflow-y-auto">
            <ul className="space-y-0.5">
              {ADMIN_NAV_ITEMS.map((item) => (
                <li key={item.id}>
                  <Link
                    to={item.href}
                    onClick={onClose}
                    title={collapsed ? item.label : undefined}
                    className={`flex items-center gap-3 px-3 py-2 rounded text-sm transition-colors ${
                      isActive(item.href)
                        ? 'bg-primary-50 text-primary-700 font-medium'
                        : 'text-gray-700 hover:bg-gray-100'
                    } ${collapsed ? 'lg:justify-center lg:px-0' : ''}`}
                  >
                    <span className="flex-shrink-0">{getIcon(item.icon)}</span>
                    <span className={`${collapsed ? 'lg:hidden' : ''}`}>{item.label}</span>
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Footer */}
          <div className="pt-2 mt-2 border-t border-gray-100">
            {(!collapsed || open) && user && (
              <div className="px-3 py-1.5 text-xs text-gray-600 truncate">
                {user.FirstName} {user.LastName}
              </div>
            )}
            <button
              onClick={() => logout()}
              title="Sign Out"
              className={`flex items-center gap-2 px-3 py-2 text-sm text-gray-600 hover:text-danger-600 hover:bg-danger-50 rounded w-full transition-colors ${
                collapsed ? 'lg:justify-center lg:px-0' : ''
              }`}
            >
              <LogOut size={16} />
              <span className={collapsed ? 'lg:hidden' : ''}>Sign Out</span>
            </button>
          </div>
        </nav>
      </aside>
    </>
  )
}
