/**
 * Parent create page
 */
import { useNavigate } from 'react-router-dom'
import { useParents } from '../hooks/useParents'
import { ParentForm } from '../components/ParentForm'
import { ParentFormInput } from '../types'

export const ParentCreatePage = () => {
  const navigate = useNavigate()
  const { createParent, loading } = useParents()

  const handleSubmit = async (data: ParentFormInput) => {
    try {
      await createParent(data)
      navigate('/parents')
    } catch (error) {
      throw error
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-2xl mx-auto px-4">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">Create New Parent</h1>
        <ParentForm onSubmit={handleSubmit} onCancel={() => navigate('/parents')} 
          isLoading={loading} />
      </div>
    </div>
  )
}
