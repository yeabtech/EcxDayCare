/**
 * Staff Sidebar - Role-specific navigation for caregivers
 */

import { SidebarNav } from './SidebarNav'
import { STAFF_NAV_ITEMS } from '@/types/roles'

interface StaffSidebarProps {
  open: boolean
  onClose: () => void
}

export const StaffSidebar = ({ open, onClose }: StaffSidebarProps) => {
  const navItems = STAFF_NAV_ITEMS.map((item) => ({
    id: item.id,
    label: item.label,
    href: item.href,
    icon: item.icon,
  }))

  return <SidebarNav open={open} onClose={onClose} navItems={navItems} />
}
