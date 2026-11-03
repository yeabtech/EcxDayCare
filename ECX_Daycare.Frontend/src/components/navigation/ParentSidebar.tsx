/**
 * Parent Sidebar - Role-specific navigation for guardians
 */

import { SidebarNav } from './SidebarNav'
import { PARENT_NAV_ITEMS } from '@/types/roles'

interface ParentSidebarProps {
  open: boolean
  onClose: () => void
}

export const ParentSidebar = ({ open, onClose }: ParentSidebarProps) => {
  const navItems = PARENT_NAV_ITEMS.map((item) => ({
    id: item.id,
    label: item.label,
    href: item.href,
    icon: item.icon,
  }))

  return <SidebarNav open={open} onClose={onClose} navItems={navItems} />
}
