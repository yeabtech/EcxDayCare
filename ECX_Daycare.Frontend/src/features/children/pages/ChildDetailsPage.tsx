/**
 * ChildDetailsPage
 * View, edit, and delete a single child
 */
import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useChildren } from '../hooks/useChildren'
import { DeleteConfirmation } from '../components/DeleteConfirmation'
import { ParentLink } from '../types'
import { childrenApi } from '../services/childrenApi'
import { formatDate, formatTime } from '@utils/index'
import { ArrowLeft, Edit2, Trash2, FileText, ClipboardCheck, Mail, Phone } from 'lucide-react'

export const ChildDetailsPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const {
    child,
    isLoading,
    isDeleting,
    error,
    success,
    fetchChild,
    deleteChild,
    clearError,
    clearSuccess,
  } = useChildren()

  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false)
  const [parents, setParents] = useState<ParentLink[]>([])

  // Fetch child on mount
  useEffect(() => {
    if (id) {
      fetchChild(id)
    }
  }, [id, fetchChild])

  // Load the parents linked to this child (ParentChildren join table).
  useEffect(() => {
    if (!id) return
    let active = true
    const loadParents = async () => {
      try {
        const links = await childrenApi.getChildParents(id)
        if (active) setParents(links)
      } catch {
        if (active) setParents([])
      }
    }
    loadParents()
    return () => {
      active = false
    }
  }, [id, success])

  // Auto-clear success message
  useEffect(() => {
    if (success) {
      const timer = setTimeout(() => {
        clearSuccess()
        if (success.includes('deleted')) {
          navigate('/children')
        }
      }, 2000)
      return () => clearTimeout(timer)
    }
  }, [success, clearSuccess, navigate])

  const handleConfirmDelete = async () => {
    if (!id) return
    try {
      await deleteChild(id)
      setShowDeleteConfirm(false)
    } catch (err) {
      console.error('Delete failed:', err)
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="h-8 bg-gray-200 rounded w-1/3 animate-pulse" />
        <div className="space-y-3 py-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-4 bg-gray-200 rounded w-full animate-pulse" />
          ))}
        </div>
      </div>
    )
  }

  if (!child) {
    return (
      <div className="card p-6 text-center">
        <p className="text-gray-600">Child not found</p>
        <button
          onClick={() => navigate('/children')}
          className="mt-4 btn-primary"
        >
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
          onClick={() => navigate('/children')}
          className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
        >
          <ArrowLeft size={20} />
        </button>
        <div className="flex-1">
          <h1 className="text-3xl font-bold text-gray-900">
            {child.FirstName} {child.LastName}
          </h1>
          <p className="text-gray-600 mt-1">
            Enrolled on {formatDate(child.EnrollmentDate)}
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
            <button
              onClick={() => navigate(`/reports/child/${child.Id}`)}
              className="px-3.5 py-2 rounded-lg border border-indigo-200 bg-indigo-50 text-indigo-700 hover:bg-indigo-100 transition-colors font-medium text-sm flex items-center gap-1.5"
            >
              <FileText size={16} />
              Daily Reports
            </button>
            <button
              onClick={() => navigate(`/attendance/child/${child.Id}`)}
              className="px-3.5 py-2 rounded-lg border border-gray-200 bg-white text-gray-700 hover:bg-gray-50 transition-colors font-medium text-sm flex items-center gap-1.5"
            >
              <ClipboardCheck size={16} />
              Attendance
            </button>
            <button
              onClick={() => navigate(`/children/${child.Id}/edit`)}
              className="btn-secondary flex items-center gap-2"
            >
              <Edit2 size={16} />
              Edit
            </button>
            <button
              onClick={() => setShowDeleteConfirm(true)}
              className="px-4 py-2 text-red-600 border border-red-300 rounded-lg hover:bg-red-50 transition-colors font-medium flex items-center gap-2"
            >
              <Trash2 size={16} />
              Delete
            </button>
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

      {/* Content */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Content */}
          <div className="lg:col-span-2 space-y-6">
            {/* Personal Information */}
            <div className="card p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Personal Information</h2>
              <div className="flex items-start gap-6">
                {child.PhotoUrl ? (
                  <img
                    src={child.PhotoUrl}
                    alt={`${child.FirstName} ${child.LastName}`}
                    className="w-24 h-24 rounded-xl object-cover border border-gray-200 shrink-0"
                  />
                ) : (
                  <div className="w-24 h-24 rounded-xl bg-gray-100 border border-gray-200 flex items-center justify-center text-gray-400 text-xs shrink-0">
                    No photo
                  </div>
                )}
                <div className="grid grid-cols-2 gap-4 flex-1">
                  <InfoField label="First Name" value={child.FirstName} />
                  <InfoField label="Last Name" value={child.LastName} />
                  <InfoField label="Date of Birth" value={formatDate(child.DateOfBirth)} />
                  <InfoField label="Age" value={String(child.Age)} />
                  <InfoField label="Gender" value={child.Gender} />
                </div>
              </div>
            </div>

            {/* Parent Information */}
            <div className="card p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Parents / Guardians</h2>
              {parents.length === 0 ? (
                <p className="text-gray-500">No parents assigned</p>
              ) : (
                <ul className="space-y-4">
                  {parents.map((parent) => (
                    <li
                      key={parent.ParentId}
                      className="p-4 border border-gray-200 rounded-lg space-y-2"
                    >
                      <div className="flex items-center justify-between gap-3">
                        <p className="font-medium text-gray-900">
                          {parent.FirstName} {parent.LastName}
                        </p>
                        <span className="px-2 py-1 rounded text-xs font-medium bg-indigo-100 text-indigo-800">
                          {parent.Role}
                        </span>
                      </div>
                      <div className="space-y-1 text-sm text-gray-600">
                        <p className="flex items-center gap-2">
                          <Mail size={14} className="text-gray-400" />
                          {parent.Email || '-'}
                        </p>
                        <p className="flex items-center gap-2">
                          <Phone size={14} className="text-gray-400" />
                          {parent.PhoneNumber || '-'}
                        </p>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </div>

            {/* Medical Information */}
            <div className="card p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Medical Information</h2>
              <div className="space-y-4">
                {child.Allergies && (
                  <div className="p-3 bg-yellow-50 border border-yellow-200 rounded">
                    <p className="text-sm font-medium text-yellow-800">Allergies</p>
                    <p className="text-yellow-700">{child.Allergies}</p>
                  </div>
                )}
                {child.MedicalNotes && (
                  <div>
                    <p className="text-sm font-medium text-gray-700">Medical Notes</p>
                    <p className="text-gray-600 mt-1">{child.MedicalNotes}</p>
                  </div>
                )}
                {!child.Allergies && !child.MedicalNotes && (
                  <p className="text-gray-500">No medical information recorded</p>
                )}
              </div>
            </div>
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Enrollment Information */}
            <div className="card p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Enrollment</h2>
              <div className="space-y-3">
                <InfoField label="Status" value={child.Status} badge />
                <InfoField label="Enrollment Date" value={formatDate(child.EnrollmentDate)} />
              </div>
            </div>

            {/* System Information */}
            <div className="card p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">System</h2>
              <div className="space-y-2 text-xs text-gray-600">
                <p>Created: {formatDate(child.CreatedAt)} at {formatTime(child.CreatedAt)}</p>
                <p>Updated: {formatDate(child.UpdatedAt)} at {formatTime(child.UpdatedAt)}</p>
              </div>
            </div>
          </div>
        </div>

      {/* Delete Confirmation Dialog */}
      <DeleteConfirmation
        isOpen={showDeleteConfirm}
        isDeleting={isDeleting}
        title="Delete Child"
        description="Permanently remove this child from the system"
        itemName={`${child.FirstName} ${child.LastName}`}
        onConfirm={handleConfirmDelete}
        onCancel={() => setShowDeleteConfirm(false)}
      />
    </div>
  )
}

interface InfoFieldProps {
  label: string
  value?: string
  badge?: boolean
}

const InfoField = ({ label, value = '-', badge = false }: InfoFieldProps) => {
  return (
    <div>
      <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">{label}</p>
      {badge ? (
        <p className={`mt-1 inline-block px-2 py-1 rounded text-xs font-medium ${getStatusBadgeColor(value)}`}>
          {value}
        </p>
      ) : (
        <p className="text-gray-900 font-medium mt-1">{value}</p>
      )}
    </div>
  )
}

function getStatusBadgeColor(status: string): string {
  const colors: Record<string, string> = {
    Active: 'bg-green-100 text-green-800',
    Inactive: 'bg-gray-100 text-gray-800',
    Enrolled: 'bg-blue-100 text-blue-800',
    Graduated: 'bg-purple-100 text-purple-800',
  }
  return colors[status] || 'bg-gray-100 text-gray-800'
}
