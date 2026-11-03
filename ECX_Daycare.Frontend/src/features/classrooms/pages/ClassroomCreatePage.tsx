/**
 * Classroom create page
 */
import { useNavigate } from 'react-router-dom'
import { useClassrooms } from '../hooks/useClassrooms'
import { ClassroomForm } from '../components/ClassroomForm'
import { ClassroomFormInput } from '../types'

export const ClassroomCreatePage = () => {
  const navigate = useNavigate()
  const { createClassroom, loading } = useClassrooms()

  const handleSubmit = async (data: ClassroomFormInput) => {
    try {
      await createClassroom(data)
      navigate('/classrooms')
    } catch (error) {
      throw error
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-2xl mx-auto px-4">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">Create New Classroom</h1>
        <ClassroomForm onSubmit={handleSubmit} onCancel={() => navigate('/classrooms')} 
          isLoading={loading} />
      </div>
    </div>
  )
}
