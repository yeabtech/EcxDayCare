import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  Activity,
  ArrowUpRight,
  Baby,
  CalendarDays,
  CheckCircle2,
  Clock3,
  DoorOpen,
  HeartPulse,
  LoaderCircle,
  RefreshCw,
  Users,
  UserRoundCheck,
  TriangleAlert,
} from 'lucide-react'
import { useAuth } from '@features/auth'
import { attendanceApi } from '@features/attendance/services/attendanceApi'
import type { AttendanceRecord, DailyAttendanceSummary } from '@features/attendance/types'
import { classroomsApi } from '@features/classrooms/services/classroomsApi'
import type { Classroom } from '@features/classrooms/types'
import { staffApi } from '@features/staff/services/staffApi'
import type { Staff } from '@features/staff/types'

type DashboardData = {
  staff: Staff
  classroom: Classroom | null
  attendance: DailyAttendanceSummary | null
}

function getGreeting(hour: number) {
  if (hour < 12) return 'Good morning'
  if (hour < 17) return 'Good afternoon'
  return 'Good evening'
}

function formatTime(value?: string | null) {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })
}

function statusPresentation(status: string) {
  const normalized = status.toLowerCase()
  if (normalized === 'present' || normalized === 'late') {
    return {
      label: normalized === 'late' ? 'Late' : 'Present',
      className: normalized === 'late' ? 'bg-amber-50 text-amber-800 ring-amber-200' : 'bg-emerald-50 text-emerald-800 ring-emerald-200',
      dotClassName: normalized === 'late' ? 'bg-amber-500' : 'bg-emerald-500',
    }
  }
  if (normalized === 'earlypickup') {
    return { label: 'Checked out', className: 'bg-slate-100 text-slate-700 ring-slate-200', dotClassName: 'bg-slate-400' }
  }
  return { label: 'Absent', className: 'bg-rose-50 text-rose-700 ring-rose-200', dotClassName: 'bg-rose-500' }
}

export const StaffDashboard = () => {
  const { user } = useAuth()
  const [data, setData] = useState<DashboardData | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  const loadDashboard = useCallback(async () => {
    if (!user?.StaffId) {
      setData(null)
      setError('This login is not linked to a staff record. Ask an administrator to link your account.')
      setIsLoading(false)
      return
    }

    setIsLoading(true)
    setError('')
    try {
      const staff = await staffApi.getStaffMember(user.StaffId)
      if (!staff.classroomId) {
        setData({ staff, classroom: null, attendance: null })
        return
      }

      const [classroom, attendance] = await Promise.all([
        classroomsApi.getClassroom(staff.classroomId),
        attendanceApi.getTodayAttendance({ classroomId: staff.classroomId }),
      ])
      setData({ staff, classroom, attendance })
    } catch (loadError) {
      setData(null)
      setError(
        loadError instanceof Error
          ? loadError.message
          : 'We could not load your classroom dashboard. Please try again.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [user?.StaffId])

  useEffect(() => {
    void loadDashboard()
  }, [loadDashboard])

  const attendanceItems = data?.attendance?.items ?? []
  const counts = useMemo(() => {
    const present = attendanceItems.filter((item) => {
      const status = item.status.toLowerCase()
      return status === 'present' || status === 'late'
    }).length
    const checkedIn = attendanceItems.filter((item) =>
      Boolean(item.checkInTime) && !item.checkOutTime,
    ).length
    const absent = attendanceItems.filter((item) => item.status.toLowerCase() === 'absent').length
    return { present, checkedIn, absent }
  }, [attendanceItems])

  const today = new Date()
  const childCount = data?.classroom?.currentEnrollment ?? 0
  const capacity = data?.classroom?.maxCapacity ?? 0
  const capacityPercent = capacity > 0 ? Math.min(100, Math.round((childCount / capacity) * 100)) : 0

  const stats = [
    {
      label: 'Children enrolled',
      value: childCount,
      detail: capacity ? `${Math.max(0, capacity - childCount)} places available` : 'Current class enrollment',
      icon: Baby,
      iconClass: 'bg-emerald-100 text-emerald-800',
    },
    {
      label: 'Present today',
      value: counts.present,
      detail: 'Marked present or late',
      icon: UserRoundCheck,
      iconClass: 'bg-sky-100 text-sky-800',
    },
    {
      label: 'Currently in care',
      value: counts.checkedIn,
      detail: 'Checked in, not checked out',
      icon: DoorOpen,
      iconClass: 'bg-violet-100 text-violet-800',
    },
    {
      label: 'Absent today',
      value: counts.absent,
      detail: 'Recorded absent for today',
      icon: CalendarDays,
      iconClass: 'bg-amber-100 text-amber-800',
    },
  ]

  return (
    <div className="brand-gradient-canvas -m-3 min-h-[calc(100vh-4rem)] p-3 sm:-m-4 sm:p-4 lg:-m-6 lg:p-7">
      <div className="mx-auto max-w-7xl space-y-6 sm:space-y-8">
        <section className="relative overflow-hidden rounded-[2rem] border border-white/70 bg-white/75 px-5 py-7 shadow-sm backdrop-blur sm:px-8 sm:py-9 lg:px-10">
          <div className="pointer-events-none absolute -right-12 -top-20 h-64 w-64 rounded-full bg-[color:var(--app-primary-soft)]/80 blur-3xl" />
          <div className="pointer-events-none absolute -bottom-24 right-1/4 h-48 w-48 rounded-full bg-[color:var(--app-accent)]/15 blur-3xl" />
          <div className="relative flex flex-col justify-between gap-6 md:flex-row md:items-end">
            <div className="max-w-2xl">
              <div className="mb-4 inline-flex items-center gap-2 rounded-full border border-[color:var(--app-line)] bg-white/80 px-3 py-1.5 text-xs font-semibold uppercase tracking-[0.16em] text-[color:var(--app-primary-deep)]">
                <Activity className="h-3.5 w-3.5" /> Staff dashboard
              </div>
              <h1 className="text-3xl font-bold tracking-tight text-gray-950 sm:text-4xl">
                {getGreeting(today.getHours())}{user?.FirstName ? `, ${user.FirstName}` : ''}
              </h1>
              <p className="mt-2 text-sm text-gray-600 sm:text-base">
                Your classroom at a glance for{' '}
                {today.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}.
              </p>
              <div className="mt-6 flex flex-wrap items-center gap-3">
                <div className="flex items-center gap-2 rounded-xl border border-white bg-white/85 px-3.5 py-2.5 shadow-sm">
                  <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-[color:var(--app-primary-soft)] text-[color:var(--app-primary)]">
                    <Users className="h-4 w-4" />
                  </span>
                  <span>
                    <span className="block text-[11px] font-semibold uppercase tracking-wide text-gray-500">Assigned classroom</span>
                    <span className="block text-sm font-semibold text-gray-900">
                      {isLoading ? 'Loading classroom…' : data?.classroom?.name ?? 'No classroom assigned'}
                    </span>
                  </span>
                </div>
                {data?.classroom && capacity > 0 && (
                  <div className="min-w-44 rounded-xl border border-white bg-white/85 px-3.5 py-2.5 shadow-sm">
                    <div className="mb-1.5 flex items-center justify-between gap-4 text-xs">
                      <span className="font-medium text-gray-600">Class capacity</span>
                      <span className="font-semibold text-gray-900">{childCount} / {capacity}</span>
                    </div>
                    <div className="h-1.5 overflow-hidden rounded-full bg-gray-100">
                      <div className="h-full rounded-full bg-[color:var(--app-primary)] transition-all" style={{ width: `${capacityPercent}%` }} />
                    </div>
                  </div>
                )}
              </div>
            </div>
            <button
              type="button"
              onClick={() => void loadDashboard()}
              disabled={isLoading}
              className="inline-flex shrink-0 items-center justify-center gap-2 self-start rounded-xl border border-[color:var(--app-line)] bg-white/90 px-4 py-2.5 text-sm font-semibold text-gray-700 shadow-sm transition hover:bg-white disabled:cursor-not-allowed disabled:opacity-60 md:self-auto"
            >
              <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
              Refresh
            </button>
          </div>
        </section>

        {error && (
          <div role="alert" className="flex items-start gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3.5 text-sm text-rose-800">
            <TriangleAlert className="mt-0.5 h-5 w-5 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        {isLoading && !data ? (
          <div className="flex min-h-56 items-center justify-center rounded-3xl border border-white/80 bg-white/75 text-sm font-medium text-gray-600 shadow-sm">
            <LoaderCircle className="mr-2 h-5 w-5 animate-spin text-[color:var(--app-primary)]" />
            Loading your classroom data…
          </div>
        ) : data && !data.classroom ? (
          <section className="rounded-3xl border border-white/80 bg-white/80 p-8 text-center shadow-sm sm:p-12">
            <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-amber-50 text-amber-700">
              <Users className="h-7 w-7" />
            </div>
            <h2 className="mt-4 text-xl font-bold text-gray-900">No classroom assigned yet</h2>
            <p className="mx-auto mt-2 max-w-md text-sm leading-6 text-gray-600">
              Your staff account is connected, but it has not been assigned to a classroom. Ask an administrator to update your staff record.
            </p>
          </section>
        ) : data?.classroom ? (
          <>
            <section className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
              {stats.map(({ label, value, detail, icon: Icon, iconClass }) => (
                <article key={label} className="rounded-2xl border border-white/80 bg-white/85 p-5 shadow-sm backdrop-blur transition hover:-translate-y-0.5 hover:shadow-md sm:p-6">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="text-sm font-medium text-gray-600">{label}</p>
                      <p className="mt-3 text-3xl font-bold tracking-tight text-gray-950">{isLoading ? '—' : value}</p>
                    </div>
                    <span className={`flex h-11 w-11 items-center justify-center rounded-xl ${iconClass}`}>
                      <Icon className="h-5 w-5" />
                    </span>
                  </div>
                  <p className="mt-3 text-xs text-gray-500">{detail}</p>
                </article>
              ))}
            </section>

            <div className="grid grid-cols-1 gap-6 xl:grid-cols-[minmax(0,1.7fr)_minmax(280px,0.8fr)]">
              <section className="overflow-hidden rounded-3xl border border-white/80 bg-white/90 shadow-sm backdrop-blur">
                <div className="flex flex-col gap-3 border-b border-gray-100 px-5 py-5 sm:flex-row sm:items-center sm:justify-between sm:px-6">
                  <div>
                    <h2 className="text-lg font-bold text-gray-950">Today’s classroom attendance</h2>
                    <p className="mt-1 text-sm text-gray-500">Attendance activity for {data.classroom.name}</p>
                  </div>
                  <Link to="/attendance" className="inline-flex items-center gap-1.5 self-start text-sm font-semibold text-[color:var(--app-primary)] hover:underline sm:self-auto">
                    Open attendance <ArrowUpRight className="h-4 w-4" />
                  </Link>
                </div>
                {isLoading ? (
                  <div className="flex min-h-48 items-center justify-center text-sm text-gray-500"><LoaderCircle className="mr-2 h-4 w-4 animate-spin" />Updating attendance…</div>
                ) : attendanceItems.length ? (
                  <div className="divide-y divide-gray-100">
                    {attendanceItems.map((record: AttendanceRecord) => {
                      const status = statusPresentation(record.status)
                      return (
                        <div key={record.id || record.childId} className="flex flex-col gap-3 px-5 py-4 sm:flex-row sm:items-center sm:justify-between sm:px-6">
                          <div className="flex min-w-0 items-center gap-3">
                            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-[color:var(--app-primary-soft)] text-[color:var(--app-primary)]"><Baby className="h-5 w-5" /></span>
                            <div className="min-w-0">
                              <p className="truncate text-sm font-semibold text-gray-900">{record.childName || 'Child'}</p>
                              <p className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-gray-500">
                                <span className="inline-flex items-center gap-1"><Clock3 className="h-3.5 w-3.5" /> In {formatTime(record.checkInTime)}</span>
                                {record.checkOutTime && <span className="inline-flex items-center gap-1"><CheckCircle2 className="h-3.5 w-3.5" /> Out {formatTime(record.checkOutTime)}</span>}
                              </p>
                            </div>
                          </div>
                          <span className={`inline-flex w-fit items-center gap-2 rounded-full px-3 py-1.5 text-xs font-semibold ring-1 ring-inset ${status.className}`}>
                            <span className={`h-1.5 w-1.5 rounded-full ${status.dotClassName}`} />{status.label}
                          </span>
                        </div>
                      )
                    })}
                  </div>
                ) : (
                  <div className="flex min-h-52 flex-col items-center justify-center px-6 py-10 text-center">
                    <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-gray-100 text-gray-500"><CalendarDays className="h-6 w-6" /></span>
                    <h3 className="mt-3 text-sm font-semibold text-gray-900">No attendance recorded today</h3>
                    <p className="mt-1 max-w-sm text-sm text-gray-500">Once children are checked in or marked absent, their classroom attendance will appear here.</p>
                  </div>
                )}
              </section>

              <aside className="space-y-5">
                <section className="rounded-3xl border border-white/80 bg-white/90 p-5 shadow-sm backdrop-blur sm:p-6">
                  <h2 className="text-base font-bold text-gray-950">Quick actions</h2>
                  <p className="mt-1 text-sm text-gray-500">Open a staff tool</p>
                  <div className="mt-5 grid gap-2.5">
                    <Link to="/attendance" className="flex items-center justify-between rounded-xl bg-[color:var(--app-primary)] px-4 py-3.5 text-sm font-semibold text-white shadow-sm transition hover:brightness-95">
                      <span className="inline-flex items-center gap-2.5"><CheckCircle2 className="h-4 w-4" /> Manage attendance</span><ArrowUpRight className="h-4 w-4" />
                    </Link>
                    <Link to="/reports/create" className="flex items-center justify-between rounded-xl border border-gray-200 bg-white px-4 py-3.5 text-sm font-semibold text-gray-700 transition hover:border-gray-300 hover:bg-gray-50">
                      <span className="inline-flex items-center gap-2.5"><CalendarDays className="h-4 w-4 text-[color:var(--app-primary)]" /> Create daily report</span><ArrowUpRight className="h-4 w-4" />
                    </Link>
                    <Link to="/health" className="flex items-center justify-between rounded-xl border border-gray-200 bg-white px-4 py-3.5 text-sm font-semibold text-gray-700 transition hover:border-gray-300 hover:bg-gray-50">
                      <span className="inline-flex items-center gap-2.5"><HeartPulse className="h-4 w-4 text-rose-600" /> Report a health event</span><ArrowUpRight className="h-4 w-4" />
                    </Link>
                    <Link to="/incidents" className="flex items-center justify-between rounded-xl border border-gray-200 bg-white px-4 py-3.5 text-sm font-semibold text-gray-700 transition hover:border-gray-300 hover:bg-gray-50">
                      <span className="inline-flex items-center gap-2.5"><TriangleAlert className="h-4 w-4 text-amber-600" /> Report an incident</span><ArrowUpRight className="h-4 w-4" />
                    </Link>
                  </div>
                </section>
                <section className="rounded-3xl border border-[color:var(--app-line)] bg-white/70 p-5 shadow-sm sm:p-6">
                  <div className="flex items-start gap-3">
                    <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-[color:var(--app-primary-soft)] text-[color:var(--app-primary)]"><Users className="h-5 w-5" /></span>
                    <div>
                      <h2 className="text-sm font-bold text-gray-900">Classroom team</h2>
                      <p className="mt-1 text-sm text-gray-600">{data.staff.firstName} {data.staff.lastName}</p>
                      <p className="mt-0.5 text-xs text-gray-500">{data.staff.role}{data.staff.shiftStart && data.staff.shiftEnd ? ` · ${data.staff.shiftStart}–${data.staff.shiftEnd}` : ''}</p>
                    </div>
                  </div>
                </section>
              </aside>
            </div>
          </>
        ) : null}
      </div>
    </div>
  )
}
