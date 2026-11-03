/**
 * Enrollment form component — used for Create and Edit
 */
import { useEffect, useState } from 'react'
import { EnrollmentFormInput, ProgramType } from '../types'
import { childrenApi } from '@features/children/services/childrenApi'
import { classroomsApi } from '@features/classrooms/services/classroomsApi'
import { Classroom } from '@features/classrooms/types'

interface Child {
  id: string
  firstName: string
  lastName: string
}

interface EnrollmentFormProps {
  enrollment?: EnrollmentFormInput & { id?: string }
  onSubmit: (data: EnrollmentFormInput) => Promise<void>
  onCancel: () => void
  isLoading?: boolean
  /** Pre-fill childId and lock it (e.g. when enrolling from child detail page) */
  lockedChildId?: string
}

const PROGRAM_TYPES: { value: ProgramType; label: string; desc: string }[] = [
  { value: 'FullTime', label: 'Full Time', desc: '5 days / week' },
  { value: 'PartTime', label: 'Part Time', desc: '3 days / week' },
  { value: 'HalfDay', label: 'Half Day', desc: 'AM or PM sessions' },
  { value: 'Occasional', label: 'Occasional', desc: 'Flexible drop-in' },
]

const DAY_OPTIONS = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']

function parseDays(daysOfWeek: string): string[] {
  if (!daysOfWeek) return []
  return daysOfWeek.split(',').map((d) => d.trim()).filter(Boolean)
}

function formatDays(days: string[]): string {
  return days.join(', ')
}

export const EnrollmentForm = ({
  enrollment,
  onSubmit,
  onCancel,
  isLoading = false,
  lockedChildId,
}: EnrollmentFormProps) => {
  const today = new Date().toISOString().substring(0, 10)

  const [formData, setFormData] = useState<EnrollmentFormInput>({
    childId: enrollment?.childId || lockedChildId || '',
    classroomId: enrollment?.classroomId || '',
    programType: enrollment?.programType || 'FullTime',
    daysOfWeek: enrollment?.daysOfWeek || 'Monday, Tuesday, Wednesday, Thursday, Friday',
    startDate: enrollment?.startDate ? enrollment.startDate.substring(0, 10) : today,
    endDate: enrollment?.endDate ? enrollment.endDate.substring(0, 10) : '',
    contractSigned: enrollment?.contractSigned ?? false,
    immunizationRecord: enrollment?.immunizationRecord ?? false,
    birthCertificate: enrollment?.birthCertificate ?? false,
    medicalForm: enrollment?.medicalForm ?? false,
    status: enrollment?.status,
    endReason: enrollment?.endReason ?? '',
  })

  const [selectedDays, setSelectedDays] = useState<string[]>(parseDays(formData.daysOfWeek))
  const [children, setChildren] = useState<Child[]>([])
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const load = async () => {
      try {
        // Load children for dropdown — backend returns PascalCase ChildListDto
        const cRes = await childrenApi.getChildren({ page: 1, pageSize: 200 })
        const rawChildren = (cRes as any).Children ?? (cRes as any).children ?? (cRes as any).items ?? []
        setChildren(
          Array.isArray(rawChildren)
            ? rawChildren.map((c: any) => ({
                id: c.Id ?? c.id ?? '',
                firstName: c.FirstName ?? c.firstName ?? '',
                lastName: c.LastName ?? c.lastName ?? '',
              }))
            : []
        )
      } catch (e) {
        console.error('Failed to load children', e)
      }
      try {
        const crRes = await classroomsApi.getClassrooms({ page: 1, pageSize: 100 })
        setClassrooms(crRes.items)
      } catch (e) {
        console.error('Failed to load classrooms', e)
      }
    }
    load()
  }, [])

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target
    const checked = (e.target as HTMLInputElement).checked
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }))
    setError(null)
  }

  const toggleDay = (day: string) => {
    setSelectedDays((prev) => {
      const next = prev.includes(day) ? prev.filter((d) => d !== day) : [...prev, day]
      setFormData((f) => ({ ...f, daysOfWeek: formatDays(next) }))
      return next
    })
  }

  const handleProgramTypeChange = (pt: ProgramType) => {
    setFormData((prev) => ({ ...prev, programType: pt }))
    // Auto-set days for convenience
    if (pt === 'FullTime') {
      const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday']
      setSelectedDays(days)
      setFormData((prev) => ({ ...prev, programType: pt, daysOfWeek: formatDays(days) }))
    } else if (pt === 'PartTime') {
      const days = ['Monday', 'Wednesday', 'Friday']
      setSelectedDays(days)
      setFormData((prev) => ({ ...prev, programType: pt, daysOfWeek: formatDays(days) }))
    } else {
      setFormData((prev) => ({ ...prev, programType: pt }))
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    if (!formData.childId) {
      setError('Please select a child.')
      return
    }
    if (selectedDays.length === 0) {
      setError('Please select at least one attendance day.')
      return
    }
    if (!formData.startDate) {
      setError('Start date is required.')
      return
    }
    if (formData.endDate && formData.endDate < formData.startDate) {
      setError('End date cannot be before start date.')
      return
    }
    if (formData.status === 'Ended' && !formData.endDate) {
      setError('End date is required for an ended enrollment.')
      return
    }

    try {
      await onSubmit({
        ...formData,
        daysOfWeek: formatDays(selectedDays),
        startDate: new Date(formData.startDate).toISOString(),
        endDate: formData.endDate ? new Date(formData.endDate).toISOString() : null,
        classroomId: formData.classroomId || null,
      })
    } catch (err: any) {
      setError(err.message || 'Submission failed')
    }
  }

  const inputClass =
    'w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors'
  const labelClass = 'block text-xs font-semibold text-gray-600 uppercase tracking-wider mb-1'

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {error && (
        <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      {/* Child selection */}
      <div className="bg-white p-6 rounded-lg shadow space-y-4">
        <h2 className="text-base font-bold text-gray-800 border-b border-gray-100 pb-2">
          Child Information
        </h2>
        <div>
          <label className={labelClass} htmlFor="childId">
            Child <span className="text-red-500">*</span>
          </label>
          {lockedChildId ? (
            <input
              type="hidden"
              name="childId"
              value={formData.childId}
            />
          ) : null}
          <select
            id="childId"
            name="childId"
            value={formData.childId}
            onChange={handleChange}
            disabled={!!lockedChildId}
            className={`${inputClass} ${lockedChildId ? 'bg-gray-50 cursor-not-allowed' : ''}`}
            required
          >
            <option value="">— Select a child —</option>
            {children.map((c) => (
              <option key={c.id} value={c.id}>
                {c.firstName} {c.lastName}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className={labelClass} htmlFor="classroomId">
            Classroom (optional)
          </label>
          <select
            id="classroomId"
            name="classroomId"
            value={formData.classroomId ?? ''}
            onChange={handleChange}
            className={inputClass}
          >
            <option value="">— No classroom assigned —</option>
            {classrooms.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name} ({c.currentEnrollment}/{c.maxCapacity})
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Program details */}
      <div className="bg-white p-6 rounded-lg shadow space-y-4">
        <h2 className="text-base font-bold text-gray-800 border-b border-gray-100 pb-2">
          Program Details
        </h2>

        <div>
          <label className={labelClass}>
            Program Type <span className="text-red-500">*</span>
          </label>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mt-1">
            {PROGRAM_TYPES.map((pt) => (
              <button
                key={pt.value}
                type="button"
                onClick={() => handleProgramTypeChange(pt.value)}
                className={`p-3 rounded-lg border-2 text-left transition-all ${
                  formData.programType === pt.value
                    ? 'border-blue-500 bg-blue-50 text-blue-900'
                    : 'border-gray-200 bg-white text-gray-700 hover:border-blue-300'
                }`}
              >
                <div className="text-sm font-semibold">{pt.label}</div>
                <div className="text-xs text-gray-500 mt-0.5">{pt.desc}</div>
              </button>
            ))}
          </div>
        </div>

        <div>
          <label className={labelClass}>
            Attendance Days <span className="text-red-500">*</span>
          </label>
          <div className="flex flex-wrap gap-2 mt-1">
            {DAY_OPTIONS.map((day) => (
              <button
                key={day}
                type="button"
                onClick={() => toggleDay(day)}
                className={`px-3 py-1.5 rounded-full text-xs font-semibold border transition-all ${
                  selectedDays.includes(day)
                    ? 'bg-blue-600 text-white border-blue-600'
                    : 'bg-white text-gray-600 border-gray-300 hover:border-blue-400'
                }`}
              >
                {day.substring(0, 3)}
              </button>
            ))}
          </div>
          {selectedDays.length > 0 && (
            <p className="text-xs text-gray-400 mt-1">{formatDays(selectedDays)}</p>
          )}
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className={labelClass} htmlFor="startDate">
              Start Date <span className="text-red-500">*</span>
            </label>
            <input
              id="startDate"
              type="date"
              name="startDate"
              value={formData.startDate}
              onChange={handleChange}
              className={inputClass}
              required
            />
          </div>
          <div>
            <label className={labelClass} htmlFor="endDate">
              End Date <span className="text-gray-400">(optional)</span>
            </label>
            <input
              id="endDate"
              type="date"
              name="endDate"
              value={formData.endDate ?? ''}
              onChange={handleChange}
              className={inputClass}
            />
          </div>
        </div>
      </div>

      {enrollment && (
        <div className="bg-white p-6 rounded-lg shadow space-y-4">
          <h2 className="text-base font-bold text-gray-800 border-b border-gray-100 pb-2">
            Enrollment Status
          </h2>
          <div>
            <label className={labelClass} htmlFor="status">Status</label>
            <select id="status" name="status" value={formData.status ?? ''} onChange={handleChange} className={inputClass} required>
              <option value="Pending">Pending</option>
              <option value="Active">Active</option>
              <option value="Ended">Ended</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>
          {(formData.status === 'Ended' || formData.status === 'Cancelled') && (
            <div>
              <label className={labelClass} htmlFor="endReason">Reason (optional)</label>
              <input id="endReason" name="endReason" value={formData.endReason ?? ''} onChange={handleChange} className={inputClass} />
            </div>
          )}
          {formData.status === 'Ended' && <p className="text-sm text-gray-500">Set an end date above when ending an enrollment.</p>}
        </div>
      )}

      {/* Documents & Compliance */}
      <div className="bg-white p-6 rounded-lg shadow space-y-4">
        <h2 className="text-base font-bold text-gray-800 border-b border-gray-100 pb-2">
          Documents &amp; Compliance
        </h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {[
            { name: 'contractSigned', label: 'Enrollment Contract Signed' },
            { name: 'immunizationRecord', label: 'Immunization Records On File' },
            { name: 'birthCertificate', label: 'Birth Certificate Provided' },
            { name: 'medicalForm', label: 'Medical Form Completed' },
          ].map(({ name, label }) => (
            <label
              key={name}
              className="flex items-center gap-3 p-3 rounded-lg border border-gray-200 cursor-pointer hover:bg-gray-50 transition-colors"
            >
              <input
                type="checkbox"
                name={name}
                checked={(formData as any)[name]}
                onChange={handleChange}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <span className="text-sm font-medium text-gray-700">{label}</span>
            </label>
          ))}
        </div>
      </div>

      {/* Actions */}
      <div className="flex justify-end gap-3">
        <button
          type="button"
          onClick={onCancel}
          disabled={isLoading}
          className="px-5 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors disabled:opacity-50"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isLoading}
          className="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors disabled:opacity-50"
        >
          {isLoading ? 'Saving...' : enrollment ? 'Save Changes' : 'Create Enrollment'}
        </button>
      </div>
    </form>
  )
}
