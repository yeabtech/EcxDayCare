/**
 * PhotoUpload
 * Image picker with live preview for child photos.
 *
 * Uploads immediately on selection and reports the resulting public URL
 * back to the parent form, which stores it as Child.PhotoUrl.
 */
import { useEffect, useRef, useState } from 'react'
import { ImagePlus, Loader2, Trash2, UploadCloud } from 'lucide-react'
import { childrenApi } from '../services/childrenApi'

const MAX_SIZE_BYTES = 5 * 1024 * 1024 // 5 MB, matches the backend limit
const ACCEPTED_TYPES = 'image/jpeg,image/png,image/gif,image/webp'

interface PhotoUploadProps {
  /** Current photo URL (existing value when editing). */
  value?: string
  onChange: (url: string) => void
  label?: string
  error?: string
  disabled?: boolean
}

export const PhotoUpload = ({
  value,
  onChange,
  label = 'Photo',
  error,
  disabled = false,
}: PhotoUploadProps) => {
  const inputRef = useRef<HTMLInputElement>(null)
  const [preview, setPreview] = useState<string>(value || '')
  const [isUploading, setIsUploading] = useState(false)
  const [localError, setLocalError] = useState<string | null>(null)

  // Keep the preview in sync when the form loads an existing child.
  useEffect(() => {
    setPreview(value || '')
  }, [value])

  const openPicker = () => {
    if (!disabled && !isUploading) inputRef.current?.click()
  }

  const handleFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    // Allow re-selecting the same file after an error.
    e.target.value = ''
    if (!file) return

    setLocalError(null)

    if (!file.type.startsWith('image/')) {
      setLocalError('Please choose an image file.')
      return
    }
    if (file.size > MAX_SIZE_BYTES) {
      setLocalError('Image must be 5 MB or smaller.')
      return
    }

    // Show the local file immediately; swap to the server URL when it returns.
    const objectUrl = URL.createObjectURL(file)
    setPreview(objectUrl)
    setIsUploading(true)

    try {
      const uploadedUrl = await childrenApi.uploadPhoto(file)
      if (!uploadedUrl) {
        setLocalError('Upload succeeded but no URL was returned.')
        setPreview(value || '')
        return
      }
      setPreview(uploadedUrl)
      onChange(uploadedUrl)
    } catch {
      setLocalError('Upload failed. Please try again.')
      setPreview(value || '')
    } finally {
      setIsUploading(false)
      URL.revokeObjectURL(objectUrl)
    }
  }

  const handleRemove = () => {
    setLocalError(null)
    setPreview('')
    onChange('')
    if (inputRef.current) inputRef.current.value = ''
  }

  const displayError = error || localError || undefined

  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>

      <div className="flex items-center gap-4">
        {/* Preview / drop target */}
        <button
          type="button"
          onClick={openPicker}
          disabled={disabled || isUploading}
          className={`relative w-24 h-24 rounded-xl border-2 border-dashed flex items-center justify-center overflow-hidden transition-colors disabled:cursor-not-allowed ${
            displayError
              ? 'border-red-400 bg-red-50'
              : 'border-gray-300 bg-gray-50 hover:border-primary-400 hover:bg-primary-50'
          }`}
          aria-label="Choose a photo"
        >
          {preview ? (
            <img src={preview} alt="Selected photo" className="w-full h-full object-cover" />
          ) : (
            <ImagePlus className="w-7 h-7 text-gray-400" />
          )}

          {isUploading && (
            <span className="absolute inset-0 flex items-center justify-center bg-black/40">
              <Loader2 className="w-6 h-6 text-white animate-spin" />
            </span>
          )}
        </button>

        {/* Actions */}
        <div className="space-y-2">
          <button
            type="button"
            onClick={openPicker}
            disabled={disabled || isUploading}
            className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <UploadCloud className="w-4 h-4" />
            {isUploading ? 'Uploading…' : preview ? 'Replace photo' : 'Upload photo'}
          </button>

          {preview && !isUploading && (
            <button
              type="button"
              onClick={handleRemove}
              disabled={disabled}
              className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-red-600 hover:text-red-700 disabled:opacity-50"
            >
              <Trash2 className="w-4 h-4" />
              Remove
            </button>
          )}

          <p className="text-xs text-gray-500">JPG, PNG, GIF or WEBP — max 5 MB.</p>
        </div>
      </div>

      {displayError && <p className="text-red-500 text-xs mt-2">{displayError}</p>}

      <input
        ref={inputRef}
        type="file"
        accept={ACCEPTED_TYPES}
        onChange={handleFile}
        className="hidden"
      />
    </div>
  )
}
