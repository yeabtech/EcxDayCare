/**
 * Sidebar Navigation Component
 * Role-aware navigation with user identity
 */
import { Link, useLocation } from 'react-router-dom'
import { LogOut } from 'lucide-react'
import { ROUTES } from '@routes/constants'
import { useAuth } from '@features/auth'
import { getNavigationForRole, UserRole } from '@/types/roles'
import * as Icons from 'lucide-react'

interface SidebarProps {
  open: boolean
  role?: UserRole
}

export const Sidebar = ({ open, role = 'Parent' }: SidebarProps) => {
  const location = useLocation()
  const { user, logout } = useAuth()

  // Get icon component by name
  const getIcon = (iconName: string) => {
    const IconComponent = Icons[iconName as keyof typeof Icons] as any
    return IconComponent ? <IconComponent size={18} /> : null
  }

  const isActive = (href: string) => {
    if (href === ROUTES.DASHBOARD) {
      return location.pathname === ROUTES.DASHBOARD
    }
    return location.pathname.startsWith(href)
  }

  // Get navigation items for the user's role
  const navItems = getNavigationForRole(role)

  return (
    <>
      {/* Mobile overlay */}
      {open && (
        <div className="fixed inset-0 bg-black/50 lg:hidden z-30" aria-hidden="true" />
      )}

      {/* Sidebar */}
      <aside
        className={`fixed lg:static inset-y-16 left-0 w-64 bg-white border-r border-gray-200 transition-transform duration-300 transform lg:transform-none z-40 flex flex-col ${
          open ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
        }`}
      >
        <nav className="p-4 flex flex-col h-full justify-between">
          <div className="space-y-1">
            <div className="px-3 py-2 text-[11px] font-semibold uppercase tracking-wider text-gray-400">
              Navigation
            </div>
            {navItems.map((item) => (
              <Link
                key={item.href}
                to={item.href}
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
              </Link>
            ))}
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
