/**
 * Enrollment detail page — view, end, and navigate history
 */
import { useEffect, useState } from 'react'
import { useNavigate, useParams, Link } from 'react-router-dom'
import { useEnrollments } from '../hooks/useEnrollments'
import { EndEnrollmentInput } from '../types'
import {
  ArrowLeft,
  Edit2,
  BookOpen,
  CalendarDays,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Layers,
  User,
  Clock,
} from 'lucide-react'

const STATUS_COLORS: Record<string, string> = {
  Active: 'bg-emerald-100 text-emerald-800',
  Pending: 'bg-amber-100 text-amber-800',
  Ended: 'bg-gray-100 text-gray-600',
  Cancelled: 'bg-red-100 text-red-700',
}

const PROGRAM_LABELS: Record<string, string> = {
  FullTime: 'Full Time',
  PartTime: 'Part Time',
  HalfDay: 'Half Day',
  Occasional: 'Occasional',
}

interface EndModalState {
  open: boolean
  endDate: string
  reason: string
}

const DocRow = ({ done, label }: { done: boolean; label: string }) => (
  <div className="flex items-center justify-between py-2.5 border-b border-gray-100 last:border-0">
    <span className="text-sm text-gray-700">{label}</span>
    {done ? (
      <span className="inline-flex items-center gap-1 text-xs font-semibold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-full">
        <CheckCircle size={12} /> On File
      </span>
    ) : (
      <span className="inline-flex items-center gap-1 text-xs font-semibold text-red-600 bg-red-50 px-2 py-0.5 rounded-full">
        <XCircle size={12} /> Missing
      </span>
    )}
  </div>
)

export const EnrollmentDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { currentEnrollment, loading, error, fetchEnrollment, endEnrollment } = useEnrollments()

  const today = new Date().toISOString().substring(0, 10)
  const [endModal, setEndModal] = useState<EndModalState>({
    open: false,
    endDate: today,
    reason: '',
  })
  const [isEnding, setIsEnding] = useState(false)
  const [endError, setEndError] = useState<string | null>(null)

  useEffect(() => {
    if (id) fetchEnrollment(id)
  }, [id, fetchEnrollment])

  const handleEndEnrollment = async () => {
    if (!id || !endModal.endDate) {
      setEndError('End date is required.')
      return
    }
    setIsEnding(true)
    setEndError(null)
    try {
      const payload: EndEnrollmentInput = {
        endDate: new Date(endModal.endDate).toISOString(),
        reason: endModal.reason.trim() || undefined,
      }
      await endEnrollment(id, payload)
      setEndModal({ open: false, endDate: today, reason: '' })
    } catch (err: any) {
      setEndError(err.message || 'Failed to end enrollment')
    } finally {
      setIsEnding(false)
    }
  }

  if (loading && !currentEnrollment) {
    return <div className="p-8 text-center text-gray-500">Loading enrollment details...</div>
  }

  if (error || !currentEnrollment) {
    return (
      <div className="max-w-4xl mx-auto space-y-4">
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error || 'Enrollment not found'}
        </div>
        <button
          onClick={() => navigate('/enrollments')}
          className="text-blue-600 hover:underline text-sm font-medium"
        >
          &larr; Back to Enrollments
        </button>
      </div>
    )
  }

  const e = currentEnrollment
  const isActive = e.status === 'Active' || e.status === 'Pending'
  const allDocsDone = e.contractSigned && e.immunizationRecord && e.birthCertificate && e.medicalForm
  const docsCount = [e.contractSigned, e.immunizationRecord, e.birthCertificate, e.medicalForm].filter(Boolean).length

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-lg shadow">
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate('/enrollments')}
            className="p-2 hover:bg-gray-100 rounded-full transition-colors text-gray-500"
            title="Back"
          >
            <ArrowLeft size={20} />
          </button>
          <div className="h-14 w-14 rounded-full bg-indigo-100 text-indigo-600 flex items-center justify-center">
            <BookOpen size={24} />
          </div>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold text-gray-900">
                {e.childName ?? 'Enrollment'}
              </h1>
              <span
                className={`px-2.5 py-0.5 rounded-full text-xs font-semibold ${
                  STATUS_COLORS[e.status] ?? 'bg-gray-100 text-gray-700'
                }`}
              >
                {e.status}
              </span>
            </div>
            <p className="text-sm text-gray-500 mt-1">
              {PROGRAM_LABELS[e.programType] ?? e.programType} · Started{' '}
              {new Date(e.startDate).toLocaleDateString()}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-3 w-full sm:w-auto">
          <Link
            to={`/enrollments/${e.id}/edit`}
            className="inline-flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            <Edit2 size={16} />
            Edit
          </Link>
          {isActive && (
            <button
              id="btn-end-enrollment"
              onClick={() => setEndModal({ open: true, endDate: today, reason: '' })}
              className="inline-flex items-center gap-2 px-4 py-2 border border-red-300 rounded-md shadow-sm text-sm font-medium text-red-700 bg-white hover:bg-red-50"
            >
              <AlertTriangle size={16} />
              End Enrollment
            </button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left — main details */}
        <div className="lg:col-span-2 space-y-6">
          {/* Program Info */}
          <div className="bg-white p-6 rounded-lg shadow space-y-4">
            <h2 className="text-lg font-bold text-gray-900 border-b border-gray-100 pb-3">
              Program Details
            </h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                <BookOpen className="text-gray-400 mt-0.5" size={18} />
                <div>
                  <div className="text-xs text-gray-500 font-medium">Program Type</div>
                  <div className="text-sm font-semibold text-gray-900 mt-0.5">
                    {PROGRAM_LABELS[e.programType] ?? e.programType}
                  </div>
                </div>
              </div>

              <div className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                <Clock className="text-gray-400 mt-0.5" size={18} />
                <div>
                  <div className="text-xs text-gray-500 font-medium">Schedule</div>
                  <div className="text-sm font-medium text-gray-900 mt-0.5">{e.daysOfWeek}</div>
                </div>
              </div>

              <div className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                <CalendarDays className="text-gray-400 mt-0.5" size={18} />
                <div>
                  <div className="text-xs text-gray-500 font-medium">Start Date</div>
                  <div className="text-sm font-medium text-gray-900 mt-0.5">
                    {new Date(e.startDate).toLocaleDateString('en-US', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                    })}
                  </div>
                </div>
              </div>

              <div className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                <CalendarDays className="text-gray-400 mt-0.5" size={18} />
                <div>
                  <div className="text-xs text-gray-500 font-medium">End Date</div>
                  <div className="text-sm font-medium text-gray-900 mt-0.5">
                    {e.endDate
                      ? new Date(e.endDate).toLocaleDateString('en-US', {
                          year: 'numeric',
                          month: 'long',
                          day: 'numeric',
                        })
                      : <span className="text-gray-400 italic">Open-ended</span>}
                  </div>
                </div>
              </div>
            </div>

            {e.endReason && (
              <div className="p-3 bg-amber-50 border border-amber-100 rounded-lg text-sm text-amber-800">
                <span className="font-semibold">End Reason: </span>{e.endReason}
              </div>
            )}
          </div>

          {/* Compliance Documents */}
          <div className="bg-white p-6 rounded-lg shadow space-y-2">
            <div className="flex items-center justify-between border-b border-gray-100 pb-3">
              <h2 className="text-lg font-bold text-gray-900">Compliance Documents</h2>
              <span
                className={`text-xs font-semibold px-2 py-0.5 rounded-full ${
                  allDocsDone
                    ? 'bg-emerald-100 text-emerald-700'
                    : 'bg-amber-100 text-amber-700'
                }`}
              >
                {docsCount}/4 Complete
              </span>
            </div>
            <DocRow done={e.contractSigned} label="Enrollment Contract Signed" />
            <DocRow done={e.immunizationRecord} label="Immunization Records" />
            <DocRow done={e.birthCertificate} label="Birth Certificate" />
            <DocRow done={e.medicalForm} label="Medical Form" />
          </div>
        </div>

        {/* Right — meta */}
        <div className="space-y-6">
          {/* Child & Classroom */}
          <div className="bg-white p-6 rounded-lg shadow space-y-4">
            <h2 className="text-lg font-bold text-gray-900 border-b border-gray-100 pb-3 flex items-center gap-2">
              <User size={20} className="text-indigo-500" />
              Linked Records
            </h2>

            <div className="space-y-3">
              <div>
                <div className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">
                  Child
                </div>
                <Link
                  to={`/children/${e.childId}`}
                  className="text-sm font-medium text-blue-600 hover:underline"
                >
                  {e.childName ?? e.childId}
                </Link>
                <div className="mt-1">
                  <Link
                    to={`/enrollments?childId=${e.childId}`}
                    className="text-xs text-gray-400 hover:text-gray-600 underline"
                  >
                    View enrollment history →
                  </Link>
                </div>
              </div>

              {e.classroomId && (
                <div>
                  <div className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">
                    Classroom
                  </div>
                  <div className="flex items-center gap-2">
                    <Layers size={14} className="text-gray-400" />
                    <Link
                      to={`/classrooms/${e.classroomId}`}
                      className="text-sm font-medium text-blue-600 hover:underline"
                    >
                      {e.classroomName ?? e.classroomId}
                    </Link>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Record metadata */}
          <div className="bg-gray-50 p-4 rounded-lg text-xs text-gray-500 space-y-1.5">
            <div>
              <span className="font-semibold text-gray-600">ID:</span> {e.id}
            </div>
            <div>
              <span className="font-semibold text-gray-600">Created:</span>{' '}
              {new Date(e.createdAt).toLocaleString()}
            </div>
            <div>
              <span className="font-semibold text-gray-600">Updated:</span>{' '}
              {new Date(e.updatedAt).toLocaleString()}
            </div>
          </div>
        </div>
      </div>

      {/* End Enrollment Modal */}
      {endModal.open && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg p-6 max-w-sm w-full shadow-xl space-y-4">
            <h3 className="text-lg font-bold text-gray-900">End Enrollment</h3>
            <p className="text-sm text-gray-600">
              This will mark the enrollment as ended. Please provide the end date and an optional
              reason.
            </p>

            {endError && (
              <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded text-sm">
                {endError}
              </div>
            )}

            <div>
              <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wider mb-1">
                End Date <span className="text-red-500">*</span>
              </label>
              <input
                id="end-date-input"
                type="date"
                value={endModal.endDate}
                onChange={(ev) => setEndModal((prev) => ({ ...prev, endDate: ev.target.value }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-400"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wider mb-1">
                Reason (optional)
              </label>
              <textarea
                id="end-reason-input"
                value={endModal.reason}
                onChange={(ev) => setEndModal((prev) => ({ ...prev, reason: ev.target.value }))}
                rows={3}
                placeholder="e.g. Child moving, family request..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-400 resize-none"
              />
            </div>

            <div className="flex justify-end gap-3">
              <button
                onClick={() => setEndModal({ open: false, endDate: today, reason: '' })}
                disabled={isEnding}
                className="px-4 py-2 border border-gray-300 rounded-md text-sm text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                id="btn-confirm-end"
                onClick={handleEndEnrollment}
                disabled={isEnding}
                className="px-4 py-2 bg-red-600 text-white rounded-md text-sm hover:bg-red-700 disabled:opacity-50"
              >
                {isEnding ? 'Ending...' : 'End Enrollment'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
