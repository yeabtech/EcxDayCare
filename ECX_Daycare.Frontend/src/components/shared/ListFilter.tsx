/**
 * ListFilter Component
 * Reusable filter UI for list pages with multiple filter types
 */
import React, { useState } from 'react'
import type { ListFilter } from '@utils/listUtils'

export interface FilterOption {
  value: string | number
  label: string
  icon?: React.ReactNode
}

export interface FilterField {
  key: string
  label: string
  type: 'select' | 'multi-select' | 'search' | 'date-range' | 'number-range'
  options?: FilterOption[]
  placeholder?: string
  operator?: 'equals' | 'contains' | 'gt' | 'lt' | 'gte' | 'lte' | 'in'
}

export interface ListFilterProps {
  fields: FilterField[]
  onApply: (filters: ListFilter[]) => void
  onClear?: () => void
  collapsible?: boolean
  initialFilters?: ListFilter[]
}

export const ListFilterComponent: React.FC<ListFilterProps> = ({
  fields,
  onApply,
  onClear,
  collapsible = false,
  initialFilters = [],
}) => {
  const [filters, setFilters] = useState<Record<string, any>>(() => {
    return initialFilters.reduce((acc, f) => {
      acc[f.key] = f.value
      return acc
    }, {} as Record<string, any>)
  })
  const [isOpen, setIsOpen] = useState(!collapsible)

  const handleFilterChange = (key: string, value: any) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  const handleApply = () => {
    const activeFilters: ListFilter[] = Object.entries(filters)
      .filter(([, value]) => value != null && value !== '')
      .map(([key, value]) => {
        const field = fields.find((f) => f.key === key)
        return {
          key,
          value,
          operator: field?.operator,
        } as ListFilter
      })

    onApply(activeFilters)
    if (collapsible) setIsOpen(false)
  }

  const handleClear = () => {
    setFilters({})
    onClear?.()
    if (collapsible) setIsOpen(false)
  }

  const activeFilterCount = Object.values(filters).filter((v) => v != null && v !== '').length

  return (
    <div className="bg-white rounded-lg border border-gray-200">
      {/* Filter Header */}
      {collapsible && (
        <button
          onClick={() => setIsOpen(!isOpen)}
          className="w-full px-4 py-3 flex items-center justify-between hover:bg-gray-50"
        >
          <div className="flex items-center space-x-2">
            <span className="font-medium text-gray-900">Filters</span>
            {activeFilterCount > 0 && (
              <span className="bg-primary-100 text-primary-800 text-xs font-medium px-2 py-1 rounded-full">
                {activeFilterCount}
              </span>
            )}
          </div>
          <span className={`transform transition-transform ${isOpen ? 'rotate-180' : ''}`}>
            ▼
          </span>
        </button>
      )}

      {/* Filter Content */}
      {isOpen && (
        <>
          <div className={`${collapsible ? 'border-t' : ''} border-gray-200 p-4 space-y-4`}>
            {fields.map((field) => (
              <div key={field.key}>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  {field.label}
                </label>

                {field.type === 'select' && (
                  <select
                    value={filters[field.key] || ''}
                    onChange={(e) => handleFilterChange(field.key, e.target.value || null)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                  >
                    <option value="">{field.placeholder || 'Select...'}</option>
                    {field.options?.map((opt) => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </select>
                )}

                {field.type === 'multi-select' && (
                  <select
                    multiple
                    value={Array.isArray(filters[field.key]) ? filters[field.key] : []}
                    onChange={(e) =>
                      handleFilterChange(
                        field.key,
                        Array.from(e.target.selectedOptions, (opt) => opt.value)
                      )
                    }
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                  >
                    {field.options?.map((opt) => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </select>
                )}

                {field.type === 'search' && (
                  <input
                    type="text"
                    value={filters[field.key] || ''}
                    onChange={(e) => handleFilterChange(field.key, e.target.value || null)}
                    placeholder={field.placeholder}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                )}

                {field.type === 'date-range' && (
                  <div className="flex space-x-2">
                    <input
                      type="date"
                      value={filters[`${field.key}_from`] || ''}
                      onChange={(e) => handleFilterChange(`${field.key}_from`, e.target.value || null)}
                      className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    />
                    <input
                      type="date"
                      value={filters[`${field.key}_to`] || ''}
                      onChange={(e) => handleFilterChange(`${field.key}_to`, e.target.value || null)}
                      className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    />
                  </div>
                )}

                {field.type === 'number-range' && (
                  <div className="flex space-x-2">
                    <input
                      type="number"
                      value={filters[`${field.key}_min`] || ''}
                      onChange={(e) =>
                        handleFilterChange(`${field.key}_min`, e.target.value ? parseInt(e.target.value) : null)
                      }
                      placeholder="Min"
                      className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    />
                    <input
                      type="number"
                      value={filters[`${field.key}_max`] || ''}
                      onChange={(e) =>
                        handleFilterChange(`${field.key}_max`, e.target.value ? parseInt(e.target.value) : null)
                      }
                      placeholder="Max"
                      className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    />
                  </div>
                )}
              </div>
            ))}
          </div>

          {/* Filter Actions */}
          <div className="border-t border-gray-200 px-4 py-3 flex justify-end space-x-2">
            {activeFilterCount > 0 && (
              <button
                onClick={handleClear}
                className="px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50"
              >
                Clear Filters
              </button>
            )}
            <button
              onClick={handleApply}
              className="px-4 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600"
            >
              Apply Filters
            </button>
          </div>
        </>
      )}
    </div>
  )
}

export default ListFilterComponent
