/**
 * Classrooms Management Page
 * Admin page for managing classrooms
 */
import React, { useState, useCallback } from 'react'
import { useApiList } from '@features/dashboards/hooks/useApiList'
import { ListPage, BulkActions, Pagination } from '@components/shared'
import type { TableColumn, BulkAction } from '@components/shared'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'
import { exportData } from '@utils/exportData'
import { Trash2, Users } from 'lucide-react'

export interface Classroom {
  id: string
  name: string
  ageGroup: string
  capacity: number
  enrolledCount: number
  teacherId: string
  teacherName: string
  assistantId?: string
  assistantName?: string
  description?: string
  location: string
  status: 'active' | 'inactive' | 'maintenance'
  createdDate: string
}

const COLUMNS: TableColumn<Classroom>[] = [
  {
    key: 'name',
    label: 'Classroom Name',
    width: '150px',
  },
  {
    key: 'ageGroup',
    label: 'Age Group',
    width: '120px',
  },
  {
    key: 'location',
    label: 'Location',
    width: '150px',
  },
  {
    key: 'enrolledCount',
    label: 'Enrollment',
    width: '120px',
    render: (value, row) => (
      <div className="flex items-center gap-2">
        <Users size={16} />
        <span>{value}/{(row as Classroom).capacity}</span>
      </div>
    ),
  },
  {
    key: 'teacherName',
    label: 'Teacher',
    width: '150px',
  },
  {
    key: 'assistantName',
    label: 'Assistant',
    width: '150px',
    render: (value) => value || '—',
  },
  {
    key: 'status',
    label: 'Status',
    width: '100px',
    render: (value) => (
      <span
        className={`px-2 py-1 rounded-full text-xs font-medium ${
          value === 'active'
            ? 'bg-green-100 text-green-800'
            : value === 'inactive'
              ? 'bg-yellow-100 text-yellow-800'
              : 'bg-blue-100 text-blue-800'
        }`}
      >
        {value.charAt(0).toUpperCase() + value.slice(1)}
      </span>
    ),
  },
]

const FILTER_FIELDS = [
  {
    key: 'status',
    label: 'Status',
    type: 'select' as const,
    options: [
      { value: 'active', label: 'Active' },
      { value: 'inactive', label: 'Inactive' },
      { value: 'maintenance', label: 'Maintenance' },
    ],
  },
  {
    key: 'ageGroup',
    label: 'Age Group',
    type: 'select' as const,
    options: [
      { value: 'infant', label: 'Infant (0-12 months)' },
      { value: 'toddler', label: 'Toddler (1-2 years)' },
      { value: 'preschool', label: 'Preschool (3-4 years)' },
      { value: 'prek', label: 'Pre-K (4-5 years)' },
    ],
  },
]

export const ClassroomsManagementPage: React.FC = () => {
  const {
    items: classrooms,
    total,
    page,
    pageSize,
    totalPages,
    isLoading,
    isRefreshing,
    error,
    refresh,
    setPage,
    setPageSize,
    setFilters,
  } = useApiList<Classroom>({
    endpoint: '/classrooms',
    pageSize: 10,
    autoFetch: true,
  })

  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [isDeleting, setIsDeleting] = useState(false)

  const handleBulkDelete = useCallback(async (selectedClassrooms: Classroom[]) => {
    if (!window.confirm(`Delete ${selectedClassrooms.length} classroom/classrooms?`)) return

    setIsDeleting(true)
    try {
      await Promise.all(
        selectedClassrooms.map((classroom) => apiClient.delete(`/classrooms/${classroom.id}`))
      )
      setSelectedIds(new Set())
      await refresh()
    } catch (err) {
      const error = handleApiError(err)
      alert(`Error deleting classrooms: ${error.message}`)
    } finally {
      setIsDeleting(false)
    }
  }, [refresh])

  const handleExport = useCallback(() => {
    const exportFields = [
      'name',
      'ageGroup',
      'location',
      'capacity',
      'enrolledCount',
      'teacherName',
      'assistantName',
      'status',
    ]
    exportData(classrooms, {
      filename: 'classrooms',
      format: 'csv',
      fields: exportFields,
    })
  }, [classrooms])

  const bulkActions: BulkAction<Classroom>[] = [
    {
      id: 'delete',
      label: 'Delete',
      icon: <Trash2 size={16} />,
      onClick: () => handleBulkDelete(Array.from(selectedIds).map((id) => classrooms.find((c) => c.id === id)!)),
      color: 'red',
      requiresConfirm: true,
      confirmMessage: `Delete ${selectedIds.size} classroom/classrooms? This action cannot be undone.`,
    },
  ]

  const selectedClassrooms = classrooms.filter((c) => selectedIds.has(c.id))

  return (
    <div className="space-y-6">
      <ListPage
        title="Classrooms Management"
        description="Manage all classrooms in the facility"
        items={classrooms}
        columns={COLUMNS}
        filterFields={FILTER_FIELDS}
        isLoading={isLoading}
        isRefreshing={isRefreshing}
        error={error}
        onRefresh={refresh}
        onAddNew={() => window.location.href = '/classrooms/create'}
        onEdit={(classroom) => window.location.href = `/classrooms/${classroom.id}/edit`}
        onDelete={(classroom) => {
          if (window.confirm(`Delete classroom "${classroom.name}"?`)) {
            apiClient.delete(`/classrooms/${classroom.id}`).then(() => refresh())
          }
        }}
        onFilterChange={setFilters}
        onSearchChange={() => {}}
        onExport={handleExport}
        selectedCount={selectedIds.size}
        totalCount={total}
        pageSize={pageSize}
        currentPage={page}
        onPageChange={setPage}
        canBulkDelete={true}
        canExport={true}
        showSearch={true}
        showFilters={true}
        createButtonLabel="+ New Classroom"
      />

      {/* Bulk Actions */}
      {selectedIds.size > 0 && (
        <div className="space-y-4">
          <BulkActions
            items={selectedClassrooms}
            actions={bulkActions}
            onComplete={() => {
              setSelectedIds(new Set())
              refresh()
            }}
            isLoading={isDeleting}
          />
        </div>
      )}

      {/* Pagination */}
      {!isLoading && classrooms.length > 0 && (
        <Pagination
          currentPage={page}
          totalPages={totalPages}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
          totalCount={total}
        />
      )}
    </div>
  )
}

export default ClassroomsManagementPage
