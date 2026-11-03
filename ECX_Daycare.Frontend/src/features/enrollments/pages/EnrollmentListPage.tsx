/**
 * Enrollment list page — browse, filter, and manage all enrollments
 */
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useEnrollments } from '../hooks/useEnrollments'
import { Enrollment, EnrollmentListParams } from '../types'
import {
  Plus,
  BookOpen,
  Eye,
  Edit2,
  Search,
  CheckCircle,
  XCircle,
  Clock,
} from 'lucide-react'

const STATUS_COLORS: Record<string, string> = {
  Active: 'bg-emerald-100 text-emerald-800',
  Pending: 'bg-amber-100 text-amber-800',
  Ended: 'bg-gray-100 text-gray-600',
  Cancelled: 'bg-red-100 text-red-700',
}

const PROGRAM_LABELS: Record<string, string> = {
  FullTime: 'Full Time',
  PartTime: 'Part Time',
  HalfDay: 'Half Day',
  Occasional: 'Occasional',
}

const PROGRAM_COLORS: Record<string, string> = {
  FullTime: 'bg-blue-100 text-blue-800',
  PartTime: 'bg-purple-100 text-purple-800',
  HalfDay: 'bg-teal-100 text-teal-800',
  Occasional: 'bg-orange-100 text-orange-800',
}

const DocBadge = ({ done, label }: { done: boolean; label: string }) => (
  <span
    title={label}
    className={`inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-[11px] font-medium ${
      done ? 'bg-emerald-50 text-emerald-700' : 'bg-gray-100 text-gray-400'
    }`}
  >
    {done ? <CheckCircle size={10} /> : <XCircle size={10} />}
    {label}
  </span>
)

export const EnrollmentListPage = () => {
  const navigate = useNavigate()
  const {
    enrollments,
    loading,
    error,
    total,
    page,
    pageSize,
    fetchEnrollments,
  } = useEnrollments()

  const [filters, setFilters] = useState<EnrollmentListParams>({
    page: 1,
    pageSize: 10,
    status: '',
    programType: '',
  })

  useEffect(() => {
    fetchEnrollments({ page: 1, pageSize: 10 })
  }, [fetchEnrollments])

  const applyFilters = (updated: Partial<EnrollmentListParams>) => {
    const next = { ...filters, ...updated, page: 1 }
    setFilters(next)
    fetchEnrollments({
      page: next.page,
      pageSize: next.pageSize,
      status: next.status || undefined,
      programType: next.programType || undefined,
    })
  }

  const handlePageChange = (newPage: number) => {
    const next = { ...filters, page: newPage }
    setFilters(next)
    fetchEnrollments({
      ...next,
      status: next.status || undefined,
      programType: next.programType || undefined,
    })
  }

  const totalPages = Math.ceil(total / pageSize)

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Enrollments</h1>
          <p className="text-sm text-gray-600 mt-1">
            Manage child enrollment records, program types, and compliance documents
          </p>
        </div>
        <button
          id="btn-create-enrollment"
          onClick={() => navigate('/enrollments/create')}
          className="inline-flex items-center gap-2 px-4 py-2 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          <Plus size={18} />
          New Enrollment
        </button>
      </div>

      {/* Filters */}
      <div className="bg-white p-4 rounded-lg shadow flex flex-col md:flex-row gap-4 items-center">
        <div className="flex items-center gap-2 w-full md:w-auto">
          <label className="text-sm font-medium text-gray-700 whitespace-nowrap">Status:</label>
          <select
            id="filter-status"
            value={filters.status || ''}
            onChange={(e) => applyFilters({ status: e.target.value })}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">All Statuses</option>
            <option value="Active">Active</option>
            <option value="Pending">Pending</option>
            <option value="Ended">Ended</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>

        <div className="flex items-center gap-2 w-full md:w-auto">
          <label className="text-sm font-medium text-gray-700 whitespace-nowrap">Program:</label>
          <select
            id="filter-program"
            value={filters.programType || ''}
            onChange={(e) => applyFilters({ programType: e.target.value })}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">All Programs</option>
            <option value="FullTime">Full Time</option>
            <option value="PartTime">Part Time</option>
            <option value="HalfDay">Half Day</option>
            <option value="Occasional">Occasional</option>
          </select>
        </div>

        <div className="flex items-center gap-1 text-xs text-gray-400 ml-auto">
          <Search size={14} />
          <span>{total} enrollment{total !== 1 ? 's' : ''} found</span>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      {/* Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        {loading ? (
          <div className="p-8 text-center text-gray-500">Loading enrollments...</div>
        ) : enrollments.length === 0 ? (
          <div className="p-10 text-center text-gray-500">
            <BookOpen className="mx-auto h-12 w-12 text-gray-300 mb-3" />
            <p className="text-base font-medium">No enrollments found</p>
            <p className="text-sm mt-1">Create an enrollment to get started.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  {['Child', 'Program', 'Schedule', 'Dates', 'Documents', 'Status', 'Actions'].map(
                    (h) => (
                      <th
                        key={h}
                        className={`px-5 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider ${
                          h === 'Actions' ? 'text-right' : ''
                        }`}
                      >
                        {h}
                      </th>
                    )
                  )}
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {enrollments.map((e: Enrollment) => (
                  <tr key={e.id} className="hover:bg-gray-50 transition-colors">
                    {/* Child */}
                    <td className="px-5 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-3">
                        <div className="h-9 w-9 rounded-full bg-indigo-100 text-indigo-600 flex items-center justify-center text-xs font-bold flex-shrink-0">
                          {(e.childName || 'U').substring(0, 2).toUpperCase()}
                        </div>
                        <div>
                          <div className="text-sm font-medium text-gray-900">
                            {e.childName ?? <span className="text-gray-400 italic">Unknown</span>}
                          </div>
                          {e.classroomName && (
                            <div className="text-xs text-gray-400">{e.classroomName}</div>
                          )}
                        </div>
                      </div>
                    </td>

                    {/* Program */}
                    <td className="px-5 py-4 whitespace-nowrap">
                      <span
                        className={`px-2.5 py-0.5 inline-flex text-xs font-semibold rounded-full ${
                          PROGRAM_COLORS[e.programType] ?? 'bg-gray-100 text-gray-700'
                        }`}
                      >
                        {PROGRAM_LABELS[e.programType] ?? e.programType}
                      </span>
                    </td>

                    {/* Schedule */}
                    <td className="px-5 py-4">
                      <div className="text-xs text-gray-600 max-w-[140px]">{e.daysOfWeek}</div>
                    </td>

                    {/* Dates */}
                    <td className="px-5 py-4 whitespace-nowrap text-xs text-gray-600">
                      <div className="flex items-center gap-1">
                        <Clock size={12} className="text-gray-400" />
                        {new Date(e.startDate).toLocaleDateString()}
                      </div>
                      {e.endDate && (
                        <div className="text-gray-400 mt-0.5">
                          → {new Date(e.endDate).toLocaleDateString()}
                        </div>
                      )}
                    </td>

                    {/* Documents */}
                    <td className="px-5 py-4">
                      <div className="flex flex-wrap gap-1">
                        <DocBadge done={e.contractSigned} label="Contract" />
                        <DocBadge done={e.immunizationRecord} label="Imm." />
                        <DocBadge done={e.birthCertificate} label="Birth" />
                        <DocBadge done={e.medicalForm} label="Medical" />
                      </div>
                    </td>

                    {/* Status */}
                    <td className="px-5 py-4 whitespace-nowrap">
                      <span
                        className={`px-2.5 py-0.5 inline-flex text-xs font-semibold rounded-full ${
                          STATUS_COLORS[e.status] ?? 'bg-gray-100 text-gray-700'
                        }`}
                      >
                        {e.status}
                      </span>
                    </td>

                    {/* Actions */}
                    <td className="px-5 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => navigate(`/enrollments/${e.id}`)}
                          className="text-gray-400 hover:text-blue-600 p-1"
                          title="View Details"
                        >
                          <Eye size={18} />
                        </button>
                        <button
                          onClick={() => navigate(`/enrollments/${e.id}/edit`)}
                          className="text-gray-400 hover:text-amber-600 p-1"
                          title="Edit"
                        >
                          <Edit2 size={18} />
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
          <div className="bg-white px-5 py-3 flex items-center justify-between border-t border-gray-200">
            <div className="text-sm text-gray-700">
              Showing{' '}
              <span className="font-medium">{(page - 1) * pageSize + 1}</span> to{' '}
              <span className="font-medium">{Math.min(page * pageSize, total)}</span> of{' '}
              <span className="font-medium">{total}</span>
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
    </div>
  )
}
