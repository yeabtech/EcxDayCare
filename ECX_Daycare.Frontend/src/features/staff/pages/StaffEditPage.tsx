/**
 * Staff edit page component
 */
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { StaffForm } from '../components/StaffForm'
import { useStaff } from '../hooks/useStaff'
import { StaffFormInput } from '../types'
import { ArrowLeft } from 'lucide-react'

export const StaffEditPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { currentStaff, loading, error: hookError, fetchStaffMember, updateStaff } = useStaff()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (id) {
      fetchStaffMember(id)
    }
  }, [id, fetchStaffMember])

  const handleSubmit = async (data: StaffFormInput) => {
    if (!id) return
    setIsLoading(true)
    setError(null)
    try {
      await updateStaff(id, data)
      navigate(`/staff/${id}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update staff member')
    } finally {
      setIsLoading(false)
    }
  }

  if (loading && !currentStaff) {
    return <div className="p-8 text-center text-gray-500">Loading staff details...</div>
  }

  if (hookError && !currentStaff) {
    return (
      <div className="max-w-4xl mx-auto space-y-4">
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {hookError}
        </div>
        <button
          onClick={() => navigate('/staff')}
          className="text-blue-600 hover:underline text-sm font-medium"
        >
          &larr; Back to Staff List
        </button>
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate(id ? `/staff/${id}` : '/staff')}
          className="p-2 hover:bg-gray-100 rounded-full transition-colors text-gray-500"
          title="Back to staff details"
        >
          <ArrowLeft size={20} />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Edit Staff Member: {currentStaff?.firstName} {currentStaff?.lastName}
          </h1>
          <p className="text-sm text-gray-600 mt-1">
            Update employee details, shift schedule, or classroom assignment
          </p>
        </div>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      {currentStaff && (
        <StaffForm
          staff={currentStaff}
          onSubmit={handleSubmit}
          onCancel={() => navigate(id ? `/staff/${id}` : '/staff')}
          isLoading={isLoading}
        />
      )}
    </div>
  )
}
