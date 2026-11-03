/**
 * Classroom detail page
 */
import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useClassrooms } from '../hooks/useClassrooms'

export const ClassroomDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { currentClassroom, loading, fetchClassroom } = useClassrooms()

  useEffect(() => {
    if (id) {
      fetchClassroom(id)
    }
  }, [id, fetchClassroom])

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-3xl mx-auto px-4 text-center">
          <p className="text-gray-500">Loading...</p>
        </div>
      </div>
    )
  }

  if (!currentClassroom) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-3xl mx-auto px-4 text-center">
          <p className="text-red-500">Classroom not found</p>
        </div>
      </div>
    )
  }

  const capacityPercentage = (currentClassroom.currentEnrollment / currentClassroom.maxCapacity) * 100

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-3xl mx-auto px-4">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">{currentClassroom.name}</h1>
          <div className="flex gap-4">
            <button
              onClick={() => navigate(`/classrooms/${id}/edit`)}
              className="px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 font-medium"
            >
              Edit
            </button>
            <button
              onClick={() => navigate('/classrooms')}
              className="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 font-medium"
            >
              Back to List
            </button>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow divide-y">
          <div className="grid grid-cols-2 gap-6 p-6">
            <div>
              <p className="text-sm text-gray-600 mb-1">Age Range</p>
              <p className="text-lg font-medium text-gray-900">
                {currentClassroom.minAgeMonths} - {currentClassroom.maxAgeMonths} months
              </p>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-1">Status</p>
              <span
                className={`inline-block px-3 py-1 rounded-full text-sm font-medium ${
                  currentClassroom.status === 'Active'
                    ? 'bg-green-100 text-green-800'
                    : 'bg-gray-100 text-gray-800'
                }`}
              >
                {currentClassroom.status}
              </span>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-6 p-6">
            <div>
              <p className="text-sm text-gray-600 mb-1">Capacity</p>
              <p className="text-lg font-medium text-gray-900">
                {currentClassroom.maxCapacity} children
              </p>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-1">Current Enrollment</p>
              <p className="text-lg font-medium text-gray-900">
                {currentClassroom.currentEnrollment} children
              </p>
            </div>
          </div>

          <div className="p-6">
            <p className="text-sm text-gray-600 mb-3">Enrollment Status</p>
            <div className="flex items-center gap-4">
              <div className="flex-1 bg-gray-200 rounded-full h-3">
                <div
                  className={`h-3 rounded-full transition-all ${
                    capacityPercentage >= 80
                      ? 'bg-red-500'
                      : capacityPercentage >= 50
                        ? 'bg-yellow-500'
                        : 'bg-green-500'
                  }`}
                  style={{ width: `${Math.min(capacityPercentage, 100)}%` }}
                />
              </div>
              <span className="text-sm font-medium text-gray-700">
                {capacityPercentage.toFixed(0)}%
              </span>
            </div>
            <p className="text-xs text-gray-500 mt-2">
              {currentClassroom.availableSpots} spot{currentClassroom.availableSpots !== 1 ? 's' : ''} available
            </p>
          </div>

          <div className="p-6">
            <p className="text-sm text-gray-600 mb-1">Lead Teacher</p>
            <p className="text-lg font-medium text-gray-900">
              {currentClassroom.leadTeacherId || 'Not assigned'}
            </p>
          </div>

          {currentClassroom.description && (
            <div className="p-6">
              <p className="text-sm text-gray-600 mb-1">Description</p>
              <p className="text-lg text-gray-700">{currentClassroom.description}</p>
            </div>
          )}

          <div className="grid grid-cols-2 gap-6 p-6">
            <div>
              <p className="text-sm text-gray-600 mb-1">Created</p>
              <p className="text-lg font-medium text-gray-900">
                {new Date(currentClassroom.createdAt).toLocaleDateString()}
              </p>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-1">Last Updated</p>
              <p className="text-lg font-medium text-gray-900">
                {new Date(currentClassroom.updatedAt).toLocaleDateString()}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
