/**
 * Classroom list page component
 */
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useClassrooms } from '../hooks/useClassrooms'
import { Classroom } from '../types'

interface DeleteConfirmState {
  isOpen: boolean
  classroomId: string | null
  classroomName: string | null
}

export const ClassroomsListPage = () => {
  const navigate = useNavigate()
  const { classrooms, loading, error, total, page, pageSize, fetchClassrooms, deleteClassroom } =
    useClassrooms()
  const [searchTerm, setSearchTerm] = useState('')
  const [deleteConfirm, setDeleteConfirm] = useState<DeleteConfirmState>({
    isOpen: false,
    classroomId: null,
    classroomName: null,
  })
  const [isDeleting, setIsDeleting] = useState(false)

  useEffect(() => {
    fetchClassrooms({ page: 1, pageSize: 10 })
  }, [fetchClassrooms])

  const handleSearch = async () => {
    await fetchClassrooms({
      page: 1,
      pageSize: 10,
      search: searchTerm || undefined,
    })
  }

  const handlePageChange = (newPage: number) => {
    fetchClassrooms({
      page: newPage,
      pageSize,
      search: searchTerm || undefined,
    })
  }

  const handleDeleteClick = (classroom: Classroom) => {
    setDeleteConfirm({
      isOpen: true,
      classroomId: classroom.id,
      classroomName: classroom.name,
    })
  }

  const handleConfirmDelete = async () => {
    if (!deleteConfirm.classroomId) return

    setIsDeleting(true)
    try {
      await deleteClassroom(deleteConfirm.classroomId)
      setDeleteConfirm({ isOpen: false, classroomId: null, classroomName: null })
      await fetchClassrooms({ page: 1, pageSize: 10 })
    } finally {
      setIsDeleting(false)
    }
  }

  const handleCancelDelete = () => {
    setDeleteConfirm({ isOpen: false, classroomId: null, classroomName: null })
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-6xl mx-auto px-4">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Classrooms Management</h1>
          <button
            onClick={() => navigate('/classrooms/create')}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium"
          >
            + Add Classroom
          </button>
        </div>

        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <div className="flex gap-4 mb-6">
            <input
              type="text"
              placeholder="Search by name..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
              className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button
              onClick={handleSearch}
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Search
            </button>
          </div>

          {error && (
            <div className="mb-4 p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg">
              {error}
            </div>
          )}

          {loading && <div className="text-center py-8 text-gray-500">Loading...</div>}

          {!loading && (classrooms?.length ?? 0) === 0 && (
            <div className="text-center py-8 text-gray-500">No classrooms found</div>
          )}

          {!loading && (classrooms?.length ?? 0) > 0 && (
            <>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-3 px-4 font-semibold text-gray-700">Name</th>
                      <th className="text-left py-3 px-4 font-semibold text-gray-700">Age Range</th>
                      <th className="text-left py-3 px-4 font-semibold text-gray-700">
                        Enrollment
                      </th>
                      <th className="text-left py-3 px-4 font-semibold text-gray-700">Status</th>
                      <th className="text-left py-3 px-4 font-semibold text-gray-700">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(classrooms ?? []).map((classroom) => (
                      <tr key={classroom.id} className="border-b border-gray-100 hover:bg-gray-50">
                        <td className="py-3 px-4">
                          <span className="font-medium">{classroom.name}</span>
                        </td>
                        <td className="py-3 px-4 text-gray-600">
                          {classroom.minAgeMonths} - {classroom.maxAgeMonths} months
                        </td>
                        <td className="py-3 px-4 text-gray-600">
                          {classroom.currentEnrollment} / {classroom.maxCapacity}
                        </td>
                        <td className="py-3 px-4">
                          <span
                            className={`px-3 py-1 rounded-full text-sm font-medium ${
                              classroom.status === 'Active'
                                ? 'bg-green-100 text-green-800'
                                : 'bg-gray-100 text-gray-800'
                            }`}
                          >
                            {classroom.status}
                          </span>
                        </td>
                        <td className="py-3 px-4 flex gap-2">
                          <button
                            onClick={() => navigate(`/classrooms/${classroom.id}`)}
                            className="text-blue-600 hover:text-blue-800 font-medium text-sm"
                          >
                            View
                          </button>
                          <button
                            onClick={() => navigate(`/classrooms/${classroom.id}/edit`)}
                            className="text-green-600 hover:text-green-800 font-medium text-sm"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => handleDeleteClick(classroom)}
                            className="text-red-600 hover:text-red-800 font-medium text-sm"
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {total > pageSize && (
                <div className="mt-6 flex justify-between items-center">
                  <span className="text-sm text-gray-600">
                    Showing {(page - 1) * pageSize + 1} to{' '}
                    {Math.min(page * pageSize, total)} of {total}
                  </span>
                  <div className="flex gap-2">
                    <button
                      onClick={() => handlePageChange(page - 1)}
                      disabled={page === 1}
                      className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
                    >
                      Previous
                    </button>
                    <button
                      onClick={() => handlePageChange(page + 1)}
                      disabled={page * pageSize >= total}
                      className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
                    >
                      Next
                    </button>
                  </div>
                </div>
              )}
            </>
          )}
        </div>

        {deleteConfirm.isOpen && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg shadow-lg p-6 max-w-sm">
              <h2 className="text-xl font-bold mb-4">Confirm Delete</h2>
              <p className="text-gray-600 mb-6">
                Are you sure you want to delete <strong>{deleteConfirm.classroomName}</strong>?
              </p>
              <div className="flex gap-4 justify-end">
                <button
                  onClick={handleCancelDelete}
                  disabled={isDeleting}
                  className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 disabled:opacity-50"
                >
                  Cancel
                </button>
                <button
                  onClick={handleConfirmDelete}
                  disabled={isDeleting}
                  className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
                >
                  {isDeleting ? 'Deleting...' : 'Delete'}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
