/**
 * Parent detail page
 */
import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useParents } from '../hooks/useParents'

export const ParentDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { currentParent, loading, fetchParent } = useParents()

  useEffect(() => {
    if (id) {
      fetchParent(id)
    }
  }, [id, fetchParent])

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-3xl mx-auto px-4 text-center">
          <p className="text-gray-500">Loading...</p>
        </div>
      </div>
    )
  }

  if (!currentParent) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-3xl mx-auto px-4 text-center">
          <p className="text-red-500">Parent not found</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-3xl mx-auto px-4">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            {currentParent.firstName} {currentParent.lastName}
          </h1>
          <div className="flex gap-4">
            <button
              onClick={() => navigate(`/parents/${id}/edit`)}
              className="px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 font-medium"
            >
              Edit
            </button>
            <button
              onClick={() => navigate('/parents')}
              className="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 font-medium"
            >
              Back to List
            </button>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow divide-y">
          <div className="grid grid-cols-2 gap-6 p-6">
            <div>
              <p className="text-sm text-gray-600 mb-1">First Name</p>
              <p className="text-lg font-medium text-gray-900">{currentParent.firstName}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-1">Last Name</p>
              <p className="text-lg font-medium text-gray-900">{currentParent.lastName}</p>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-6 p-6">
            <div>
              <p className="text-sm text-gray-600 mb-1">Email</p>
              <p className="text-lg font-medium text-gray-900">{currentParent.email}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-1">Phone Number</p>
              <p className="text-lg font-medium text-gray-900">{currentParent.phoneNumber}</p>
            </div>
          </div>

          <div className="p-6">
            <p className="text-sm text-gray-600 mb-1">Address</p>
            <p className="text-lg font-medium text-gray-900">{currentParent.address}</p>
          </div>

          <div className="grid grid-cols-2 gap-6 p-6">
            <div>
              <p className="text-sm text-gray-600 mb-1">Status</p>
              <span
                className={`inline-block px-3 py-1 rounded-full text-sm font-medium ${
                  currentParent.status === 'Active'
                    ? 'bg-green-100 text-green-800'
                    : 'bg-gray-100 text-gray-800'
                }`}
              >
                {currentParent.status}
              </span>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-1">Last Updated</p>
              <p className="text-lg font-medium text-gray-900">
                {new Date(currentParent.updatedAt).toLocaleDateString()}
              </p>
            </div>
          </div>

          {currentParent.notes && (
            <div className="p-6">
              <p className="text-sm text-gray-600 mb-1">Notes</p>
              <p className="text-lg text-gray-700">{currentParent.notes}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
