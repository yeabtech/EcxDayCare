/**
 * ChildForm component
 * Form for creating and editing children
 */
import { useEffect, useState } from 'react'
import { Child, ChildFormInput, ParentAssignment } from '../types'
import { ParentMultiSelect } from './ParentMultiSelect'
import { PhotoUpload } from './PhotoUpload'
import { childrenApi } from '../services/childrenApi'

interface ChildFormProps {
  child?: Child
  onSubmit: (data: ChildFormInput) => Promise<void>
  isLoading?: boolean
  onCancel?: () => void
}

export const ChildForm = ({ child, onSubmit, isLoading = false, onCancel }: ChildFormProps) => {
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [parents, setParents] = useState<ParentAssignment[]>([])
  const [formData, setFormData] = useState<ChildFormInput>(
    child
      ? {
          FirstName: child.FirstName,
          LastName: child.LastName,
          DateOfBirth: child.DateOfBirth ? child.DateOfBirth.split('T')[0] : '',
          Gender: child.Gender,
          EnrollmentDate: child.EnrollmentDate ? child.EnrollmentDate.split('T')[0] : '',
          PhotoUrl: child.PhotoUrl || '',
          Allergies: child.Allergies || '',
          MedicalNotes: child.MedicalNotes || '',
          status: child.Status || 'Enrolled',
        }
      : {
          FirstName: '',
          LastName: '',
          DateOfBirth: '',
          Gender: 'Male',
          EnrollmentDate: new Date().toISOString().split('T')[0],
          PhotoUrl: '',
          Allergies: '',
          MedicalNotes: '',
          status: 'Enrolled',
        }
  )

  // When editing, load the parents already linked to this child.
  useEffect(() => {
    if (!child) return
    let active = true
    const loadLinks = async () => {
      try {
        const links = await childrenApi.getChildParents(child.Id)
        if (active) {
          setParents(
            links.map((l) => ({
              ParentId: l.ParentId,
              Role: (l.Role as ParentAssignment['Role']) || 'Guardian',
            }))
          )
        }
      } catch {
        if (active) setParents([])
      }
    }
    loadLinks()
    return () => {
      active = false
    }
  }, [child])

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {}

    if (!formData.FirstName.trim()) newErrors.FirstName = 'First name is required'
    if (!formData.LastName.trim()) newErrors.LastName = 'Last name is required'
    if (!formData.DateOfBirth) newErrors.DateOfBirth = 'Date of birth is required'
    if (!formData.EnrollmentDate) newErrors.EnrollmentDate = 'Enrollment date is required'
    if (parents.length === 0) newErrors.Parents = 'Assign at least one parent or guardian'

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!validate()) return

    try {
      // Always send Parents (even when empty) so removing the last parent
      // clears the ParentChildren rows on the backend.
      await onSubmit({ ...formData, Parents: parents })
    } catch (error) {
      console.error('Form submission error:', error)
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target
    setFormData((prev) => ({ ...prev, [name]: value }))
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: '' }))
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Photo */}
      <div>
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Photo</h2>
        <PhotoUpload
          value={formData.PhotoUrl}
          onChange={(url) => setFormData((prev) => ({ ...prev, PhotoUrl: url }))}
          disabled={isLoading}
        />
      </div>

      {/* Personal Information */}
      <div>
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Personal Information</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField
            label="First Name"
            name="FirstName"
            type="text"
            value={formData.FirstName}
            onChange={handleChange}
            error={errors.FirstName}
            required
          />
          <FormField
            label="Last Name"
            name="LastName"
            type="text"
            value={formData.LastName}
            onChange={handleChange}
            error={errors.LastName}
            required
          />
          <FormField
            label="Date of Birth"
            name="DateOfBirth"
            type="date"
            value={formData.DateOfBirth}
            onChange={handleChange}
            error={errors.DateOfBirth}
            required
          />
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Gender</label>
            <select
              name="Gender"
              value={formData.Gender}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="Male">Male</option>
              <option value="Female">Female</option>
              <option value="Other">Other</option>
            </select>
          </div>
        </div>
      </div>

      {/* Parent Information */}
      <div>
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Parents / Guardians</h2>
        <p className="text-sm text-gray-500 mb-3">
          Search for existing parents and assign them to this child. Each assignment is saved to
          the ParentChildren table with the selected role.
        </p>
        <ParentMultiSelect
          value={parents}
          onChange={setParents}
          error={errors.Parents}
          disabled={isLoading}
        />
      </div>

      {/* Medical Information */}
      <div>
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Medical Information</h2>
        <div className="space-y-4">
          <FormField
            label="Allergies"
            name="Allergies"
            type="text"
            value={formData.Allergies || ''}
            onChange={handleChange}
            placeholder="e.g., Peanuts, Dairy"
          />
          <FormField
            label="Medical Notes"
            name="MedicalNotes"
            type="textarea"
            value={formData.MedicalNotes || ''}
            onChange={handleChange}
            placeholder="Any medical conditions or medications"
          />
        </div>
      </div>

      {/* Enrollment Information */}
      <div>
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Enrollment Information</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField
            label="Enrollment Date"
            name="EnrollmentDate"
            type="date"
            value={formData.EnrollmentDate}
            onChange={handleChange}
            error={errors.EnrollmentDate}
            required
          />
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select
              name="status"
              value={formData.status || 'Enrolled'}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="Enrolled">Enrolled</option>
              <option value="Inactive">Inactive</option>
              <option value="Withdrawn">Withdrawn</option>
            </select>
          </div>
        </div>
      </div>

      {/* Form Actions */}
      <div className="flex gap-3 justify-end pt-6 border-t">
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            disabled={isLoading}
            className="btn-secondary"
          >
            Cancel
          </button>
        )}
        <button
          type="submit"
          disabled={isLoading}
          className="btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isLoading ? 'Saving...' : child ? 'Update Child' : 'Create Child'}
        </button>
      </div>
    </form>
  )
}

interface FormFieldProps {
  label?: string
  name: string
  type?: 'text' | 'email' | 'tel' | 'date' | 'textarea'
  value?: string
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => void
  error?: string
  required?: boolean
  placeholder?: string
}

const FormField = ({
  label,
  name,
  type = 'text',
  value = '',
  onChange,
  error,
  required = false,
  placeholder,
}: FormFieldProps) => {
  return (
    <div>
      {label && (
        <label className="block text-sm font-medium text-gray-700 mb-1">
          {label}
          {required && <span className="text-red-500">*</span>}
        </label>
      )}
      {type === 'textarea' ? (
        <textarea
          name={name}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          rows={4}
          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 ${
            error
              ? 'border-red-500 focus:ring-red-500'
              : 'border-gray-300 focus:ring-primary-500'
          }`}
        />
      ) : (
        <input
          type={type}
          name={name}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 ${
            error
              ? 'border-red-500 focus:ring-red-500'
              : 'border-gray-300 focus:ring-primary-500'
          }`}
        />
      )}
      {error && <p className="text-red-500 text-xs mt-1">{error}</p>}
    </div>
  )
}

