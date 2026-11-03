/**
 * DataTable Component
 * Reusable table component for displaying data lists
 */
import React from 'react'

export interface TableColumn<T> {
  key: keyof T
  label: string
  render?: (value: any, row: T) => React.ReactNode
  sortable?: boolean
  width?: string
}

export interface DataTableProps<T> {
  columns: TableColumn<T>[]
  data: T[]
  isLoading?: boolean
  emptyMessage?: string
  onRowClick?: (row: T) => void
  selectable?: boolean
  selectedRows?: Set<any>
  onSelectRow?: (row: T, selected: boolean) => void
  onSelectAll?: (selected: boolean) => void
}

export const DataTable = React.forwardRef<HTMLDivElement, DataTableProps<any>>(
  (
    {
      columns,
      data,
      isLoading = false,
      emptyMessage = 'No data available',
      onRowClick,
      selectable = false,
      selectedRows = new Set(),
      onSelectRow,
      onSelectAll,
    },
    ref
  ) => {
    if (isLoading) {
      return (
        <div className="text-center py-8">
          <p className="text-gray-500">Loading...</p>
        </div>
      )
    }

    if (data.length === 0) {
      return (
        <div className="text-center py-8">
          <p className="text-gray-500">{emptyMessage}</p>
        </div>
      )
    }

    const allSelected = data.length > 0 && data.every((row) => selectedRows.has(row))

    return (
      <div ref={ref} className="overflow-x-auto">
        <table className="w-full text-left text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {selectable && (
                <th className="px-4 py-3 font-medium text-gray-700">
                  <input
                    type="checkbox"
                    checked={allSelected}
                    onChange={(e) => onSelectAll?.(e.target.checked)}
                    className="rounded"
                  />
                </th>
              )}
              {columns.map((column) => (
                <th
                  key={String(column.key)}
                  className="px-4 py-3 font-medium text-gray-700"
                  style={{ width: column.width }}
                >
                  {column.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((row, idx) => (
              <tr
                key={idx}
                className={`border-b border-gray-200 ${onRowClick ? 'cursor-pointer hover:bg-gray-50' : ''}`}
                onClick={() => onRowClick?.(row)}
              >
                {selectable && (
                  <td className="px-4 py-3">
                    <input
                      type="checkbox"
                      checked={selectedRows.has(row)}
                      onChange={(e) => onSelectRow?.(row, e.target.checked)}
                      onClick={(e) => e.stopPropagation()}
                      className="rounded"
                    />
                  </td>
                )}
                {columns.map((column) => (
                  <td key={String(column.key)} className="px-4 py-3 text-gray-900">
                    {column.render
                      ? column.render(row[column.key], row)
                      : String(row[column.key] || '—')}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    )
  }
)

DataTable.displayName = 'DataTable'
