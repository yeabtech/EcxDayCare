/**
 * Staff detail page component
 */
import { useEffect, useState } from 'react'
import { useNavigate, useParams, Link } from 'react-router-dom'
import { useStaff } from '../hooks/useStaff'
import { classroomsApi } from '@features/classrooms/services/classroomsApi'
import { Classroom } from '@features/classrooms/types'
import {
  ArrowLeft,
  Edit2,
  Trash2,
  Mail,
  Phone,
  Calendar,
  Clock,
  Award,
  Layers,
  MapPin,
  CheckCircle,
} from 'lucide-react'

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

export const StaffDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { currentStaff, loading, error, fetchStaffMember, deleteStaff, assignClassroom } = useStaff()
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [selectedClassroomId, setSelectedClassroomId] = useState<string>('')
  const [isAssigning, setIsAssigning] = useState(false)
  const [assignSuccess, setAssignSuccess] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false)

  useEffect(() => {
    if (id) {
      fetchStaffMember(id)
    }
  }, [id, fetchStaffMember])

  useEffect(() => {
    if (currentStaff) {
      setSelectedClassroomId(currentStaff.classroomId || '')
    }
  }, [currentStaff])

  useEffect(() => {
    const loadClassrooms = async () => {
      try {
        const res = await classroomsApi.getClassrooms({ page: 1, pageSize: 100 })
        setClassrooms(res.items)
      } catch (err) {
        console.error('Failed to load classrooms', err)
      }
    }
    loadClassrooms()
  }, [])

  const handleClassroomAssignment = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id) return

    setIsAssigning(true)
    setAssignSuccess(false)
    try {
      await assignClassroom(id, selectedClassroomId || null)
      setAssignSuccess(true)
      setTimeout(() => setAssignSuccess(false), 3000)
      fetchStaffMember(id)
    } catch (err) {
      console.error('Assignment error', err)
    } finally {
      setIsAssigning(false)
    }
  }

  const handleDelete = async () => {
    if (!id) return
    setIsDeleting(true)
    try {
      await deleteStaff(id)
      navigate('/staff')
    } catch {
      // Handled by hook
    } finally {
      setIsDeleting(false)
    }
  }

  if (loading && !currentStaff) {
    return <div className="p-8 text-center text-gray-500">Loading staff details...</div>
  }

  if (error || !currentStaff) {
    return (
      <div className="max-w-4xl mx-auto space-y-4">
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error || 'Staff member not found'}
        </div>
        <button
          onClick={() => navigate('/staff')}
          className="text-blue-600 hover:underline text-sm font-medium"
        >
          &larr; Back to Staff List
        </button>
      </div>
    )
  }

  const assignedClassroom = classrooms.find((c) => c.id === currentStaff.classroomId)

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      {/* Header bar */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-6 rounded-lg shadow">
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate('/staff')}
            className="p-2 hover:bg-gray-100 rounded-full transition-colors text-gray-500"
            title="Back to staff list"
          >
            <ArrowLeft size={20} />
          </button>
          <div className="h-14 w-14 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center text-xl font-bold">
            {currentStaff.firstName[0]}
            {currentStaff.lastName[0]}
          </div>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold text-gray-900">
                {currentStaff.firstName} {currentStaff.lastName}
              </h1>
              <span className={`px-2.5 py-0.5 rounded-full text-xs font-semibold ${getRoleBadgeClass(currentStaff.role)}`}>
                {currentStaff.role}
              </span>
            </div>
            <p className="text-sm text-gray-500 mt-1">
              Member since {new Date(currentStaff.hireDate).toLocaleDateString()}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-3 w-full sm:w-auto">
          <Link
            to={`/staff/${currentStaff.id}/edit`}
            className="inline-flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            <Edit2 size={16} />
            Edit Profile
          </Link>
          <button
            onClick={() => setShowDeleteConfirm(true)}
            className="inline-flex items-center gap-2 px-4 py-2 border border-red-300 rounded-md shadow-sm text-sm font-medium text-red-700 bg-white hover:bg-red-50"
          >
            <Trash2 size={16} />
            Delete
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column (2 Cols): Contact, Schedule, Certifications */}
        <div className="lg:col-span-2 space-y-6">
          {/* Contact Details */}
          <div className="bg-white p-6 rounded-lg shadow space-y-4">
            <h2 className="text-lg font-bold text-gray-900 border-b border-gray-100 pb-3">
              Contact Information
            </h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                <Mail className="text-gray-400 mt-0.5" size={18} />
                <div>
                  <div className="text-xs text-gray-500 font-medium">Email Address</div>
                  <div className="text-sm font-medium text-gray-900 mt-0.5">{currentStaff.email}</div>
                </div>
              </div>

              <div className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                <Phone className="text-gray-400 mt-0.5" size={18} />
                <div>
                  <div className="text-xs text-gray-500 font-medium">Phone Number</div>
                  <div className="text-sm font-medium text-gray-900 mt-0.5">{currentStaff.phone}</div>
                </div>
              </div>

              <div className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg sm:col-span-2">
                <Calendar className="text-gray-400 mt-0.5" size={18} />
                <div>
                  <div className="text-xs text-gray-500 font-medium">Hire Date</div>
                  <div className="text-sm font-medium text-gray-900 mt-0.5">
                    {new Date(currentStaff.hireDate).toLocaleDateString('en-US', {
                      weekday: 'long',
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                    })}
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Shift Schedule */}
          <div className="bg-white p-6 rounded-lg shadow space-y-4">
            <h2 className="text-lg font-bold text-gray-900 border-b border-gray-100 pb-3 flex items-center gap-2">
              <Clock size={20} className="text-blue-500" />
              Work Shift & Hours
            </h2>
            <div className="flex items-center justify-between p-4 bg-blue-50 rounded-lg border border-blue-100">
              <div>
                <div className="text-xs text-blue-600 font-medium uppercase tracking-wide">
                  Daily Shift Window
                </div>
                <div className="text-xl font-bold text-blue-950 mt-1">
                  {currentStaff.shiftStart.substring(0, 5)} - {currentStaff.shiftEnd.substring(0, 5)}
                </div>
              </div>
              <div className="text-right text-xs text-blue-700 bg-white px-3 py-1.5 rounded-full shadow-sm">
                Active Schedule
              </div>
            </div>
          </div>

          {/* Certifications */}
          <div className="bg-white p-6 rounded-lg shadow space-y-4">
            <h2 className="text-lg font-bold text-gray-900 border-b border-gray-100 pb-3 flex items-center gap-2">
              <Award size={20} className="text-amber-500" />
              Certifications & Qualifications
            </h2>
            {currentStaff.certifications ? (
              <div className="p-4 bg-amber-50/50 rounded-lg border border-amber-100 text-sm text-gray-800 leading-relaxed whitespace-pre-wrap">
                {currentStaff.certifications}
              </div>
            ) : (
              <p className="text-sm text-gray-400 italic">No certifications listed for this staff member.</p>
            )}
          </div>
        </div>

        {/* Right Column (1 Col): Classroom Assignment Card */}
        <div className="space-y-6">
          <div className="bg-white p-6 rounded-lg shadow space-y-4">
            <h2 className="text-lg font-bold text-gray-900 border-b border-gray-100 pb-3 flex items-center gap-2">
              <Layers size={20} className="text-indigo-500" />
              Classroom Assignment
            </h2>

            {currentStaff.classroomId ? (
              <div className="p-4 bg-emerald-50 rounded-lg border border-emerald-200">
                <div className="flex items-center gap-2 text-emerald-800 font-bold text-base">
                  <MapPin size={18} />
                  {currentStaff.classroomName || assignedClassroom?.name || 'Classroom Assigned'}
                </div>
                {assignedClassroom && (
                  <p className="text-xs text-emerald-700 mt-1">
                    Capacity: {assignedClassroom.currentEnrollment}/{assignedClassroom.maxCapacity} children enrolled
                  </p>
                )}
                <div className="mt-3">
                  <Link
                    to={`/classrooms/${currentStaff.classroomId}`}
                    className="text-xs text-emerald-700 hover:text-emerald-900 underline font-medium"
                  >
                    View Classroom Details &rarr;
                  </Link>
                </div>
              </div>
            ) : (
              <div className="p-4 bg-gray-50 rounded-lg border border-gray-200 text-center text-gray-500 text-sm">
                This staff member is currently unassigned to any classroom.
              </div>
            )}

            {/* Quick assignment form */}
            <form onSubmit={handleClassroomAssignment} className="pt-2 space-y-3">
              <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wider">
                Change Assignment
              </label>
              <select
                value={selectedClassroomId}
                onChange={(e) => setSelectedClassroomId(e.target.value)}
                className="w-full text-sm border border-gray-300 rounded-md p-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="">(Unassign / Float)</option>
                {classrooms.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name} ({c.currentEnrollment}/{c.maxCapacity})
                  </option>
                ))}
              </select>

              <button
                type="submit"
                disabled={isAssigning}
                className="w-full py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-md text-sm font-medium transition-colors disabled:opacity-50"
              >
                {isAssigning ? 'Updating...' : 'Save Classroom Assignment'}
              </button>

              {assignSuccess && (
                <div className="flex items-center gap-1.5 text-xs text-emerald-600 justify-center">
                  <CheckCircle size={14} />
                  Classroom assignment updated!
                </div>
              )}
            </form>
          </div>

          {/* Record Metadata */}
          <div className="bg-gray-50 p-4 rounded-lg text-xs text-gray-500 space-y-1.5">
            <div>
              <span className="font-semibold text-gray-600">ID:</span> {currentStaff.id}
            </div>
            <div>
              <span className="font-semibold text-gray-600">Created:</span>{' '}
              {new Date(currentStaff.createdAt).toLocaleString()}
            </div>
            <div>
              <span className="font-semibold text-gray-600">Last Updated:</span>{' '}
              {new Date(currentStaff.updatedAt).toLocaleString()}
            </div>
          </div>
        </div>
      </div>

      {/* Delete Modal */}
      {showDeleteConfirm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg p-6 max-w-sm w-full shadow-xl">
            <h3 className="text-lg font-bold text-gray-900 mb-2">Delete Staff Member</h3>
            <p className="text-sm text-gray-600 mb-4">
              Are you sure you want to remove{' '}
              <span className="font-semibold">
                {currentStaff.firstName} {currentStaff.lastName}
              </span>
              ? This action cannot be undone.
            </p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setShowDeleteConfirm(false)}
                disabled={isDeleting}
                className="px-4 py-2 border border-gray-300 rounded-md text-sm text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={handleDelete}
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
