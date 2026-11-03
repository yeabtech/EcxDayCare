/**
 * Children Management Page
 * Modern admin page for managing children:
 * - Card (grid) view with photo/image holder and initials fallback
 * - Modern info list (table view) for records fetched from the database
 * - Search, status filtering, multi-select, bulk actions and pagination
 */
import React, { useCallback, useEffect, useMemo, useState } from 'react'
import { useApiList } from '@features/dashboards/hooks/useApiList'
import { BulkActions, EmptyState, ErrorAlert, LoadingSpinner, Pagination } from '@components/shared'
import type { BulkAction } from '@components/shared'
import { apiClient } from '@services/api'
import { handleApiError, formatDate } from '@utils/index'
import { exportData } from '@utils/exportData'
import { childrenApi } from '@features/children/services/childrenApi'
import {
  CalendarDays,
  Download,
  GraduationCap,
  ImageOff,
  LayoutGrid,
  ListChecks,
  Pencil,
  Phone,
  Plus,
  RefreshCw,
  Search,
  Trash2,
  Users,
} from 'lucide-react'

export interface Child {
  id: string
  firstName: string
  lastName: string
  dateOfBirth: string
  enrollmentDate: string
  classroomId?: string
  classroomName?: string
  parentId?: string
  parentName?: string
  /** Raw status string from the API (e.g. "Enrolled", "Active", "Graduated") */
  status: string
  allergies?: string[] | string
  specialNeeds?: string
  emergencyContact?: string
  /** Optional fields provided by the backend ChildDto */
  age?: number
  gender?: string
  photoUrl?: string | null
  medicalNotes?: string | null
}

type ViewMode = 'grid' | 'list'

/* ------------------------------------------------------------------ */
/* Helpers                                                             */
/* ------------------------------------------------------------------ */

const AVATAR_COLORS = [
  'bg-primary-100 text-primary-700',
  'bg-accent-100 text-accent-800',
  'bg-amber-100 text-amber-700',
  'bg-emerald-100 text-emerald-700',
  'bg-accent-200 text-accent-800',
  'bg-primary-200 text-primary-800',
]

const STATUS_STYLES: Record<string, { label: string; badge: string; dot: string }> = {
  active: {
    label: 'Active',
    badge: 'bg-emerald-50 text-emerald-700 ring-emerald-600/20',
    dot: 'bg-emerald-500',
  },
  inactive: {
    label: 'Inactive',
    badge: 'bg-amber-50 text-amber-700 ring-amber-600/20',
    dot: 'bg-amber-500',
  },
  enrolled: {
    label: 'Enrolled',
    badge: 'bg-accent-50 text-accent-700 ring-accent-600/20',
    dot: 'bg-accent-500',
  },
  graduated: {
    label: 'Graduated',
    badge: 'bg-slate-100 text-slate-600 ring-slate-500/20',
    dot: 'bg-slate-400',
  },
}

const getFullName = (child: Child): string => `${child.firstName} ${child.lastName}`.trim()

const getInitials = (child: Child): string => {
  const initials = `${(child.firstName || '').charAt(0)}${(child.lastName || '').charAt(0)}`
  return initials.toUpperCase() || '?'
}

const avatarColorFor = (name: string): string =>
  AVATAR_COLORS[(name.charCodeAt(0) || 0) % AVATAR_COLORS.length]

const getStatusStyle = (status?: string) => {
  const key = (status || '').toLowerCase()
  return (
    STATUS_STYLES[key] ?? {
      label: status || 'Unknown',
      badge: 'bg-gray-100 text-gray-600 ring-gray-500/20',
      dot: 'bg-gray-400',
    }
  )
}

const getAgeYears = (child: Child): number | null => {
  if (typeof child.age === 'number') return child.age
  const dob = new Date(child.dateOfBirth)
  if (Number.isNaN(dob.getTime())) return null
  const years = Math.floor((Date.now() - dob.getTime()) / (365.25 * 24 * 60 * 60 * 1000))
  return years >= 0 ? years : null
}

const ageText = (child: Child): string => {
  const age = getAgeYears(child)
  return age !== null ? `${age} yrs` : '—'
}

const getAllergies = (child: Child): string[] => {
  const value = child.allergies
  const isPlaceholder = (item: string) => ['none', 'null', 'n/a', '-'].includes(item.toLowerCase())
  if (Array.isArray(value)) {
    return value.filter((item) => item && !isPlaceholder(String(item)))
  }
  if (typeof value === 'string' && value.trim()) {
    return value
      .split(',')
      .map((item) => item.trim())
      .filter((item) => item && !isPlaceholder(item))
  }
  return []
}

const formatDateSafe = (value?: string): string => (value ? formatDate(value) : '—')

/**
 * Reads a field from an API record regardless of casing.
 * The backend serializes PascalCase (Id, FirstName, ...), while some
 * endpoints/tests may provide camelCase - support both.
 */
const pick = (raw: any, camelKey: string, pascalKey: string): any =>
  raw?.[camelKey] !== undefined && raw?.[camelKey] !== null ? raw[camelKey] : raw?.[pascalKey]

/** Normalizes a raw API child record into the camelCase Child shape */
const normalizeChild = (raw: any): Child => ({
  id: pick(raw, 'id', 'Id') ?? '',
  firstName: pick(raw, 'firstName', 'FirstName') ?? '',
  lastName: pick(raw, 'lastName', 'LastName') ?? '',
  dateOfBirth: pick(raw, 'dateOfBirth', 'DateOfBirth') ?? '',
  enrollmentDate: pick(raw, 'enrollmentDate', 'EnrollmentDate') ?? '',
  classroomId: pick(raw, 'classroomId', 'ClassroomId') ?? undefined,
  classroomName: pick(raw, 'classroomName', 'ClassroomName') ?? undefined,
  parentId: pick(raw, 'parentId', 'ParentId') ?? undefined,
  parentName: pick(raw, 'parentName', 'ParentName') ?? undefined,
  status: String(pick(raw, 'status', 'Status') ?? 'active'),
  allergies: pick(raw, 'allergies', 'Allergies') ?? undefined,
  specialNeeds: pick(raw, 'specialNeeds', 'SpecialNeeds') ?? undefined,
  emergencyContact: pick(raw, 'emergencyContact', 'EmergencyContact') ?? undefined,
  age: typeof pick(raw, 'age', 'Age') === 'number' ? pick(raw, 'age', 'Age') : undefined,
  gender: pick(raw, 'gender', 'Gender') ?? undefined,
  photoUrl: pick(raw, 'photoUrl', 'PhotoUrl') ?? undefined,
  medicalNotes: pick(raw, 'medicalNotes', 'MedicalNotes') ?? undefined,
})

/* ------------------------------------------------------------------ */
/* Shared building blocks                                              */
/* ------------------------------------------------------------------ */

const AVATAR_SIZES = {
  sm: 'h-9 w-9 text-xs',
  md: 'h-12 w-12 text-sm',
  lg: 'h-20 w-20 text-xl',
}

const StatusBadge: React.FC<{ status?: string; className?: string }> = ({ status, className = '' }) => {
  const style = getStatusStyle(status)
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset ${style.badge} ${className}`}
    >
      <span className={`h-1.5 w-1.5 rounded-full ${style.dot}`} />
      {style.label}
    </span>
  )
}

interface ChildAvatarProps {
  child: Child
  size?: keyof typeof AVATAR_SIZES
}

const ChildAvatar: React.FC<ChildAvatarProps> = ({ child, size = 'md' }) => {
  const [failed, setFailed] = useState(false)
  const fullName = getFullName(child)

  if (child.photoUrl && !failed) {
    return (
      <img
        src={child.photoUrl}
        alt={fullName}
        onError={() => setFailed(true)}
        className={`${AVATAR_SIZES[size]} shrink-0 rounded-full object-cover shadow-sm ring-2 ring-white`}
      />
    )
  }

  return (
    <div
      className={`${AVATAR_SIZES[size]} flex shrink-0 items-center justify-center rounded-full font-semibold shadow-sm ring-2 ring-white ${avatarColorFor(fullName)}`}
    >
      {getInitials(child)}
    </div>
  )
}

/* ------------------------------------------------------------------ */
/* Grid view: child card with image holder                             */
/* ------------------------------------------------------------------ */

interface ChildCardProps {
  child: Child
  isSelected: boolean
  onToggleSelect: (id: string) => void
  onEdit: (child: Child) => void
  onDelete: (child: Child) => void
}

const ChildCard: React.FC<ChildCardProps> = ({ child, isSelected, onToggleSelect, onEdit, onDelete }) => {
  const [photoFailed, setPhotoFailed] = useState(false)
  const fullName = getFullName(child)
  const allergies = getAllergies(child)
  const hasPhoto = Boolean(child.photoUrl) && !photoFailed

  return (
    <div
      className={`group relative flex flex-col overflow-hidden rounded-2xl bg-white shadow-sm transition duration-200 hover:-translate-y-0.5 hover:shadow-xl ${
        isSelected ? 'ring-2 ring-accent-500' : 'ring-1 ring-gray-200'
      }`}
    >
      {/* Image holder */}
      <div className="relative h-44 overflow-hidden bg-gradient-to-br from-primary-50 via-white to-accent-100">
        {hasPhoto ? (
          <img
            src={child.photoUrl as string}
            alt={fullName}
            onError={() => setPhotoFailed(true)}
            className="h-full w-full object-cover transition duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full flex-col items-center justify-center gap-2">
            <div
              className={`flex h-20 w-20 items-center justify-center rounded-full text-2xl font-bold shadow-inner ring-4 ring-white ${avatarColorFor(fullName)}`}
            >
              {getInitials(child)}
            </div>
            <span className="inline-flex items-center gap-1 rounded-full bg-white/80 px-2.5 py-1 text-[11px] font-medium text-gray-500 shadow-sm">
              <ImageOff size={12} />
              No photo
            </span>
          </div>
        )}

        {/* Selection checkbox */}
        <label className="absolute left-3 top-3 flex h-6 w-6 cursor-pointer items-center justify-center rounded-md bg-white/85 shadow-sm ring-1 ring-black/5 backdrop-blur transition hover:bg-white">
          <input
            type="checkbox"
            checked={isSelected}
            onChange={() => onToggleSelect(child.id)}
            onClick={(e) => e.stopPropagation()}
            className="h-3.5 w-3.5 accent-primary-600"
            aria-label={`Select ${fullName}`}
          />
        </label>

        {/* Status pill */}
        <div className="absolute right-3 top-3">
          <StatusBadge status={child.status} className="shadow-sm" />
        </div>

        {/* Hover actions */}
        <div className="absolute inset-x-0 bottom-0 flex translate-y-full items-center justify-center gap-2 bg-gradient-to-t from-black/70 via-black/40 to-transparent p-3 opacity-0 transition-all duration-200 group-hover:translate-y-0 group-hover:opacity-100">
          <button
            type="button"
            onClick={() => onEdit(child)}
            className="inline-flex items-center gap-1.5 rounded-lg bg-accent-500/95 px-3 py-1.5 text-xs font-semibold text-primary-900 shadow-sm transition hover:bg-accent-500"
          >
            <Pencil size={13} />
            Edit
          </button>
          <button
            type="button"
            onClick={() => onDelete(child)}
            className="inline-flex items-center gap-1.5 rounded-lg bg-red-600/95 px-3 py-1.5 text-xs font-semibold text-white shadow-sm transition hover:bg-red-700"
          >
            <Trash2 size={13} />
            Delete
          </button>
        </div>
      </div>

      {/* Card body */}
      <div className="flex flex-1 flex-col gap-3 p-4">
        <div>
          <h3 className="text-base font-semibold leading-tight text-gray-900">{fullName}</h3>
          <p className="mt-0.5 text-xs text-gray-500">
            {ageText(child)}
            {child.gender ? ` · ${child.gender}` : ''}
          </p>
        </div>

        <dl className="space-y-2 text-sm">
          <div className="flex items-center gap-2 text-gray-600">
            <GraduationCap size={15} className="shrink-0 text-primary-600" />
            <dt className="sr-only">Classroom</dt>
            <dd className="truncate">{child.classroomName || 'Not assigned'}</dd>
          </div>
          <div className="flex items-center gap-2 text-gray-600">
            <Users size={15} className="shrink-0 text-accent-600" />
            <dt className="sr-only">Parent/Guardian</dt>
            <dd className="truncate">{child.parentName || 'On file'}</dd>
          </div>
          <div className="flex items-center gap-2 text-gray-600">
            <CalendarDays size={15} className="shrink-0 text-primary-500" />
            <dt className="sr-only">Date of birth</dt>
            <dd className="truncate">{formatDateSafe(child.dateOfBirth)}</dd>
          </div>
          {child.emergencyContact && (
            <div className="flex items-center gap-2 text-gray-600">
              <Phone size={15} className="shrink-0 text-rose-500" />
              <dt className="sr-only">Emergency contact</dt>
              <dd className="truncate">{child.emergencyContact}</dd>
            </div>
          )}
        </dl>

        {allergies.length > 0 && (
          <div className="mt-auto rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-xs leading-relaxed text-amber-800">
            <span className="font-semibold">Allergies:</span> {allergies.join(', ')}
          </div>
        )}
      </div>
    </div>
  )
}

/* ------------------------------------------------------------------ */
/* List view: modern table row                                         */
/* ------------------------------------------------------------------ */

interface ChildRowProps {
  child: Child
  isSelected: boolean
  onToggleSelect: (id: string) => void
  onEdit: (child: Child) => void
  onDelete: (child: Child) => void
}

const ChildRow: React.FC<ChildRowProps> = ({ child, isSelected, onToggleSelect, onEdit, onDelete }) => {
  const fullName = getFullName(child)

  return (
    <tr
      className={`group border-b border-gray-100 transition last:border-0 hover:bg-primary-50/60 ${
        isSelected ? 'bg-accent-50' : 'bg-white'
      }`}
    >
      <td className="px-4 py-3 align-middle">
        <input
          type="checkbox"
          checked={isSelected}
          onChange={() => onToggleSelect(child.id)}
          className="h-4 w-4 rounded accent-primary-600"
          aria-label={`Select ${fullName}`}
        />
      </td>
      <td className="px-4 py-3 align-middle">
        <div className="flex items-center gap-3">
          <ChildAvatar child={child} size="sm" />
          <div className="min-w-0">
            <div className="truncate font-medium text-gray-900">{fullName}</div>
            <div className="truncate text-xs text-gray-500">
              {ageText(child)}
              {child.gender ? ` · ${child.gender}` : ''}
            </div>
          </div>
        </div>
      </td>
      <td className="px-4 py-3 align-middle text-gray-600">
        <span className="inline-flex items-center gap-1.5">
          <GraduationCap size={15} className="shrink-0 text-primary-600" />
          <span className="truncate">{child.classroomName || 'Not assigned'}</span>
        </span>
      </td>
      <td className="px-4 py-3 align-middle text-gray-600">
        <span className="inline-flex items-center gap-1.5">
          <Users size={15} className="shrink-0 text-accent-600" />
          <span className="truncate">{child.parentName || 'On file'}</span>
        </span>
      </td>
      <td className="whitespace-nowrap px-4 py-3 align-middle text-gray-600">
        <span className="inline-flex items-center gap-1.5">
          <CalendarDays size={15} className="shrink-0 text-primary-500" />
          {formatDateSafe(child.dateOfBirth)}
        </span>
      </td>
      <td className="whitespace-nowrap px-4 py-3 align-middle text-gray-600">
        {formatDateSafe(child.enrollmentDate)}
      </td>
      <td className="px-4 py-3 align-middle">
        <StatusBadge status={child.status} />
      </td>
      <td className="px-4 py-3 align-middle">
        <div className="flex items-center justify-end gap-1 opacity-0 transition group-hover:opacity-100 focus-within:opacity-100">
          <button
            type="button"
            onClick={() => onEdit(child)}
            className="rounded-lg p-2 text-gray-500 transition hover:bg-primary-50 hover:text-primary-700"
            title="Edit child"
          >
            <Pencil size={15} />
          </button>
          <button
            type="button"
            onClick={() => onDelete(child)}
            className="rounded-lg p-2 text-gray-500 transition hover:bg-red-50 hover:text-red-600"
            title="Delete child"
          >
            <Trash2 size={15} />
          </button>
        </div>
      </td>
    </tr>
  )
}

/* ------------------------------------------------------------------ */
/* Page                                                                */
/* ------------------------------------------------------------------ */

export const ChildrenManagementPage: React.FC = () => {
  const {
    items: rawChildren,
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
  } = useApiList<Child>({
    endpoint: '/children',
    pageSize: 12,
    autoFetch: true,
  })

  /** Normalize records coming from the API (PascalCase) into the camelCase Child shape */
  const [parentNamesByChild, setParentNamesByChild] = useState<Record<string, string>>({})
  const children = useMemo(() => rawChildren.map(normalizeChild), [rawChildren])
  const childIdsKey = useMemo(() => children.map((child) => child.id).filter(Boolean).join('|'), [children])

  useEffect(() => {
    const ids = childIdsKey ? childIdsKey.split('|') : []
    if (!ids.length) {
      setParentNamesByChild({})
      return
    }
    let active = true
    void Promise.all(ids.map(async (childId) => {
      try {
        const parents = await childrenApi.getChildParents(childId)
        const names = parents
          .map((parent) => `${parent.FirstName ?? ''} ${parent.LastName ?? ''}`.trim())
          .filter(Boolean)
        return [childId, names.join(', ')] as const
      } catch {
        return [childId, ''] as const
      }
    })).then((entries) => {
      if (active) setParentNamesByChild(Object.fromEntries(entries))
    })
    return () => { active = false }
  }, [childIdsKey])

  const childrenWithParents = useMemo(() => children.map((child) => ({
    ...child,
    parentName: parentNamesByChild[child.id] || child.parentName,
  })), [children, parentNamesByChild])

  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [isDeleting, setIsDeleting] = useState(false)
  const [viewMode, setViewMode] = useState<ViewMode>('grid')
  const [searchQuery, setSearchQuery] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')

  /** Client-side filtering of the records loaded from the database */
  const filteredChildren = useMemo(() => {
    const query = searchQuery.trim().toLowerCase()
    return childrenWithParents.filter((child) => {
      const matchesStatus =
        statusFilter === 'all' || (child.status || '').toLowerCase() === statusFilter
      if (!matchesStatus) return false
      if (!query) return true
      return [child.firstName, child.lastName, child.classroomName, child.parentName]
        .filter(Boolean)
        .some((value) => String(value).toLowerCase().includes(query))
    })
  }, [childrenWithParents, searchQuery, statusFilter])

  const activeCount = useMemo(
    () =>
      children.filter((child) => {
        const status = (child.status || '').toLowerCase()
        return status === 'active' || status === 'enrolled'
      }).length,
    [children]
  )

  const allSelected =
    filteredChildren.length > 0 && filteredChildren.every((child) => selectedIds.has(child.id))

  const toggleSelection = useCallback((id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev)
      if (next.has(id)) {
        next.delete(id)
      } else {
        next.add(id)
      }
      return next
    })
  }, [])

  const toggleSelectAll = useCallback(() => {
    setSelectedIds((prev) => {
      const next = new Set(prev)
      if (allSelected) {
        filteredChildren.forEach((child) => next.delete(child.id))
      } else {
        filteredChildren.forEach((child) => next.add(child.id))
      }
      return next
    })
  }, [allSelected, filteredChildren])

  const handleDelete = useCallback(
    async (child: Child) => {
      if (!window.confirm(`Delete ${getFullName(child)}?`)) return
      try {
        await apiClient.delete(`/children/${child.id}`)
        setSelectedIds((prev) => {
          const next = new Set(prev)
          next.delete(child.id)
          return next
        })
        await refresh()
      } catch (err) {
        const apiError = handleApiError(err)
        alert(`Error deleting child: ${apiError.message}`)
      }
    },
    [refresh]
  )

  const handleBulkDelete = useCallback(
    async (selectedChildren: Child[]) => {
      if (selectedChildren.length === 0) return
      setIsDeleting(true)
      try {
        await Promise.all(
          selectedChildren.map((child) => apiClient.delete(`/children/${child.id}`))
        )
        setSelectedIds(new Set())
        await refresh()
      } catch (err) {
        const apiError = handleApiError(err)
        alert(`Error deleting children: ${apiError.message}`)
      } finally {
        setIsDeleting(false)
      }
    },
    [refresh]
  )

  const handleExport = useCallback(() => {
    exportData(children, {
      filename: 'children',
      format: 'csv',
      fields: ['firstName', 'lastName', 'gender', 'classroomName', 'parentName', 'dateOfBirth', 'status'],
    })
  }, [children])

  const handleEdit = useCallback((child: Child) => {
    window.location.href = `/children/${child.id}/edit`
  }, [])

  const handleAddNew = useCallback(() => {
    window.location.href = '/children/create'
  }, [])

  const bulkActions: BulkAction<Child>[] = [
    {
      id: 'delete',
      label: 'Delete',
      icon: <Trash2 size={16} />,
      onClick: (items) => handleBulkDelete(items),
      color: 'red',
      requiresConfirm: true,
      confirmMessage: `Delete ${selectedIds.size} child/children? This action cannot be undone.`,
    },
  ]

  const selectedChildren = children.filter((child) => selectedIds.has(child.id))

  const statChips = [
    { label: 'Total children', value: total },
    { label: 'On this page', value: children.length },
    { label: 'Active', value: activeCount },
    { label: 'Selected', value: selectedIds.size },
  ]

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="overflow-hidden rounded-2xl bg-gradient-to-r from-primary-600 via-primary-700 to-primary-800 p-6 text-white shadow-md shadow-primary-900/30">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold tracking-tight sm:text-3xl">Children Management</h1>
            <p className="mt-1 text-sm text-primary-100 sm:text-base">
              Manage all enrolled children in the system
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              onClick={refresh}
              disabled={isRefreshing || isLoading}
              className="inline-flex items-center gap-2 rounded-lg bg-white/15 px-3.5 py-2 text-sm font-medium text-white ring-1 ring-inset ring-white/20 transition hover:bg-white/25 disabled:opacity-50"
            >
              <RefreshCw size={16} className={isRefreshing ? 'animate-spin' : ''} />
              Refresh
            </button>
            <button
              type="button"
              onClick={handleExport}
              className="inline-flex items-center gap-2 rounded-lg bg-white/15 px-3.5 py-2 text-sm font-medium text-white ring-1 ring-inset ring-white/20 transition hover:bg-white/25"
            >
              <Download size={16} />
              Export
            </button>
            <button
              type="button"
              onClick={handleAddNew}
              className="inline-flex items-center gap-2 rounded-lg bg-accent-500 px-4 py-2 text-sm font-semibold text-primary-900 shadow-sm transition hover:bg-accent-400"
            >
              <Plus size={16} />
              Add Child
            </button>
          </div>
        </div>

        {/* Stat chips */}
        <div className="mt-5 grid grid-cols-2 gap-3 sm:grid-cols-4">
          {statChips.map((chip) => (
            <div
              key={chip.label}
              className="rounded-xl bg-white/10 px-4 py-3 ring-1 ring-inset ring-white/15"
            >
              <p className="text-[11px] font-medium uppercase tracking-wider text-accent-200">
                {chip.label}
              </p>
              <p className="mt-0.5 text-xl font-semibold text-accent-300">{chip.value}</p>
            </div>
          ))}
        </div>
      </div>

      {/* Error */}
      {error && <ErrorAlert message={error} onDismiss={refresh} />}

      {/* Toolbar */}
      <div className="flex flex-col gap-3 rounded-2xl border border-gray-200 bg-white p-4 shadow-sm lg:flex-row lg:items-center lg:justify-between">
        <div className="flex flex-1 flex-col gap-3 sm:flex-row sm:items-center">
          {/* Search */}
          <div className="relative flex-1 lg:max-w-sm">
            <Search
              size={18}
              className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
            />
            <input
              type="text"
              value={searchQuery}
              onChange={(event) => setSearchQuery(event.target.value)}
              placeholder="Search by name, classroom or parent..."
              className="w-full rounded-lg border border-gray-300 py-2.5 pl-10 pr-9 text-sm transition placeholder:text-gray-400 focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500/30"
            />
            {searchQuery && (
              <button
                type="button"
                onClick={() => setSearchQuery('')}
                className="absolute right-2.5 top-1/2 -translate-y-1/2 rounded p-0.5 text-gray-400 transition hover:text-gray-600"
                title="Clear search"
              >
                ✕
              </button>
            )}
          </div>

          {/* Status filter */}
          <select
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value)}
            className="rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm text-gray-700 transition focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500/30"
            aria-label="Filter by status"
          >
            <option value="all">All statuses</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
            <option value="enrolled">Enrolled</option>
            <option value="graduated">Graduated</option>
          </select>
        </div>

        <div className="flex items-center justify-between gap-3 lg:justify-end">
          {/* View toggle */}
          <div className="flex items-center rounded-lg border border-gray-200 bg-gray-50 p-1">
            <button
              type="button"
              onClick={() => setViewMode('grid')}
              className={`inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition ${
                viewMode === 'grid'
                  ? 'bg-white text-primary-700 shadow-sm'
                  : 'text-gray-500 hover:text-gray-700'
              }`}
            >
              <LayoutGrid size={16} />
              Cards
            </button>
            <button
              type="button"
              onClick={() => setViewMode('list')}
              className={`inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition ${
                viewMode === 'list'
                  ? 'bg-white text-primary-700 shadow-sm'
                  : 'text-gray-500 hover:text-gray-700'
              }`}
            >
              <ListChecks size={16} />
              List
            </button>
          </div>
          <span className="hidden whitespace-nowrap text-sm text-gray-500 sm:inline">
            {filteredChildren.length} of {total} shown
          </span>
        </div>
      </div>

      {/* Bulk actions */}
      {selectedIds.size > 0 && (
        <BulkActions
          items={selectedChildren}
          actions={bulkActions}
          onComplete={() => {
            setSelectedIds(new Set())
            refresh()
          }}
          isLoading={isDeleting}
        />
      )}

      {/* Content */}
      {isLoading ? (
        <LoadingSpinner message="Loading children..." />
      ) : filteredChildren.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-gray-300 bg-white p-6">
          <EmptyState
            icon={<Users size={40} className="text-primary-300" />}
            title="No children found"
            description={
              searchQuery || statusFilter !== 'all'
                ? 'No records match your search or filters. Try adjusting them.'
                : 'Get started by adding the first child to the system.'
            }
            action={
              searchQuery || statusFilter !== 'all'
                ? {
                    label: 'Clear search & filters',
                    onClick: () => {
                      setSearchQuery('')
                      setStatusFilter('all')
                    },
                  }
                : { label: '+ Add Child', onClick: handleAddNew }
            }
          />
        </div>
      ) : viewMode === 'grid' ? (
        /* Grid view with image holders */
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
          {filteredChildren.map((child) => (
            <ChildCard
              key={child.id}
              child={child}
              isSelected={selectedIds.has(child.id)}
              onToggleSelect={toggleSelection}
              onEdit={handleEdit}
              onDelete={handleDelete}
            />
          ))}
        </div>
      ) : (
        /* Modern info list */
        <div className="overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[900px] text-left text-sm">
              <thead className="border-b border-gray-200 bg-gray-50/80 text-xs font-semibold uppercase tracking-wider text-gray-500">
                <tr>
                  <th className="w-10 px-4 py-3">
                    <input
                      type="checkbox"
                      checked={allSelected}
                      onChange={toggleSelectAll}
                      className="h-4 w-4 rounded accent-primary-600"
                      aria-label="Select all children"
                    />
                  </th>
                  <th className="px-4 py-3">Child</th>
                  <th className="px-4 py-3">Classroom</th>
                  <th className="px-4 py-3">Parent/Guardian</th>
                  <th className="px-4 py-3">Date of Birth</th>
                  <th className="px-4 py-3">Enrolled</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredChildren.map((child) => (
                  <ChildRow
                    key={child.id}
                    child={child}
                    isSelected={selectedIds.has(child.id)}
                    onToggleSelect={toggleSelection}
                    onEdit={handleEdit}
                    onDelete={handleDelete}
                  />
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Pagination */}
      {!isLoading && filteredChildren.length > 0 && (
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

export default ChildrenManagementPage





