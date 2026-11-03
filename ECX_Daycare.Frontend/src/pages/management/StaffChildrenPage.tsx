import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  AlertCircle,
  ArrowUpRight,
  Baby,
  CalendarDays,
  CheckCircle2,
  ClipboardCheck,
  Clock3,
  HeartPulse,
  LoaderCircle,
  Pencil,
  RefreshCw,
  Search,
  Users,
} from 'lucide-react'
import { useAuth } from '@features/auth'
import { attendanceApi } from '@features/attendance/services/attendanceApi'
import type { AttendanceRecord } from '@features/attendance/types'
import { classroomsApi } from '@features/classrooms/services/classroomsApi'
import type { Classroom } from '@features/classrooms/types'
import { staffApi } from '@features/staff/services/staffApi'
import type { Staff } from '@features/staff/types'
import type { Child } from '@features/children/types'

type StaffChildrenData = {
  staff: Staff
  classroom: Classroom
  children: Child[]
  attendance: AttendanceRecord[]
}

function fullName(child: Child) {
  return `${child.FirstName} ${child.LastName}`.trim()
}

function getAgeInMonths(dateOfBirth: string) {
  const birth = new Date(dateOfBirth)
  if (Number.isNaN(birth.getTime())) return null
  const now = new Date()
  return Math.max(0, (now.getFullYear() - birth.getFullYear()) * 12 + now.getMonth() - birth.getMonth() - (now.getDate() < birth.getDate() ? 1 : 0))
}

function ageLabel(dateOfBirth: string) {
  const months = getAgeInMonths(dateOfBirth)
  if (months === null) return 'Age unavailable'
  if (months < 24) return `${months} months`
  const years = Math.floor(months / 12)
  const remainingMonths = months % 12
  return remainingMonths ? `${years} yr ${remainingMonths} mo` : `${years} ${years === 1 ? 'year' : 'years'}`
}

function statusFor(child: Child, attendanceByChild: Map<string, AttendanceRecord>) {
  const record = attendanceByChild.get(child.Id)
  if (!record) return { label: 'Not checked in', classes: 'bg-gray-100 text-gray-700', isPresent: false }
  const status = record.status.toLowerCase()
  if (status === 'absent') return { label: 'Absent', classes: 'bg-rose-50 text-rose-700', isPresent: false }
  if (record.checkOutTime) return { label: 'Checked out', classes: 'bg-slate-100 text-slate-700', isPresent: false }
  if (status === 'late') return { label: 'Present · late', classes: 'bg-amber-50 text-amber-800', isPresent: true }
  return { label: 'Present', classes: 'bg-emerald-50 text-emerald-800', isPresent: true }
}

function timeLabel(value?: string | null) {
  if (!value) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? value : date.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })
}

export const StaffChildrenPage = () => {
  const { user } = useAuth()
  const [data, setData] = useState<StaffChildrenData | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')

  const loadChildren = useCallback(async () => {
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
        setData(null)
        setError('Your staff record is not assigned to a classroom yet. Ask an administrator to assign one.')
        return
      }

      const [classroom, children, today] = await Promise.all([
        classroomsApi.getClassroom(staff.classroomId),
        classroomsApi.getClassroomChildren(staff.classroomId),
        attendanceApi.getTodayAttendance({ classroomId: staff.classroomId }),
      ])
      setData({ staff, classroom, children, attendance: today.items })
    } catch (loadError) {
      setData(null)
      setError(loadError instanceof Error ? loadError.message : 'Could not load your classroom children. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }, [user?.StaffId])

  useEffect(() => {
    void loadChildren()
  }, [loadChildren])

  const attendanceByChild = useMemo(() => {
    const byChild = new Map<string, AttendanceRecord>()
    data?.attendance.forEach((record) => byChild.set(record.childId, record))
    return byChild
  }, [data?.attendance])

  const filteredChildren = useMemo(() => {
    const query = search.trim().toLocaleLowerCase()
    return (data?.children ?? []).filter((child) => {
      const matchesSearch = !query || fullName(child).toLocaleLowerCase().includes(query)
      if (!matchesSearch) return false
      if (statusFilter === 'all') return true
      const enrollmentStatus = child.Status.toLowerCase()
      return statusFilter === 'enrolled'
        ? enrollmentStatus === 'enrolled' || enrollmentStatus === 'active'
        : enrollmentStatus === statusFilter
    })
  }, [data?.children, search, statusFilter])

  const presentCount = data?.children.filter((child) => statusFor(child, attendanceByChild).isPresent).length ?? 0
  const careInfoCount = data?.children.filter((child) => Boolean(child.Allergies || child.MedicalNotes)).length ?? 0
  const enrolledCount = data?.children.filter((child) => ['enrolled', 'active'].includes(child.Status.toLowerCase())).length ?? 0

  return (
    <div className="brand-gradient-canvas -m-3 min-h-[calc(100vh-4rem)] p-3 sm:-m-4 sm:p-4 lg:-m-6 lg:p-7">
      <div className="mx-auto max-w-7xl space-y-6 sm:space-y-8">
        <section className="relative overflow-hidden rounded-[2rem] border border-white/70 bg-white/80 px-5 py-7 shadow-sm backdrop-blur sm:px-8 sm:py-9">
          <div className="pointer-events-none absolute -right-12 -top-20 h-64 w-64 rounded-full bg-[color:var(--app-primary-soft)]/80 blur-3xl" />
          <div className="pointer-events-none absolute -bottom-24 right-1/4 h-48 w-48 rounded-full bg-[color:var(--app-accent)]/15 blur-3xl" />
          <div className="relative flex flex-col justify-between gap-5 md:flex-row md:items-end">
            <div>
              <div className="mb-4 inline-flex items-center gap-2 rounded-full border border-[color:var(--app-line)] bg-white/85 px-3 py-1.5 text-xs font-semibold uppercase tracking-[0.16em] text-[color:var(--app-primary-deep)]">
                <Users className="h-3.5 w-3.5" /> Staff workspace
              </div>
              <h1 className="text-3xl font-bold tracking-tight text-gray-950 sm:text-4xl">My children</h1>
              <p className="mt-2 max-w-2xl text-sm text-gray-600 sm:text-base">
                The children assigned to your classroom, with today’s attendance and care notes at a glance.
              </p>
              {data && (
                <div className="mt-5 inline-flex flex-wrap items-center gap-x-2 gap-y-1 rounded-xl border border-white bg-white/85 px-3.5 py-2.5 text-sm shadow-sm">
                  <Baby className="h-4 w-4 text-[color:var(--app-primary)]" />
                  <span className="font-semibold text-gray-900">{data.classroom.name}</span>
                  <span className="text-gray-400">·</span>
                  <span className="text-gray-600">{data.staff.firstName} {data.staff.lastName}</span>
                </div>
              )}
            </div>
            <button
              type="button"
              onClick={() => void loadChildren()}
              disabled={isLoading}
              className="inline-flex shrink-0 items-center justify-center gap-2 self-start rounded-xl border border-[color:var(--app-line)] bg-white px-4 py-2.5 text-sm font-semibold text-gray-700 shadow-sm transition hover:bg-gray-50 disabled:opacity-60 md:self-auto"
            >
              <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} /> Refresh list
            </button>
          </div>
        </section>

        {error && (
          <div role="alert" className="flex items-start gap-3 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3.5 text-sm text-amber-900">
            <AlertCircle className="mt-0.5 h-5 w-5 shrink-0" /> <span>{error}</span>
          </div>
        )}

        {isLoading && !data ? (
          <div className="flex min-h-56 items-center justify-center rounded-3xl border border-white/80 bg-white/80 text-sm font-medium text-gray-600 shadow-sm">
            <LoaderCircle className="mr-2 h-5 w-5 animate-spin text-[color:var(--app-primary)]" /> Loading your classroom roster…
          </div>
        ) : data ? (
          <>
            <section className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
              {[
                { label: 'Children in class', value: data.children.length, note: `Capacity ${data.classroom.maxCapacity || '—'}`, icon: Baby, tone: 'bg-emerald-100 text-emerald-800' },
                { label: 'Enrolled', value: enrolledCount, note: 'Active classroom roster', icon: Users, tone: 'bg-sky-100 text-sky-800' },
                { label: 'Present today', value: presentCount, note: 'Checked in and still in care', icon: CheckCircle2, tone: 'bg-violet-100 text-violet-800' },
                { label: 'Care notes', value: careInfoCount, note: 'Allergies or medical notes recorded', icon: HeartPulse, tone: 'bg-rose-100 text-rose-800' },
              ].map(({ label, value, note, icon: Icon, tone }) => (
                <article key={label} className="rounded-2xl border border-white/80 bg-white/85 p-5 shadow-sm backdrop-blur transition hover:-translate-y-0.5 hover:shadow-md sm:p-6">
                  <div className="flex items-start justify-between gap-3">
                    <div><p className="text-sm font-medium text-gray-600">{label}</p><p className="mt-3 text-3xl font-bold tracking-tight text-gray-950">{value}</p></div>
                    <span className={`flex h-11 w-11 items-center justify-center rounded-xl ${tone}`}><Icon className="h-5 w-5" /></span>
                  </div>
                  <p className="mt-3 text-xs text-gray-500">{note}</p>
                </article>
              ))}
            </section>

            <section className="overflow-hidden rounded-3xl border border-white/80 bg-white/90 shadow-sm backdrop-blur">
              <div className="flex flex-col gap-4 border-b border-gray-100 px-5 py-5 sm:px-6 lg:flex-row lg:items-center lg:justify-between">
                <div>
                  <h2 className="text-lg font-bold text-gray-950">Classroom roster</h2>
                  <p className="mt-1 text-sm text-gray-500">{filteredChildren.length} of {data.children.length} children</p>
                </div>
                <div className="flex flex-col gap-2 sm:flex-row">
                  <label className="relative min-w-0 sm:min-w-64">
                    <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                    <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search children…" className="w-full rounded-xl border border-gray-200 bg-white py-2.5 pl-9 pr-3 text-sm text-gray-900 outline-none transition placeholder:text-gray-400 focus:border-[color:var(--app-primary)] focus:ring-2 focus:ring-[color:var(--app-primary-soft)]" />
                  </label>
                  <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)} aria-label="Filter children by enrollment status" className="rounded-xl border border-gray-200 bg-white px-3 py-2.5 text-sm text-gray-700 outline-none focus:border-[color:var(--app-primary)] focus:ring-2 focus:ring-[color:var(--app-primary-soft)]">
                    <option value="all">All enrollment statuses</option>
                    <option value="enrolled">Enrolled</option>
                    <option value="inactive">Inactive</option>
                    <option value="withdrawn">Withdrawn</option>
                  </select>
                </div>
              </div>

              {isLoading ? (
                <div className="flex min-h-48 items-center justify-center text-sm text-gray-500"><LoaderCircle className="mr-2 h-4 w-4 animate-spin" />Refreshing roster…</div>
              ) : filteredChildren.length ? (
                <div className="grid grid-cols-1 gap-3 p-4 sm:p-5 lg:grid-cols-2">
                  {filteredChildren.map((child) => {
                    const attendance = attendanceByChild.get(child.Id)
                    const attendanceStatus = statusFor(child, attendanceByChild)
                    const initials = `${child.FirstName.charAt(0)}${child.LastName.charAt(0)}`.toUpperCase()
                    const isEnrolled = ['enrolled', 'active'].includes(child.Status.toLowerCase())
                    return (
                      <article key={child.Id} className="group rounded-2xl border border-gray-100 bg-white p-4 transition hover:border-[color:var(--app-line)] hover:shadow-sm sm:p-5">
                        <div className="flex items-start gap-3">
                          {child.PhotoUrl ? (
                            <img src={child.PhotoUrl} alt={fullName(child)} className="h-12 w-12 shrink-0 rounded-2xl object-cover" />
                          ) : (
                            <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-[color:var(--app-primary-soft)] text-sm font-bold text-[color:var(--app-primary-deep)]">{initials}</span>
                          )}
                          <div className="min-w-0 flex-1">
                            <div className="flex flex-wrap items-start justify-between gap-2">
                              <div className="min-w-0"><h3 className="truncate font-bold text-gray-950">{fullName(child)}</h3><p className="mt-0.5 text-xs text-gray-500">{ageLabel(child.DateOfBirth)} <span className="px-1">·</span> {child.Gender}</p></div>
                              <span className={`inline-flex rounded-full px-2.5 py-1 text-[11px] font-semibold ${attendanceStatus.classes}`}>{attendanceStatus.label}</span>
                            </div>
                            <div className="mt-3 flex flex-wrap gap-x-4 gap-y-2 text-xs text-gray-500">
                              <span className="inline-flex items-center gap-1.5"><CalendarDays className="h-3.5 w-3.5" /> Enrolled {new Date(child.EnrollmentDate).toLocaleDateString()}</span>
                              {attendance?.checkInTime && (
                                <span className="inline-flex items-center gap-1.5"><Clock3 className="h-3.5 w-3.5" /> In {timeLabel(attendance.checkInTime)}</span>
                              )}
                            </div>
                            <div className="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-gray-100 pt-3">
                              <div className="flex flex-wrap items-center gap-2">
                                <span className={`rounded-full px-2.5 py-1 text-[11px] font-semibold ${isEnrolled ? 'bg-emerald-50 text-emerald-800' : 'bg-gray-100 text-gray-600'}`}>{child.Status}</span>
                                {(child.Allergies || child.MedicalNotes) && <span className="inline-flex items-center gap-1 rounded-full bg-rose-50 px-2.5 py-1 text-[11px] font-semibold text-rose-700"><HeartPulse className="h-3 w-3" /> Care info</span>}
                              </div>
                              <div className="flex items-center gap-1.5">
                                <Link to={`/children/${child.Id}`} className="inline-flex items-center gap-1 rounded-lg px-2.5 py-1.5 text-xs font-semibold text-gray-600 transition hover:bg-gray-100 hover:text-gray-900">Profile <ArrowUpRight className="h-3.5 w-3.5" /></Link>
                                <Link to={`/children/${child.Id}/edit`} className="inline-flex items-center gap-1 rounded-lg bg-[color:var(--app-primary-soft)] px-2.5 py-1.5 text-xs font-semibold text-[color:var(--app-primary-deep)] transition hover:brightness-95"><Pencil className="h-3 w-3" /> Edit</Link>
                              </div>
                            </div>
                          </div>
                        </div>
                      </article>
                    )
                  })}
                </div>
              ) : (
                <div className="flex min-h-56 flex-col items-center justify-center px-6 py-10 text-center">
                  <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-gray-100 text-gray-500"><ClipboardCheck className="h-6 w-6" /></span>
                  <h3 className="mt-3 text-sm font-semibold text-gray-900">{data.children.length ? 'No children match these filters' : 'No children assigned to this classroom'}</h3>
                  <p className="mt-1 max-w-sm text-sm text-gray-500">{data.children.length ? 'Try a different name or enrollment status.' : 'Children enrolled in your class will appear here.'}</p>
                </div>
              )}
            </section>
          </>
        ) : null}
      </div>
    </div>
  )
}
