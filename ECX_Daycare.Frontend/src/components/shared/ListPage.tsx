/**
 * ListPage Component
 * Reusable generic list page template with filtering, sorting, and pagination
 */
import React, { useState } from 'react'
import { LoadingSpinner, ErrorAlert, ListFilter } from '@components/shared'
import { DataTable } from '@components/shared'
import type { TableColumn } from '@components/shared'
import type { ListFilter as IListFilter } from '@utils/listUtils'
import { Search, Download, Plus, Trash2, ChevronLeft, ChevronRight } from 'lucide-react'

export interface ListPageProps<T extends Record<string, any>> {
  title: string
  description?: string
  items: T[]
  columns: TableColumn<T>[]
  filterFields?: any[] // FilterField[]
  isLoading?: boolean
  isRefreshing?: boolean
  error?: string | null
  onRefresh?: () => void
  onAddNew?: () => void
  onEdit?: (item: T) => void
  onDelete?: (item: T) => void
  onBulkDelete?: (items: T[]) => void
  onExport?: () => void
  onFilterChange?: (filters: IListFilter[]) => void
  onSearchChange?: (query: string) => void
  onPageChange?: (page: number) => void
  selectedCount?: number
  totalCount?: number
  pageSize?: number
  currentPage?: number
  canBulkDelete?: boolean
  canExport?: boolean
  showSearch?: boolean
  showFilters?: boolean
  createButtonLabel?: string
  deleteButtonLabel?: string
}

export const ListPage = React.forwardRef<HTMLDivElement, ListPageProps<any>>(
  (
    {
      title,
      description,
      items,
      columns,
      filterFields = [],
      isLoading = false,
      isRefreshing = false,
      error = null,
      onRefresh,
      onAddNew,
      onBulkDelete,
      onExport,
      onFilterChange,
      onSearchChange,
      selectedCount = 0,
      totalCount = 0,
      pageSize = 10,
      currentPage = 1,
      onPageChange,
      canBulkDelete = false,
      canExport = false,
      showSearch = true,
      showFilters = true,
      createButtonLabel = '+ Add New',
      deleteButtonLabel = 'Delete Selected',
    },
    ref
  ) => {
    const [searchQuery, setSearchQuery] = useState('')
    const [isSearching, setIsSearching] = useState(false)

    const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const query = e.target.value
      setSearchQuery(query)
      setIsSearching(true)
      setTimeout(() => {
        onSearchChange?.(query)
        setIsSearching(false)
      }, 300)
    }

    return (
      <div ref={ref} className="space-y-6">
        {/* Error display */}
        {error && <ErrorAlert message={error} onDismiss={() => window.location.reload()} />}

        {/* Page header */}
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{title}</h1>
          {description && <p className="text-gray-600 mt-2">{description}</p>}
        </div>

        {/* Toolbar */}
        <div className="flex items-center justify-between gap-4 flex-wrap">
          {/* Left side - Search and Filters */}
          <div className="flex items-center gap-4 flex-1 min-w-0">
            {showSearch && (
              <div className="relative flex-1 max-w-xs">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={18} />
                <input
                  type="text"
                  placeholder="Search..."
                  value={searchQuery}
                  onChange={handleSearchChange}
                  disabled={isSearching}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>
            )}
          </div>

          {/* Right side - Actions */}
          <div className="flex items-center gap-2">
            {isRefreshing && <span className="text-sm text-gray-500">Refreshing...</span>}
            {onRefresh && (
              <button
                onClick={onRefresh}
                disabled={isRefreshing}
                className="px-3 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
                title="Refresh"
              >
                ↻
              </button>
            )}
            {canExport && onExport && (
              <button
                onClick={onExport}
                className="px-3 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 flex items-center gap-2"
              >
                <Download size={16} />
                Export
              </button>
            )}
            {selectedCount > 0 && canBulkDelete && onBulkDelete && (
              <button
                onClick={() => onBulkDelete(items)}
                className="px-3 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 flex items-center gap-2"
              >
                <Trash2 size={16} />
                {deleteButtonLabel}
              </button>
            )}
            {onAddNew && (
              <button
                onClick={onAddNew}
                className="px-4 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 flex items-center gap-2"
              >
                <Plus size={16} />
                {createButtonLabel}
              </button>
            )}
          </div>
        </div>

        {/* Loading state */}
        {isLoading && <LoadingSpinner />}

        {/* Filters */}
        {!isLoading && showFilters && filterFields.length > 0 && (
          <ListFilter fields={filterFields} onApply={onFilterChange || (() => {})} collapsible />
        )}

        {/* Data Table */}
        {!isLoading && items.length > 0 && (
          <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
            <DataTable
              columns={columns}
              data={items}
            />
          </div>
        )}

        {/* Empty state */}
        {!isLoading && items.length === 0 && (
          <div className="bg-gray-50 rounded-lg border border-gray-200 p-12 text-center">
            <p className="text-gray-500 mb-4">No items found</p>
            {onAddNew && (
              <button
                onClick={onAddNew}
                className="px-4 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600"
              >
                {createButtonLabel}
              </button>
            )}
          </div>
        )}

        {/* Pagination info */}
        {!isLoading && items.length > 0 && totalCount > 0 && (
          <div className="flex flex-wrap items-center justify-between gap-3 text-sm text-gray-600">
            <span>
              Showing {(currentPage - 1) * pageSize + 1} to{' '}
              {Math.min(currentPage * pageSize, totalCount)} of {totalCount} items
              {selectedCount > 0 && <span className="ml-3 text-primary-600 font-medium">{selectedCount} selected</span>}
            </span>
            {onPageChange && Math.ceil(totalCount / pageSize) > 1 && (
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => onPageChange(currentPage - 1)}
                  disabled={currentPage <= 1}
                  className="inline-flex items-center gap-1 rounded-lg border border-gray-200 px-3 py-2 text-xs font-semibold text-gray-600 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-40"
                >
                  <ChevronLeft size={14} /> Previous
                </button>
                <span className="min-w-16 text-center text-xs text-gray-500">Page {currentPage} of {Math.ceil(totalCount / pageSize)}</span>
                <button
                  type="button"
                  onClick={() => onPageChange(currentPage + 1)}
                  disabled={currentPage >= Math.ceil(totalCount / pageSize)}
                  className="inline-flex items-center gap-1 rounded-lg border border-gray-200 px-3 py-2 text-xs font-semibold text-gray-600 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-40"
                >
                  Next <ChevronRight size={14} />
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    )
  }
)

ListPage.displayName = 'ListPage'

export default ListPage
