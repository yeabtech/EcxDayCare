/**
 * ParentReportViewPage
 * Parent-facing view for daily reports. Read-only cards for their children.
 */
import { useEffect, useState, useMemo } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  Heart,
  ArrowLeft,
  Calendar,
  Loader2,
  AlertTriangle,
  ChevronLeft,
  ChevronRight,
  Baby,
} from 'lucide-react'
import { useDailyReports } from '../hooks/useDailyReports'
import { ReportCard } from '../components/ReportCard'
import { apiClient } from '@services/api'
import { useAuth } from '@features/auth'

export const ParentReportViewPage = () => {
  const { isParent } = useAuth()
  const { childId: routeChildId } = useParams<{ childId?: string }>()
  const navigate = useNavigate()

  const [selectedChildId, setSelectedChildId] = useState<string>(routeChildId ?? '')
  const [children, setChildren] = useState<Array<{ id: string; name: string }>>([])
  const [startDate, setStartDate] = useState<string>('')
  const [endDate, setEndDate] = useState<string>('')
  const [page, setPage] = useState<number>(1)

  const {
    childReports,
    loading,
    error,
    fetchChildReports,
    clearError,
  } = useDailyReports()

  // Load children list if childId not specified or parent needs switcher
  useEffect(() => {
    apiClient
      .get<any>(isParent ? '/parent-portal/children' : '/children')
      .then((res) => {
        const items = res.data?.data ?? res.data ?? []
        const childItems = Array.isArray(items)
          ? items
          : items.children ?? items.Children ?? items.items ?? []
        const list = childItems.map((c: any) => ({
          id: c.Id ?? c.id,
          name: `${c.FirstName ?? c.firstName ?? c.Name?.FirstName ?? ''} ${c.LastName ?? c.lastName ?? c.Name?.LastName ?? ''}`.trim(),
        }))
        setChildren(list)

        // If no child selected yet, default to first child
        if (!selectedChildId && list.length > 0) {
          setSelectedChildId(list[0].id)
        }
      })
      .catch((err) => console.error('Failed to load children list', err))
  }, [selectedChildId, isParent])

  // Sync route param with state
  useEffect(() => {
    if (routeChildId && routeChildId !== selectedChildId) {
      setSelectedChildId(routeChildId)
    }
  }, [routeChildId])

  // Fetch reports for the selected child
  useEffect(() => {
    if (selectedChildId) {
      fetchChildReports(selectedChildId, {
        startDate: startDate || undefined,
        endDate: endDate || undefined,
        page,
        pageSize: 10,
      })
    }
  }, [selectedChildId, startDate, endDate, page, fetchChildReports])

  const childName = useMemo(() => {
    if (childReports?.childName) return childReports.childName
    const match = children.find((c) => c.id === selectedChildId)
    return match?.name ?? 'Your Child'
  }, [childReports, children, selectedChildId])

  const totalPages = useMemo(() => {
    if (!childReports || childReports.totalCount === 0) return 1
    return Math.ceil(childReports.totalCount / (childReports.pageSize || 10))
  }, [childReports])

  return (
    <div className="max-w-4xl mx-auto space-y-6 pb-12">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <button
            onClick={() => navigate(-1)}
            className="p-2 rounded-xl border border-gray-200 hover:bg-gray-50 text-gray-600 transition-colors"
          >
            <ArrowLeft size={18} />
          </button>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">
                {childName}'s Daily Reports
              </h1>
              <Heart size={20} className="text-pink-500 fill-pink-500" />
            </div>
            <p className="text-gray-500 text-sm mt-0.5">
              Review daily updates, meals, naps, and special milestones
            </p>
          </div>
        </div>

        {/* Child Selector if multiple children exist */}
        {children.length > 1 && (
          <div className="flex items-center gap-2">
            <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide">
              Child:
            </span>
            <select
              value={selectedChildId}
              onChange={(e) => {
                setSelectedChildId(e.target.value)
                setPage(1)
              }}
              className="px-3.5 py-2 rounded-xl border border-gray-200 bg-white text-sm font-semibold text-gray-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 shadow-xs"
            >
              {children.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>
        )}
      </div>

      {/* Date Filter Bar */}
      <div className="bg-white rounded-2xl border border-gray-200 p-4 shadow-xs flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2 text-sm text-gray-600">
          <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl border border-gray-200 bg-gray-50 text-xs">
            <Calendar size={14} className="text-gray-400" />
            <span>From:</span>
            <input
              type="date"
              value={startDate}
              onChange={(e) => {
                setStartDate(e.target.value)
                setPage(1)
              }}
              className="bg-transparent font-medium focus:outline-none text-gray-800"
            />
          </div>

          <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl border border-gray-200 bg-gray-50 text-xs">
            <Calendar size={14} className="text-gray-400" />
            <span>To:</span>
            <input
              type="date"
              value={endDate}
              onChange={(e) => {
                setEndDate(e.target.value)
                setPage(1)
              }}
              className="bg-transparent font-medium focus:outline-none text-gray-800"
            />
          </div>

          {(startDate || endDate) && (
            <button
              onClick={() => {
                setStartDate('')
                setEndDate('')
                setPage(1)
              }}
              className="px-3 py-1.5 rounded-xl text-xs font-semibold bg-gray-100 hover:bg-gray-200 text-gray-700 transition-colors"
            >
              Clear Filter
            </button>
          )}
        </div>

        {childReports && (
          <div className="text-xs text-gray-500 font-medium">
            Showing {childReports.items.length} of {childReports.totalCount} reports
          </div>
        )}
      </div>

      {/* Error banner */}
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

      {/* Reports Feed */}
      {loading ? (
        <div className="flex flex-col items-center justify-center py-20 gap-3">
          <Loader2 size={32} className="animate-spin text-indigo-600" />
          <span className="text-sm text-gray-500">Loading {childName}'s reports...</span>
        </div>
      ) : !childReports || childReports.items.length === 0 ? (
        <div className="bg-white rounded-2xl border border-gray-200 p-12 text-center space-y-4">
          <div className="w-16 h-16 rounded-2xl bg-pink-50 text-pink-500 mx-auto flex items-center justify-center">
            <Baby size={28} />
          </div>
          <div>
            <h3 className="text-lg font-bold text-gray-900">No Reports Shared Yet</h3>
            <p className="text-sm text-gray-500 max-w-sm mx-auto mt-1">
              Your childcare staff hasn't shared any daily reports for {childName} in this date range. Once they are sent, you'll see them here!
            </p>
          </div>
        </div>
      ) : (
        <div className="space-y-4">
          {childReports.items.map((report) => (
            <ReportCard
              key={report.id}
              report={report}
              canManage={false}
            />
          ))}

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between pt-4">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page <= 1}
                className="inline-flex items-center gap-1 px-4 py-2 rounded-xl text-xs font-semibold border border-gray-200 bg-white hover:bg-gray-50 text-gray-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                <ChevronLeft size={14} />
                Previous
              </button>

              <span className="text-xs text-gray-500 font-medium">
                Page {page} of {totalPages}
              </span>

              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page >= totalPages}
                className="inline-flex items-center gap-1 px-4 py-2 rounded-xl text-xs font-semibold border border-gray-200 bg-white hover:bg-gray-50 text-gray-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                Next
                <ChevronRight size={14} />
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
