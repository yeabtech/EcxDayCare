import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  Activity,
  AlertCircle,
  Check,
  Clock3,
  Database,
  HardDrive,
  LoaderCircle,
  LockKeyhole,
  RefreshCw,
  Server,
  ShieldCheck,
  UsersRound,
  UserRound,
  Baby,
  School,
  Eye,
  EyeOff,
} from 'lucide-react'
import { apiClient } from '@services/api'

interface AdminMetrics {
  database: {
    name: string
    sizeBytes: number
    activeConnections: number
    maxConnections: number
    cacheHitPercent: number
    serverStartedAt: string
    measuredQueryMilliseconds: number
  }
  uploadThing: {
    connected: boolean
    message: string | null
    totalBytes: number | null
    appTotalBytes: number | null
    filesUploaded: number | null
    limitBytes: number | null
    localUploadLimitBytes: number
  }
  records: {
    children: number
    staff: number
    parents: number
    accounts: number
    activeAccounts: number
    attendanceRecords: number
  }
  application: {
    environment: string
    runtime: string
    startedAt: string
    uptimeSeconds: number
  }
}

const pick = (source: any, camel: string, pascal: string) => source?.[camel] ?? source?.[pascal]

const normalizeMetrics = (source: any): AdminMetrics => {
  const database = pick(source, 'database', 'Database') ?? {}
  const upload = pick(source, 'uploadThing', 'UploadThing') ?? {}
  const records = pick(source, 'records', 'Records') ?? {}
  const application = pick(source, 'application', 'Application') ?? {}
  return {
    database: {
      name: pick(database, 'name', 'Name') ?? '—',
      sizeBytes: pick(database, 'sizeBytes', 'SizeBytes') ?? 0,
      activeConnections: pick(database, 'activeConnections', 'ActiveConnections') ?? 0,
      maxConnections: pick(database, 'maxConnections', 'MaxConnections') ?? 0,
      cacheHitPercent: pick(database, 'cacheHitPercent', 'CacheHitPercent') ?? 0,
      serverStartedAt: pick(database, 'serverStartedAt', 'ServerStartedAt') ?? '',
      measuredQueryMilliseconds: pick(database, 'measuredQueryMilliseconds', 'MeasuredQueryMilliseconds') ?? 0,
    },
    uploadThing: {
      connected: pick(upload, 'connected', 'Connected') ?? false,
      message: pick(upload, 'message', 'Message') ?? null,
      totalBytes: pick(upload, 'totalBytes', 'TotalBytes') ?? null,
      appTotalBytes: pick(upload, 'appTotalBytes', 'AppTotalBytes') ?? null,
      filesUploaded: pick(upload, 'filesUploaded', 'FilesUploaded') ?? null,
      limitBytes: pick(upload, 'limitBytes', 'LimitBytes') ?? null,
      localUploadLimitBytes: pick(upload, 'localUploadLimitBytes', 'LocalUploadLimitBytes') ?? 0,
    },
    records: {
      children: pick(records, 'children', 'Children') ?? 0,
      staff: pick(records, 'staff', 'Staff') ?? 0,
      parents: pick(records, 'parents', 'Parents') ?? 0,
      accounts: pick(records, 'accounts', 'Accounts') ?? 0,
      activeAccounts: pick(records, 'activeAccounts', 'ActiveAccounts') ?? 0,
      attendanceRecords: pick(records, 'attendanceRecords', 'AttendanceRecords') ?? 0,
    },
    application: {
      environment: pick(application, 'environment', 'Environment') ?? 'Unknown',
      runtime: pick(application, 'runtime', 'Runtime') ?? 'Unknown',
      startedAt: pick(application, 'startedAt', 'StartedAt') ?? '',
      uptimeSeconds: pick(application, 'uptimeSeconds', 'UptimeSeconds') ?? 0,
    },
  }
}

const formatBytes = (bytes: number | null | undefined) => {
  if (bytes == null || !Number.isFinite(bytes)) return 'Unavailable'
  if (bytes < 1024) return `${bytes} B`
  const units = ['KB', 'MB', 'GB', 'TB']
  let value = bytes / 1024
  let unitIndex = 0
  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024
    unitIndex += 1
  }
  return `${value.toFixed(value >= 10 ? 1 : 2)} ${units[unitIndex]}`
}

const formatDuration = (seconds: number) => {
  const days = Math.floor(seconds / 86400)
  const hours = Math.floor((seconds % 86400) / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return days ? `${days}d ${hours}h` : hours ? `${hours}h ${minutes}m` : `${minutes}m`
}

const getError = (error: any) =>
  error?.response?.data?.error ?? error?.response?.data?.message ?? error?.message ?? 'Request failed. Please try again.'

export const AdminPreferencesPage = () => {
  const [metrics, setMetrics] = useState<AdminMetrics | null>(null)
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [metricsError, setMetricsError] = useState('')
  const [passwords, setPasswords] = useState({ current: '', next: '', confirm: '' })
  const [showPasswords, setShowPasswords] = useState(false)
  const [savingPassword, setSavingPassword] = useState(false)
  const [passwordError, setPasswordError] = useState('')
  const [passwordSuccess, setPasswordSuccess] = useState('')

  const loadMetrics = useCallback(async (quiet = false) => {
    if (quiet) setRefreshing(true)
    else setLoading(true)
    setMetricsError('')
    try {
      const response = await apiClient.get<any>('/admin/settings/metrics')
      setMetrics(normalizeMetrics(response.data?.data ?? response.data))
    } catch (error) {
      setMetricsError(getError(error))
    } finally {
      setLoading(false)
      setRefreshing(false)
    }
  }, [])

  useEffect(() => { void loadMetrics() }, [loadMetrics])

  const connectionUse = useMemo(() => {
    if (!metrics?.database.maxConnections) return 0
    return Math.min(100, Math.round(metrics.database.activeConnections / metrics.database.maxConnections * 100))
  }, [metrics])

  const uploadUse = useMemo(() => {
    const used = metrics?.uploadThing.appTotalBytes
    const limit = metrics?.uploadThing.limitBytes
    if (used == null || !limit) return 0
    return Math.min(100, Math.round(used / limit * 100))
  }, [metrics])

  const changePassword = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setPasswordError('')
    setPasswordSuccess('')
    if (passwords.next !== passwords.confirm) {
      setPasswordError('The new password and confirmation do not match.')
      return
    }
    if (passwords.next.length < 8) {
      setPasswordError('Use at least 8 characters for the new password.')
      return
    }
    setSavingPassword(true)
    try {
      const response = await apiClient.post<any>('/admin/settings/change-password', {
        currentPassword: passwords.current,
        newPassword: passwords.next,
      })
      setPasswordSuccess(response.data?.message ?? 'Your password has been changed.')
      setPasswords({ current: '', next: '', confirm: '' })
    } catch (error) {
      setPasswordError(getError(error))
    } finally {
      setSavingPassword(false)
    }
  }

  return (
    <main className="mx-auto max-w-7xl space-y-6 px-4 py-6 sm:px-6 lg:px-8">
      <section className="relative overflow-hidden rounded-3xl bg-gradient-to-br from-[#153e2a] via-[#1b5e3a] to-[#317550] px-6 py-7 text-white shadow-sm md:px-8 md:py-8">
        <div className="absolute -right-16 -top-24 h-72 w-72 rounded-full border-[42px] border-white/5" />
        <div className="relative flex flex-col justify-between gap-5 md:flex-row md:items-end">
          <div>
            <div className="mb-3 inline-flex items-center gap-2 rounded-full bg-white/10 px-3 py-1.5 text-xs font-semibold text-white/80"><ShieldCheck size={15} /> ADMINISTRATOR CONSOLE</div>
            <h1 className="text-3xl font-bold tracking-tight text-white md:text-4xl">System settings</h1>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-white/75">Review live database and file storage health, and protect your administrator account.</p>
          </div>
          <button onClick={() => void loadMetrics(true)} disabled={loading || refreshing} className="inline-flex items-center justify-center gap-2 self-start rounded-xl border border-white/20 bg-white/10 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-white/15 disabled:opacity-50 md:self-auto"><RefreshCw size={16} className={refreshing ? 'animate-spin' : ''} />Refresh metrics</button>
        </div>
      </section>

      {metricsError && <div role="alert" className="flex items-center gap-2 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"><AlertCircle size={17} />{metricsError}</div>}

      <section>
        <div className="mb-3 flex items-center justify-between"><div><h2 className="text-lg font-bold text-slate-900">Database performance</h2><p className="mt-1 text-sm text-slate-500">Live values queried from PostgreSQL.</p></div>{metrics && <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">Connected · {metrics.database.name}</span>}</div>
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          {[
            { label: 'Database size', value: metrics ? formatBytes(metrics.database.sizeBytes) : '—', helper: metrics?.database.name ?? 'PostgreSQL', icon: Database, tint: 'bg-[#e6efeb] text-[#1b5e3a]' },
            { label: 'Query response', value: metrics ? `${metrics.database.measuredQueryMilliseconds} ms` : '—', helper: 'Metrics query round trip', icon: Activity, tint: 'bg-blue-50 text-blue-600' },
            { label: 'Buffer cache hit', value: metrics ? `${metrics.database.cacheHitPercent.toFixed(1)}%` : '—', helper: 'PostgreSQL shared buffers', icon: HardDrive, tint: 'bg-violet-50 text-violet-600' },
            { label: 'Database uptime', value: metrics ? formatDuration(Math.max(0, Math.floor((Date.now() - new Date(metrics.database.serverStartedAt).getTime()) / 1000))) : '—', helper: metrics?.database.serverStartedAt ? `Since ${new Date(metrics.database.serverStartedAt).toLocaleString()}` : 'PostgreSQL service', icon: Clock3, tint: 'bg-amber-50 text-amber-600' },
          ].map(({ label, value, helper, icon: Icon, tint }) => <article key={label} className="rounded-2xl border border-[#dfe8e2] bg-white p-5 shadow-sm"><div className="flex items-start justify-between"><div><p className="text-sm font-medium text-slate-500">{label}</p><p className="mt-2 text-2xl font-bold tracking-tight text-slate-900">{loading ? '…' : value}</p><p className="mt-1 truncate text-xs text-slate-400">{helper}</p></div><span className={`flex h-10 w-10 items-center justify-center rounded-xl ${tint}`}><Icon size={19} /></span></div></article>)}
        </div>
        <div className="mt-3 grid gap-3 lg:grid-cols-2">
          <article className="rounded-2xl border border-[#dfe8e2] bg-white p-5 shadow-sm"><div className="flex items-center justify-between"><div><p className="text-sm font-semibold text-slate-800">Database connections</p><p className="mt-1 text-xs text-slate-500">Current sessions compared with PostgreSQL limit</p></div><span className="text-sm font-bold text-slate-700">{metrics ? `${metrics.database.activeConnections} / ${metrics.database.maxConnections}` : '—'}</span></div><div className="mt-4 h-2 overflow-hidden rounded-full bg-slate-100"><div className="h-full rounded-full bg-[#1b5e3a] transition-all" style={{ width: `${connectionUse}%` }} /></div><p className="mt-2 text-right text-xs text-slate-400">{connectionUse}% in use</p></article>
          <article className="rounded-2xl border border-[#dfe8e2] bg-white p-5 shadow-sm"><div className="flex items-center justify-between"><div><p className="text-sm font-semibold text-slate-800">Service runtime</p><p className="mt-1 text-xs text-slate-500">Application process and deployed environment</p></div><span className="rounded-full bg-[#e6efeb] px-2.5 py-1 text-xs font-semibold text-[#1b5e3a]">{metrics?.application.environment ?? '—'}</span></div><div className="mt-4 flex flex-wrap gap-x-6 gap-y-2 text-sm"><span className="text-slate-600"><strong className="text-slate-800">Uptime:</strong> {metrics ? formatDuration(metrics.application.uptimeSeconds) : '—'}</span><span className="text-slate-600"><strong className="text-slate-800">Runtime:</strong> {metrics?.application.runtime ?? '—'}</span></div></article>
        </div>
      </section>

      <section>
        <div className="mb-3"><h2 className="text-lg font-bold text-slate-900">UploadThing storage</h2><p className="mt-1 text-sm text-slate-500">Provider usage is fetched with a server-side API key. The key is never sent to the browser.</p></div>
        <article className="rounded-2xl border border-[#dfe8e2] bg-white p-5 shadow-sm md:p-6">
          {loading && !metrics ? <div className="py-4 text-sm text-slate-500"><LoaderCircle size={17} className="mr-2 inline animate-spin" />Loading provider usage…</div> : metrics?.uploadThing.connected ? <>
            <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-start"><div><div className="inline-flex items-center gap-2 text-sm font-bold text-slate-800"><span className="h-2 w-2 rounded-full bg-emerald-500" />Connected to UploadThing</div><p className="mt-1 text-xs text-slate-500">Usage includes this app’s uploaded files.</p></div><div className="sm:text-right"><p className="text-2xl font-bold text-slate-900">{formatBytes(metrics.uploadThing.appTotalBytes)}</p><p className="mt-1 text-xs text-slate-500">of {formatBytes(metrics.uploadThing.limitBytes)} app storage</p></div></div>
            <div className="mt-5 h-2.5 overflow-hidden rounded-full bg-slate-100"><div className="h-full rounded-full bg-[#f5a623] transition-all" style={{ width: `${uploadUse}%` }} /></div>
            <div className="mt-4 grid gap-4 sm:grid-cols-3"><div><p className="text-xs text-slate-500">Files uploaded</p><p className="mt-1 font-bold text-slate-800">{metrics.uploadThing.filesUploaded?.toLocaleString() ?? '—'}</p></div><div><p className="text-xs text-slate-500">Workspace usage</p><p className="mt-1 font-bold text-slate-800">{formatBytes(metrics.uploadThing.totalBytes)}</p></div><div><p className="text-xs text-slate-500">Per-file upload cap</p><p className="mt-1 font-bold text-slate-800">{formatBytes(metrics.uploadThing.localUploadLimitBytes)}</p></div></div>
          </> : <div className="flex flex-col gap-4 sm:flex-row sm:items-center"><span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-amber-50 text-amber-600"><AlertCircle size={20} /></span><div className="flex-1"><p className="text-sm font-bold text-slate-800">UploadThing usage is unavailable</p><p className="mt-1 text-sm text-slate-500">{metrics?.uploadThing.message ?? 'The provider usage endpoint could not be reached.'}</p></div><div className="rounded-xl bg-slate-50 px-4 py-3"><p className="text-xs text-slate-500">Local per-file cap</p><p className="mt-1 text-sm font-bold text-slate-800">{formatBytes(metrics?.uploadThing.localUploadLimitBytes)}</p></div></div>}
        </article>
      </section>

      <section>
        <div className="mb-3"><h2 className="text-lg font-bold text-slate-900">System records</h2><p className="mt-1 text-sm text-slate-500">Live totals from the connected application database.</p></div>
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {[
            { label: 'Children', value: metrics?.records.children, icon: Baby },
            { label: 'Staff profiles', value: metrics?.records.staff, icon: School },
            { label: 'Parent profiles', value: metrics?.records.parents, icon: UsersRound },
            { label: 'Registered accounts', value: metrics?.records.accounts, icon: UserRound },
            { label: 'Active accounts', value: metrics?.records.activeAccounts, icon: ShieldCheck },
            { label: 'Attendance records', value: metrics?.records.attendanceRecords, icon: Activity },
          ].map(({ label, value, icon: Icon }) => <article key={label} className="flex items-center gap-3 rounded-2xl border border-[#dfe8e2] bg-white p-4 shadow-sm"><span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#e6efeb] text-[#1b5e3a]"><Icon size={18} /></span><div><p className="text-xs font-medium text-slate-500">{label}</p><p className="mt-0.5 text-lg font-bold text-slate-900">{loading ? '…' : value?.toLocaleString() ?? '—'}</p></div></article>)}
        </div>
      </section>

      <section className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_minmax(300px,0.8fr)]">
        <div className="rounded-2xl border border-[#dfe8e2] bg-white p-5 shadow-sm md:p-6">
          <div className="mb-5 flex items-start gap-3"><span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#e6efeb] text-[#1b5e3a]"><LockKeyhole size={19} /></span><div><h2 className="text-lg font-bold text-slate-900">Change administrator password</h2><p className="mt-1 text-sm text-slate-500">Confirm your current password before setting a new one.</p></div></div>
          <form onSubmit={(event) => void changePassword(event)} className="space-y-4">
            {[
              { key: 'current' as const, label: 'Current password', autoComplete: 'current-password' },
              { key: 'next' as const, label: 'New password', autoComplete: 'new-password' },
              { key: 'confirm' as const, label: 'Confirm new password', autoComplete: 'new-password' },
            ].map(({ key, label, autoComplete }) => <label key={key} className="block"><span className="mb-1.5 block text-sm font-semibold text-slate-700">{label}</span><span className="flex rounded-xl border border-slate-200 focus-within:border-emerald-600 focus-within:ring-4 focus-within:ring-emerald-600/10"><input required minLength={key === 'current' ? undefined : 8} type={showPasswords ? 'text' : 'password'} autoComplete={autoComplete} value={passwords[key]} onChange={(event) => setPasswords((current) => ({ ...current, [key]: event.target.value }))} className="min-w-0 flex-1 rounded-xl px-3.5 py-3 text-sm outline-none" />{key === 'current' && <button type="button" onClick={() => setShowPasswords((visible) => !visible)} aria-label={showPasswords ? 'Hide password' : 'Show password'} className="px-3 text-slate-400 hover:text-slate-700">{showPasswords ? <EyeOff size={17} /> : <Eye size={17} />}</button>}</span></label>)}
            {passwordError && <div role="alert" className="flex items-start gap-2 rounded-xl border border-rose-200 bg-rose-50 px-3.5 py-3 text-sm text-rose-700"><AlertCircle size={16} className="mt-0.5 shrink-0" />{passwordError}</div>}
            {passwordSuccess && <div role="status" className="flex items-start gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-3.5 py-3 text-sm text-emerald-800"><Check size={16} className="mt-0.5 shrink-0" />{passwordSuccess}</div>}
            <button type="submit" disabled={savingPassword} className="inline-flex items-center gap-2 rounded-xl bg-[#1b5e3a] px-4 py-2.5 text-sm font-bold text-white transition hover:bg-[#153e2a] disabled:opacity-50">{savingPassword ? <LoaderCircle size={16} className="animate-spin" /> : <LockKeyhole size={16} />}{savingPassword ? 'Updating…' : 'Update password'}</button>
          </form>
        </div>

        <aside className="rounded-2xl border border-[#dfe8e2] bg-gradient-to-br from-white to-[#f3f8f5] p-5 shadow-sm md:p-6"><span className="flex h-10 w-10 items-center justify-center rounded-xl bg-white text-[#1b5e3a] shadow-sm"><Server size={19} /></span><h2 className="mt-4 text-lg font-bold text-slate-900">Security & service status</h2><p className="mt-1 text-sm leading-6 text-slate-500">Password changes require the current password. Active refresh tokens are revoked after a successful change.</p><div className="mt-5 space-y-3 border-t border-[#dfe8e2] pt-4 text-sm"><div className="flex items-center justify-between gap-4"><span className="text-slate-500">API environment</span><strong className="text-slate-800">{metrics?.application.environment ?? '—'}</strong></div><div className="flex items-center justify-between gap-4"><span className="text-slate-500">Database</span><strong className="text-slate-800">{metrics?.database.name ?? '—'}</strong></div><div className="flex items-center justify-between gap-4"><span className="text-slate-500">Upload provider</span><strong className={metrics?.uploadThing.connected ? 'text-emerald-700' : 'text-amber-700'}>{metrics?.uploadThing.connected ? 'Connected' : 'Needs attention'}</strong></div></div></aside>
      </section>
    </main>
  )
}

export default AdminPreferencesPage
