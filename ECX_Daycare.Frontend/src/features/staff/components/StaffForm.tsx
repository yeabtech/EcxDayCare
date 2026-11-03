/**
 * Staff form component
 */
import { useEffect, useState } from 'react'
import { Staff, StaffFormInput, StaffRole } from '../types'
import { classroomsApi } from '@features/classrooms/services/classroomsApi'
import { Classroom } from '@features/classrooms/types'

interface StaffFormProps {
  staff?: Staff
  onSubmit: (data: StaffFormInput) => Promise<void>
  onCancel: () => void
  isLoading?: boolean
}

const ROLES: { value: StaffRole; label: string }[] = [
  { value: 'LeadTeacher', label: 'Lead Teacher' },
  { value: 'Teacher', label: 'Teacher' },
  { value: 'Assistant', label: 'Assistant Teacher' },
  { value: 'Aide', label: 'Classroom Aide' },
  { value: 'Director', label: 'Director' },
  { value: 'Admin', label: 'Administrative Staff' },
]

export const StaffForm = ({
  staff,
  onSubmit,
  onCancel,
  isLoading = false,
}: StaffFormProps) => {
  const [formData, setFormData] = useState<StaffFormInput>({
    firstName: staff?.firstName || '',
    lastName: staff?.lastName || '',
    role: staff?.role || 'Teacher',
    classroomId: staff?.classroomId || '',
    phone: staff?.phone || '',
    email: staff?.email || '',
    hireDate: staff?.hireDate ? staff.hireDate.substring(0, 10) : new Date().toISOString().substring(0, 10),
    certifications: staff?.certifications || '',
    shiftStart: staff?.shiftStart ? staff.shiftStart.substring(0, 5) : '08:00',
    shiftEnd: staff?.shiftEnd ? staff.shiftEnd.substring(0, 5) : '17:00',
  })
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadClassrooms = async () => {
      try {
        const res = await classroomsApi.getClassrooms({ page: 1, pageSize: 100 })
        setClassrooms(res.items)
      } catch (err) {
        console.error('Failed to load classrooms for dropdown', err)
      }
    }
    loadClassrooms()
  }, [])

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target
    setFormData((prev) => ({ ...prev, [name]: value }))
    setError(null)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (formData.shiftEnd <= formData.shiftStart) {
      setError('Shift End time must be after Shift Start time')
      return
    }

    try {
      await onSubmit({
        ...formData,
        hireDate: new Date(formData.hireDate).toISOString(),
        shiftStart: formData.shiftStart.length === 5 ? `${formData.shiftStart}:00` : formData.shiftStart,
        shiftEnd: formData.shiftEnd.length === 5 ? `${formData.shiftEnd}:00` : formData.shiftEnd,
        classroomId: formData.classroomId ? formData.classroomId : null,
      })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred')
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6 bg-white p-6 rounded-lg shadow">
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded text-sm">
          {error}
        </div>
      )}

      {/* Basic Information */}
      <div className="border-b border-gray-200 pb-4">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Personal Information</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">First Name *</label>
            <input
              type="text"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              required
              maxLength={100}
              placeholder="e.g. Jane"
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Last Name *</label>
            <input
              type="text"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              required
              maxLength={100}
              placeholder="e.g. Doe"
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Email Address *</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              placeholder="e.g. jane.doe@daycare.com"
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Phone Number *</label>
            <input
              type="tel"
              name="phone"
              value={formData.phone}
              onChange={handleChange}
              required
              placeholder="e.g. 555-123-4567"
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
      </div>

      {/* Role & Classroom Assignment */}
      <div className="border-b border-gray-200 pb-4">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Role & Classroom Assignment</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Role *</label>
            <select
              name="role"
              value={formData.role}
              onChange={handleChange}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            >
              {ROLES.map((r) => (
                <option key={r.value} value={r.value}>
                  {r.label}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Assigned Classroom</label>
            <select
              name="classroomId"
              value={formData.classroomId || ''}
              onChange={handleChange}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">(No classroom assigned)</option>
              {classrooms.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name} (Capacity: {c.currentEnrollment}/{c.maxCapacity})
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Employment & Shift Details */}
      <div className="border-b border-gray-200 pb-4">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Employment & Schedule</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Hire Date *</label>
            <input
              type="date"
              name="hireDate"
              value={formData.hireDate}
              onChange={handleChange}
              required
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Shift Start *</label>
            <input
              type="time"
              name="shiftStart"
              value={formData.shiftStart}
              onChange={handleChange}
              required
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Shift End *</label>
            <input
              type="time"
              name="shiftEnd"
              value={formData.shiftEnd}
              onChange={handleChange}
              required
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
      </div>

      {/* Certifications */}
      <div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">Certifications & Qualifications</h3>
        <textarea
          name="certifications"
          value={formData.certifications}
          onChange={handleChange}
          rows={3}
          maxLength={500}
          placeholder="e.g. CPR & First Aid Certified, CDA Credential, Early Childhood Education Degree"
          className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        />
        <p className="text-xs text-gray-500 mt-1">Optional. Maximum 500 characters.</p>
      </div>

      {/* Actions */}
      <div className="flex justify-end gap-3 pt-4">
        <button
          type="button"
          onClick={onCancel}
          disabled={isLoading}
          className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isLoading}
          className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
        >
          {isLoading ? 'Saving...' : staff ? 'Update Staff Member' : 'Create Staff Member'}
        </button>
      </div>
    </form>
  )
}
