/**
 * Role-based UI Configuration
 * Defines what features, pages, and navigation items each role can access
 */

export type UserRole = 'Admin' | 'Staff' | 'Parent'

export type FeatureName =
  | 'dashboard'
  | 'children'
  | 'parents'
  | 'staff'
  | 'classrooms'
  | 'enrollments'
  | 'attendance'
  | 'daily_reports'
  | 'feeding'
  | 'nap'
  | 'activities'
  | 'health'
  | 'incidents'
  | 'settings'

export interface NavigationItem {
  id: string
  label: string
  href: string
  icon: string // Icon name from lucide-react
  roles: UserRole[]
  subItems?: NavigationItem[]
  badge?: {
    count?: number
    variant?: 'default' | 'warning' | 'critical'
  }
}

export interface NavigationSection {
  id: string
  title: string
  items: NavigationItem[]
  collapsible?: boolean
  defaultOpen?: boolean
}

export interface DashboardConfig {
  title: string
  greeting: string
  showStats: boolean
  statFields: string[]
  showQuickActions: boolean
  quickActions: QuickAction[]
  mainSections: DashboardSection[]
}

export interface QuickAction {
  id: string
  label: string
  icon: string
  href: string
  variant?: 'primary' | 'secondary' | 'danger'
  roles: UserRole[]
}

export interface DashboardSection {
  id: string
  title: string
  type:
    | 'stats'
    | 'table'
    | 'timeline'
    | 'cards'
    | 'chart'
    | 'activity_feed'
    | 'incidents'
    | 'empty'
  roles: UserRole[]
  columns?: string
  maxItems?: number
}

export interface RoleConfig {
  role: UserRole
  displayName: string
  navigationItems: NavigationItem[]
  dashboardConfig: DashboardConfig
  pagePermissions: Record<string, boolean>
}

/**
 * Navigation items by role
 */

// Admin Navigation - Flat array for compatibility
export const ADMIN_NAV_ITEMS: NavigationItem[] = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    href: '/dashboard',
    icon: 'LayoutDashboard',
    roles: ['Admin'],
  },
  {
    id: 'children',
    label: 'Children',
    href: '/children',
    icon: 'Users',
    roles: ['Admin'],
  },
  {
    id: 'parents',
    label: 'Parents',
    href: '/parents',
    icon: 'Users2',
    roles: ['Admin'],
  },
  {
    id: 'staff',
    label: 'Staff',
    href: '/staff',
    icon: 'UserCheck',
    roles: ['Admin'],
  },
  {
    id: 'classrooms',
    label: 'Classrooms',
    href: '/classrooms',
    icon: 'Layers',
    roles: ['Admin'],
  },
  {
    id: 'enrollments',
    label: 'Enrollments',
    href: '/enrollments',
    icon: 'ClipboardList',
    roles: ['Admin'],
  },
  {
    id: 'attendance',
    label: 'Attendance',
    href: '/attendance',
    icon: 'ClipboardCheck',
    roles: ['Admin'],
  },
  {
    id: 'reports',
    label: 'Daily Reports',
    href: '/reports',
    icon: 'FileText',
    roles: ['Admin'],
  },
  {
    id: 'health',
    label: 'Health',
    href: '/health',
    icon: 'Heart',
    roles: ['Admin'],
  },
  {
    id: 'incidents',
    label: 'Incidents',
    href: '/incidents',
    icon: 'AlertTriangle',
    roles: ['Admin'],
  },
  {
    id: 'users-access',
    label: 'Users & Access',
    href: '/users-access',
    icon: 'Lock',
    roles: ['Admin'],
  },
  {
    id: 'settings',
    label: 'Settings',
    href: '/settings',
    icon: 'Settings',
    roles: ['Admin'],
  },
]

// Admin Navigation Sections - Organized with categories
export const ADMIN_NAV_SECTIONS: NavigationSection[] = [
  {
    id: 'overview',
    title: 'OVERVIEW',
    collapsible: false,
    defaultOpen: true,
    items: [
      {
        id: 'dashboard',
        label: 'Dashboard',
        href: '/dashboard',
        icon: 'LayoutDashboard',
        roles: ['Admin'],
      },
    ],
  },
  {
    id: 'people',
    title: 'PEOPLE',
    collapsible: true,
    defaultOpen: true,
    items: [
      {
        id: 'children',
        label: 'Children',
        href: '/children',
        icon: 'Users',
        roles: ['Admin'],
      },
      {
        id: 'parents',
        label: 'Parents',
        href: '/parents',
        icon: 'Users2',
        roles: ['Admin'],
      },
      {
        id: 'staff',
        label: 'Staff',
        href: '/staff',
        icon: 'UserCheck',
        roles: ['Admin'],
      },
    ],
  },
  {
    id: 'organization',
    title: 'ORGANIZATION',
    collapsible: true,
    defaultOpen: true,
    items: [
      {
        id: 'classrooms',
        label: 'Classrooms',
        href: '/classrooms',
        icon: 'Layers',
        roles: ['Admin'],
      },
      {
        id: 'enrollments',
        label: 'Enrollments',
        href: '/enrollments',
        icon: 'ClipboardList',
        roles: ['Admin'],
      },
    ],
  },
  {
    id: 'care',
    title: 'CARE',
    collapsible: true,
    defaultOpen: true,
    items: [
      {
        id: 'attendance',
        label: 'Attendance',
        href: '/attendance',
        icon: 'ClipboardCheck',
        roles: ['Admin'],
      },
      {
        id: 'reports',
        label: 'Daily Reports',
        href: '/reports',
        icon: 'FileText',
        roles: ['Admin'],
      },
      {
        id: 'activities',
        label: 'Activities',
        href: '/activities',
        icon: 'Lightbulb',
        roles: ['Admin'],
      },
      {
        id: 'feeding',
        label: 'Feeding',
        href: '/feeding',
        icon: 'Apple',
        roles: ['Admin'],
      },
      {
        id: 'nap',
        label: 'Nap',
        href: '/nap',
        icon: 'Moon',
        roles: ['Admin'],
      },
      {
        id: 'health',
        label: 'Health',
        href: '/health',
        icon: 'Heart',
        roles: ['Admin'],
      },
      {
        id: 'incidents',
        label: 'Incidents',
        href: '/incidents',
        icon: 'AlertTriangle',
        roles: ['Admin'],
      },
    ],
  },
  {
    id: 'system',
    title: 'SYSTEM',
    collapsible: true,
    defaultOpen: false,
    items: [
      {
        id: 'notifications',
        label: 'Notifications',
        href: '/notifications',
        icon: 'Bell',
        roles: ['Admin'],
      },
      {
        id: 'users-access',
        label: 'Users & Access',
        href: '/users-access',
        icon: 'Lock',
        roles: ['Admin'],
      },
      {
        id: 'settings',
        label: 'Settings',
        href: '/settings',
        icon: 'Settings',
        roles: ['Admin'],
      },
    ],
  },
]

// Staff Navigation
export const STAFF_NAV_ITEMS: NavigationItem[] = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    href: '/dashboard',
    icon: 'LayoutDashboard',
    roles: ['Staff'],
  },
  {
    id: 'my_children',
    label: 'My Children',
    href: '/staff/children',
    icon: 'Users',
    roles: ['Staff'],
  },
  {
    id: 'attendance',
    label: 'Attendance',
    href: '/attendance',
    icon: 'ClipboardCheck',
    roles: ['Staff'],
  },
  {
    id: 'reports',
    label: 'Daily Reports',
    href: '/reports',
    icon: 'FileText',
    roles: ['Staff'],
  },
  {
    id: 'activities',
    label: 'Activities',
    href: '/activities',
    icon: 'Activity',
    roles: ['Staff'],
  },
  {
    id: 'feeding',
    label: 'Feeding',
    href: '/feeding',
    icon: 'Apple',
    roles: ['Staff'],
  },
  {
    id: 'nap',
    label: 'Nap',
    href: '/nap',
    icon: 'Moon',
    roles: ['Staff'],
  },
  {
    id: 'health',
    label: 'Health',
    href: '/health',
    icon: 'Heart',
    roles: ['Staff'],
  },
  {
    id: 'incidents',
    label: 'Incidents',
    href: '/incidents',
    icon: 'AlertTriangle',
    roles: ['Staff'],
  },
  {
    id: 'profile',
    label: 'Profile',
    href: '/profile',
    icon: 'User',
    roles: ['Staff'],
  },
]

// Parent Navigation
export const PARENT_NAV_ITEMS: NavigationItem[] = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    href: '/dashboard',
    icon: 'Home',
    roles: ['Parent'],
  },
  {
    id: 'my_children',
    label: 'My Children',
    href: '/parent/children',
    icon: 'Users',
    roles: ['Parent'],
  },
  {
    id: 'notifications',
    label: 'Notifications',
    href: '/notifications',
    icon: 'Bell',
    roles: ['Parent'],
  },
  {
    id: 'classroom',
    label: 'Classroom & Teacher',
    href: '/parent/classroom',
    icon: 'School',
    roles: ['Parent'],
  },
  {
    id: 'profile',
    label: 'Profile',
    href: '/profile',
    icon: 'User',
    roles: ['Parent'],
  },
]

/**
 * Role configurations
 */

export const ROLE_CONFIGS: Record<UserRole, RoleConfig> = {
  Admin: {
    role: 'Admin',
    displayName: 'Administrator',
    navigationItems: ADMIN_NAV_ITEMS,
    dashboardConfig: {
      title: 'Dashboard',
      greeting: 'Good morning',
      showStats: true,
      statFields: ['totalChildren', 'presentToday', 'absentToday', 'activeStaff'],
      showQuickActions: true,
      quickActions: [
        {
          id: 'add_child',
          label: 'Add Child',
          icon: 'Plus',
          href: '/children/create',
          variant: 'primary',
          roles: ['Admin'],
        },
        {
          id: 'add_staff',
          label: 'Add Staff',
          icon: 'Plus',
          href: '/staff/create',
          variant: 'primary',
          roles: ['Admin'],
        },
        {
          id: 'add_parent',
          label: 'Add Parent',
          icon: 'Plus',
          href: '/parents/create',
          variant: 'primary',
          roles: ['Admin'],
        },
        {
          id: 'record_attendance',
          label: 'Record Attendance',
          icon: 'CheckSquare',
          href: '/attendance',
          variant: 'secondary',
          roles: ['Admin'],
        },
        {
          id: 'view_incidents',
          label: 'View Incidents',
          icon: 'AlertTriangle',
          href: '/incidents',
          variant: 'secondary',
          roles: ['Admin'],
        },
      ],
      mainSections: [
        {
          id: 'today_attendance',
          title: "Today's Attendance",
          type: 'table',
          roles: ['Admin'],
          maxItems: 10,
        },
        {
          id: 'classroom_overview',
          title: 'Classroom Overview',
          type: 'cards',
          roles: ['Admin'],
        },
        {
          id: 'recent_incidents',
          title: 'Recent Incidents',
          type: 'incidents',
          roles: ['Admin'],
          maxItems: 5,
        },
        {
          id: 'recent_activity',
          title: 'Recent Activity',
          type: 'activity_feed',
          roles: ['Admin'],
          maxItems: 5,
        },
      ],
    },
    pagePermissions: {
      '/dashboard': true,
      '/children': true,
      '/parents': true,
      '/staff': true,
      '/classrooms': true,
      '/enrollments': true,
      '/attendance': true,
      '/reports': true,
      '/health': true,
      '/incidents': true,
    },
  },
  Staff: {
    role: 'Staff',
    displayName: 'Caregiver',
    navigationItems: STAFF_NAV_ITEMS,
    dashboardConfig: {
      title: 'My Classroom',
      greeting: 'Good morning',
      showStats: true,
      statFields: ['classroomName', 'childrenCount', 'presentCount', 'absentCount'],
      showQuickActions: true,
      quickActions: [
        {
          id: 'check_in_child',
          label: 'Check In Child',
          icon: 'LogIn',
          href: '/attendance',
          variant: 'primary',
          roles: ['Staff'],
        },
        {
          id: 'check_out_child',
          label: 'Check Out Child',
          icon: 'LogOut',
          href: '/attendance',
          variant: 'primary',
          roles: ['Staff'],
        },
        {
          id: 'daily_report',
          label: 'Daily Report',
          icon: 'FileText',
          href: '/reports/create',
          variant: 'secondary',
          roles: ['Staff'],
        },
        {
          id: 'record_feeding',
          label: 'Record Feeding',
          icon: 'Apple',
          href: '/feeding',
          variant: 'secondary',
          roles: ['Staff'],
        },
        {
          id: 'record_nap',
          label: 'Record Nap',
          icon: 'Moon',
          href: '/nap',
          variant: 'secondary',
          roles: ['Staff'],
        },
        {
          id: 'record_activity',
          label: 'Record Activity',
          icon: 'Activity',
          href: '/activities',
          variant: 'secondary',
          roles: ['Staff'],
        },
        {
          id: 'record_health',
          label: 'Record Health Event',
          icon: 'Heart',
          href: '/health',
          variant: 'secondary',
          roles: ['Staff'],
        },
        {
          id: 'report_incident',
          label: 'Report Incident',
          icon: 'AlertTriangle',
          href: '/incidents',
          variant: 'danger',
          roles: ['Staff'],
        },
      ],
      mainSections: [
        {
          id: 'my_children',
          title: "Today's Children",
          type: 'table',
          roles: ['Staff'],
          maxItems: 20,
        },
        {
          id: 'child_timeline',
          title: 'Child Timeline',
          type: 'timeline',
          roles: ['Staff'],
        },
        {
          id: 'recent_activity',
          title: 'Recent Activity',
          type: 'activity_feed',
          roles: ['Staff'],
          maxItems: 5,
        },
      ],
    },
    pagePermissions: {
      '/dashboard': true,
      '/staff/children': true,
      '/attendance': true,
      '/reports': true,
      '/activities': true,
      '/feeding': true,
      '/nap': true,
      '/health': true,
      '/incidents': true,
    },
  },
  Parent: {
    role: 'Parent',
    displayName: 'Guardian',
    navigationItems: PARENT_NAV_ITEMS,
    dashboardConfig: {
      title: 'My Child',
      greeting: 'Good morning',
      showStats: false,
      statFields: [],
      showQuickActions: false,
      quickActions: [],
      mainSections: [
        {
          id: 'child_status',
          title: 'Child Status',
          type: 'cards',
          roles: ['Parent'],
        },
        {
          id: 'daily_timeline',
          title: "Today's Timeline",
          type: 'timeline',
          roles: ['Parent'],
        },
        {
          id: 'daily_report',
          title: "Today's Report",
          type: 'cards',
          roles: ['Parent'],
        },
      ],
    },
    pagePermissions: {
      '/dashboard': true,
      '/parent/children': true,
      '/parent/today': true,
      '/parent/attendance': true,
      '/parent/reports': true,
      '/parent/meals': true,
      '/parent/nap': true,
      '/parent/activities': true,
      '/parent/health': true,
      '/parent/incidents': true,
    },
  },
}

/**
 * Helper function to get navigation items for a role
 */
export const getNavigationForRole = (role: UserRole): NavigationItem[] => {
  return ROLE_CONFIGS[role]?.navigationItems || []
}

/**
 * Helper function to get dashboard config for a role
 */
export const getDashboardConfigForRole = (role: UserRole): DashboardConfig => {
  return ROLE_CONFIGS[role]?.dashboardConfig || ROLE_CONFIGS.Admin.dashboardConfig
}

/**
 * Helper function to check if a role can access a page
 */
export const canAccessPage = (role: UserRole, path: string): boolean => {
  return ROLE_CONFIGS[role]?.pagePermissions[path] || false
}
