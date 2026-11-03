/**
 * Parent edit page
 */
import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useParents } from '../hooks/useParents'
import { ParentForm } from '../components/ParentForm'
import { ParentFormInput } from '../types'

export const ParentEditPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { currentParent, loading, fetchParent, updateParent } = useParents()

  useEffect(() => {
    if (id) {
      fetchParent(id)
    }
  }, [id, fetchParent])

  const handleSubmit = async (data: ParentFormInput) => {
    if (!id) return
    try {
      await updateParent(id, data)
      navigate(`/parents/${id}`)
    } catch (error) {
      throw error
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-2xl mx-auto px-4 text-center">
          <p className="text-gray-500">Loading...</p>
        </div>
      </div>
    )
  }

  if (!currentParent) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-2xl mx-auto px-4 text-center">
          <p className="text-red-500">Parent not found</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-2xl mx-auto px-4">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">
          Edit Parent - {currentParent.firstName} {currentParent.lastName}
        </h1>
        <ParentForm parent={currentParent} onSubmit={handleSubmit} 
          onCancel={() => navigate(`/parents/${id}`)} isLoading={loading} />
      </div>
    </div>
  )
}
