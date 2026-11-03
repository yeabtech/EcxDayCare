/**
 * Parents Management Page
 * Admin page for managing parents/guardians
 */
import React, { useState, useCallback } from 'react'
import { useApiList } from '@features/dashboards/hooks/useApiList'
import { ListPage, BulkActions, Pagination } from '@components/shared'
import type { TableColumn, BulkAction } from '@components/shared'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'
import { exportData } from '@utils/exportData'
import { Trash2, Mail, Phone } from 'lucide-react'

export interface Parent {
  id: string
  firstName: string
  lastName: string
  email: string
  phone: string
  relationship: string
  childrenCount: number
  status: 'active' | 'inactive'
  address: string
  city: string
  state: string
  zipCode: string
  joinDate: string
}

const COLUMNS: TableColumn<Parent>[] = [
  {
    key: 'firstName',
    label: 'First Name',
    width: '120px',
  },
  {
    key: 'lastName',
    label: 'Last Name',
    width: '120px',
  },
  {
    key: 'email',
    label: 'Email',
    width: '200px',
    render: (value) => (
      <a href={`mailto:${value}`} className="text-blue-600 hover:underline flex items-center gap-1">
        <Mail size={14} /> {value}
      </a>
    ),
  },
  {
    key: 'phone',
    label: 'Phone',
    width: '130px',
    render: (value) => (
      <a href={`tel:${value}`} className="text-blue-600 hover:underline flex items-center gap-1">
        <Phone size={14} /> {value}
      </a>
    ),
  },
  {
    key: 'childrenCount',
    label: 'Children',
    width: '80px',
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
            : 'bg-yellow-100 text-yellow-800'
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
    ],
  },
  {
    key: 'relationship',
    label: 'Relationship',
    type: 'select' as const,
    options: [
      { value: 'mother', label: 'Mother' },
      { value: 'father', label: 'Father' },
      { value: 'guardian', label: 'Guardian' },
      { value: 'grandparent', label: 'Grandparent' },
    ],
  },
]

export const ParentsManagementPage: React.FC = () => {
  const {
    items: parents,
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
  } = useApiList<Parent>({
    endpoint: '/parents',
    pageSize: 10,
    autoFetch: true,
  })

  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [isDeleting, setIsDeleting] = useState(false)

  const handleBulkDelete = useCallback(async (selectedParents: Parent[]) => {
    if (!window.confirm(`Delete ${selectedParents.length} parent/parents?`)) return

    setIsDeleting(true)
    try {
      await Promise.all(
        selectedParents.map((parent) => apiClient.delete(`/parents/${parent.id}`))
      )
      setSelectedIds(new Set())
      await refresh()
    } catch (err) {
      const error = handleApiError(err)
      alert(`Error deleting parents: ${error.message}`)
    } finally {
      setIsDeleting(false)
    }
  }, [refresh])

  const handleExport = useCallback(() => {
    const exportFields = [
      'firstName',
      'lastName',
      'email',
      'phone',
      'relationship',
      'childrenCount',
      'status',
    ]
    exportData(parents, {
      filename: 'parents',
      format: 'csv',
      fields: exportFields,
    })
  }, [parents])

  const bulkActions: BulkAction<Parent>[] = [
    {
      id: 'delete',
      label: 'Delete',
      icon: <Trash2 size={16} />,
      onClick: () => handleBulkDelete(Array.from(selectedIds).map((id) => parents.find((p) => p.id === id)!)),
      color: 'red',
      requiresConfirm: true,
      confirmMessage: `Delete ${selectedIds.size} parent/parents? This action cannot be undone.`,
    },
  ]

  const selectedParents = parents.filter((p) => selectedIds.has(p.id))

  return (
    <div className="space-y-6">
      <ListPage
        title="Parents Management"
        description="Manage all parents and guardians in the system"
        items={parents}
        columns={COLUMNS}
        filterFields={FILTER_FIELDS}
        isLoading={isLoading}
        isRefreshing={isRefreshing}
        error={error}
        onRefresh={refresh}
        onAddNew={() => window.location.href = '/parents/create'}
        onEdit={(parent) => window.location.href = `/parents/${parent.id}/edit`}
        onDelete={(parent) => {
          if (window.confirm(`Delete ${parent.firstName} ${parent.lastName}?`)) {
            apiClient.delete(`/parents/${parent.id}`).then(() => refresh())
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
        createButtonLabel="+ Add Parent"
      />

      {/* Bulk Actions */}
      {selectedIds.size > 0 && (
        <div className="space-y-4">
          <BulkActions
            items={selectedParents}
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
      {!isLoading && parents.length > 0 && (
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

export default ParentsManagementPage
