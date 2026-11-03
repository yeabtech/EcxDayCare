/**
 * EmptyState Component
 * Displays a consistent empty state message across the application
 */
import React from 'react'

export interface EmptyStateProps {
  icon?: React.ReactNode
  title: string
  description?: string
  action?: {
    label: string
    href?: string
    onClick?: () => void
  }
  variant?: 'default' | 'compact'
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  icon,
  title,
  description,
  action,
  variant = 'default',
}) => {
  const content = (
    <div
      className={`flex flex-col items-center justify-center ${variant === 'compact' ? 'py-6' : 'py-12'}`}
    >
      {icon && <div className="mb-4 text-4xl">{icon}</div>}
      <h3 className={`font-semibold text-gray-900 ${variant === 'compact' ? 'text-sm' : 'text-lg'}`}>
        {title}
      </h3>
      {description && (
        <p className={`text-gray-600 mt-1 text-center max-w-md ${variant === 'compact' ? 'text-xs' : 'text-sm'}`}>
          {description}
        </p>
      )}
      {action && (
        <div className="mt-4">
          {action.href ? (
            <a
              href={action.href}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-primary-500 hover:bg-primary-600 transition-colors"
            >
              {action.label}
            </a>
          ) : (
            <button
              onClick={action.onClick}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-primary-500 hover:bg-primary-600 transition-colors"
            >
              {action.label}
            </button>
          )}
        </div>
      )}
    </div>
  )

  return variant === 'compact' ? content : <div className="text-center">{content}</div>
}
