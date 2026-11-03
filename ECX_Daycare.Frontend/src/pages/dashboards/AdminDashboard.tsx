/**
 * Admin Dashboard - Modernized with Design System
 * Overview of entire daycare operations
 */
import React, { useMemo } from 'react'
import { useAdminDashboard, type AttendanceDay } from '@features/dashboards/hooks/useAdminDashboard'
import { LoadingSpinner, ErrorAlert } from '@components/shared'
import {
  AttendanceTrendChart,
  ClassroomCapacityChart,
  IncidentSummaryChart,
} from '@components/dashboards'
import {
  AlertCircle,
  TrendingUp,
  RefreshCw,
  Plus,
  BarChart3,
  CheckCircle2,
  XCircle,
  UserCheck,
  Layers,
  Users,
} from 'lucide-react'
import { Link } from 'react-router-dom'

// Mock attendance data for the trend chart (this will be replaced with real API data)
const generateMockAttendanceTrend = (): AttendanceDay[] => {
  const days: AttendanceDay[] = []
  for (let i = 6; i >= 0; i--) {
    const date = new Date()
    date.setDate(date.getDate() - i)
    const total = Math.floor(Math.random() * 40) + 60
    const present = Math.floor(total * (0.8 + Math.random() * 0.15))
    days.push({
      date: date.toISOString().split('T')[0],
      presentCount: present,
      absentCount: total - present,
      totalCount: total,
      rate: Math.round((present / total) * 100),
    })
  }
  return days
}

export const AdminDashboard: React.FC = () => {
  const {
    totalChildren,
    presentToday,
    absentToday,
    activeStaff,
    totalClassrooms,
    attendanceRate,
    classroomUtilization,
    recentIncidents,
    isLoading,
    isRefreshing,
    error,
    refreshDashboard,
  } = useAdminDashboard()

  // Generate mock attendance trend data
  const mockAttendanceTrend = useMemo(() => generateMockAttendanceTrend(), [])

  return (
    <div className="space-y-8">
      {/* Error display */}
      {error && <ErrorAlert message={error} onDismiss={() => window.location.reload()} />}

      {/* Page header with refresh */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Admin Dashboard</h1>
          <p className="text-gray-600 mt-2 text-sm">Overview of daycare operations and key metrics</p>
        </div>
        <button
          onClick={refreshDashboard}
          disabled={isRefreshing}
          className="btn-primary-lg flex items-center gap-2 hover:shadow-md transition-all disabled:opacity-60"
        >
          <RefreshCw size={18} className={isRefreshing ? 'animate-spin' : ''} />
          {isRefreshing ? 'Refreshing...' : 'Refresh'}
        </button>
      </div>

      {/* Loading state */}
      {isLoading && <LoadingSpinner />}

      {!isLoading && (
        <>
{/* Quick Stats Grid */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
            <StatCard
              label="Total Children"
              value={totalChildren}
              icon={<Users size={24} />}
              color="blue"
              href="/children"
            />
            <StatCard
              label="Present Today"
              value={presentToday}
              icon={<CheckCircle2 size={24} />}
              color="success"
              href="/attendance"
            />
            <StatCard
              label="Absent Today"
              value={absentToday}
              icon={<XCircle size={24} />}
              color="danger"
              href="/attendance"
            />
            <StatCard
              label="Active Staff"
              value={activeStaff}
              icon={<UserCheck size={24} />}
              color="primary"
              href="/staff"
            />
            <StatCard
              label="Classrooms"
              value={totalClassrooms}
              icon={<Layers size={24} />}
              color="accent"
              href="/classrooms"
            />
          </div>

          {/* Attendance Rate Card - Featured */}
          <div className="bg-white rounded-lg border border-primary-200 shadow-sm overflow-hidden">
            <div className="bg-gradient-to-r from-primary-500 to-primary-600 p-6 text-white">
              <div className="flex items-start justify-between">
                <div>
                  <p className="text-primary-100 text-sm font-medium">Overall Attendance Rate</p>
                  <p className="text-4xl sm:text-5xl font-bold mt-3">{attendanceRate}%</p>
                  <p className="text-primary-200 text-xs mt-2">Last 30 days average</p>
                </div>
                <div className="h-20 w-20 bg-white/20 rounded-full flex items-center justify-center">
                  <TrendingUp size={32} />
                </div>
              </div>
            </div>
          </div>

          {/* Main content area */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Left column - full width on mobile, 2/3 on large screens */}
            <div className="lg:col-span-2 space-y-6">
              {/* Attendance Trend Chart */}
              <div className="card">
                <div className="card-header border-b border-gray-200">
                  <div className="flex items-center gap-2">
                    <BarChart3 size={20} className="text-primary-600" />
                    <h2 className="section-title text-base">Attendance Trend (Last 7 Days)</h2>
                  </div>
                </div>
                <div className="card-body">
                  <AttendanceTrendChart data={mockAttendanceTrend} isLoading={isLoading} title="" />
                </div>
              </div>

              {/* Classroom Utilization */}
              <div className="card">
                <div className="card-header border-b border-gray-200">
                  <div className="flex items-center gap-2">
                    <BarChart3 size={20} className="text-primary-600" />
                    <h2 className="section-title text-base">Classroom Capacity Distribution</h2>
                  </div>
                </div>
                <div className="card-body">
                  <ClassroomCapacityChart
                    data={classroomUtilization}
                    isLoading={isLoading}
                    title=""
                  />
                </div>
              </div>

              {/* Recent Incidents */}
              <div className="card">
                <div className="card-header border-b border-gray-200">
                  <div className="flex items-center gap-2">
                    <AlertCircle size={20} className="text-danger-600" />
                    <h2 className="section-title text-base">Incident Summary</h2>
                  </div>
                </div>
                <div className="card-body">
                  <IncidentSummaryChart data={recentIncidents} isLoading={isLoading} title="" />
                </div>
              </div>
            </div>

            {/* Right column - full width on mobile, 1/3 on large screens */}
            <div className="space-y-6">
              {/* Quick Actions */}
              <div className="card">
                <div className="card-header border-b border-gray-200">
                  <h2 className="section-title text-base">Quick Actions</h2>
                </div>
                <div className="card-body space-y-2">
                  <QuickActionButton href="/children/create" label="+ Add Child" />
                  <QuickActionButton href="/staff/create" label="+ Add Staff" />
                  <QuickActionButton href="/parents/create" label="+ Add Parent" />
                  <QuickActionButton href="/attendance" label="Record Attendance" />
                  <QuickActionButton href="/classrooms/create" label="+ New Classroom" />
                </div>
              </div>

              {/* Summary Cards */}
              <div className="card">
                <div className="card-header border-b border-gray-200">
                  <h2 className="section-title text-base">Metrics Summary</h2>
                </div>
                <div className="card-body space-y-4">
                  <SummaryItem
                    label="Enrollment Rate"
                    value={
                      totalClassrooms > 0
                        ? Math.round((totalChildren / (totalClassrooms * 20)) * 100)
                        : 0
                    }
                    unit="%"
                    variant="primary"
                  />
                  <SummaryItem
                    label="Staff-to-Child Ratio"
                    value={
                      totalChildren > 0
                        ? Math.round(totalChildren / Math.max(activeStaff, 1))
                        : 0
                    }
                    unit=":1"
                    variant="accent"
                  />
                  <SummaryItem
                    label="Total Enrollments"
                    value={Math.round((totalChildren / (totalClassrooms || 1)) * 100)}
                    unit="/class"
                    variant="success"
                  />
                </div>
              </div>

              {/* Quick Stats Summary */}
              <div className="card bg-gradient-to-br from-primary-50 to-accent-50 border border-primary-100">
                <div className="card-body">
                  <h3 className="text-sm font-semibold text-gray-900 mb-3">Today's Summary</h3>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Present</span>
                      <span className="font-semibold text-success-600">{presentToday}</span>
                    </div>
                    <div className="flex justify-between pb-2 border-b border-gray-200">
                      <span className="text-gray-600">Absent</span>
                      <span className="font-semibold text-danger-600">{absentToday}</span>
                    </div>
                    <div className="flex justify-between pt-2">
                      <span className="text-gray-600">Total</span>
                      <span className="font-semibold text-gray-900">{totalChildren}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  )
}

/**
 * Stat Card Component - Modern design with colors
 */
interface StatCardProps {
  label: string
  value: number
  icon: React.ReactNode
  color: 'blue' | 'success' | 'danger' | 'primary' | 'accent'
  href?: string
}

const StatCard: React.FC<StatCardProps> = ({ label, value, icon, color, href }) => {
  const colorClasses = {
    blue: 'stat-card-primary bg-gradient-to-br from-blue-50 to-blue-100/50 border-l-4 border-l-blue-500 text-blue-600',
    success:
      'stat-card-primary bg-gradient-to-br from-success-50 to-success-100/50 border-l-4 border-l-success-500 text-success-600',
    danger:
      'stat-card-primary bg-gradient-to-br from-danger-50 to-danger-100/50 border-l-4 border-l-danger-500 text-danger-600',
    primary:
      'stat-card-primary bg-gradient-to-br from-primary-50 to-primary-100/50 border-l-4 border-l-primary-500 text-primary-600',
    accent:
      'stat-card-primary bg-gradient-to-br from-accent-50 to-accent-100/50 border-l-4 border-l-accent-500 text-accent-600',
  }

  const content = (
    <div className={colorClasses[color]}>
      <div className="flex justify-between items-start">
        <div>
          <p className="text-sm font-medium text-gray-600">{label}</p>
          <p className="text-3xl sm:text-4xl font-bold text-gray-900 mt-3">{value}</p>
        </div>
        <div className="opacity-80">{icon}</div>
      </div>
    </div>
  )

  return href ? (
    <Link to={href} className="block hover:shadow-md transition-shadow">
      {content}
    </Link>
  ) : (
    content
  )
}

/**
 * Quick Action Button Component
 */
interface QuickActionButtonProps {
  href: string
  label: string
}

const QuickActionButton: React.FC<QuickActionButtonProps> = ({ href, label }) => (
  <Link
    to={href}
    className="w-full px-4 py-3 bg-primary-500 text-white text-sm font-medium rounded-lg hover:bg-primary-600 transition-colors text-center block flex items-center justify-center gap-2 font-button hover:shadow-md"
  >
    <Plus size={16} />
    {label}
  </Link>
)

/**
 * Summary Item Component
 */
interface SummaryItemProps {
  label: string
  value: number
  unit: string
  variant: 'primary' | 'accent' | 'success' | 'danger'
}

const SummaryItem: React.FC<SummaryItemProps> = ({ label, value, unit, variant }) => {
  const variantClasses = {
    primary: 'bg-primary-50 text-primary-700 border-primary-200',
    accent: 'bg-accent-50 text-accent-700 border-accent-200',
    success: 'bg-success-50 text-success-700 border-success-200',
    danger: 'bg-danger-50 text-danger-700 border-danger-200',
  }

  return (
    <div className={`p-3 rounded-lg border ${variantClasses[variant]}`}>
      <p className="text-xs font-medium opacity-75">{label}</p>
      <p className="text-2xl font-bold mt-1">
        {value}
        <span className="text-sm ml-1">{unit}</span>
      </p>
    </div>
  )
}
