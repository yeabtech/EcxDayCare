/**
 * Attendance Dashboard — daily attendance management with check-in/out controls
 */
import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  ClipboardCheck,
  UserCheck,
  UserX,
  LogIn,
  LogOut,
  AlertCircle,
  Clock,
  Calendar,
  Search,
  RefreshCw,
  TrendingUp,
  Users,
  ChevronRight,
  School,
} from 'lucide-react'
import { useAttendance } from '../hooks/useAttendance'
import type { AttendanceRecord } from '../types'
import { CheckInModal } from '../components/CheckInModal'
import { CheckOutModal } from '../components/CheckOutModal'
import { MarkAbsentModal } from '../components/MarkAbsentModal'

const STATUS_CONFIG: Record<
  string,
  { label: string; className: string; dotClass: string }
> = {
  Present: {
    label: 'Present',
    className: 'bg-emerald-100 text-emerald-800',
    dotClass: 'bg-emerald-500',
  },
  Late: {
    label: 'Late',
    className: 'bg-amber-100 text-amber-800',
    dotClass: 'bg-amber-500',
  },
  Absent: {
    label: 'Absent',
    className: 'bg-red-100 text-red-700',
    dotClass: 'bg-red-500',
  },
  EarlyPickup: {
    label: 'Early Pickup',
    className: 'bg-purple-100 text-purple-800',
    dotClass: 'bg-purple-500',
  },
}

function formatTime(dt: string | null | undefined): string {
  if (!dt) return '—'
  return new Date(dt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

function formatDate(dt: string): string {
  return new Date(dt).toLocaleDateString([], { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
}

function getInitials(name: string | null | undefined): string {
  if (!name) return '?'
  return name
    .split(' ')
    .map((p) => p[0])
    .join('')
    .substring(0, 2)
    .toUpperCase()
}

const AVATAR_COLORS = [
  'bg-indigo-100 text-indigo-700',
  'bg-violet-100 text-violet-700',
  'bg-teal-100 text-teal-700',
  'bg-rose-100 text-rose-700',
  'bg-sky-100 text-sky-700',
]

function avatarColor(name: string | null | undefined): string {
  if (!name) return AVATAR_COLORS[0]
  const code = name.charCodeAt(0) % AVATAR_COLORS.length
  return AVATAR_COLORS[code]
}

interface StatCardProps {
  icon: React.ReactNode
  label: string
  value: number
  total?: number
  color: string
  bgColor: string
}

const StatCard = ({ icon, label, value, total, color, bgColor }: StatCardProps) => (
  <div className="bg-white rounded-2xl p-5 shadow-sm border border-gray-100 flex items-center gap-4">
    <div className={`${bgColor} p-3 rounded-xl flex-shrink-0`}>
      <div className={color}>{icon}</div>
    </div>
    <div className="min-w-0">
      <div className="text-2xl font-bold text-gray-900">
        {value}
        {total !== undefined && (
          <span className="text-base font-medium text-gray-400 ml-1">/ {total}</span>
        )}
      </div>
      <div className="text-sm text-gray-500 truncate">{label}</div>
    </div>
  </div>
)

export const AttendanceDashboardPage = () => {
  const navigate = useNavigate()
  const { summary, loading, actionLoading, error, fetchTodayAttendance, checkOutById, clearError } =
    useAttendance()

  const [selectedDate, setSelectedDate] = useState<string>(
    new Date().toISOString().split('T')[0],
  )
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [searchQuery, setSearchQuery] = useState<string>('')
  const [checkInOpen, setCheckInOpen] = useState(false)
  const [checkOutOpen, setCheckOutOpen] = useState(false)
  const [absentOpen, setAbsentOpen] = useState(false)
  const [selectedRecord, setSelectedRecord] = useState<AttendanceRecord | null>(null)

  const load = useCallback(() => {
    fetchTodayAttendance({
      date: selectedDate,
      status: statusFilter || undefined,
    })
  }, [fetchTodayAttendance, selectedDate, statusFilter])

  useEffect(() => {
    load()
  }, [load])

  const handleCheckOutClick = (record: AttendanceRecord) => {
    setSelectedRecord(record)
    setCheckOutOpen(true)
  }

  const handleCheckOutConfirm = async (pickedUpBy: string, isEarlyPickup: boolean) => {
    if (!selectedRecord) return
    await checkOutById(selectedRecord.id, {
      childId: selectedRecord.childId,
      pickedUpBy: pickedUpBy || null,
      isEarlyPickup,
    })
    setCheckOutOpen(false)
    setSelectedRecord(null)
    load()
  }

  // Filter rows locally by search
  const filteredItems = (summary?.items ?? []).filter((r) => {
    if (!searchQuery) return true
    const q = searchQuery.toLowerCase()
    return (r.childName ?? '').toLowerCase().includes(q)
  })

  const classroomGroups = Array.from(
    filteredItems.reduce((groups, record) => {
      const classroom = record.classroomName?.trim() || 'Unassigned classroom'
      groups.set(classroom, [...(groups.get(classroom) ?? []), record])
      return groups
    }, new Map<string, AttendanceRecord[]>()).entries(),
  ).sort(([a], [b]) => a.localeCompare(b))

  const today = summary?.date ? formatDate(summary.date) : formatDate(new Date().toISOString())

  return (
    <div className="space-y-6">
      {/* ── Header ── */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Attendance</h1>
          <div className="flex items-center gap-1.5 text-sm text-gray-500 mt-1">
            <Calendar size={14} />
            <span>{today}</span>
          </div>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          <input
            id="attendance-date-picker"
            type="date"
            value={selectedDate}
            onChange={(e) => setSelectedDate(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <button
            id="btn-refresh-attendance"
            onClick={load}
            disabled={loading}
            className="p-2 border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50 disabled:opacity-50"
            title="Refresh"
          >
            <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
          </button>
          <button
            id="btn-mark-absent"
            onClick={() => setAbsentOpen(true)}
            className="inline-flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            <UserX size={16} />
            Mark Absent
          </button>
          <button
            id="btn-check-in"
            onClick={() => setCheckInOpen(true)}
            className="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-semibold text-white bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 shadow-md shadow-blue-600/20 transition-all hover:-translate-y-0.5"
          >
            <LogIn size={16} />
            Check In
          </button>
        </div>
      </div>

      {/* ── Error Banner ── */}
      {error && (
        <div className="flex items-center gap-3 p-4 bg-red-50 border border-red-200 text-red-700 rounded-xl text-sm">
          <AlertCircle size={16} className="flex-shrink-0" />
          <span>{error}</span>
          <button onClick={clearError} className="ml-auto text-red-500 hover:text-red-700">
            ✕
          </button>
        </div>
      )}

      {/* ── Stat Cards ── */}
      {summary && (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard
            icon={<Users size={20} />}
            label="Checked In Now"
            value={summary.totalCurrentlyCheckedIn}
            total={summary.totalEnrolled}
            color="text-blue-600"
            bgColor="bg-blue-50"
          />
          <StatCard
            icon={<UserCheck size={20} />}
            label="Present Today"
            value={summary.totalPresent}
            color="text-emerald-600"
            bgColor="bg-emerald-50"
          />
          <StatCard
            icon={<Clock size={20} />}
            label="Late Arrivals"
            value={summary.totalLate}
            color="text-amber-600"
            bgColor="bg-amber-50"
          />
          <StatCard
            icon={<UserX size={20} />}
            label="Absent"
            value={summary.totalAbsent}
            color="text-red-600"
            bgColor="bg-red-50"
          />
        </div>
      )}

      {/* ── Attendance Rate Bar ── */}
      {summary && summary.totalEnrolled > 0 && (
        <div className="bg-white rounded-2xl p-5 shadow-sm border border-gray-100">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center gap-2 text-sm font-medium text-gray-700">
              <TrendingUp size={16} className="text-blue-500" />
              Daily Attendance Rate
            </div>
            <span className="text-sm font-bold text-gray-900">
              {summary.totalEnrolled > 0
                ? Math.round((summary.totalPresent / summary.totalEnrolled) * 100)
                : 0}
              %
            </span>
          </div>
          <div className="h-2.5 bg-gray-100 rounded-full overflow-hidden">
            <div
              className="h-full bg-gradient-to-r from-blue-500 to-emerald-500 rounded-full transition-all duration-700"
              style={{
                width: `${summary.totalEnrolled > 0 ? (summary.totalPresent / summary.totalEnrolled) * 100 : 0}%`,
              }}
            />
          </div>
          <div className="flex justify-between text-xs text-gray-400 mt-1.5">
            <span>{summary.totalPresent} present</span>
            <span>{summary.totalEnrolled} enrolled</span>
          </div>
        </div>
      )}

      {/* ── Filter Bar ── */}
      <div className="bg-white rounded-xl p-4 shadow-sm border border-gray-100 flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            id="attendance-search"
            type="text"
            placeholder="Search by child name…"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9 pr-3 py-2 w-full border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <select
          id="attendance-status-filter"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">All Statuses</option>
          <option value="Present">Present</option>
          <option value="Late">Late</option>
          <option value="Absent">Absent</option>
          <option value="EarlyPickup">Early Pickup</option>
        </select>
        <div className="flex items-center gap-1.5 text-xs text-gray-400 self-center whitespace-nowrap">
          <ClipboardCheck size={13} />
          <span>{filteredItems.length} record{filteredItems.length !== 1 ? 's' : ''}</span>
        </div>
      </div>

      {/* ── Table ── */}
      <div className="space-y-5">
        {loading ? (
          <div className="bg-white rounded-2xl border border-gray-100 p-12 text-center">
            <RefreshCw size={28} className="animate-spin text-blue-400 mx-auto mb-3" />
            <p className="text-sm text-gray-500">Loading attendance…</p>
          </div>
        ) : classroomGroups.length === 0 ? (
          <div className="bg-white rounded-2xl border border-gray-100 p-12 text-center">
            <ClipboardCheck size={40} className="mx-auto text-gray-300 mb-3" />
            <p className="text-base font-medium text-gray-600">No attendance records</p>
            <p className="text-sm text-gray-400 mt-1">
              Check in a child or try a different date / filter.
            </p>
          </div>
        ) : classroomGroups.map(([classroomName, records]) => {
          const presentCount = records.filter((record) => record.status === 'Present' || record.status === 'Late').length
          const checkedInCount = records.filter((record) => record.checkInTime && !record.checkOutTime).length
          return (
          <section key={classroomName} className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="flex flex-wrap items-center justify-between gap-3 px-5 py-4 bg-gradient-to-r from-slate-50 to-white border-b border-gray-100">
            <div className="flex items-center gap-3">
              <div className="rounded-xl bg-indigo-50 p-2.5 text-indigo-600"><School size={18} /></div>
              <div><h2 className="font-semibold text-gray-900">{classroomName}</h2><p className="text-xs text-gray-500">{records.length} {records.length === 1 ? 'child' : 'children'} · {presentCount} present</p></div>
            </div>
            <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-50 px-3 py-1.5 text-xs font-semibold text-emerald-700"><span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />{checkedInCount} checked in now</span>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-100">
              <thead className="bg-gray-50/80">
                <tr>
                  {['Child', 'Status', 'Check In', 'Check Out', 'Dropped/Picked', 'Notes', 'Actions'].map(
                    (h) => (
                      <th
                        key={h}
                        className={`px-5 py-3.5 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider ${h === 'Actions' ? 'text-right' : ''}`}
                      >
                        {h}
                      </th>
                    ),
                  )}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {records.map((record) => {
                  const cfg = STATUS_CONFIG[record.status] ?? {
                    label: record.status,
                    className: 'bg-gray-100 text-gray-600',
                    dotClass: 'bg-gray-400',
                  }
                  const isCheckedIn = !!record.checkInTime && !record.checkOutTime

                  return (
                    <tr key={record.id} className="hover:bg-gray-50/60 transition-colors">
                      {/* Child */}
                      <td className="px-5 py-4 whitespace-nowrap">
                        <div className="flex items-center gap-3">
                          <div
                            className={`h-9 w-9 rounded-full flex items-center justify-center text-xs font-bold flex-shrink-0 ${avatarColor(record.childName)}`}
                          >
                            {getInitials(record.childName)}
                          </div>
                          <div>
                            <div className="text-sm font-semibold text-gray-900">
                              {record.childName ?? (
                                <span className="text-gray-400 italic">Unknown</span>
                              )}
                            </div>
                            {record.classroomName && (
                              <div className="text-xs text-gray-400">{record.classroomName}</div>
                            )}
                          </div>
                        </div>
                      </td>

                      {/* Status */}
                      <td className="px-5 py-4 whitespace-nowrap">
                        <span
                          className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ${cfg.className}`}
                        >
                          <span className={`h-1.5 w-1.5 rounded-full ${cfg.dotClass}`} />
                          {cfg.label}
                        </span>
                      </td>

                      {/* Check In */}
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-gray-700">
                        <div className="flex items-center gap-1.5">
                          <LogIn size={13} className="text-emerald-400" />
                          {formatTime(record.checkInTime)}
                        </div>
                      </td>

                      {/* Check Out */}
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-gray-700">
                        <div className="flex items-center gap-1.5">
                          <LogOut size={13} className="text-blue-400" />
                          {formatTime(record.checkOutTime)}
                        </div>
                      </td>

                      {/* Dropped / Picked */}
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-gray-500">
                        <div className="text-xs">
                          {record.droppedOffBy && (
                            <div>↓ {record.droppedOffBy}</div>
                          )}
                          {record.pickedUpBy && (
                            <div className="text-gray-400">↑ {record.pickedUpBy}</div>
                          )}
                          {!record.droppedOffBy && !record.pickedUpBy && (
                            <span className="text-gray-300">—</span>
                          )}
                        </div>
                      </td>

                      {/* Notes */}
                      <td className="px-5 py-4 max-w-[160px]">
                        <p className="text-xs text-gray-500 truncate" title={record.notes ?? ''}>
                          {record.notes || <span className="text-gray-300">—</span>}
                        </p>
                      </td>

                      {/* Actions */}
                      <td className="px-5 py-4 whitespace-nowrap text-right">
                        <div className="flex justify-end items-center gap-2">
                          {isCheckedIn && (
                            <button
                              id={`btn-checkout-${record.id}`}
                              onClick={() => handleCheckOutClick(record)}
                              disabled={actionLoading}
                              className="inline-flex items-center gap-1.5 px-3 py-2 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-xl shadow-sm disabled:opacity-50 transition-colors"
                            >
                              <LogOut size={12} />
                              Check Out
                            </button>
                          )}
                          <button
                            id={`btn-history-${record.childId}`}
                            onClick={() => navigate(`/attendance/child/${record.childId}`)}
                            className="p-1.5 text-gray-400 hover:text-blue-600 rounded-lg hover:bg-blue-50 transition-colors"
                            title="View History"
                          >
                            <ChevronRight size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
          </section>
          )
        })}
      </div>

      {/* ── Modals ── */}
      {checkInOpen && (
        <CheckInModal
          onClose={() => setCheckInOpen(false)}
          onSuccess={() => {
            setCheckInOpen(false)
            load()
          }}
        />
      )}
      {checkOutOpen && selectedRecord && (
        <CheckOutModal
          record={selectedRecord}
          onClose={() => {
            setCheckOutOpen(false)
            setSelectedRecord(null)
          }}
          onConfirm={handleCheckOutConfirm}
          loading={actionLoading}
        />
      )}
      {absentOpen && (
        <MarkAbsentModal
          onClose={() => setAbsentOpen(false)}
          onSuccess={() => {
            setAbsentOpen(false)
            load()
          }}
        />
      )}
    </div>
  )
}
