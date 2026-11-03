import { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Activity, Clock3, Loader2, Moon, Plus, Trash2, Utensils, RefreshCw } from 'lucide-react'
import { apiClient } from '@services/api'
import { useAuth } from '@features/auth'
import { staffApi } from '@features/staff/services/staffApi'
import { classroomsApi } from '@features/classrooms/services/classroomsApi'
import { dailyReportsApi } from '../services/dailyReportsApi'
import type { DailyLogsSummary } from '../types'

type LogKind = 'activities' | 'feeding' | 'nap'

const ACTIVITY_PRESETS = [
  { title: 'Outdoor play', type: 'OutdoorPlay' },
  { title: 'Sensory play', type: 'Sensory' },
  { title: 'Arts and crafts', type: 'ArtsAndCrafts' },
  { title: 'Story time', type: 'StoryTime' },
  { title: 'Music and movement', type: 'MusicAndMovement' },
  { title: 'Circle time', type: 'CircleTime' },
  { title: 'Tummy time', type: 'TummyTime' },
  { title: 'Motor skills practice', type: 'MotorSkills' },
  { title: 'Free play', type: 'FreePlay' },
  { title: 'Other classroom activity', type: 'Other' },
]
const FOOD_PRESETS = [
  'Injera and shiro', 'Genfo porridge', 'Oatmeal porridge', 'Egg and bread',
  'Rice and vegetables', 'Mixed vegetables', 'Banana', 'Avocado', 'Fruit pieces',
  'Yogurt', 'Milk', 'Formula', 'Breast milk', 'Water', 'Other',
]
const AMOUNT_PRESETS = ['A few bites', 'Small serving', 'Half serving', 'Full serving', '120 ml', '180 ml', '240 ml']
const NOTE_PRESETS = ['No additional note', 'Needed help', 'Tried something new', 'Refused at first', 'Settled with support']
const ACTIVITY_DURATIONS = ['10', '15', '20', '30', '45', '60']
const NAP_DURATIONS = [30, 45, 60, 75, 90, 120]

const PAGE: Record<LogKind, { title: string; singular: string; icon: typeof Activity; description: string }> = {
  activities: { title: 'Activity log', singular: 'activity', icon: Activity, description: 'Record the learning and play moments from the classroom.' },
  feeding: { title: 'Feeding log', singular: 'feeding', icon: Utensils, description: 'Track meals, snacks, portions, and feeding notes.' },
  nap: { title: 'Nap log', singular: 'nap', icon: Moon, description: 'Track rest sessions and how each child settled.' },
}

const localDate = () => {
  const now = new Date()
  return new Date(now.getTime() - now.getTimezoneOffset() * 60_000).toISOString().slice(0, 10)
}
const localDateTime = () => {
  const now = new Date()
  return new Date(now.getTime() - now.getTimezoneOffset() * 60_000).toISOString().slice(0, 16)
}
const toLocalInput = (value: string) => value ? new Date(value).toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' }) : 'In progress'

export const DailyLogsPage = ({ kind }: { kind: LogKind }) => {
  const { user, isStaff } = useAuth()
  const [searchParams] = useSearchParams()
  const requestedChildId = searchParams.get('childId') ?? ''
  const page = PAGE[kind]
  const Icon = page.icon
  const [children, setChildren] = useState<Array<{ id: string; name: string }>>([])
  const [childrenLoading, setChildrenLoading] = useState(true)
  const [childrenError, setChildrenError] = useState('')
  const [childId, setChildId] = useState('')
  const [date, setDate] = useState(searchParams.get('date') || localDate())
  const [logs, setLogs] = useState<DailyLogsSummary | null>(null)
  const [logsLoading, setLogsLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [form, setForm] = useState({
    time: localDateTime(), type: '', title: '', duration: '30', napDuration: '',
    participation: 'Engaged', foodItems: '', amount: 'Small serving', portion: 'Most', quality: 'Peaceful', notes: '',
  })

  useEffect(() => {
    let active = true
    const loadChildren = async () => {
      setChildrenLoading(true)
      setChildrenError('')
      try {
        let options: Array<{ id: string; name: string }> = []
        if (isStaff) {
          if (!user?.StaffId) throw new Error('This login is not linked to a staff record.')
          const staff = await staffApi.getStaffMember(user.StaffId)
          if (!staff.classroomId) throw new Error('Your staff record is not assigned to a classroom.')
          const roster = await classroomsApi.getClassroomChildren(staff.classroomId)
          options = roster
            .filter((child) => ['enrolled', 'active'].includes(child.Status.toLowerCase()))
            .map((child) => ({ id: child.Id, name: `${child.FirstName} ${child.LastName}`.trim() }))
        } else {
          const response = await apiClient.get<any>('/children', { params: { skip: 0, take: 500 } })
          const payload = response.data?.data ?? response.data
          const rows = Array.isArray(payload) ? payload : payload?.children ?? payload?.Children ?? payload?.items ?? payload?.Items ?? []
          options = rows.map((child: any) => ({
            id: child.Id ?? child.id ?? '',
            name: `${child.FirstName ?? child.firstName ?? ''} ${child.LastName ?? child.lastName ?? ''}`.trim(),
          })).filter((child: { id: string; name: string }) => child.id && child.name)
        }
        if (active) {
          setChildren(options)
          setChildId((current) => current || (options.some((child) => child.id === requestedChildId) ? requestedChildId : '') || options[0]?.id || '')
        }
      } catch (loadError) {
        if (active) setChildrenError(loadError instanceof Error ? loadError.message : 'The child list could not be loaded.')
      } finally {
        if (active) setChildrenLoading(false)
      }
    }
    void loadChildren()
    return () => { active = false }
  }, [isStaff, user?.StaffId, requestedChildId])

  const refreshLogs = useCallback(async () => {
    if (!childId || !date) { setLogs(null); return }
    setLogsLoading(true)
    setError('')
    try {
      setLogs(await dailyReportsApi.getDailyLogs(childId, date))
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Daily logs could not be loaded.')
    } finally {
      setLogsLoading(false)
    }
  }, [childId, date])

  useEffect(() => { void refreshLogs() }, [refreshLogs])

  const entries = useMemo(() => {
    if (!logs) return []
    return kind === 'feeding' ? logs.feedings : kind === 'nap' ? logs.naps : logs.activities
  }, [logs, kind])
  const update = (key: keyof typeof form, value: string) => setForm((current) => ({ ...current, [key]: value }))

  const submit = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!childId) { setError('Choose a child before saving a log.'); return }
    setSaving(true)
    setError('')
    setSuccess('')
    try {
      const time = new Date(form.time).toISOString()
      const route = kind === 'activities' ? '/activitylogs' : kind === 'feeding' ? '/feedinglogs' : '/naplogs'
      const body = kind === 'activities'
        ? { childId, time, durationMinutes: Number(form.duration), activityType: form.type || 'FreePlay', title: form.title, description: null, participation: form.participation, notes: form.notes && form.notes !== 'No additional note' ? form.notes : null }
        : kind === 'feeding'
          ? { childId, time, feedingType: form.type || 'Snack', foodItems: form.foodItems || null, amount: form.amount || null, portionConsumed: form.portion, notes: form.notes && form.notes !== 'No additional note' ? form.notes : null }
          : {
              childId,
              startTime: time,
              endTime: form.quality === 'Refused'
                ? time
                : form.napDuration
                  ? new Date(new Date(time).getTime() + Number(form.napDuration) * 60_000).toISOString()
                  : null,
              quality: form.quality,
              notes: form.notes && form.notes !== 'No additional note' ? form.notes : null,
            }
      await apiClient.post(route, body)
      setSuccess(`${page.singular[0].toUpperCase()}${page.singular.slice(1)} saved.`)
      setForm((current) => ({ ...current, time: localDateTime(), title: '', foodItems: '', napDuration: '', notes: '' }))
      await refreshLogs()
    } catch (saveError: any) {
      setError(saveError?.response?.data?.error ?? saveError?.response?.data?.message ?? saveError?.response?.data?.Message ?? 'The log could not be saved. Check the fields and try again.')
    } finally {
      setSaving(false)
    }
  }

  const remove = async (id: string) => {
    if (!window.confirm(`Delete this ${page.singular} record?`)) return
    setError('')
    try {
      const route = kind === 'activities' ? '/activitylogs' : kind === 'feeding' ? '/feedinglogs' : '/naplogs'
      await apiClient.delete(`${route}/${id}`)
      setSuccess('Log removed.')
      await refreshLogs()
    } catch (deleteError: any) {
      setError(deleteError?.response?.data?.error ?? deleteError?.response?.data?.message ?? deleteError?.response?.data?.Message ?? 'The log could not be deleted.')
    }
  }

  const inputClass = 'w-full rounded-xl border border-gray-200 bg-white px-3.5 py-2.5 text-sm text-gray-800 outline-none transition focus:border-[color:var(--app-primary)] focus:ring-2 focus:ring-[color:var(--app-primary-soft)]'

  return (
    <div className="brand-gradient-canvas -m-3 min-h-[calc(100vh-4rem)] p-4 sm:-m-4 sm:p-6 lg:-m-6 lg:p-8">
      <div className="mx-auto max-w-6xl space-y-6">
        <header className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <div className="mb-3 inline-flex items-center gap-2 rounded-full bg-white/75 px-3 py-1.5 text-xs font-semibold text-[color:var(--app-primary-deep)] shadow-sm">
              <Icon size={15} /> Child daily care
            </div>
            <h1 className="text-3xl font-bold tracking-tight text-gray-900">{page.title}</h1>
            <p className="mt-1 text-sm text-gray-600">{page.description}</p>
          </div>
          <button type="button" onClick={() => void refreshLogs()} disabled={logsLoading} className="inline-flex items-center justify-center gap-2 rounded-xl border border-white/80 bg-white/80 px-4 py-2.5 text-sm font-semibold text-gray-700 shadow-sm hover:bg-white disabled:opacity-60">
            <RefreshCw size={16} className={logsLoading ? 'animate-spin' : ''} /> Refresh logs
          </button>
        </header>

        <section className="grid gap-4 rounded-2xl border border-white/80 bg-white/85 p-4 shadow-sm sm:grid-cols-[1fr_200px] sm:p-5">
          <label className="space-y-1.5 text-sm font-medium text-gray-700">Child
            <select className={inputClass} value={childId} onChange={(event) => setChildId(event.target.value)} disabled={childrenLoading || !!childrenError}>
              <option value="">{childrenLoading ? 'Loading children…' : 'Select a child'}</option>
              {children.map((child) => <option key={child.id} value={child.id}>{child.name}</option>)}
            </select>
          </label>
          <label className="space-y-1.5 text-sm font-medium text-gray-700">Date
            <input type="date" className={inputClass} value={date} max={localDate()} onChange={(event) => setDate(event.target.value)} />
          </label>
        </section>

        {childrenError && <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{childrenError}</div>}
        {error && <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>}
        {success && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{success}</div>}

        <div className="grid items-start gap-5 lg:grid-cols-[minmax(0,1.25fr)_minmax(320px,.75fr)]">
          <section className="rounded-2xl border border-white/80 bg-white/90 p-5 shadow-sm sm:p-6">
            <div className="mb-5 flex items-center justify-between gap-3">
              <div><h2 className="font-semibold text-gray-900">{page.title} history</h2><p className="mt-1 text-xs text-gray-500">Records saved in the daycare database for this day.</p></div>
              <span className="rounded-full bg-[color:var(--app-primary-soft)] px-3 py-1 text-xs font-semibold text-[color:var(--app-primary-deep)]">{entries.length} entries</span>
            </div>
            {logsLoading ? <div className="flex min-h-36 items-center justify-center gap-2 text-sm text-gray-500"><Loader2 size={18} className="animate-spin" /> Loading records…</div> : entries.length ? (
              <div className="space-y-3">
                {kind === 'activities' && logs?.activities.map((entry) => <article key={entry.id} className="flex items-start justify-between gap-3 rounded-xl border border-green-100 bg-green-50/60 p-4"><div className="min-w-0"><div className="font-semibold text-gray-900">{entry.title}</div><div className="mt-1 text-xs text-gray-600">{toLocalInput(entry.time)} · {entry.activityTypeName ?? 'Activity'}{entry.durationMinutes != null ? ` · ${entry.durationMinutes} min` : ''} · {entry.participationName}</div>{entry.description && <p className="mt-2 text-sm text-gray-600">{entry.description}</p>}</div><button type="button" title="Delete entry" onClick={() => void remove(entry.id)} className="rounded-lg p-2 text-gray-400 hover:bg-white hover:text-rose-600"><Trash2 size={16} /></button></article>)}
                {kind === 'feeding' && logs?.feedings.map((entry) => <article key={entry.id} className="flex items-start justify-between gap-3 rounded-xl border border-orange-100 bg-orange-50/60 p-4"><div className="min-w-0"><div className="font-semibold text-gray-900">{entry.foodItems || entry.feedingTypeName || 'Feeding'}</div><div className="mt-1 text-xs text-gray-600">{toLocalInput(entry.time)} · {entry.feedingTypeName}{entry.amount ? ` · ${entry.amount}` : ''} · Ate {entry.portionConsumedName}</div>{entry.notes && <p className="mt-2 text-sm text-gray-600">{entry.notes}</p>}</div><button type="button" title="Delete entry" onClick={() => void remove(entry.id)} className="rounded-lg p-2 text-gray-400 hover:bg-white hover:text-rose-600"><Trash2 size={16} /></button></article>)}
                {kind === 'nap' && logs?.naps.map((entry) => <article key={entry.id} className="flex items-start justify-between gap-3 rounded-xl border border-blue-100 bg-blue-50/60 p-4"><div className="min-w-0"><div className="font-semibold text-gray-900">Rest session · {entry.qualityName}</div><div className="mt-1 text-xs text-gray-600">{toLocalInput(entry.startTime)} – {toLocalInput(entry.endTime ?? '')}{entry.durationMinutes != null ? ` · ${entry.durationMinutes} min` : ''}</div>{entry.notes && <p className="mt-2 text-sm text-gray-600">{entry.notes}</p>}</div><button type="button" title="Delete entry" onClick={() => void remove(entry.id)} className="rounded-lg p-2 text-gray-400 hover:bg-white hover:text-rose-600"><Trash2 size={16} /></button></article>)}
              </div>
            ) : <div className="flex min-h-36 flex-col items-center justify-center rounded-xl border border-dashed border-gray-200 bg-gray-50/60 p-6 text-center"><Clock3 className="mb-2 text-gray-400" size={24} /><p className="text-sm font-medium text-gray-700">No {page.singular} records yet</p><p className="mt-1 text-xs text-gray-500">Add the first entry with the form.</p></div>}
          </section>

          <form onSubmit={(event) => void submit(event)} className="space-y-4 rounded-2xl border border-white/80 bg-white/90 p-5 shadow-sm sm:p-6">
            <div className="mb-1 flex items-center gap-2"><span className="rounded-lg bg-[color:var(--app-primary-soft)] p-2 text-[color:var(--app-primary-deep)]"><Plus size={18} /></span><div><h2 className="font-semibold text-gray-900">Add {page.singular}</h2><p className="text-xs text-gray-500">Saved records appear in the daily report.</p></div></div>
            {kind === 'activities' && <>
              <label className="block space-y-1.5 text-sm font-medium text-gray-700">Activity<select className={inputClass} value={form.title} required onChange={(e) => {
                const selected = ACTIVITY_PRESETS.find((item) => item.title === e.target.value)
                update('title', selected?.title ?? '')
                update('type', selected?.type ?? '')
              }}><option value="">Choose an activity</option>{ACTIVITY_PRESETS.map((item) => <option key={item.title} value={item.title}>{item.title}</option>)}</select></label>
              <label className="block space-y-1.5 text-sm font-medium text-gray-700">Participation<select className={inputClass} value={form.participation} onChange={(e) => update('participation', e.target.value)}>{[['Engaged','Joined in'],['Excited','Very enthusiastic'],['Observing','Watched the group'],['Hesitant','Needed encouragement'],['SoloPlay','Played independently']].map(([value,label]) => <option key={value} value={value}>{label}</option>)}</select></label>
              <label className="block space-y-1.5 text-sm font-medium text-gray-700">Duration<select className={inputClass} value={form.duration} onChange={(e) => update('duration', e.target.value)}>{ACTIVITY_DURATIONS.map((minutes) => <option key={minutes} value={minutes}>{minutes} minutes</option>)}</select></label>
            </>}
            {kind === 'feeding' && <>
              <label className="block space-y-1.5 text-sm font-medium text-gray-700">Meal type<select className={inputClass} value={form.type || 'Snack'} onChange={(e) => update('type', e.target.value)}>{['Bottle','BreastMilk','Formula','Breakfast','Lunch','Snack','SolidFood','Water'].map((value) => <option key={value}>{value}</option>)}</select></label>
              <label className="block space-y-1.5 text-sm font-medium text-gray-700">Food / drink<select className={inputClass} value={form.foodItems} onChange={(e) => update('foodItems', e.target.value)}><option value="">Choose food or drink</option>{FOOD_PRESETS.map((value) => <option key={value}>{value}</option>)}</select></label>
              <label className="block space-y-1.5 text-sm font-medium text-gray-700">Serving amount<select className={inputClass} value={form.amount} onChange={(e) => update('amount', e.target.value)}>{AMOUNT_PRESETS.map((value) => <option key={value}>{value}</option>)}</select></label>
              <label className="block space-y-1.5 text-sm font-medium text-gray-700">Amount consumed<select className={inputClass} value={form.portion} onChange={(e) => update('portion', e.target.value)}>{[['All','All'],['Most','Most'],['Half','About half'],['Some','A little'],['None','None']].map(([value,label]) => <option key={value} value={value}>{label}</option>)}</select></label>
            </>}
            {kind === 'nap' && <>
              <label className="block space-y-1.5 text-sm font-medium text-gray-700">Rest quality<select className={inputClass} value={form.quality} onChange={(e) => update('quality', e.target.value)}>{[['Peaceful','Rested peacefully'],['Restless','Rested restlessly'],['Fussy','Needed comfort'],['Refused','Did not nap']].map(([value,label]) => <option key={value} value={value}>{label}</option>)}</select></label>
              <label className="block space-y-1.5 text-sm font-medium text-gray-700">Nap length<select className={inputClass} value={form.napDuration} onChange={(e) => update('napDuration', e.target.value)}><option value="">Still resting</option>{NAP_DURATIONS.map((minutes) => <option key={minutes} value={minutes}>{minutes < 60 ? `${minutes} minutes` : `${minutes / 60} hour${minutes > 60 ? 's' : ''}`}</option>)}</select></label>
            </>}
            <label className="block space-y-1.5 text-sm font-medium text-gray-700">{kind === 'nap' ? 'Start time' : 'Time'}<input type="datetime-local" className={inputClass} value={form.time} onChange={(e) => update('time', e.target.value)} required /></label>
            <label className="block space-y-1.5 text-sm font-medium text-gray-700">Observation <span className="font-normal text-gray-400">(optional)</span><select className={inputClass} value={form.notes} onChange={(e) => update('notes', e.target.value)}><option value="">No additional note</option>{NOTE_PRESETS.filter((note) => note !== 'No additional note').map((note) => <option key={note}>{note}</option>)}</select></label>
            <button type="submit" disabled={saving || !childId} className="flex w-full items-center justify-center gap-2 rounded-xl bg-[color:var(--app-primary)] px-4 py-3 text-sm font-semibold text-white shadow-sm transition hover:brightness-95 disabled:cursor-not-allowed disabled:opacity-50">
              {saving ? <Loader2 size={17} className="animate-spin" /> : <Plus size={17} />} Save {page.singular}
            </button>
          </form>
        </div>
      </div>
    </div>
  )
}
