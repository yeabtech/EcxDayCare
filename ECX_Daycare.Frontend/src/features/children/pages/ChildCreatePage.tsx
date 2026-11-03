/**
 * ChildCreatePage
 * Page for creating a new child
 */
import { useNavigate } from 'react-router-dom'
import { useChildren } from '../hooks/useChildren'
import { ChildForm } from '../components/ChildForm'
import { ChildFormInput } from '../types'
import { ArrowLeft } from 'lucide-react'

export const ChildCreatePage = () => {
  const navigate = useNavigate()
  const { isCreating, error, success, createChild, clearError } = useChildren()

  const handleSubmit = async (data: ChildFormInput) => {
    try {
      await createChild(data)
      // Navigate back to list after successful creation
      setTimeout(() => navigate('/children'), 1500)
    } catch (err) {
      console.error('Creation failed:', err)
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/children')}
          className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
        >
          <ArrowLeft size={20} />
        </button>
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Add New Child</h1>
          <p className="text-gray-600 mt-1">Fill in the information below to create a new child record</p>
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
          <button
            onClick={clearError}
            className="text-red-600 hover:text-red-700 font-medium"
          >
            Dismiss
          </button>
        </div>
      )}

      {/* Form */}
      <div className="card p-6">
        <ChildForm
          onSubmit={handleSubmit}
          isLoading={isCreating}
          onCancel={() => navigate('/children')}
        />
      </div>
    </div>
  )
}
