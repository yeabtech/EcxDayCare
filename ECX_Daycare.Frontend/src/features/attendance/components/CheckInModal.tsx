/**
 * Check-In Modal — select a child and log arrival details
 */
import { useState, useEffect } from 'react'
import { X, LogIn, AlertCircle, Loader2, School } from 'lucide-react'
import { useAttendance } from '../hooks/useAttendance'
import { apiClient } from '@services/api'

interface Child {
  id: string
  fullName: string
}

interface Classroom {
  id: string
  name: string
}

interface CheckInModalProps {
  onClose: () => void
  onSuccess: () => void
  initialChildId?: string
  initialChildName?: string
  initialClassroomId?: string
  restrictToClassroom?: boolean
}

export const CheckInModal = ({ onClose, onSuccess, initialChildId, initialChildName, initialClassroomId, restrictToClassroom = false }: CheckInModalProps) => {
  const { checkIn, actionLoading, error, clearError } = useAttendance()

  const [children, setChildren] = useState<Child[]>([])
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [childrenLoading, setChildrenLoading] = useState(true)

  const [childId, setChildId] = useState(initialChildId ?? '')
  const [classroomId, setClassroomId] = useState(initialClassroomId ?? '')
  const [childSearch, setChildSearch] = useState('')
  const [droppedOffBy, setDroppedOffBy] = useState('')
  const [notes, setNotes] = useState('')
  const [status, setStatus] = useState<string>('Present')
  const [localError, setLocalError] = useState<string | null>(null)

  useEffect(() => {
    const fetchChildren = async () => {
      try {
        const [res, classroomRes] = await Promise.all(restrictToClassroom && initialClassroomId
          ? [
              apiClient.get<any>(`/classrooms/${initialClassroomId}/children`),
              apiClient.get<any>(`/classrooms/${initialClassroomId}`),
            ]
          : [
              apiClient.get<any>('/children', { params: { skip: 0, take: 100 } }),
              apiClient.get<any>('/classrooms', { params: { page: 1, pageSize: 100 } }),
            ])
        const payload = res.data?.data ?? res.data?.Data ?? res.data
        const rawItems = payload?.Items ?? payload?.items ?? payload?.Children ?? payload?.children ?? (Array.isArray(payload) ? payload : [])
        setChildren(
          rawItems.map((c: any) => ({
            id: c.Id ?? c.id,
            fullName: c.FullName ?? c.fullName ?? `${c.FirstName ?? c.firstName ?? ''} ${c.LastName ?? c.lastName ?? ''}`.trim(),
          })),
        )
        const classroomPayload = classroomRes.data?.data ?? classroomRes.data?.Data ?? classroomRes.data
        const rawClassrooms = restrictToClassroom && initialClassroomId
          ? [classroomPayload]
          : classroomPayload?.Items ?? classroomPayload?.items ?? classroomPayload?.Classrooms ?? classroomPayload?.classrooms ?? []
        setClassrooms(rawClassrooms.map((room: any) => ({
          id: room.Id ?? room.id,
          name: room.Name ?? room.name,
        })))
      } catch {
        setLocalError('Failed to load children list')
      } finally {
        setChildrenLoading(false)
      }
    }
    fetchChildren()
  }, [initialClassroomId, restrictToClassroom])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLocalError(null)
    clearError()
    if (!childId) {
      setLocalError('Please select a child')
      return
    }
    try {
      await checkIn({ childId, classroomId: classroomId || null, droppedOffBy: droppedOffBy || null, notes: notes || null, status })
      onSuccess()
    } catch (err: any) {
      setLocalError(err.message || 'Check-in failed')
    }
  }

  const displayError = localError || error

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-950/55 backdrop-blur-sm" onClick={onClose} />
      <div className="relative z-10 w-full max-w-lg overflow-hidden rounded-3xl bg-white shadow-2xl ring-1 ring-black/5">
        {/* Header */}
        <div className="flex items-center justify-between bg-primary-700 px-6 py-5 text-white">
          <div className="flex items-center gap-3">
            <div className="rounded-2xl bg-white/15 p-3 ring-1 ring-white/20"><LogIn size={20} /></div>
            <div>
              <h2 className="text-lg font-bold">Check in a child</h2>
              <p className="mt-0.5 text-xs text-white/75">Add today’s arrival to the classroom register</p>
            </div>
          </div>
          <button onClick={onClose} aria-label="Close check-in" className="rounded-xl p-2 text-white/80 transition hover:bg-white/15 hover:text-white">
            <X size={18} />
          </button>
        </div>

        {/* Error */}
        {displayError && (
          <div className="flex items-center gap-2 p-3 mb-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
            <AlertCircle size={14} className="flex-shrink-0" />
            {displayError}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4 p-6">
          {/* Child */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="checkin-child">
              Child <span className="text-red-500">*</span>
            </label>
            {childrenLoading ? (
              <div className="flex items-center gap-2 text-sm text-gray-400">
                <Loader2 size={14} className="animate-spin" />
                Loading children…
              </div>
            ) : initialChildId ? (
              <div className="rounded-xl border border-primary-100 bg-primary-50 px-4 py-3 text-sm font-semibold text-primary-800">{initialChildName || children.find((child) => child.id === initialChildId)?.fullName || 'Selected child'}</div>
            ) : (
              <div className="space-y-2">
              <input id="checkin-child-search" value={childSearch} onChange={(event) => setChildSearch(event.target.value)} placeholder="Search children…" className="w-full rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm outline-none focus:border-primary-400 focus:bg-white focus:ring-4 focus:ring-primary-500/10" />
              <select
                id="checkin-child"
                value={childId}
                onChange={(e) => setChildId(e.target.value)}
                disabled={Boolean(initialChildId)}
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 disabled:bg-slate-50"
              >
                <option value="">— Select a child —</option>
                {children.filter((child) => child.fullName.toLowerCase().includes(childSearch.trim().toLowerCase())).map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.fullName}
                  </option>
                ))}
              </select>
              </div>
            )}
          </div>

          <div>
            <label className="mb-1 flex items-center gap-1.5 text-sm font-medium text-gray-700" htmlFor="checkin-classroom"><School size={15} className="text-gray-400" /> Classroom</label>
            <select id="checkin-classroom" value={classroomId} onChange={(event) => setClassroomId(event.target.value)} disabled={Boolean(initialClassroomId)} className="w-full rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm outline-none focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 disabled:cursor-not-allowed disabled:bg-slate-50">
              <option value="">Use child’s assigned classroom</option>
              {classrooms.map((room) => <option key={room.id} value={room.id}>{room.name}</option>)}
            </select>
            <p className="mt-1 text-xs text-slate-400">Choose a room only when this arrival should be recorded elsewhere.</p>
          </div>

          {/* Status */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="checkin-status">
              Arrival Status
            </label>
            <select
              id="checkin-status"
              value={status}
              onChange={(e) => setStatus(e.target.value)}
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10"
            >
              <option value="Present">Present (on time)</option>
              <option value="Late">Late</option>
            </select>
          </div>

          {/* Dropped off by */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="checkin-dropped">
              Dropped off by
            </label>
            <input
              id="checkin-dropped"
              type="text"
              maxLength={150}
              value={droppedOffBy}
              onChange={(e) => setDroppedOffBy(e.target.value)}
              placeholder="Parent / guardian name"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="checkin-notes">
              Notes
            </label>
            <textarea
              id="checkin-notes"
              rows={2}
              maxLength={500}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Optional notes…"
              className="w-full resize-none rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10"
            />
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-3 border-t border-slate-100 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold text-slate-600 transition hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={actionLoading || childrenLoading}
              className="inline-flex items-center gap-2 rounded-xl bg-primary-500 px-5 py-2.5 text-sm font-bold text-white shadow-md transition hover:-translate-y-0.5 hover:bg-primary-600 disabled:translate-y-0 disabled:opacity-60"
            >
              {actionLoading ? <Loader2 size={14} className="animate-spin" /> : <LogIn size={14} />}
              Check In
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
