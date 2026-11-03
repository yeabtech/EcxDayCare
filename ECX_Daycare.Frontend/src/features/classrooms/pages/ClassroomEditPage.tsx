/**
 * Classroom edit page
 */
import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useClassrooms } from '../hooks/useClassrooms'
import { ClassroomForm } from '../components/ClassroomForm'
import { ClassroomFormInput } from '../types'

export const ClassroomEditPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { currentClassroom, loading, fetchClassroom, updateClassroom } = useClassrooms()

  useEffect(() => {
    if (id) {
      fetchClassroom(id)
    }
  }, [id, fetchClassroom])

  const handleSubmit = async (data: ClassroomFormInput) => {
    if (!id) return
    try {
      await updateClassroom(id, data)
      navigate(`/classrooms/${id}`)
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

  if (!currentClassroom) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-2xl mx-auto px-4 text-center">
          <p className="text-red-500">Classroom not found</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-2xl mx-auto px-4">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">
          Edit Classroom - {currentClassroom.name}
        </h1>
        <ClassroomForm classroom={currentClassroom} onSubmit={handleSubmit} 
          onCancel={() => navigate(`/classrooms/${id}`)} isLoading={loading} />
      </div>
    </div>
  )
}
