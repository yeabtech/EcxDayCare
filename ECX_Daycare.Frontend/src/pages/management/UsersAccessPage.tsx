import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  BadgeCheck,
  Check,
  ChevronDown,
  CircleAlert,
  Clock3,
  KeyRound,
  LoaderCircle,
  LockKeyhole,
  Mail,
  Plus,
  Search,
  ShieldCheck,
  UserRound,
  UsersRound,
  X,
} from 'lucide-react'
import { apiClient } from '@services/api'
import { staffApi } from '@features/staff/services/staffApi'
import { parentsApi } from '@features/parents/services/parentsApi'
import type { Staff } from '@features/staff/types'
import type { Parent } from '@features/parents/types'

type AccountRole = 'Staff' | 'Parent' | 'Admin'
type RoleFilter = 'All' | AccountRole
type StatusFilter = 'All' | 'Active' | 'Disabled'

interface ManagedUser {
  id: string
  email: string
  firstName: string
  lastName: string
  phoneNumber: string | null
  role: AccountRole | string
  isActive: boolean
  emailConfirmed: boolean
  staffId: string | null
  parentId: string | null
  createdAt: string
}

interface AccountForm {
  role: 'Staff' | 'Parent'
  profileId: string
  email: string
  password: string
}

const initialForm: AccountForm = { role: 'Staff', profileId: '', email: '', password: '' }

const unwrap = <T,>(response: any): T => response?.data?.data ?? response?.data ?? response

const getErrorMessage = (error: any): string =>
  error?.response?.data?.error ??
  error?.response?.data?.message ??
  error?.response?.data?.errors?.[0] ??
  error?.message ??
  'Something went wrong. Please try again.'

const initials = (firstName: string, lastName: string) =>
  `${firstName?.[0] ?? ''}${lastName?.[0] ?? ''}`.toUpperCase() || 'U'

const formatDate = (value: string) => {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
}

export const UsersAccessPage = () => {
  const [users, setUsers] = useState<ManagedUser[]>([])
  const [staff, setStaff] = useState<Staff[]>([])
  const [parents, setParents] = useState<Parent[]>([])
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState('')
  const [search, setSearch] = useState('')
  const [roleFilter, setRoleFilter] = useState<RoleFilter>('All')
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All')
  const [modalOpen, setModalOpen] = useState(false)
  const [form, setForm] = useState<AccountForm>(initialForm)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState('')
  const [updatingId, setUpdatingId] = useState('')
  const [notice, setNotice] = useState('')

  const loadPage = useCallback(async () => {
    setLoading(true)
    setPageError('')
    try {
      const [userResponse, staffResponse, parentResponse] = await Promise.all([
        apiClient.get('/auth/users'),
        staffApi.getStaff({ page: 1, pageSize: 100 }),
        parentsApi.getParents({ page: 1, pageSize: 100 }),
      ])
      const userRows = unwrap<any[]>(userResponse)
      setUsers((Array.isArray(userRows) ? userRows : []).map((row) => ({
        id: row.id ?? row.Id,
        email: row.email ?? row.Email ?? '',
        firstName: row.firstName ?? row.FirstName ?? '',
        lastName: row.lastName ?? row.LastName ?? '',
        phoneNumber: row.phoneNumber ?? row.PhoneNumber ?? null,
        role: row.role ?? row.Role ?? 'Parent',
        isActive: row.isActive ?? row.IsActive ?? false,
        emailConfirmed: row.emailConfirmed ?? row.EmailConfirmed ?? false,
        staffId: row.staffId ?? row.StaffId ?? null,
        parentId: row.parentId ?? row.ParentId ?? null,
        createdAt: row.createdAt ?? row.CreatedAt ?? '',
      })))
      setStaff(staffResponse.items)
      setParents(parentResponse.items)
    } catch (error) {
      setPageError(getErrorMessage(error))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { void loadPage() }, [loadPage])

  const filteredUsers = useMemo(() => {
    const query = search.trim().toLowerCase()
    return users.filter((user) => {
      const matchesQuery = !query ||
        `${user.firstName} ${user.lastName} ${user.email} ${user.role}`.toLowerCase().includes(query)
      const matchesRole = roleFilter === 'All' || user.role.toLowerCase() === roleFilter.toLowerCase()
      const matchesStatus = statusFilter === 'All' || (statusFilter === 'Active' ? user.isActive : !user.isActive)
      return matchesQuery && matchesRole && matchesStatus
    })
  }, [users, search, roleFilter, statusFilter])

  const linkedStaffIds = useMemo(() => new Set(users.map((user) => user.staffId).filter(Boolean)), [users])
  const linkedParentIds = useMemo(() => new Set(users.map((user) => user.parentId).filter(Boolean)), [users])
  const profiles = form.role === 'Staff'
    ? staff.filter((person) => !linkedStaffIds.has(person.id)).map((person) => ({
        id: person.id,
        name: `${person.firstName} ${person.lastName}`.trim(),
        email: person.email,
        detail: person.classroomName || person.role,
      }))
    : parents.filter((person) => !linkedParentIds.has(person.id)).map((person) => ({
        id: person.id,
        name: `${person.firstName} ${person.lastName}`.trim(),
        email: person.email,
        detail: person.phoneNumber,
      }))

  const stats = useMemo(() => ({
    total: users.length,
    active: users.filter((user) => user.isActive).length,
    staff: users.filter((user) => user.role.toLowerCase() === 'staff').length,
    parents: users.filter((user) => user.role.toLowerCase() === 'parent').length,
  }), [users])

  const openModal = () => {
    setForm(initialForm)
    setFormError('')
    setModalOpen(true)
  }

  const changeRole = (role: 'Staff' | 'Parent') => {
    setForm({ ...initialForm, role })
    setFormError('')
  }

  const selectProfile = (profileId: string) => {
    const selected = profiles.find((profile) => profile.id === profileId)
    setForm((current) => ({ ...current, profileId, email: selected?.email ?? '' }))
  }

  const createAccount = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError('')
    if (!form.profileId) {
      setFormError(`Choose an existing ${form.role.toLowerCase()} profile to link this login.`)
      return
    }

    setSaving(true)
    try {
      await apiClient.post('/auth/users', {
        email: form.email.trim(),
        password: form.password,
        role: form.role,
        profileId: form.profileId,
      })
      setModalOpen(false)
      setNotice('Account created and linked to its profile.')
      await loadPage()
      window.setTimeout(() => setNotice(''), 3500)
    } catch (error) {
      setFormError(getErrorMessage(error))
    } finally {
      setSaving(false)
    }
  }

  const toggleStatus = async (user: ManagedUser) => {
    setUpdatingId(user.id)
    setPageError('')
    try {
      await apiClient.patch(`/auth/users/${user.id}/status`, { isActive: !user.isActive })
      setUsers((current) => current.map((item) => item.id === user.id ? { ...item, isActive: !item.isActive } : item))
      setNotice(`${user.firstName} ${user.lastName}'s account ${user.isActive ? 'disabled' : 'enabled'}.`)
      window.setTimeout(() => setNotice(''), 3500)
    } catch (error) {
      setPageError(getErrorMessage(error))
    } finally {
      setUpdatingId('')
    }
  }

  return (
    <main className="mx-auto max-w-7xl space-y-6 px-4 py-6 sm:px-6 lg:px-8">
      <section className="relative overflow-hidden rounded-3xl bg-gradient-to-br from-[#153e2a] via-[#1b5e3a] to-[#317550] px-6 py-7 text-white shadow-sm md:px-8 md:py-8">
        <div className="absolute -right-14 -top-20 h-64 w-64 rounded-full border-[36px] border-white/5" />
        <div className="relative flex flex-col justify-between gap-6 md:flex-row md:items-end">
          <div>
            <div className="mb-3 inline-flex items-center gap-2 rounded-full bg-white/10 px-3 py-1.5 text-xs font-semibold text-white/80">
              <ShieldCheck size={15} /> ADMINISTRATION · IDENTITY & ACCESS
            </div>
            <h1 className="text-3xl font-bold tracking-tight text-white md:text-4xl">Users & access</h1>
            <p className="mt-2 max-w-xl text-sm leading-6 text-white/75">
              Create sign-in accounts for existing staff and parent profiles, review access, and manage account status.
            </p>
          </div>
          <button onClick={openModal} className="inline-flex shrink-0 items-center justify-center gap-2 rounded-xl bg-[#f5a623] px-4 py-3 text-sm font-bold text-[#153e2a] shadow-sm transition hover:bg-amber-300">
            <Plus size={18} /> Create account
          </button>
        </div>
      </section>

      {notice && <div role="status" className="flex items-center gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-800"><Check size={17} />{notice}</div>}
      {pageError && <div role="alert" className="flex items-center gap-2 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"><CircleAlert size={17} />{pageError}</div>}

      <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4" aria-label="Account summary">
        {[
          { label: 'Registered accounts', value: stats.total, icon: UsersRound, tint: 'bg-[#e6efeb] text-[#1b5e3a]' },
          { label: 'Active accounts', value: stats.active, icon: BadgeCheck, tint: 'bg-emerald-50 text-emerald-600' },
          { label: 'Staff logins', value: stats.staff, icon: UserRound, tint: 'bg-blue-50 text-blue-600' },
          { label: 'Parent logins', value: stats.parents, icon: LockKeyhole, tint: 'bg-amber-50 text-amber-600' },
        ].map(({ label, value, icon: Icon, tint }) => (
          <article key={label} className="flex items-center justify-between rounded-2xl border border-[#dfe8e2] bg-white p-5 shadow-sm">
            <div><p className="text-sm font-medium text-slate-500">{label}</p><p className="mt-1 text-2xl font-bold text-slate-900">{loading ? '—' : value}</p></div>
            <span className={`flex h-11 w-11 items-center justify-center rounded-xl ${tint}`}><Icon size={20} /></span>
          </article>
        ))}
      </section>

      <section className="overflow-hidden rounded-2xl border border-[#dfe8e2] bg-white shadow-sm">
        <div className="flex flex-col gap-4 border-b border-[#dfe8e2] p-5 md:flex-row md:items-center md:justify-between md:px-6">
          <div><h2 className="text-lg font-bold text-slate-900">Registered users</h2><p className="mt-1 text-sm text-slate-500">Accounts and the staff or parent records they can access.</p></div>
          <div className="flex flex-wrap gap-2">
            <label className="flex min-w-[220px] flex-1 items-center gap-2 rounded-xl border border-[#dfe8e2] bg-white px-3 py-2.5 text-slate-400 focus-within:border-emerald-500 focus-within:ring-4 focus-within:ring-emerald-500/10">
              <Search size={17} /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search name or email" className="w-full bg-transparent text-sm text-slate-800 outline-none placeholder:text-slate-400" />
            </label>
            <div className="relative"><select value={roleFilter} onChange={(event) => setRoleFilter(event.target.value as RoleFilter)} className="h-full appearance-none rounded-xl border border-[#dfe8e2] bg-white py-2.5 pl-3 pr-9 text-sm text-slate-700 outline-none focus:border-emerald-500"><option value="All">All roles</option><option value="Staff">Staff</option><option value="Parent">Parent</option><option value="Admin">Admin</option></select><ChevronDown size={15} className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-slate-400" /></div>
            <div className="relative"><select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value as StatusFilter)} className="h-full appearance-none rounded-xl border border-[#dfe8e2] bg-white py-2.5 pl-3 pr-9 text-sm text-slate-700 outline-none focus:border-emerald-500"><option value="All">Any status</option><option value="Active">Active</option><option value="Disabled">Disabled</option></select><ChevronDown size={15} className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-slate-400" /></div>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full min-w-[800px] text-left">
            <thead className="bg-slate-50 text-[11px] font-bold uppercase tracking-wider text-slate-500"><tr><th className="px-6 py-3">Account</th><th className="px-5 py-3">Role & profile</th><th className="px-5 py-3">Status</th><th className="px-5 py-3">Created</th><th className="px-6 py-3 text-right">Access</th></tr></thead>
            <tbody className="divide-y divide-slate-100">
              {loading ? <tr><td colSpan={5} className="px-6 py-14 text-center text-sm text-slate-500"><LoaderCircle size={18} className="mr-2 inline animate-spin" />Loading accounts…</td></tr> : filteredUsers.length ? filteredUsers.map((user) => {
                const hasProfile = Boolean(user.staffId || user.parentId)
                const role = user.role.toLowerCase()
                return <tr key={user.id} className="transition hover:bg-[#f8fbf9]">
                  <td className="px-6 py-4"><div className="flex items-center gap-3"><span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-[#e6efeb] text-xs font-bold text-[#1b5e3a]">{initials(user.firstName, user.lastName)}</span><div className="min-w-0"><p className="truncate text-sm font-bold text-slate-800">{user.firstName} {user.lastName}</p><p className="mt-0.5 flex items-center gap-1 truncate text-xs text-slate-500"><Mail size={12} />{user.email}</p></div></div></td>
                  <td className="px-5 py-4"><span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${role === 'staff' ? 'bg-blue-50 text-blue-700' : role === 'parent' ? 'bg-amber-50 text-amber-700' : 'bg-violet-50 text-violet-700'}`}>{user.role}</span><p className={`mt-1.5 text-xs ${hasProfile ? 'text-slate-500' : 'text-amber-700'}`}>{hasProfile ? `Linked ${user.staffId ? 'staff' : 'parent'} profile` : 'No profile linked'}</p></td>
                  <td className="px-5 py-4"><span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${user.isActive ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'}`}><i className={`h-1.5 w-1.5 rounded-full ${user.isActive ? 'bg-emerald-500' : 'bg-slate-400'}`} />{user.isActive ? 'Active' : 'Disabled'}</span><p className="mt-1.5 text-xs text-slate-400">{user.emailConfirmed ? 'Email confirmed' : 'Email not confirmed'}</p></td>
                  <td className="px-5 py-4"><span className="inline-flex items-center gap-1.5 text-xs text-slate-500"><Clock3 size={14} />{formatDate(user.createdAt)}</span></td>
                  <td className="px-6 py-4 text-right"><button onClick={() => void toggleStatus(user)} disabled={updatingId === user.id} className={`inline-flex min-w-[96px] items-center justify-center gap-1.5 rounded-lg border px-3 py-2 text-xs font-semibold transition disabled:opacity-50 ${user.isActive ? 'border-rose-200 text-rose-700 hover:bg-rose-50' : 'border-emerald-200 text-emerald-700 hover:bg-emerald-50'}`}>{updatingId === user.id ? <LoaderCircle size={14} className="animate-spin" /> : user.isActive ? <LockKeyhole size={14} /> : <KeyRound size={14} />}{user.isActive ? 'Disable' : 'Enable'}</button></td>
                </tr>
              }) : <tr><td colSpan={5} className="px-6 py-14 text-center"><div className="mx-auto flex max-w-sm flex-col items-center"><span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-[#e6efeb] text-[#1b5e3a]"><UsersRound size={22} /></span><p className="mt-3 text-sm font-bold text-slate-800">{users.length ? 'No accounts match these filters' : 'No accounts found'}</p><p className="mt-1 text-xs text-slate-500">Create a staff or parent login to give someone access.</p>{!users.length && <button onClick={openModal} className="mt-4 inline-flex items-center gap-1.5 rounded-lg bg-[#1b5e3a] px-3 py-2 text-xs font-semibold text-white hover:bg-[#153e2a]"><Plus size={14} />Create account</button>}</div></td></tr>}
            </tbody>
          </table>
        </div>
        {!loading && <div className="border-t border-[#dfe8e2] px-6 py-3 text-xs text-slate-500">Showing {filteredUsers.length} of {users.length} accounts</div>}
      </section>

      {modalOpen && <div className="fixed inset-0 z-[100] flex items-center justify-center overflow-y-auto bg-slate-950/50 p-4 backdrop-blur-sm" role="presentation" onMouseDown={(event) => { if (event.target === event.currentTarget) setModalOpen(false) }}>
        <section role="dialog" aria-modal="true" aria-labelledby="create-account-title" className="my-auto w-full max-w-lg overflow-hidden rounded-3xl bg-white shadow-2xl">
          <div className="flex items-start justify-between border-b border-slate-100 px-6 py-5"><div><div className="mb-2 inline-flex h-9 w-9 items-center justify-center rounded-xl bg-[#e6efeb] text-[#1b5e3a]"><UserRound size={18} /></div><h2 id="create-account-title" className="text-xl font-bold text-slate-900">Create login account</h2><p className="mt-1 text-sm text-slate-500">Link the account to a current profile.</p></div><button aria-label="Close" onClick={() => setModalOpen(false)} className="rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700"><X size={19} /></button></div>
          <form onSubmit={(event) => void createAccount(event)} className="space-y-5 px-6 py-5">
            <div><p className="mb-2 text-xs font-bold uppercase tracking-wide text-slate-500">Account type</p><div className="grid grid-cols-2 gap-2"><button type="button" onClick={() => changeRole('Staff')} className={`rounded-xl border px-4 py-3 text-left transition ${form.role === 'Staff' ? 'border-[#1b5e3a] bg-[#e6efeb] text-[#153e2a] ring-1 ring-[#1b5e3a]' : 'border-slate-200 text-slate-600 hover:bg-slate-50'}`}><span className="block text-sm font-bold">Staff</span><span className="mt-1 block text-xs opacity-70">Teacher and team access</span></button><button type="button" onClick={() => changeRole('Parent')} className={`rounded-xl border px-4 py-3 text-left transition ${form.role === 'Parent' ? 'border-[#1b5e3a] bg-[#e6efeb] text-[#153e2a] ring-1 ring-[#1b5e3a]' : 'border-slate-200 text-slate-600 hover:bg-slate-50'}`}><span className="block text-sm font-bold">Parent</span><span className="mt-1 block text-xs opacity-70">Family portal access</span></button></div></div>
            <label className="block"><span className="mb-1.5 block text-sm font-semibold text-slate-700">Existing {form.role.toLowerCase()} profile</span><select required value={form.profileId} onChange={(event) => selectProfile(event.target.value)} className="w-full rounded-xl border border-slate-200 bg-white px-3.5 py-3 text-sm text-slate-800 outline-none focus:border-emerald-600 focus:ring-4 focus:ring-emerald-600/10"><option value="">Select a profile…</option>{profiles.map((profile) => <option key={profile.id} value={profile.id}>{profile.name} · {profile.email}</option>)}</select><span className="mt-1.5 block text-xs text-slate-500">{profiles.length ? `${profiles.length} unlinked profile${profiles.length === 1 ? '' : 's'} available` : `No unlinked ${form.role.toLowerCase()} profiles are available.`}</span></label>
            <label className="block"><span className="mb-1.5 block text-sm font-semibold text-slate-700">Login email</span><input required type="email" autoComplete="email" value={form.email} onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))} placeholder="name@example.com" className="w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-emerald-600 focus:ring-4 focus:ring-emerald-600/10" /></label>
            <label className="block"><span className="mb-1.5 block text-sm font-semibold text-slate-700">Temporary password</span><input required minLength={6} type="password" autoComplete="new-password" value={form.password} onChange={(event) => setForm((current) => ({ ...current, password: event.target.value }))} placeholder="At least 6 characters" className="w-full rounded-xl border border-slate-200 px-3.5 py-3 text-sm outline-none focus:border-emerald-600 focus:ring-4 focus:ring-emerald-600/10" /></label>
            {formError && <div role="alert" className="flex items-start gap-2 rounded-xl border border-rose-200 bg-rose-50 px-3.5 py-3 text-sm text-rose-700"><CircleAlert size={16} className="mt-0.5 shrink-0" />{formError}</div>}
            <div className="flex justify-end gap-2 border-t border-slate-100 pt-4"><button type="button" onClick={() => setModalOpen(false)} className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-50">Cancel</button><button type="submit" disabled={saving || !profiles.length} className="inline-flex items-center gap-2 rounded-xl bg-[#1b5e3a] px-4 py-2.5 text-sm font-bold text-white hover:bg-[#153e2a] disabled:cursor-not-allowed disabled:opacity-50">{saving ? <LoaderCircle size={16} className="animate-spin" /> : <Plus size={16} />}{saving ? 'Creating…' : 'Create account'}</button></div>
          </form>
        </section>
      </div>}
    </main>
  )
}
