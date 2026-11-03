/**
 * Mark Absent Modal — mark a child absent for a given date
 */
import { useState, useEffect } from 'react'
import { X, UserX, AlertCircle, Loader2 } from 'lucide-react'
import { useAttendance } from '../hooks/useAttendance'
import { apiClient } from '@services/api'

interface Child {
  id: string
  fullName: string
}

interface MarkAbsentModalProps {
  onClose: () => void
  onSuccess: () => void
}

export const MarkAbsentModal = ({ onClose, onSuccess }: MarkAbsentModalProps) => {
  const { markAbsent, actionLoading, error, clearError } = useAttendance()

  const [children, setChildren] = useState<Child[]>([])
  const [childrenLoading, setChildrenLoading] = useState(true)

  const [childId, setChildId] = useState('')
  const [date, setDate] = useState<string>(new Date().toISOString().split('T')[0])
  const [notes, setNotes] = useState('')
  const [localError, setLocalError] = useState<string | null>(null)

  useEffect(() => {
    const fetch = async () => {
      try {
        const res = await apiClient.get<any>('/children', { params: { page: 1, pageSize: 200 } })
        const payload = res.data?.data ?? res.data
        const rawItems = payload?.Items ?? payload?.items ?? (Array.isArray(payload) ? payload : [])
        setChildren(
          rawItems.map((c: any) => ({
            id: c.Id ?? c.id,
            fullName:
              c.FullName ??
              c.fullName ??
              `${c.FirstName ?? c.firstName ?? ''} ${c.LastName ?? c.lastName ?? ''}`.trim(),
          })),
        )
      } catch {
        setLocalError('Failed to load children list')
      } finally {
        setChildrenLoading(false)
      }
    }
    fetch()
  }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLocalError(null)
    clearError()
    if (!childId) {
      setLocalError('Please select a child')
      return
    }
    try {
      await markAbsent({ childId, date: date || null, notes: notes || null })
      onSuccess()
    } catch (err: any) {
      setLocalError(err.message || 'Failed to mark absent')
    }
  }

  const displayError = localError || error

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6 z-10">
        {/* Header */}
        <div className="flex items-center justify-between mb-5">
          <div className="flex items-center gap-2.5">
            <div className="p-2 bg-red-100 rounded-xl">
              <UserX size={18} className="text-red-600" />
            </div>
            <div>
              <h2 className="text-base font-bold text-gray-900">Mark Absent</h2>
              <p className="text-xs text-gray-500">Record an absence for a child</p>
            </div>
          </div>
          <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400">
            <X size={18} />
          </button>
        </div>

        {displayError && (
          <div className="flex items-center gap-2 p-3 mb-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
            <AlertCircle size={14} className="flex-shrink-0" />
            {displayError}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Child */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="absent-child">
              Child <span className="text-red-500">*</span>
            </label>
            {childrenLoading ? (
              <div className="flex items-center gap-2 text-sm text-gray-400">
                <Loader2 size={14} className="animate-spin" />
                Loading children…
              </div>
            ) : (
              <select
                id="absent-child"
                value={childId}
                onChange={(e) => setChildId(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">— Select a child —</option>
                {children.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.fullName}
                  </option>
                ))}
              </select>
            )}
          </div>

          {/* Date */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="absent-date">
              Date
            </label>
            <input
              id="absent-date"
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="absent-notes">
              Notes
            </label>
            <textarea
              id="absent-notes"
              rows={2}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Reason for absence (optional)…"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={actionLoading || childrenLoading}
              className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg disabled:opacity-60 transition-colors"
            >
              {actionLoading ? <Loader2 size={14} className="animate-spin" /> : <UserX size={14} />}
              Mark Absent
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
