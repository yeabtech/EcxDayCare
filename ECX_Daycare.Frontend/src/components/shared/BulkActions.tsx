/**
 * BulkActions Component
 * Handles bulk operations on multiple selected items
 */
import React, { useState } from 'react'

export interface BulkAction<T> {
  id: string
  label: string
  icon: React.ReactNode
  onClick: (items: T[]) => Promise<void> | void
  className?: string
  requiresConfirm?: boolean
  confirmMessage?: string
  color?: 'red' | 'blue' | 'green' | 'gray'
}

export interface BulkActionsProps<T> {
  items: T[]
  actions: BulkAction<T>[]
  onComplete?: () => void
  isLoading?: boolean
}

export const BulkActions = React.forwardRef<HTMLDivElement, BulkActionsProps<any>>(
  ({ items, actions, onComplete, isLoading = false }, ref) => {
    const [selectedAction, setSelectedAction] = useState<BulkAction<any> | null>(null)
    const [isProcessing, setIsProcessing] = useState(false)
    const [showConfirm, setShowConfirm] = useState(false)

    const handleActionClick = async (action: BulkAction<any>) => {
      if (action.requiresConfirm) {
        setSelectedAction(action)
        setShowConfirm(true)
        return
      }

      await executeAction(action)
    }

    const executeAction = async (action: BulkAction<any>) => {
      setIsProcessing(true)
      try {
        await action.onClick(items)
        onComplete?.()
      } catch (error) {
        console.error(`Error executing action ${action.id}:`, error)
      } finally {
        setIsProcessing(false)
        setShowConfirm(false)
        setSelectedAction(null)
      }
    }

    const colorClasses = {
      red: 'text-red-600 hover:text-red-900 hover:bg-red-50',
      blue: 'text-primary-600 hover:text-primary-900 hover:bg-primary-50',
      green: 'text-green-600 hover:text-green-900 hover:bg-green-50',
      gray: 'text-gray-600 hover:text-gray-900 hover:bg-gray-50',
    }

    return (
      <>
        <div
          ref={ref}
          className="bg-primary-50 border border-primary-200 rounded-lg p-4 flex items-center justify-between"
        >
          <div>
            <p className="text-sm font-medium text-primary-900">{items.length} items selected</p>
            <p className="text-xs text-primary-700 mt-1">
              {actions.length} action{actions.length !== 1 ? 's' : ''} available
            </p>
          </div>

          <div className="flex items-center gap-2">
            {actions.map((action) => (
              <button
                key={action.id}
                onClick={() => handleActionClick(action)}
                disabled={isProcessing || isLoading}
                className={`p-2 rounded-lg transition-colors disabled:opacity-50 ${
                  colorClasses[action.color || 'gray']
                }`}
                title={action.label}
              >
                {action.icon}
              </button>
            ))}
          </div>
        </div>

        {/* Confirmation Dialog */}
        {showConfirm && selectedAction && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-lg max-w-sm w-full p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-2">Confirm Action</h3>
              <p className="text-gray-600 mb-6">
                {selectedAction.confirmMessage ||
                  `Are you sure you want to ${selectedAction.label.toLowerCase()} ${items.length} item${
                    items.length !== 1 ? 's' : ''
                  }?`}
              </p>

              <div className="flex justify-end gap-3">
                <button
                  onClick={() => {
                    setShowConfirm(false)
                    setSelectedAction(null)
                  }}
                  className="px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  onClick={() => executeAction(selectedAction)}
                  disabled={isProcessing}
                  className={`px-4 py-2 text-white rounded-lg ${
                    selectedAction.color === 'red'
                      ? 'bg-red-600 hover:bg-red-700'
                      : 'bg-primary-500 hover:bg-primary-600'
                  } disabled:opacity-50`}
                >
                  {isProcessing ? 'Processing...' : selectedAction.label}
                </button>
              </div>
            </div>
          </div>
        )}
      </>
    )
  }
)

BulkActions.displayName = 'BulkActions'

export default BulkActions
