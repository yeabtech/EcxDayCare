/**
 * ChildCard component
 * Displays a single child in list view
 */
import { Child } from '../types'
import { formatDate } from '@utils/index'
import { ChevronRight } from 'lucide-react'

interface ChildCardProps {
  child: Child
  onView: (id: string) => void
}

export const ChildCard = ({ child, onView }: ChildCardProps) => {
  return (
    <div className="card p-4 hover:shadow-md transition-shadow cursor-pointer" onClick={() => onView(child.Id)}>
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <h3 className="text-lg font-semibold text-gray-900">
              {child.FirstName} {child.LastName}
            </h3>
            <span className={`inline-block px-2 py-1 rounded text-xs font-medium ${getStatusColor(child.Status)}`}>
              {child.Status}
            </span>
          </div>
          <div className="mt-2 grid grid-cols-2 gap-4 text-sm text-gray-600">
            <div>
              <p className="text-gray-500">Age</p>
              <p className="font-medium">{child.Age} years old</p>
            </div>
            <div>
              <p className="text-gray-500">Gender</p>
              <p className="font-medium">{child.Gender}</p>
            </div>
            <div>
              <p className="text-gray-500">Enrolled</p>
              <p className="font-medium">{formatDate(child.EnrollmentDate)}</p>
            </div>
            <div>
              <p className="text-gray-500">DOB</p>
              <p className="font-medium">{formatDate(child.DateOfBirth)}</p>
            </div>
          </div>
          {child.Allergies && (
            <div className="mt-3 p-2 bg-yellow-50 border border-yellow-200 rounded text-xs text-yellow-800">
              <p className="font-medium">Allergies: {child.Allergies}</p>
            </div>
          )}
        </div>
        <ChevronRight size={20} className="text-gray-400 flex-shrink-0 ml-2" />
      </div>
    </div>
  )
}

/**
 * Helper to get status badge color
 */
function getStatusColor(status: string): string {
  const colors: Record<string, string> = {
    Active: 'bg-green-100 text-green-800',
    Inactive: 'bg-gray-100 text-gray-800',
    Enrolled: 'bg-blue-100 text-blue-800',
    Graduated: 'bg-purple-100 text-purple-800',
  }
  return colors[status] || 'bg-gray-100 text-gray-800'
}
