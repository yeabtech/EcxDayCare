/**
 * Enrollment Management Page
 * Admin page for managing child enrollments in classrooms
 */
import React, { useState, useCallback } from 'react'
import { useApiList } from '@features/dashboards/hooks/useApiList'
import { ListPage, BulkActions, Pagination } from '@components/shared'
import type { TableColumn, BulkAction } from '@components/shared'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'
import { exportData } from '@utils/exportData'
import { LogOut } from 'lucide-react'

export interface Enrollment {
  id: string
  childId: string
  childName: string
  childDateOfBirth: string
  classroomId: string
  classroomName: string
  enrollmentDate: string
  expectedGraduationDate?: string
  status: 'active' | 'graduated' | 'transferred' | 'withdrawn'
  tuitionPlan: string
  monthlyFee: number
  enrolledBy: string
  notes?: string
}

const COLUMNS: TableColumn<Enrollment>[] = [
  {
    key: 'childName',
    label: 'Child Name',
    width: '150px',
  },
  {
    key: 'classroomName',
    label: 'Classroom',
    width: '150px',
  },
  {
    key: 'enrollmentDate',
    label: 'Enrollment Date',
    width: '130px',
    render: (value) => new Date(value).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
  },
  {
    key: 'expectedGraduationDate',
    label: 'Expected Graduation',
    width: '130px',
    render: (value) => value ? new Date(value).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) : '—',
  },
  {
    key: 'tuitionPlan',
    label: 'Tuition Plan',
    width: '120px',
  },
  {
    key: 'monthlyFee',
    label: 'Monthly Fee',
    width: '120px',
    render: (value) => `$${value.toFixed(2)}`,
  },
  {
    key: 'status',
    label: 'Status',
    width: '120px',
    render: (value) => (
      <span
        className={`px-2 py-1 rounded-full text-xs font-medium ${
          value === 'active'
            ? 'bg-green-100 text-green-800'
            : value === 'graduated'
              ? 'bg-blue-100 text-blue-800'
              : value === 'transferred'
                ? 'bg-purple-100 text-purple-800'
                : 'bg-gray-100 text-gray-800'
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
      { value: 'graduated', label: 'Graduated' },
      { value: 'transferred', label: 'Transferred' },
      { value: 'withdrawn', label: 'Withdrawn' },
    ],
  },
  {
    key: 'classroomId',
    label: 'Classroom',
    type: 'select' as const,
    options: [],
  },
  {
    key: 'tuitionPlan',
    label: 'Tuition Plan',
    type: 'select' as const,
    options: [
      { value: 'fulltime', label: 'Full-Time' },
      { value: 'parttime', label: 'Part-Time' },
      { value: 'flexible', label: 'Flexible' },
    ],
  },
]

export const EnrollmentManagementPage: React.FC = () => {
  const {
    items: enrollments,
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
  } = useApiList<Enrollment>({
    endpoint: '/enrollments',
    pageSize: 10,
    autoFetch: true,
  })

  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [isProcessing, setIsProcessing] = useState(false)

  const handleWithdrawal = useCallback(async (selectedEnrollments: Enrollment[]) => {
    if (!window.confirm(`Withdraw ${selectedEnrollments.length} enrollment/enrollments?`)) return

    setIsProcessing(true)
    try {
      await Promise.all(
        selectedEnrollments.map((enrollment) =>
          apiClient.patch(`/enrollments/${enrollment.id}`, { status: 'withdrawn' })
        )
      )
      setSelectedIds(new Set())
      await refresh()
    } catch (err) {
      const error = handleApiError(err)
      alert(`Error withdrawing enrollments: ${error.message}`)
    } finally {
      setIsProcessing(false)
    }
  }, [refresh])

  const handleExport = useCallback(() => {
    const exportFields = [
      'childName',
      'classroomName',
      'enrollmentDate',
      'expectedGraduationDate',
      'status',
      'tuitionPlan',
      'monthlyFee',
    ]
    exportData(enrollments, {
      filename: 'enrollments',
      format: 'csv',
      fields: exportFields,
    })
  }, [enrollments])

  const bulkActions: BulkAction<Enrollment>[] = [
    {
      id: 'withdraw',
      label: 'Withdraw',
      icon: <LogOut size={16} />,
      onClick: () => handleWithdrawal(Array.from(selectedIds).map((id) => enrollments.find((e) => e.id === id)!)),
      color: 'blue',
      requiresConfirm: true,
      confirmMessage: `Withdraw ${selectedIds.size} enrollment/enrollments? This will mark them as withdrawn.`,
    },
  ]

  const selectedEnrollments = enrollments.filter((e) => selectedIds.has(e.id))

  return (
    <div className="space-y-6">
      <ListPage
        title="Enrollment Management"
        description="Manage child enrollments and tuition information"
        items={enrollments}
        columns={COLUMNS}
        filterFields={FILTER_FIELDS}
        isLoading={isLoading}
        isRefreshing={isRefreshing}
        error={error}
        onRefresh={refresh}
        onAddNew={() => window.location.href = '/enrollments/create'}
        onEdit={(enrollment) => window.location.href = `/enrollments/${enrollment.id}/edit`}
        onDelete={(enrollment) => {
          if (window.confirm('Are you sure you want to delete this enrollment?')) {
            apiClient.delete(`/enrollments/${enrollment.id}`).then(() => refresh())
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
        canBulkDelete={false}
        canExport={true}
        showSearch={true}
        showFilters={true}
        createButtonLabel="+ New Enrollment"
      />

      {/* Bulk Actions */}
      {selectedIds.size > 0 && (
        <div className="space-y-4">
          <BulkActions
            items={selectedEnrollments}
            actions={bulkActions}
            onComplete={() => {
              setSelectedIds(new Set())
              refresh()
            }}
            isLoading={isProcessing}
          />
        </div>
      )}

      {/* Pagination */}
      {!isLoading && enrollments.length > 0 && (
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

export default EnrollmentManagementPage
