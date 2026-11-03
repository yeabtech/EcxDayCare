/**
 * Staff create page component
 */
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { StaffForm } from '../components/StaffForm'
import { useStaff } from '../hooks/useStaff'
import { StaffFormInput } from '../types'
import { ArrowLeft } from 'lucide-react'

export const StaffCreatePage = () => {
  const navigate = useNavigate()
  const { createStaff } = useStaff()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (data: StaffFormInput) => {
    setIsLoading(true)
    setError(null)
    try {
      const created = await createStaff(data)
      navigate(`/staff/${created.id}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create staff member')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/staff')}
          className="p-2 hover:bg-gray-100 rounded-full transition-colors text-gray-500"
          title="Back to staff list"
        >
          <ArrowLeft size={20} />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Add New Staff Member</h1>
          <p className="text-sm text-gray-600 mt-1">
            Create an employee profile, role, shifts, and assign a classroom
          </p>
        </div>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      <StaffForm
        onSubmit={handleSubmit}
        onCancel={() => navigate('/staff')}
        isLoading={isLoading}
      />
    </div>
  )
}
