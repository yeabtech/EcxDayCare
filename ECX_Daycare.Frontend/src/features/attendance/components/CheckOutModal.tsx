/**
 * Check-Out Modal — confirm child departure
 */
import { useState } from 'react'
import { X, LogOut, Loader2 } from 'lucide-react'
import type { AttendanceRecord } from '../types'

interface CheckOutModalProps {
  record: AttendanceRecord
  onClose: () => void
  onConfirm: (pickedUpBy: string, isEarlyPickup: boolean) => Promise<void>
  loading: boolean
}

export const CheckOutModal = ({ record, onClose, onConfirm, loading }: CheckOutModalProps) => {
  const [pickedUpBy, setPickedUpBy] = useState('')
  const [isEarlyPickup, setIsEarlyPickup] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      await onConfirm(pickedUpBy, isEarlyPickup)
    } catch (err: any) {
      setError(err.message || 'Check-out failed')
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-sm p-6 z-10">
        {/* Header */}
        <div className="flex items-center justify-between mb-5">
          <div className="flex items-center gap-2.5">
            <div className="p-2 bg-blue-100 rounded-xl">
              <LogOut size={18} className="text-blue-600" />
            </div>
            <div>
              <h2 className="text-base font-bold text-gray-900">Check Out</h2>
              <p className="text-xs text-gray-500 truncate max-w-[200px]">
                {record.childName ?? 'Child'}
              </p>
            </div>
          </div>
          <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400">
            <X size={18} />
          </button>
        </div>

        {error && (
          <div className="p-3 mb-4 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="checkout-pickedup">
              Picked up by
            </label>
            <input
              id="checkout-pickedup"
              type="text"
              value={pickedUpBy}
              onChange={(e) => setPickedUpBy(e.target.value)}
              placeholder="Parent / guardian name"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <label className="flex items-center gap-3 cursor-pointer select-none">
            <input
              id="checkout-early"
              type="checkbox"
              checked={isEarlyPickup}
              onChange={(e) => setIsEarlyPickup(e.target.checked)}
              className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <span className="text-sm text-gray-700">Early pickup</span>
          </label>

          <div className="flex justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-60 transition-colors"
            >
              {loading ? <Loader2 size={14} className="animate-spin" /> : <LogOut size={14} />}
              Confirm Check Out
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
