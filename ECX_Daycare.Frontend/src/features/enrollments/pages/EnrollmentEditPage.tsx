/**
 * Enrollment edit page
 */
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { EnrollmentForm } from '../components/EnrollmentForm'
import { useEnrollments } from '../hooks/useEnrollments'
import { EnrollmentFormInput } from '../types'
import { ArrowLeft } from 'lucide-react'

export const EnrollmentEditPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { currentEnrollment, loading, error: hookError, fetchEnrollment, updateEnrollment } =
    useEnrollments()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (id) {
      fetchEnrollment(id)
    }
  }, [id, fetchEnrollment])

  const handleSubmit = async (data: EnrollmentFormInput) => {
    if (!id) return
    setIsLoading(true)
    setError(null)
    try {
      await updateEnrollment(id, data)
      navigate(`/enrollments/${id}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update enrollment')
    } finally {
      setIsLoading(false)
    }
  }

  if (loading && !currentEnrollment) {
    return <div className="p-8 text-center text-gray-500">Loading enrollment...</div>
  }

  if (hookError && !currentEnrollment) {
    return (
      <div className="max-w-4xl mx-auto space-y-4">
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {hookError}
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

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate(id ? `/enrollments/${id}` : '/enrollments')}
          className="p-2 hover:bg-gray-100 rounded-full transition-colors text-gray-500"
          title="Back"
        >
          <ArrowLeft size={20} />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Edit Enrollment</h1>
          <p className="text-sm text-gray-600 mt-1">
            Update program details, schedule, or compliance documents
          </p>
        </div>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      {currentEnrollment && (
        <EnrollmentForm
          enrollment={{
            childId: currentEnrollment.childId,
            classroomId: currentEnrollment.classroomId,
            programType: currentEnrollment.programType,
            daysOfWeek: currentEnrollment.daysOfWeek,
            startDate: currentEnrollment.startDate,
            endDate: currentEnrollment.endDate,
            contractSigned: currentEnrollment.contractSigned,
            immunizationRecord: currentEnrollment.immunizationRecord,
            birthCertificate: currentEnrollment.birthCertificate,
            medicalForm: currentEnrollment.medicalForm,
            status: currentEnrollment.status,
            endReason: currentEnrollment.endReason,
          }}
          lockedChildId={currentEnrollment.childId}
          onSubmit={handleSubmit}
          onCancel={() => navigate(id ? `/enrollments/${id}` : '/enrollments')}
          isLoading={isLoading}
        />
      )}
    </div>
  )
}
