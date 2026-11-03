/**
 * ErrorAlert Component
 * Displays error messages to users
 */
import React from 'react'

export interface ErrorAlertProps {
  title?: string
  message: string
  onDismiss?: () => void
  variant?: 'error' | 'warning' | 'info'
}

export const ErrorAlert: React.FC<ErrorAlertProps> = ({
  title,
  message,
  onDismiss,
  variant = 'error',
}) => {
  const variantStyles = {
    error: 'bg-red-50 border-red-200 text-red-800',
    warning: 'bg-yellow-50 border-yellow-200 text-yellow-800',
    info: 'bg-primary-50 border-primary-200 text-primary-800',
  }

  const icons = {
    error: '⚠️',
    warning: '⚡',
    info: 'ℹ️',
  }

  return (
    <div className={`border rounded-lg p-4 ${variantStyles[variant]}`}>
      <div className="flex items-start gap-3">
        <span className="text-xl mt-0.5">{icons[variant]}</span>
        <div className="flex-1">
          {title && <p className="font-semibold mb-1">{title}</p>}
          <p className="text-sm">{message}</p>
        </div>
        {onDismiss && (
          <button
            onClick={onDismiss}
            className="text-current hover:opacity-70 transition"
            aria-label="Dismiss"
          >
            ✕
          </button>
        )}
      </div>
    </div>
  )
}
