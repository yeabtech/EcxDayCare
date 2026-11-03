/**
 * Staff list page component
 */
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useStaff } from '../hooks/useStaff'
import { Staff } from '../types'
import { Plus, Search, Trash2, Edit2, Eye, UserCheck, Clock, MapPin } from 'lucide-react'

interface DeleteConfirmState {
  isOpen: boolean
  staffId: string | null
  staffName: string | null
}

const getRoleBadgeClass = (role: string) => {
  switch (role) {
    case 'LeadTeacher':
      return 'bg-purple-100 text-purple-800'
    case 'Teacher':
      return 'bg-blue-100 text-blue-800'
    case 'Assistant':
      return 'bg-green-100 text-green-800'
    case 'Aide':
      return 'bg-amber-100 text-amber-800'
    case 'Director':
      return 'bg-rose-100 text-rose-800'
    case 'Admin':
      return 'bg-slate-100 text-slate-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
}

const formatRoleLabel = (role: string) => {
  switch (role) {
    case 'LeadTeacher':
      return 'Lead Teacher'
    case 'Assistant':
      return 'Assistant Teacher'
    default:
      return role
  }
}

export const StaffListPage = () => {
  const navigate = useNavigate()
  const { staff, loading, error, total, page, pageSize, fetchStaff, deleteStaff } = useStaff()
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedRole, setSelectedRole] = useState('')
  const [deleteConfirm, setDeleteConfirm] = useState<DeleteConfirmState>({
    isOpen: false,
    staffId: null,
    staffName: null,
  })
  const [isDeleting, setIsDeleting] = useState(false)

  useEffect(() => {
    fetchStaff({ page: 1, pageSize: 10 })
  }, [fetchStaff])

  const handleSearch = async (e?: React.FormEvent) => {
    if (e) e.preventDefault()
    await fetchStaff({
      page: 1,
      pageSize,
      search: searchTerm.trim() || undefined,
      role: selectedRole || undefined,
    })
  }

  const handleRoleFilter = async (role: string) => {
    setSelectedRole(role)
    await fetchStaff({
      page: 1,
      pageSize,
      search: searchTerm.trim() || undefined,
      role: role || undefined,
    })
  }

  const handlePageChange = (newPage: number) => {
    fetchStaff({
      page: newPage,
      pageSize,
      search: searchTerm.trim() || undefined,
      role: selectedRole || undefined,
    })
  }

  const handleDeleteClick = (s: Staff) => {
    setDeleteConfirm({
      isOpen: true,
      staffId: s.id,
      staffName: `${s.firstName} ${s.lastName}`,
    })
  }

  const handleConfirmDelete = async () => {
    if (!deleteConfirm.staffId) return
    setIsDeleting(true)
    try {
      await deleteStaff(deleteConfirm.staffId)
      setDeleteConfirm({ isOpen: false, staffId: null, staffName: null })
      fetchStaff({ page, pageSize, search: searchTerm || undefined })
    } catch {
      // Handled by hook
    } finally {
      setIsDeleting(false)
    }
  }

  const totalPages = Math.ceil(total / pageSize)

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Staff Members</h1>
          <p className="text-sm text-gray-600 mt-1">
            Manage teachers, assistants, aides, and administrative staff
          </p>
        </div>
        <button
          onClick={() => navigate('/staff/create')}
          className="inline-flex items-center gap-2 px-4 py-2 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          <Plus size={18} />
          Add Staff Member
        </button>
      </div>

      {/* Filters & Search */}
      <div className="bg-white p-4 rounded-lg shadow flex flex-col md:flex-row gap-4 items-center justify-between">
        <form onSubmit={handleSearch} className="flex-1 w-full flex gap-2">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-2.5 text-gray-400" size={18} />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search staff by name, email, phone, or certifications..."
              className="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 text-sm"
            />
          </div>
          <button
            type="submit"
            className="px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-md text-sm font-medium"
          >
            Search
          </button>
        </form>

        <div className="flex items-center gap-2 w-full md:w-auto">
          <label className="text-sm font-medium text-gray-700 whitespace-nowrap">Filter Role:</label>
          <select
            value={selectedRole}
            onChange={(e) => handleRoleFilter(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">All Roles</option>
            <option value="LeadTeacher">Lead Teacher</option>
            <option value="Teacher">Teacher</option>
            <option value="Assistant">Assistant Teacher</option>
            <option value="Aide">Classroom Aide</option>
            <option value="Director">Director</option>
            <option value="Admin">Admin</option>
          </select>
        </div>
      </div>

      {/* Error alert */}
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      {/* Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        {loading ? (
          <div className="p-8 text-center text-gray-500">Loading staff members...</div>
        ) : staff.length === 0 ? (
          <div className="p-8 text-center text-gray-500">
            <UserCheck className="mx-auto h-12 w-12 text-gray-300 mb-3" />
            <p className="text-base font-medium">No staff members found</p>
            <p className="text-sm mt-1">Get started by creating a new staff profile.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Staff Member
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Role
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Assigned Classroom
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Contact
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Shift Hours
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {staff.map((s) => (
                  <tr key={s.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div className="h-10 w-10 flex-shrink-0 bg-blue-100 rounded-full flex items-center justify-center font-bold text-blue-600 text-sm">
                          {s.firstName[0]}
                          {s.lastName[0]}
                        </div>
                        <div className="ml-4">
                          <div className="text-sm font-medium text-gray-900">
                            {s.firstName} {s.lastName}
                          </div>
                          {s.certifications && (
                            <div className="text-xs text-gray-500 truncate max-w-xs" title={s.certifications}>
                              {s.certifications}
                            </div>
                          )}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-2.5 py-0.5 inline-flex text-xs leading-5 font-semibold rounded-full ${getRoleBadgeClass(s.role)}`}>
                        {formatRoleLabel(s.role)}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {s.classroomName || s.classroomId ? (
                        <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-50 text-emerald-700 border border-emerald-200">
                          <MapPin size={12} />
                          {s.classroomName || 'Assigned'}
                        </span>
                      ) : (
                        <span className="text-xs text-gray-400 italic">Unassigned</span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      <div>{s.email}</div>
                      <div className="text-xs text-gray-400">{s.phone}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      <div className="inline-flex items-center gap-1 text-xs text-gray-700 bg-gray-50 px-2 py-1 rounded">
                        <Clock size={12} className="text-gray-400" />
                        {s.shiftStart.substring(0, 5)} - {s.shiftEnd.substring(0, 5)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => navigate(`/staff/${s.id}`)}
                          className="text-gray-400 hover:text-blue-600 p-1"
                          title="View Details"
                        >
                          <Eye size={18} />
                        </button>
                        <button
                          onClick={() => navigate(`/staff/${s.id}/edit`)}
                          className="text-gray-400 hover:text-amber-600 p-1"
                          title="Edit"
                        >
                          <Edit2 size={18} />
                        </button>
                        <button
                          onClick={() => handleDeleteClick(s)}
                          className="text-gray-400 hover:text-red-600 p-1"
                          title="Delete"
                        >
                          <Trash2 size={18} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination */}
        {total > 0 && (
          <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
            <div className="text-sm text-gray-700">
              Showing <span className="font-medium">{(page - 1) * pageSize + 1}</span> to{' '}
              <span className="font-medium">{Math.min(page * pageSize, total)}</span> of{' '}
              <span className="font-medium">{total}</span> staff members
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => handlePageChange(page - 1)}
                disabled={page <= 1}
                className="px-3 py-1 border border-gray-300 rounded text-sm disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Previous
              </button>
              <span className="px-3 py-1 text-sm text-gray-700 self-center">
                Page {page} of {totalPages || 1}
              </span>
              <button
                onClick={() => handlePageChange(page + 1)}
                disabled={page >= totalPages}
                className="px-3 py-1 border border-gray-300 rounded text-sm disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Delete Confirmation Modal */}
      {deleteConfirm.isOpen && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg p-6 max-w-sm w-full shadow-xl">
            <h3 className="text-lg font-bold text-gray-900 mb-2">Delete Staff Member</h3>
            <p className="text-sm text-gray-600 mb-4">
              Are you sure you want to delete{' '}
              <span className="font-semibold">{deleteConfirm.staffName}</span>? This action cannot
              be undone.
            </p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setDeleteConfirm({ isOpen: false, staffId: null, staffName: null })}
                disabled={isDeleting}
                className="px-4 py-2 border border-gray-300 rounded-md text-sm text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={handleConfirmDelete}
                disabled={isDeleting}
                className="px-4 py-2 bg-red-600 text-white rounded-md text-sm hover:bg-red-700 disabled:opacity-50"
              >
                {isDeleting ? 'Deleting...' : 'Delete'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
