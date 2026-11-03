/**
 * DailyReportsDashboardPage
 * Staff & Admin dashboard to view, create, edit, send, and manage daily reports
 */
import { useEffect, useState, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  FileText,
  Plus,
  Calendar,
  ChevronLeft,
  ChevronRight,
  Search,
  CheckCircle,
  Clock,
  Sparkles,
  Loader2,
  AlertTriangle,
  School,
  RefreshCw,
} from 'lucide-react'
import { useDailyReports } from '../hooks/useDailyReports'
import { ReportCard } from '../components/ReportCard'
import { DeleteConfirmation } from '@features/children/components/DeleteConfirmation'
import { useAuth } from '@features/auth'
import { staffApi } from '@features/staff/services/staffApi'
import { classroomsApi } from '@features/classrooms/services/classroomsApi'

const localDate = () => {
  const now = new Date()
  return new Date(now.getTime() - now.getTimezoneOffset() * 60_000).toISOString().slice(0, 10)
}

export const DailyReportsDashboardPage = () => {
  const navigate = useNavigate()
  const { user, isStaff } = useAuth()
  const todayStr = useMemo(localDate, [])
  const [selectedDate, setSelectedDate] = useState<string>(todayStr)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<'all' | 'sent' | 'draft'>('all')

  const {
    dailyFeed,
    loading,
    error,
    fetchDailyFeed,
    sendToParent,
    deleteReport,
    clearError,
  } = useDailyReports()

  const [deleteTargetId, setDeleteTargetId] = useState<string | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)
  const [sendingId, setSendingId] = useState<string | null>(null)
  const [classroomName, setClassroomName] = useState('')
  const [classroomChildIds, setClassroomChildIds] = useState<Set<string> | null>(null)
  const [classroomLoading, setClassroomLoading] = useState(isStaff)
  const [classroomError, setClassroomError] = useState<string | null>(null)

  useEffect(() => {
    if (!isStaff) {
      setClassroomChildIds(null)
      setClassroomName('')
      setClassroomLoading(false)
      setClassroomError(null)
      return
    }
    let active = true
    setClassroomLoading(true)
    setClassroomError(null)
    const loadClassroom = async () => {
      try {
        if (!user?.StaffId) throw new Error('This login is not linked to a staff record. Ask an administrator to link your account.')
        const staff = await staffApi.getStaffMember(user.StaffId)
        if (!staff.classroomId) throw new Error('Your staff record is not assigned to a classroom yet. Ask an administrator to assign one.')
        const [classroom, children] = await Promise.all([
          classroomsApi.getClassroom(staff.classroomId),
          classroomsApi.getClassroomChildren(staff.classroomId),
        ])
        if (!active) return
        setClassroomName(classroom.name)
        setClassroomChildIds(new Set(children.map((child) => child.Id)))
      } catch (loadError) {
        if (!active) return
        setClassroomChildIds(new Set())
        setClassroomError(loadError instanceof Error ? loadError.message : 'Could not load your classroom children.')
      } finally {
        if (active) setClassroomLoading(false)
      }
    }
    void loadClassroom()
    return () => { active = false }
  }, [isStaff, user?.StaffId])

  // Fetch feed whenever selectedDate changes
  useEffect(() => {
    fetchDailyFeed(selectedDate)
  }, [selectedDate, fetchDailyFeed])

  // Date stepper handlers
  const handleStepDay = (delta: number) => {
    const [year, month, day] = selectedDate.split('-').map(Number)
    const current = new Date(year, month - 1, day)
    current.setDate(current.getDate() + delta)
    setSelectedDate(new Date(current.getTime() - current.getTimezoneOffset() * 60_000).toISOString().slice(0, 10))
  }

  // Filtered reports
  const filteredReports = useMemo(() => {
    const scopedFeed = isStaff
      ? dailyFeed.filter((report) => classroomChildIds?.has(report.childId))
      : dailyFeed
    return scopedFeed.filter((report) => {
      // Search
      const normalizedSearch = searchTerm.trim().toLocaleLowerCase()
      const searchableFields = [
        report.childName,
        report.mood,
        report.mealsSummary,
        report.napSummary,
        report.activitiesSummary,
        report.diaperSummary,
        report.milestones,
        report.concerns,
      ]
      const matchesSearch = !normalizedSearch || searchableFields.some((field) =>
        (field ?? '').toLocaleLowerCase().includes(normalizedSearch),
      )

      // Status
      let matchesStatus = true
      if (statusFilter === 'sent') matchesStatus = report.sentToParent
      if (statusFilter === 'draft') matchesStatus = !report.sentToParent

      return matchesSearch && matchesStatus
    })
  }, [classroomChildIds, dailyFeed, isStaff, searchTerm, statusFilter])

  // Stats
  const visibleFeed = isStaff
    ? dailyFeed.filter((report) => classroomChildIds?.has(report.childId))
    : dailyFeed
  const totalCount = visibleFeed.length
  const sentCount = visibleFeed.filter((r) => r.sentToParent).length
  const draftCount = totalCount - sentCount

  const handleSend = async (id: string) => {
    setSendingId(id)
    await sendToParent(id)
    setSendingId(null)
  }

  const handleConfirmDelete = async () => {
    if (!deleteTargetId) return
    setIsDeleting(true)
    await deleteReport(deleteTargetId)
    setIsDeleting(false)
    setDeleteTargetId(null)
  }

  return (
    <div className="brand-gradient-canvas -m-3 min-h-[calc(100vh-4rem)] p-3 sm:-m-4 sm:p-4 lg:-m-6 lg:p-7">
    <div className="mx-auto max-w-7xl space-y-6 pb-12">
      {/* Header */}
      <div className="flex flex-col justify-between gap-5 rounded-3xl border border-white/70 bg-white/80 p-5 shadow-sm backdrop-blur sm:flex-row sm:items-center sm:p-7">
        <div>
          <div className="mb-3 inline-flex items-center gap-2 rounded-full border border-[color:var(--app-line)] bg-white/80 px-3 py-1.5 text-xs font-semibold uppercase tracking-[0.15em] text-[color:var(--app-primary-deep)]">
            <FileText size={14} /> Daily care journal
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-950">Daily Reports</h1>
          <p className="mt-1 text-sm text-gray-600">
            Record and share each child's daily activities, meals, and milestones
          </p>
          {isStaff && classroomName && <span className="mt-4 inline-flex items-center gap-2 rounded-xl bg-[color:var(--app-primary-soft)] px-3 py-2 text-sm font-semibold text-[color:var(--app-primary-deep)]"><School size={16} /> {classroomName} <span className="font-normal text-gray-600">· {classroomChildIds?.size ?? 0} children</span></span>}
        </div>

        <button
          onClick={() => navigate('/reports/create')}
          className="inline-flex items-center justify-center gap-2 self-start rounded-xl bg-[color:var(--app-primary)] px-5 py-3 font-semibold text-white shadow-sm transition hover:-translate-y-0.5 hover:brightness-95 sm:self-auto"
        >
          <Plus size={18} />
          New Daily Report
        </button>
      </div>

      {/* Date Navigation & Filters */}
      <div className="space-y-4 rounded-3xl border border-white/80 bg-white/90 p-4 shadow-sm backdrop-blur sm:p-5">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          {/* Date controls */}
          <div className="flex items-center gap-2">
            <button
              onClick={() => handleStepDay(-1)}
              className="p-2 rounded-xl border border-gray-200 hover:bg-gray-50 text-gray-600 transition-colors"
              title="Previous Day"
            >
              <ChevronLeft size={18} />
            </button>

            <div className="flex items-center gap-2 px-3 py-1.5 rounded-xl border border-gray-200 bg-white">
              <Calendar size={16} className="text-gray-400" />
              <input
                type="date"
                value={selectedDate}
                onChange={(e) => setSelectedDate(e.target.value)}
                className="text-sm font-semibold text-gray-800 bg-transparent focus:outline-none"
              />
            </div>

            <button
              onClick={() => handleStepDay(1)}
              disabled={selectedDate >= todayStr}
              className="p-2 rounded-xl border border-gray-200 hover:bg-gray-50 text-gray-600 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              title="Next Day"
            >
              <ChevronRight size={18} />
            </button>

            {selectedDate !== todayStr && (
              <button
                onClick={() => setSelectedDate(todayStr)}
                className="px-3 py-1.5 rounded-xl text-xs font-semibold bg-gray-100 hover:bg-gray-200 text-gray-700 transition-colors"
              >
                Today
              </button>
            )}
            <button
              type="button"
              onClick={() => fetchDailyFeed(selectedDate)}
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-xl border border-gray-200 bg-white px-3 py-2 text-xs font-semibold text-gray-700 transition hover:bg-gray-50 disabled:opacity-50"
              title="Refresh reports"
            >
              <RefreshCw size={14} className={loading ? 'animate-spin' : ''} /> Refresh
            </button>
          </div>

          {/* Search + Filter */}
          <div className="flex flex-col sm:flex-row items-center gap-2.5">
            <div className="relative w-full sm:w-64">
              <Search
                size={16}
                className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
              />
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search child or report details..."
                className="w-full pl-9 pr-3 py-2 text-sm rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-[color:var(--app-primary)]"
              />
            </div>

            <div className="flex rounded-xl border border-gray-200 p-0.5 bg-gray-50 w-full sm:w-auto">
              <button
                onClick={() => setStatusFilter('all')}
                className={`flex-1 sm:flex-none px-3 py-1.5 rounded-lg text-xs font-semibold transition-all ${
                  statusFilter === 'all'
                    ? 'bg-white text-gray-900 shadow-xs'
                    : 'text-gray-600 hover:text-gray-900'
                }`}
              >
                All
              </button>
              <button
                onClick={() => setStatusFilter('sent')}
                className={`flex-1 sm:flex-none px-3 py-1.5 rounded-lg text-xs font-semibold transition-all ${
                  statusFilter === 'sent'
                    ? 'bg-white text-emerald-700 shadow-xs'
                    : 'text-gray-600 hover:text-emerald-700'
                }`}
              >
                Sent
              </button>
              <button
                onClick={() => setStatusFilter('draft')}
                className={`flex-1 sm:flex-none px-3 py-1.5 rounded-lg text-xs font-semibold transition-all ${
                  statusFilter === 'draft'
                    ? 'bg-white text-amber-700 shadow-xs'
                    : 'text-gray-600 hover:text-amber-700'
                }`}
              >
                Draft
              </button>
            </div>
          </div>
        </div>

        {/* Stats Row */}
        <div className="grid grid-cols-3 gap-3 pt-3 border-t border-gray-100">
          <div className="p-3 rounded-xl bg-gray-50 border border-gray-100">
            <div className="text-[11px] font-medium text-gray-500 uppercase tracking-wide">
              Total Reports
            </div>
            <div className="text-xl font-bold text-gray-900 mt-0.5">{totalCount}</div>
          </div>

          <div className="p-3 rounded-xl bg-emerald-50/70 border border-emerald-100">
            <div className="flex items-center gap-1 text-[11px] font-medium text-emerald-700 uppercase tracking-wide">
              <CheckCircle size={12} />
              <span>Sent to Parents</span>
            </div>
            <div className="text-xl font-bold text-emerald-800 mt-0.5">{sentCount}</div>
          </div>

          <div className="p-3 rounded-xl bg-amber-50/70 border border-amber-100">
            <div className="flex items-center gap-1 text-[11px] font-medium text-amber-700 uppercase tracking-wide">
              <Clock size={12} />
              <span>Draft / Pending</span>
            </div>
            <div className="text-xl font-bold text-amber-800 mt-0.5">{draftCount}</div>
          </div>
        </div>
      </div>

      {/* Error state */}
      {error && (
        <div className="p-4 rounded-xl bg-red-50 border border-red-200 text-red-700 text-sm flex items-center justify-between">
          <div className="flex items-center gap-2">
            <AlertTriangle size={18} />
            <span>{error}</span>
          </div>
          <button onClick={clearError} className="font-semibold hover:underline">
            Dismiss
          </button>
        </div>
      )}

      {classroomError && (
        <div role="alert" className="flex items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-900">
          <AlertTriangle size={18} className="mt-0.5 shrink-0" />
          <span>{classroomError}</span>
        </div>
      )}

      {/* Feed list */}
      {loading || classroomLoading ? (
        <div className="flex flex-col items-center justify-center py-20 gap-3">
          <Loader2 size={32} className="animate-spin text-[color:var(--app-primary)]" />
          <span className="text-sm text-gray-500">Loading daily reports...</span>
        </div>
      ) : filteredReports.length === 0 ? (
        <div className="space-y-4 rounded-3xl border border-white/80 bg-white/90 p-8 text-center shadow-sm backdrop-blur sm:p-12">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-[color:var(--app-primary-soft)] text-[color:var(--app-primary)]">
            <Sparkles size={28} />
          </div>
          <div>
            <h3 className="text-lg font-bold text-gray-900">No Daily Reports Found</h3>
            <p className="text-sm text-gray-500 max-w-sm mx-auto mt-1">
              {visibleFeed.length === 0
                ? `No reports have been documented for ${new Date(selectedDate).toLocaleDateString(
                    undefined,
                    { month: 'short', day: 'numeric', year: 'numeric' },
                  )} yet.`
                : 'No reports matched your current search or status filter.'}
            </p>
          </div>
          <button
            onClick={() => navigate('/reports/create')}
            className="inline-flex items-center gap-2 rounded-xl bg-[color:var(--app-primary)] px-4 py-2 text-sm font-semibold text-white transition-colors hover:brightness-95"
          >
            <Plus size={16} />
            Create First Report
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4">
          {filteredReports.map((report) => (
            <ReportCard
              key={report.id}
              report={report}
              canManage={true}
              onSend={handleSend}
              onEdit={(id) => navigate(`/reports/${id}/edit`)}
              onDelete={(id) => setDeleteTargetId(id)}
              isSending={sendingId === report.id}
            />
          ))}
        </div>
      )}

      {/* Delete confirmation modal */}
      <DeleteConfirmation
        isOpen={Boolean(deleteTargetId)}
        isDeleting={isDeleting}
        title="Delete Daily Report"
        description="Are you sure you want to delete this daily report? This action cannot be undone."
        itemName="Daily Report"
        onConfirm={handleConfirmDelete}
        onCancel={() => setDeleteTargetId(null)}
      />
    </div>
    </div>
  )
}
