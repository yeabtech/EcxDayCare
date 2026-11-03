/**
 * ChildrenListPage
 * Main page for managing children list with search, pagination, and actions
 */
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useChildren } from '../hooks/useChildren'
import { ChildCard } from '../components/ChildCard'
import { EmptyState } from '../components/EmptyState'
import { LoadingState } from '../components/LoadingState'
import { DeleteConfirmation } from '../components/DeleteConfirmation'
import { Search, Plus } from 'lucide-react'

export const ChildrenListPage = () => {
  const navigate = useNavigate()
  const {
    children,
    isLoading,
    isDeleting,
    error,
    success,
    fetchChildren,
    deleteChild,
    clearError,
    clearSuccess,
  } = useChildren()

  const [searchTerm, setSearchTerm] = useState('')
  const [page, setPage] = useState(1)
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false)
  const [childToDelete, setChildToDelete] = useState<{ id: string; name: string } | null>(null)

  const pageSize = 10

  // Fetch children on mount and when params change
  useEffect(() => {
    fetchChildren({
      page,
      pageSize,
      search: searchTerm || undefined,
    })
  }, [page, searchTerm, fetchChildren])

  // Auto-clear success message
  useEffect(() => {
    if (success) {
      const timer = setTimeout(clearSuccess, 3000)
      return () => clearTimeout(timer)
    }
  }, [success, clearSuccess])

  const handleSearch = (value: string) => {
    setSearchTerm(value)
    setPage(1) // Reset to first page on search
  }

  const handleCreateChild = () => {
    navigate('/children/create')
  }

  const handleViewChild = (id: string) => {
    navigate(`/children/${id}`)
  }

  const handleDeleteClick = (id: string, name: string) => {
    setChildToDelete({ id, name })
    setShowDeleteConfirm(true)
  }

  const handleConfirmDelete = async () => {
    if (!childToDelete) return
    try {
      await deleteChild(childToDelete.id)
      setShowDeleteConfirm(false)
      setChildToDelete(null)
      // Refresh the list
      fetchChildren({ page, pageSize, search: searchTerm || undefined })
    } catch (err) {
      console.error('Delete failed:', err)
    }
  }

  const totalPages = children ? Math.ceil(children.Total / pageSize) : 0

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Children</h1>
          <p className="text-gray-600 mt-1">
            {children ? `Total: ${children.Total}` : 'Loading...'}
          </p>
        </div>
        <button onClick={handleCreateChild} className="btn-primary flex items-center gap-2 justify-center sm:justify-start">
          <Plus size={20} />
          Add Child
        </button>
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

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-3 top-3 text-gray-400" size={20} />
        <input
          type="text"
          placeholder="Search by name or parent name..."
          value={searchTerm}
          onChange={(e) => handleSearch(e.target.value)}
          className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
        />
      </div>

      {/* Content */}
      {isLoading && !children ? (
        <LoadingState count={5} />
      ) : children && children.Children.length > 0 ? (
        <>
          {/* List */}
          <div className="space-y-3">
            {children.Children.map((child) => (
              <div key={child.Id} className="flex items-stretch gap-2 group">
                <div className="flex-1">
                  <ChildCard child={child} onView={handleViewChild} />
                </div>
                <button
                  onClick={() => handleDeleteClick(child.Id, `${child.FirstName} ${child.LastName}`)}
                  className="hidden group-hover:block px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors font-medium"
                >
                  Delete
                </button>
              </div>
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between pt-4 border-t">
              <p className="text-sm text-gray-600">
                Page {page} of {totalPages}
              </p>
              <div className="flex gap-2">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Previous
                </button>
                <button
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </>
      ) : (
        <EmptyState
          title="No children found"
          description={
            searchTerm
              ? 'No children match your search. Try a different search term.'
              : 'Start by adding your first child'
          }
          action={{
            label: 'Add Child',
            onClick: handleCreateChild,
          }}
        />
      )}

      {/* Delete Confirmation Dialog */}
      <DeleteConfirmation
        isOpen={showDeleteConfirm}
        isDeleting={isDeleting}
        title="Delete Child"
        description="Permanently remove this child from the system"
        itemName={childToDelete?.name || ''}
        onConfirm={handleConfirmDelete}
        onCancel={() => {
          setShowDeleteConfirm(false)
          setChildToDelete(null)
        }}
      />
    </div>
  )
}
