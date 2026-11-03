/**
 * DeleteConfirmation component
 * Confirmation dialog for destructive actions
 */
import { AlertCircle } from 'lucide-react'

interface DeleteConfirmationProps {
  isOpen: boolean
  isDeleting?: boolean
  title: string
  description: string
  itemName: string
  onConfirm: () => Promise<void>
  onCancel: () => void
}

export const DeleteConfirmation = ({
  isOpen,
  isDeleting = false,
  title,
  description,
  itemName,
  onConfirm,
  onCancel,
}: DeleteConfirmationProps) => {
  if (!isOpen) return null

  return (
    <>
      {/* Overlay */}
      <div className="fixed inset-0 bg-black/50 z-40" onClick={onCancel} />

      {/* Dialog */}
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-lg shadow-lg max-w-sm w-full">
          {/* Header */}
          <div className="flex items-center gap-4 p-6 border-b border-gray-200">
            <div className="flex-shrink-0">
              <AlertCircle className="text-red-600" size={24} />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-gray-900">{title}</h2>
              <p className="text-sm text-gray-600 mt-1">{description}</p>
            </div>
          </div>

          {/* Content */}
          <div className="p-6">
            <p className="text-gray-700">
              Are you sure you want to delete{' '}
              <span className="font-semibold text-gray-900">"{itemName}"</span>?
            </p>
            <p className="text-sm text-gray-500 mt-2">
              This action cannot be undone.
            </p>
          </div>

          {/* Actions */}
          <div className="flex gap-3 justify-end p-6 border-t border-gray-200">
            <button
              onClick={onCancel}
              disabled={isDeleting}
              className="btn-secondary"
            >
              Cancel
            </button>
            <button
              onClick={onConfirm}
              disabled={isDeleting}
              className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isDeleting ? 'Deleting...' : 'Delete'}
            </button>
          </div>
        </div>
      </div>
    </>
  )
}
