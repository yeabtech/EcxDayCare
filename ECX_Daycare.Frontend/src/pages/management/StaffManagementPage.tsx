/**
 * Staff Management Page
 * Admin page for managing staff members
 */
import React, { useState, useCallback } from 'react'
import { useApiList } from '@features/dashboards/hooks/useApiList'
import { ListPage, BulkActions, Pagination } from '@components/shared'
import type { TableColumn, BulkAction } from '@components/shared'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'
import { exportData } from '@utils/exportData'
import { Trash2, Mail } from 'lucide-react'

export interface Staff {
  id: string
  firstName: string
  lastName: string
  email: string
  phone: string
  role: 'teacher' | 'assistant' | 'director' | 'nurse'
  classroomId?: string
  classroomName?: string
  status: 'active' | 'inactive' | 'onLeave'
  hireDate: string
  certifications: string[]
  experience: number // years
}

const COLUMNS: TableColumn<Staff>[] = [
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
    key: 'role',
    label: 'Role',
    width: '120px',
    render: (value) => (
      <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm font-medium">
        {value.charAt(0).toUpperCase() + value.slice(1)}
      </span>
    ),
  },
  {
    key: 'classroomName',
    label: 'Classroom',
    width: '150px',
    render: (value) => value || '—',
  },
  {
    key: 'email',
    label: 'Email',
    width: '200px',
    render: (value) => (
      <a href={`mailto:${value}`} className="text-blue-600 hover:underline flex items-center gap-1 text-sm">
        <Mail size={14} /> {value}
      </a>
    ),
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
            : value === 'onLeave'
              ? 'bg-yellow-100 text-yellow-800'
              : 'bg-gray-100 text-gray-800'
        }`}
      >
        {value === 'onLeave' ? 'On Leave' : value.charAt(0).toUpperCase() + value.slice(1)}
      </span>
    ),
  },
]

const FILTER_FIELDS = [
  {
    key: 'role',
    label: 'Role',
    type: 'select' as const,
    options: [
      { value: 'teacher', label: 'Teacher' },
      { value: 'assistant', label: 'Assistant' },
      { value: 'director', label: 'Director' },
      { value: 'nurse', label: 'Nurse' },
    ],
  },
  {
    key: 'status',
    label: 'Status',
    type: 'select' as const,
    options: [
      { value: 'active', label: 'Active' },
      { value: 'inactive', label: 'Inactive' },
      { value: 'onLeave', label: 'On Leave' },
    ],
  },
]

export const StaffManagementPage: React.FC = () => {
  const {
    items: staff,
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
  } = useApiList<Staff>({
    endpoint: '/staff',
    pageSize: 10,
    autoFetch: true,
  })

  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [isDeleting, setIsDeleting] = useState(false)

  const handleBulkDelete = useCallback(async (selectedStaff: Staff[]) => {
    if (!window.confirm(`Delete ${selectedStaff.length} staff member/members?`)) return

    setIsDeleting(true)
    try {
      await Promise.all(
        selectedStaff.map((member) => apiClient.delete(`/staff/${member.id}`))
      )
      setSelectedIds(new Set())
      await refresh()
    } catch (err) {
      const error = handleApiError(err)
      alert(`Error deleting staff: ${error.message}`)
    } finally {
      setIsDeleting(false)
    }
  }, [refresh])

  const handleExport = useCallback(() => {
    const exportFields = [
      'firstName',
      'lastName',
      'role',
      'classroomName',
      'email',
      'phone',
      'status',
      'experience',
    ]
    exportData(staff, {
      filename: 'staff',
      format: 'csv',
      fields: exportFields,
    })
  }, [staff])

  const bulkActions: BulkAction<Staff>[] = [
    {
      id: 'delete',
      label: 'Delete',
      icon: <Trash2 size={16} />,
      onClick: () => handleBulkDelete(Array.from(selectedIds).map((id) => staff.find((s) => s.id === id)!)),
      color: 'red',
      requiresConfirm: true,
      confirmMessage: `Delete ${selectedIds.size} staff member/members? This action cannot be undone.`,
    },
  ]

  const selectedStaff = staff.filter((s) => selectedIds.has(s.id))

  return (
    <div className="space-y-6">
      <ListPage
        title="Staff Management"
        description="Manage all staff members in the system"
        items={staff}
        columns={COLUMNS}
        filterFields={FILTER_FIELDS}
        isLoading={isLoading}
        isRefreshing={isRefreshing}
        error={error}
        onRefresh={refresh}
        onAddNew={() => window.location.href = '/staff/create'}
        onEdit={(staffMember) => window.location.href = `/staff/${staffMember.id}/edit`}
        onDelete={(staffMember) => {
          if (window.confirm(`Delete ${staffMember.firstName} ${staffMember.lastName}?`)) {
            apiClient.delete(`/staff/${staffMember.id}`).then(() => refresh())
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
        createButtonLabel="+ Add Staff"
      />

      {/* Bulk Actions */}
      {selectedIds.size > 0 && (
        <div className="space-y-4">
          <BulkActions
            items={selectedStaff}
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
      {!isLoading && staff.length > 0 && (
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

export default StaffManagementPage
