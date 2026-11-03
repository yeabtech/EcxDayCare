/**
 * Child attendance history page — analytics and per-day log
 */
import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  ArrowLeft,
  CalendarDays,
  TrendingUp,
  UserCheck,
  Clock,
  UserX,
  LogIn,
  LogOut,
  RefreshCw,
  AlertCircle,
} from 'lucide-react'
import { useAttendance } from '../hooks/useAttendance'
import type { AttendanceRecord } from '../types'

const STATUS_CONFIG: Record<string, { label: string; className: string; dotClass: string }> = {
  Present: { label: 'Present', className: 'bg-emerald-100 text-emerald-800', dotClass: 'bg-emerald-500' },
  Late: { label: 'Late', className: 'bg-amber-100 text-amber-800', dotClass: 'bg-amber-500' },
  Absent: { label: 'Absent', className: 'bg-red-100 text-red-700', dotClass: 'bg-red-500' },
  EarlyPickup: { label: 'Early Pickup', className: 'bg-purple-100 text-purple-800', dotClass: 'bg-purple-500' },
}

function formatTime(dt: string | null | undefined) {
  if (!dt) return '—'
  return new Date(dt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

function formatShortDate(dt: string) {
  return new Date(dt).toLocaleDateString([], { month: 'short', day: 'numeric', year: 'numeric' })
}

interface MiniStatProps {
  label: string
  value: number | string
  icon: React.ReactNode
  color: string
}

const MiniStat = ({ label, value, icon, color }: MiniStatProps) => (
  <div className="bg-white rounded-xl p-4 border border-gray-100 shadow-sm flex items-center gap-3">
    <div className={`p-2.5 rounded-lg ${color}`}>{icon}</div>
    <div>
      <div className="text-xl font-bold text-gray-900">{value}</div>
      <div className="text-xs text-gray-500">{label}</div>
    </div>
  </div>
)

export const ChildAttendanceHistoryPage = () => {
  const { childId } = useParams<{ childId: string }>()
  const navigate = useNavigate()
  const { history, loading, error, fetchChildHistory, clearError } = useAttendance()

  const [startDate, setStartDate] = useState<string>('')
  const [endDate, setEndDate] = useState<string>('')

  const load = () => {
    if (!childId) return
    fetchChildHistory(childId, {
      startDate: startDate || undefined,
      endDate: endDate || undefined,
      page: 1,
      pageSize: 50,
    })
  }

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [childId])

  return (
    <div className="space-y-6">
      {/* ── Header ── */}
      <div className="flex items-center gap-3">
        <button
          onClick={() => navigate('/attendance')}
          className="p-2 rounded-lg border border-gray-200 text-gray-500 hover:bg-gray-100 transition-colors"
        >
          <ArrowLeft size={18} />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            {history?.childName ?? 'Child'} — Attendance History
          </h1>
          <p className="text-sm text-gray-500 mt-0.5">Full attendance log and analytics</p>
        </div>
      </div>

      {/* ── Error ── */}
      {error && (
        <div className="flex items-center gap-3 p-4 bg-red-50 border border-red-200 text-red-700 rounded-xl text-sm">
          <AlertCircle size={16} className="flex-shrink-0" />
          <span>{error}</span>
          <button onClick={clearError} className="ml-auto text-red-400 hover:text-red-600">✕</button>
        </div>
      )}

      {/* ── Date Range Filter ── */}
      <div className="bg-white rounded-xl p-4 shadow-sm border border-gray-100 flex flex-wrap items-end gap-4">
        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">From</label>
          <input
            id="history-start-date"
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
            className="px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">To</label>
          <input
            id="history-end-date"
            type="date"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
            className="px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <button
          id="btn-apply-history-filter"
          onClick={load}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 flex items-center gap-2"
        >
          <RefreshCw size={14} />
          Apply
        </button>
      </div>

      {/* ── Analytics ── */}
      {history && (
        <>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
            <MiniStat label="Total Days" value={history.totalDays} icon={<CalendarDays size={18} />} color="bg-blue-50 text-blue-600" />
            <MiniStat label="Present" value={history.presentDays} icon={<UserCheck size={18} />} color="bg-emerald-50 text-emerald-600" />
            <MiniStat label="Late" value={history.lateDays} icon={<Clock size={18} />} color="bg-amber-50 text-amber-600" />
            <MiniStat label="Absent" value={history.absentDays} icon={<UserX size={18} />} color="bg-red-50 text-red-600" />
            <MiniStat label="Early Pickup" value={history.earlyPickupDays} icon={<LogOut size={18} />} color="bg-purple-50 text-purple-600" />
            <MiniStat label="Attendance Rate" value={`${Math.round(history.attendanceRate)}%`} icon={<TrendingUp size={18} />} color="bg-indigo-50 text-indigo-600" />
          </div>

          {/* Rate bar */}
          <div className="bg-white rounded-xl p-4 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-2 text-sm font-medium text-gray-700">
              <span>Attendance Rate</span>
              <span className="font-bold text-gray-900">{Math.round(history.attendanceRate)}%</span>
            </div>
            <div className="h-3 bg-gray-100 rounded-full overflow-hidden">
              <div
                className="h-full rounded-full bg-gradient-to-r from-blue-500 to-emerald-500 transition-all duration-700"
                style={{ width: `${history.attendanceRate}%` }}
              />
            </div>
          </div>
        </>
      )}

      {/* ── Records Table ── */}
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        {loading ? (
          <div className="p-12 text-center">
            <RefreshCw size={28} className="animate-spin text-blue-400 mx-auto mb-3" />
            <p className="text-sm text-gray-500">Loading history…</p>
          </div>
        ) : !history || history.items.length === 0 ? (
          <div className="p-12 text-center">
            <CalendarDays size={40} className="mx-auto text-gray-300 mb-3" />
            <p className="text-base font-medium text-gray-600">No attendance records found</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-100">
              <thead className="bg-gray-50/80">
                <tr>
                  {['Date', 'Status', 'Check In', 'Check Out', 'Dropped By', 'Picked By', 'Notes'].map(
                    (h) => (
                      <th key={h} className="px-5 py-3.5 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">
                        {h}
                      </th>
                    ),
                  )}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {history.items.map((r: AttendanceRecord) => {
                  const cfg = STATUS_CONFIG[r.status] ?? {
                    label: r.status,
                    className: 'bg-gray-100 text-gray-600',
                    dotClass: 'bg-gray-400',
                  }
                  return (
                    <tr key={r.id} className="hover:bg-gray-50/60 transition-colors">
                      <td className="px-5 py-4 whitespace-nowrap text-sm font-medium text-gray-800">
                        {formatShortDate(r.date)}
                      </td>
                      <td className="px-5 py-4 whitespace-nowrap">
                        <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ${cfg.className}`}>
                          <span className={`h-1.5 w-1.5 rounded-full ${cfg.dotClass}`} />
                          {cfg.label}
                        </span>
                      </td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-gray-600">
                        <div className="flex items-center gap-1.5">
                          <LogIn size={13} className="text-emerald-400" />
                          {formatTime(r.checkInTime)}
                        </div>
                      </td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-gray-600">
                        <div className="flex items-center gap-1.5">
                          <LogOut size={13} className="text-blue-400" />
                          {formatTime(r.checkOutTime)}
                        </div>
                      </td>
                      <td className="px-5 py-4 text-sm text-gray-500">{r.droppedOffBy || <span className="text-gray-300">—</span>}</td>
                      <td className="px-5 py-4 text-sm text-gray-500">{r.pickedUpBy || <span className="text-gray-300">—</span>}</td>
                      <td className="px-5 py-4 max-w-[160px]">
                        <p className="text-xs text-gray-500 truncate" title={r.notes ?? ''}>
                          {r.notes || <span className="text-gray-300">—</span>}
                        </p>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
