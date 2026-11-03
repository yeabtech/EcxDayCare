/**
 * ReportCard Component
 * Displays a single child daily activity report with rich sections and optional actions
 */
import React from 'react'
import {
  Calendar,
  Smile,
  Utensils,
  Moon,
  Activity,
  Baby,
  Star,
  AlertTriangle,
  Send,
  Edit2,
  Trash2,
  CheckCircle,
  Clock,
} from 'lucide-react'
import type { DailyReport } from '../types'

interface ReportCardProps {
  report: DailyReport
  canManage?: boolean
  onSend?: (id: string) => void
  onEdit?: (id: string) => void
  onDelete?: (id: string) => void
  isSending?: boolean
}

export const ReportCard: React.FC<ReportCardProps> = ({
  report,
  canManage = false,
  onSend,
  onEdit,
  onDelete,
  isSending = false,
}) => {
  const getMoodBadge = (mood: string | null) => {
    if (!mood) return null
    const lower = mood.toLowerCase()
    let colorClass = 'bg-gray-100 text-gray-700 border-gray-200'

    if (lower.includes('happy') || lower.includes('excited')) {
      colorClass = 'bg-amber-50 text-amber-700 border-amber-200'
    } else if (lower.includes('calm')) {
      colorClass = 'bg-blue-50 text-blue-700 border-blue-200'
    } else if (lower.includes('tired')) {
      colorClass = 'bg-indigo-50 text-indigo-700 border-indigo-200'
    } else if (lower.includes('fussy')) {
      colorClass = 'bg-orange-50 text-orange-700 border-orange-200'
    } else if (lower.includes('sick')) {
      colorClass = 'bg-red-50 text-red-700 border-red-200'
    } else if (lower.includes('sad')) {
      colorClass = 'bg-purple-50 text-purple-700 border-purple-200'
    }

    return (
      <span
        className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold border ${colorClass}`}
      >
        <Smile size={13} />
        {mood}
      </span>
    )
  }

  const formattedDate = new Date(report.date).toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })

  return (
    <div className="bg-white rounded-2xl border border-gray-200 p-6 shadow-xs hover:shadow-md transition-shadow duration-200 space-y-4">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-gray-100">
        <div>
          <div className="flex items-center gap-2">
            <h3 className="text-lg font-bold text-gray-900">
              {report.childName ?? 'Child Report'}
            </h3>
            {getMoodBadge(report.mood)}
          </div>
          <div className="flex items-center gap-1.5 text-xs text-gray-500 mt-1">
            <Calendar size={13} />
            <span>{formattedDate}</span>
          </div>
        </div>

        {/* Sent / Draft badge */}
        <div>
          {report.sentToParent ? (
            <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
              <CheckCircle size={13} />
              Sent to Parent
            </span>
          ) : (
            <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-amber-50 text-amber-700 border border-amber-200">
              <Clock size={13} />
              Draft / Pending
            </span>
          )}
        </div>
      </div>

      {/* Sections Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Meals */}
        {report.mealsSummary && (
          <div className="p-3.5 rounded-xl bg-orange-50/60 border border-orange-100 text-sm">
            <div className="flex items-center gap-1.5 font-semibold text-orange-800 mb-1">
              <Utensils size={15} />
              <span>Meals & Snacks</span>
            </div>
            <p className="text-gray-700 leading-relaxed text-xs sm:text-sm whitespace-pre-line">
              {report.mealsSummary}
            </p>
          </div>
        )}

        {/* Nap */}
        {report.napSummary && (
          <div className="p-3.5 rounded-xl bg-blue-50/60 border border-blue-100 text-sm">
            <div className="flex items-center gap-1.5 font-semibold text-blue-800 mb-1">
              <Moon size={15} />
              <span>Nap & Rest</span>
            </div>
            <p className="text-gray-700 leading-relaxed text-xs sm:text-sm whitespace-pre-line">
              {report.napSummary}
            </p>
          </div>
        )}

        {/* Activities */}
        {report.activitiesSummary && (
          <div className="p-3.5 rounded-xl bg-green-50/60 border border-green-100 text-sm">
            <div className="flex items-center gap-1.5 font-semibold text-green-800 mb-1">
              <Activity size={15} />
              <span>Daily Activities</span>
            </div>
            <p className="text-gray-700 leading-relaxed text-xs sm:text-sm whitespace-pre-line">
              {report.activitiesSummary}
            </p>
          </div>
        )}

        {/* Diapers */}
        {report.diaperSummary && (
          <div className="p-3.5 rounded-xl bg-purple-50/60 border border-purple-100 text-sm">
            <div className="flex items-center gap-1.5 font-semibold text-purple-800 mb-1">
              <Baby size={15} />
              <span>Diaper Changes</span>
            </div>
            <p className="text-gray-700 leading-relaxed text-xs sm:text-sm whitespace-pre-line">
              {report.diaperSummary}
            </p>
          </div>
        )}
      </div>

      {/* Highlights: Milestones */}
      {report.milestones && (
        <div className="p-3.5 rounded-xl bg-amber-50 border border-amber-200 text-sm">
          <div className="flex items-center gap-1.5 font-semibold text-amber-900 mb-1">
            <Star size={15} className="text-amber-600 fill-amber-500" />
            <span>Milestones & Achievements</span>
          </div>
          <p className="text-amber-950 leading-relaxed text-xs sm:text-sm whitespace-pre-line">
            {report.milestones}
          </p>
        </div>
      )}

      {/* Concerns */}
      {report.concerns && (
        <div className="p-3.5 rounded-xl bg-red-50 border border-red-200 text-sm">
          <div className="flex items-center gap-1.5 font-semibold text-red-800 mb-1">
            <AlertTriangle size={15} />
            <span>Notes for Parent / Concerns</span>
          </div>
          <p className="text-red-900 leading-relaxed text-xs sm:text-sm whitespace-pre-line">
            {report.concerns}
          </p>
        </div>
      )}

      {/* Management Footer Actions (Staff/Admin only) */}
      {canManage && (
        <div className="flex items-center justify-between pt-3 border-t border-gray-100">
          <div className="text-[11px] text-gray-400">
            {report.sentAt
              ? `Sent: ${new Date(report.sentAt).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' })}`
              : 'Not sent yet'}
          </div>

          <div className="flex items-center gap-2">
            {!report.sentToParent && onSend && (
              <button
                onClick={() => onSend(report.id)}
                disabled={isSending}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold bg-emerald-600 text-white hover:bg-emerald-700 disabled:opacity-50 transition-colors shadow-xs"
              >
                <Send size={13} />
                Send to Parent
              </button>
            )}

            {onEdit && (
              <button
                onClick={() => onEdit(report.id)}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold border border-gray-200 text-gray-700 hover:bg-gray-50 transition-colors"
              >
                <Edit2 size={13} />
                Edit
              </button>
            )}

            {!report.sentToParent && onDelete && (
              <button
                onClick={() => onDelete(report.id)}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold text-red-600 border border-red-200 hover:bg-red-50 transition-colors"
              >
                <Trash2 size={13} />
                Delete
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
