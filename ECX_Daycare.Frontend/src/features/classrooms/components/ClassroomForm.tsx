/**
 * Classroom form component
 */
import { useState } from 'react'
import { Classroom, ClassroomFormInput } from '../types'

interface ClassroomFormProps {
  classroom?: Classroom
  onSubmit: (data: ClassroomFormInput) => Promise<void>
  onCancel: () => void
  isLoading?: boolean
}

export const ClassroomForm = ({
  classroom,
  onSubmit,
  onCancel,
  isLoading = false,
}: ClassroomFormProps) => {
  const [formData, setFormData] = useState<ClassroomFormInput>({
    name: classroom?.name || '',
    minAgeMonths: classroom?.minAgeMonths || 0,
    maxAgeMonths: classroom?.maxAgeMonths || 12,
    maxCapacity: classroom?.maxCapacity || 10,
    status: classroom?.status || 'Active',
    leadTeacherId: classroom?.leadTeacherId || '',
    description: classroom?.description || '',
  })
  const [error, setError] = useState<string | null>(null)

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseInt(value, 10) : value,
    }))
    setError(null)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (formData.minAgeMonths >= formData.maxAgeMonths) {
      setError('Maximum age must be greater than minimum age')
      return
    }

    try {
      await onSubmit({
        ...formData,
        leadTeacherId: formData.leadTeacherId && formData.leadTeacherId.trim() ? formData.leadTeacherId.trim() : undefined,
        description: formData.description && formData.description.trim() ? formData.description.trim() : undefined,
      })
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data?.error || err.message || 'An error occurred'
      setError(msg)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6 bg-white p-6 rounded-lg shadow">
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded">
          {error}
        </div>
      )}

      <div>
        <label className="block text-sm font-medium text-gray-700">Classroom Name</label>
        <input
          type="text"
          name="name"
          value={formData.name}
          onChange={handleChange}
          required
          className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      <div className="grid grid-cols-3 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Min Age (months)</label>
          <input
            type="number"
            name="minAgeMonths"
            value={formData.minAgeMonths}
            onChange={handleChange}
            min="0"
            max="84"
            required
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700">Max Age (months)</label>
          <input
            type="number"
            name="maxAgeMonths"
            value={formData.maxAgeMonths}
            onChange={handleChange}
            min="0"
            max="84"
            required
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700">Max Capacity</label>
          <input
            type="number"
            name="maxCapacity"
            value={formData.maxCapacity}
            onChange={handleChange}
            min="1"
            max="30"
            required
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Status</label>
          <select
            name="status"
            value={formData.status}
            onChange={handleChange}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700">Lead Teacher ID</label>
          <input
            type="text"
            name="leadTeacherId"
            value={formData.leadTeacherId || ''}
            onChange={handleChange}
            placeholder="Optional UUID"
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700">Description</label>
        <textarea
          name="description"
          value={formData.description || ''}
          onChange={handleChange}
          rows={3}
          className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      <div className="flex gap-4">
        <button
          type="submit"
          disabled={isLoading}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
        >
          {isLoading ? 'Saving...' : classroom ? 'Update Classroom' : 'Create Classroom'}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isLoading}
          className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 disabled:opacity-50"
        >
          Cancel
        </button>
      </div>
    </form>
  )
}
