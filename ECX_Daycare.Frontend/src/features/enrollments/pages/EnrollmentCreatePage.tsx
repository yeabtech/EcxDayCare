/**
 * Enrollment create page
 */
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { EnrollmentForm } from '../components/EnrollmentForm'
import { useEnrollments } from '../hooks/useEnrollments'
import { EnrollmentFormInput } from '../types'
import { ArrowLeft } from 'lucide-react'

export const EnrollmentCreatePage = () => {
  const navigate = useNavigate()
  const { createEnrollment } = useEnrollments()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (data: EnrollmentFormInput) => {
    setIsLoading(true)
    setError(null)
    try {
      const created = await createEnrollment(data)
      navigate(`/enrollments/${created.id}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create enrollment')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/enrollments')}
          className="p-2 hover:bg-gray-100 rounded-full transition-colors text-gray-500"
          title="Back to enrollments"
        >
          <ArrowLeft size={20} />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">New Enrollment</h1>
          <p className="text-sm text-gray-600 mt-1">
            Enroll a child in a program and record compliance documents
          </p>
        </div>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      <EnrollmentForm
        onSubmit={handleSubmit}
        onCancel={() => navigate('/enrollments')}
        isLoading={isLoading}
      />
    </div>
  )
}
