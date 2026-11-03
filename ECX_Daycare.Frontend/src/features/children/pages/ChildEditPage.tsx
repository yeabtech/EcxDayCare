/**
 * ChildEditPage
 * Loads a child (and its linked parents) and edits every available field.
 */
import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useChildren } from '../hooks/useChildren'
import { ChildForm } from '../components/ChildForm'
import { ChildFormInput } from '../types'
import { ArrowLeft } from 'lucide-react'

export const ChildEditPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const {
    child,
    isLoading,
    isUpdating,
    error,
    success,
    fetchChild,
    updateChild,
    clearError,
  } = useChildren()

  // Fetch the child (and its linked parents) on mount.
  useEffect(() => {
    if (id) fetchChild(id)
  }, [id, fetchChild])

  // Return to the details view once the update succeeds.
  useEffect(() => {
    if (success?.includes('updated')) {
      const timer = setTimeout(() => navigate(`/children/${id}`), 1200)
      return () => clearTimeout(timer)
    }
  }, [success, navigate, id])

  const handleSubmit = async (data: ChildFormInput) => {
    if (!id) return
    try {
      await updateChild(id, data)
    } catch (err) {
      console.error('Update failed:', err)
    }
  }

  if (isLoading && !child) {
    return (
      <div className="space-y-4">
        <div className="h-8 bg-gray-200 rounded w-1/3 animate-pulse" />
        <div className="card p-6 space-y-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="h-10 bg-gray-200 rounded animate-pulse" />
          ))}
        </div>
      </div>
    )
  }

  if (!child) {
    return (
      <div className="card p-6 text-center">
        <p className="text-gray-600">Child not found</p>
        <button onClick={() => navigate('/children')} className="mt-4 btn-primary">
          Back to List
        </button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate(`/children/${id}`)}
          className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
        >
          <ArrowLeft size={20} />
        </button>
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Edit Child</h1>
          <p className="text-gray-600 mt-1">
            {child.FirstName} {child.LastName}
          </p>
        </div>
      </div>

      {/* Success Message */}
      {success && (
        <div className="p-4 bg-green-50 border border-green-200 rounded-lg text-green-800">
          <p>{success}</p>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-red-800 flex justify-between items-center">
          <p>{error}</p>
          <button onClick={clearError} className="text-red-600 hover:text-red-700 font-medium">
            Dismiss
          </button>
        </div>
      )}

      {/* Form */}
      <div className="card p-6">
        <ChildForm
          child={child}
          onSubmit={handleSubmit}
          isLoading={isUpdating}
          onCancel={() => navigate(`/children/${id}`)}
        />
      </div>
    </div>
  )
}
