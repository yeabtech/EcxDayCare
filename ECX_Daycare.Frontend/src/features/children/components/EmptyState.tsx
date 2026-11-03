/**
 * EmptyState component
 * Shows when there are no children to display
 */
import { Users } from 'lucide-react'

interface EmptyStateProps {
  title?: string
  description?: string
  action?: {
    label: string
    onClick: () => void
  }
}

export const EmptyState = ({
  title = 'No children found',
  description = 'Start by adding a new child',
  action,
}: EmptyStateProps) => {
  return (
    <div className="flex flex-col items-center justify-center py-12 px-4">
      <div className="rounded-lg bg-gray-50 p-6 text-center">
        <Users size={48} className="mx-auto text-gray-400 mb-4" />
        <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
        <p className="text-gray-600 mt-1">{description}</p>
        {action && (
          <button
            onClick={action.onClick}
            className="mt-4 btn-primary"
          >
            {action.label}
          </button>
        )}
      </div>
    </div>
  )
}
