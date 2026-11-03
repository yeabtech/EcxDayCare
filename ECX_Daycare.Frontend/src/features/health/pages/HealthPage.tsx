import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import {
  Activity,
  AlertTriangle,
  Baby,
  CalendarClock,
  CheckCircle2,
  Droplets,
  HeartPulse,
  Loader2,
  Pill,
  Plus,
  Search,
  Thermometer,
  X,
  type LucideIcon,
} from 'lucide-react'
import type { FormEvent, ReactNode } from 'react'
import { useLocation } from 'react-router-dom'
import { useAuth } from '@features/auth/hooks/useAuth'
import { ChildSearchSelect } from '../components/ChildSearchSelect'
import { healthRecordsApi } from '../services/healthRecordsApi'
import type { ChildOption, HealthLog } from '../types'

const HEALTH_EVENT_TYPES = [
  { value: 1, label: 'Diaper', icon: Baby },
  { value: 2, label: 'Toilet', icon: Droplets },
  { value: 3, label: 'Medication', icon: Pill },
  { value: 4, label: 'Temperature', icon: Thermometer },
  { value: 5, label: 'Symptoms', icon: HeartPulse },
]

interface HealthForm {
  time: string
  eventType: number
  description: string
  notes: string
}

const localDateTimeValue = (date: Date) => {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
  return local.toISOString().slice(0, 16)
}

const blankForm = (): HealthForm => ({
  time: localDateTimeValue(new Date()),
  eventType: 1,
  description: '',
  notes: '',
})

const errorMessage = (error: any) =>
  error?.response?.data?.error ?? error?.response?.data?.message ?? error?.message ?? 'Something went wrong. Please try again.'

const formatDateTime = (value: string) =>
  new Date(value).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

export const HealthPage = () => {
  const location = useLocation()
  const parentView = location.pathname.startsWith('/parent/')
  const { user } = useAuth()
  const [children, setChildren] = useState<ChildOption[]>([])
  const [childId, setChildId] = useState('')
  const [historyChildId, setHistoryChildId] = useState('all')
  const [events, setEvents] = useState<HealthLog[]>([])
  const [childrenLoading, setChildrenLoading] = useState(true)
  const [eventsLoading, setEventsLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [formOpen, setFormOpen] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<HealthForm>(blankForm)
  const [search, setSearch] = useState('')
  const [typeFilter, setTypeFilter] = useState('all')
  const [error, setError] = useState<string | null>(null)
  const [childrenError, setChildrenError] = useState<string | null>(null)
  const [saved, setSaved] = useState(false)
  const eventRequest = useRef(0)

  useEffect(() => {
    let alive = true
    setChildrenLoading(true)
    healthRecordsApi.getChildren(parentView, user?.ParentId).then((loaded) => {
      if (!alive) return
      setChildren(loaded)
      setChildId((current) => current || loaded[0]?.id || '')
    }).catch((loadError) => {
      if (alive) setChildrenError(errorMessage(loadError))
    }).finally(() => {
      if (alive) setChildrenLoading(false)
    })
    return () => { alive = false }
  }, [parentView, user?.ParentId])

  const loadEvents = useCallback(async () => {
    if (childrenLoading) return

    const selectedChildren = historyChildId === 'all'
      ? children
      : children.filter((child) => child.id === historyChildId)

    if (!selectedChildren.length) {
      eventRequest.current += 1
      setEvents([])
      setEventsLoading(false)
      return
    }
    const requestId = ++eventRequest.current
    setEventsLoading(true)
    setError(null)
    try {
      const eventGroups = await Promise.all(
        selectedChildren.map((child) => healthRecordsApi.getByChild(child.id)),
      )
      const loaded = eventGroups
        .flat()
        .sort((first, second) => new Date(second.time).getTime() - new Date(first.time).getTime())
      if (requestId === eventRequest.current) setEvents(loaded)
    } catch (loadError) {
      if (requestId === eventRequest.current) {
        setError(errorMessage(loadError))
        setEvents([])
      }
    } finally {
      if (requestId === eventRequest.current) setEventsLoading(false)
    }
  }, [children, childrenLoading, historyChildId])

  useEffect(() => { void loadEvents() }, [loadEvents])

  const filteredEvents = useMemo(() => events.filter((event) => {
    const matchesType = typeFilter === 'all' || event.eventType === Number(typeFilter)
    const text = `${event.eventTypeName} ${event.description ?? ''} ${event.notes ?? ''}`.toLocaleLowerCase()
    return matchesType && (!search.trim() || text.includes(search.trim().toLocaleLowerCase()))
  }), [events, search, typeFilter])

  const todaysEvents = events.filter((event) => new Date(event.time).toDateString() === new Date().toDateString()).length
  const medicationCount = events.filter((event) => event.eventType === 3).length

  const openCreate = () => {
    setEditingId(null)
    setForm(blankForm())
    setError(null)
    setSaved(false)
    setFormOpen(true)
  }

  const openEdit = (event: HealthLog) => {
    setEditingId(event.id)
    setForm({
      time: localDateTimeValue(new Date(event.time)),
      eventType: event.eventType,
      description: event.description ?? '',
      notes: event.notes ?? '',
    })
    setError(null)
    setSaved(false)
    setFormOpen(true)
  }

  const saveEvent = async (submitEvent: FormEvent) => {
    submitEvent.preventDefault()
    if (!childId) {
      setError('Choose a child before recording a health event.')
      return
    }
    setSaving(true)
    setError(null)
    setSaved(false)
    const payload = {
      ...(!editingId ? { childId } : {}),
      time: new Date(form.time).toISOString(),
      eventType: form.eventType,
      description: form.description.trim() || null,
      notes: form.notes.trim() || null,
    }
    try {
      if (editingId) await healthRecordsApi.update(editingId, payload)
      else await healthRecordsApi.create(payload)
      setSaved(true)
      setFormOpen(false)
      await loadEvents()
    } catch (saveError) {
      setError(errorMessage(saveError))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="space-y-6 pb-12">
      <section className="overflow-hidden rounded-3xl bg-gradient-to-br from-teal-800 via-teal-700 to-cyan-700 p-6 text-white shadow-lg sm:p-8">
        <div className="flex flex-col gap-6 md:flex-row md:items-end md:justify-between">
          <div className="max-w-2xl">
            <span className="inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-teal-50">
              <HeartPulse size={14} /> Child wellness
            </span>
            <h1 className="mt-4 text-3xl font-bold tracking-tight sm:text-4xl">Health records</h1>
            <p className="mt-2 max-w-xl text-sm leading-6 text-teal-50/90 sm:text-base">
              Keep care notes, medication events, and daily health observations together for each child.
            </p>
          </div>
          {!parentView && (
            <button onClick={openCreate} disabled={!childId} className="inline-flex shrink-0 items-center justify-center gap-2 rounded-xl bg-white px-5 py-3 text-sm font-bold text-teal-800 shadow-sm transition hover:bg-teal-50 disabled:cursor-not-allowed disabled:opacity-50">
              <Plus size={18} /> Record health event
            </button>
          )}
        </div>
      </section>

      {(error || childrenError) && (
        <div role="alert" className="flex items-start gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800">
          <AlertTriangle size={18} className="mt-0.5 shrink-0" />
          <span>{error ?? childrenError}</span>
          {error && <button type="button" onClick={() => setError(null)} className="ml-auto"><X size={16} /></button>}
        </div>
      )}
      {saved && <div className="flex items-center gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-800"><CheckCircle2 size={17} /> Health record saved.</div>}

      <section className="grid gap-4 sm:grid-cols-3">
        <Metric icon={Activity} label="Health events" value={events.length} tone="teal" />
        <Metric icon={CalendarClock} label="Recorded today" value={todaysEvents} tone="sky" />
        <Metric icon={Pill} label="Medication records" value={medicationCount} tone="amber" />
      </section>

      <section className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
        <label className="mb-2 block text-sm font-semibold text-slate-700">Child for new records</label>
        <ChildSearchSelect
          children={children}
          value={childId}
          onChange={setChildId}
          disabled={childrenLoading || children.length === 0}
          placeholder={childrenLoading ? 'Loading children…' : 'Search children by name'}
        />
        {!childrenLoading && !children.length && <p className="mt-3 text-sm text-slate-500">No linked children are available.</p>}
      </section>

      {formOpen && !parentView && (
        <section className="rounded-2xl border border-teal-200 bg-white p-5 shadow-sm sm:p-6">
          <div className="mb-5 flex items-start justify-between gap-4">
            <div>
              <p className="text-xs font-bold uppercase tracking-wider text-teal-700">Health log</p>
              <h2 className="mt-1 text-xl font-bold text-slate-900">{editingId ? 'Update health event' : 'Record a health event'}</h2>
            </div>
            <button type="button" onClick={() => setFormOpen(false)} className="rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700" aria-label="Close form"><X size={19} /></button>
          </div>
          <form onSubmit={saveEvent} className="grid gap-4 md:grid-cols-2">
            <Field label="Event type" required>
              <select required value={form.eventType} onChange={(event) => setForm((previous) => ({ ...previous, eventType: Number(event.target.value) }))} className={inputClass}>
                {HEALTH_EVENT_TYPES.map((type) => <option key={type.value} value={type.value}>{type.label}</option>)}
              </select>
            </Field>
            <Field label="Date and time" required>
              <input type="datetime-local" required min={localDateTimeValue(new Date(Date.now() - 60 * 24 * 60 * 60 * 1000))} max={localDateTimeValue(new Date())} value={form.time} onChange={(event) => setForm((previous) => ({ ...previous, time: event.target.value }))} className={inputClass} />
            </Field>
            <Field label="Description" hint="Saved as the health event description.">
              <textarea maxLength={500} rows={3} value={form.description} onChange={(event) => setForm((previous) => ({ ...previous, description: event.target.value }))} placeholder="Record what happened or what care was provided." className={`${inputClass} resize-y`} />
            </Field>
            <Field label="Care notes" hint="Visible to authorized staff and this child’s parent.">
              <textarea maxLength={1000} rows={3} value={form.notes} onChange={(event) => setForm((previous) => ({ ...previous, notes: event.target.value }))} placeholder="Add any follow-up or care instructions." className={`${inputClass} resize-y`} />
            </Field>
            <div className="flex flex-wrap items-center justify-end gap-3 md:col-span-2">
              <button type="button" onClick={() => setFormOpen(false)} className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50">Cancel</button>
              <button type="submit" disabled={saving} className="inline-flex items-center gap-2 rounded-xl bg-teal-700 px-5 py-2.5 text-sm font-bold text-white hover:bg-teal-800 disabled:opacity-60">
                {saving && <Loader2 size={16} className="animate-spin" />}{saving ? 'Saving…' : editingId ? 'Save changes' : 'Save health event'}
              </button>
            </div>
          </form>
        </section>
      )}

      <section className="space-y-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div><h2 className="text-xl font-bold text-slate-900">Health timeline</h2><p className="mt-1 text-sm text-slate-500">Events recorded across all children, newest first.</p></div>
          <div className="flex flex-col gap-2 sm:flex-row">
            <select aria-label="Filter health records by child" value={historyChildId} onChange={(event) => setHistoryChildId(event.target.value)} className="rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm outline-none focus:border-teal-500"><option value="all">All children</option>{children.map((child) => <option key={child.id} value={child.id}>{child.name}</option>)}</select>
            <div className="relative">
              <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
              <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search health notes" className="w-full rounded-xl border border-slate-200 bg-white py-2.5 pl-9 pr-3 text-sm outline-none focus:border-teal-500 sm:w-56" />
            </div>
            <select value={typeFilter} onChange={(event) => setTypeFilter(event.target.value)} className="rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm outline-none focus:border-teal-500">
              <option value="all">All event types</option>
              {HEALTH_EVENT_TYPES.map((type) => <option key={type.value} value={type.value}>{type.label}</option>)}
            </select>
          </div>
        </div>

        {eventsLoading ? <LoadingState label="Loading health records…" /> : filteredEvents.length ? (
          <div className="space-y-3">
            {filteredEvents.map((event) => {
              const type = HEALTH_EVENT_TYPES.find((item) => item.value === event.eventType) ?? HEALTH_EVENT_TYPES[0]
              const EventIcon = type.icon
              return (
                <article key={event.id} className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm transition hover:border-teal-200 sm:p-5">
                  <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
                    <div className="flex min-w-0 flex-1 gap-3">
                      <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-teal-50 text-teal-700"><EventIcon size={20} /></span>
                      <div className="min-w-0 flex-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <h3 className="font-bold text-slate-900">{event.eventTypeName || type.label}</h3>
                          <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">{formatDateTime(event.time)}</span>
                          {historyChildId === 'all' && event.childName && <span className="rounded-full bg-teal-50 px-2.5 py-1 text-xs font-semibold text-teal-800">{event.childName}</span>}
                        </div>
                        {event.description && <p className="mt-2 whitespace-pre-line text-sm leading-6 text-slate-700">{event.description}</p>}
                        {event.notes && <p className="mt-2 rounded-xl bg-slate-50 px-3 py-2.5 text-sm leading-5 text-slate-600"><strong className="text-slate-700">Care notes: </strong>{event.notes}</p>}
                        <p className="mt-3 text-xs text-slate-400">Recorded by daycare staff</p>
                      </div>
                    </div>
                    {!parentView && <button onClick={() => openEdit(event)} className="shrink-0 rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50">Edit record</button>}
                  </div>
                </article>
              )
            })}
          </div>
        ) : (
          <div className="rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-14 text-center">
            <span className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-teal-50 text-teal-700"><HeartPulse size={26} /></span>
            <h3 className="mt-4 font-bold text-slate-900">No health events to show</h3>
            <p className="mx-auto mt-1 max-w-md text-sm text-slate-500">{search || typeFilter !== 'all' ? 'Try changing your search or event type filter.' : `Health events for ${historyChildId === 'all' ? 'these children' : 'this child'} will appear here when staff record them.`}</p>
            {!parentView && childId && !search && typeFilter === 'all' && <button onClick={openCreate} className="mt-5 inline-flex items-center gap-2 rounded-xl bg-teal-700 px-4 py-2.5 text-sm font-bold text-white hover:bg-teal-800"><Plus size={16} />Record first event</button>}
          </div>
        )}
      </section>
    </div>
  )
}

const inputClass = 'w-full rounded-xl border border-slate-200 bg-white px-3.5 py-3 text-sm text-slate-800 outline-none transition placeholder:text-slate-400 focus:border-teal-500 focus:ring-4 focus:ring-teal-500/10'

const Field = ({ label, required, hint, children }: { label: string; required?: boolean; hint?: string; children: ReactNode }) => (
  <label className="block text-sm font-semibold text-slate-700">
    <span>{label}{required && <span className="ml-1 text-rose-500">*</span>}</span>
    <span className="mt-1.5 block">{children}</span>
    {hint && <span className="mt-1 block text-xs font-normal text-slate-500">{hint}</span>}
  </label>
)

const Metric = ({ icon: Icon, label, value, tone }: { icon: LucideIcon; label: string; value: number; tone: 'teal' | 'sky' | 'amber' }) => {
  const styles = {
    teal: 'bg-teal-50 text-teal-700 ring-teal-100',
    sky: 'bg-sky-50 text-sky-700 ring-sky-100',
    amber: 'bg-amber-50 text-amber-700 ring-amber-100',
  }
  return <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"><div className="flex items-center justify-between"><span className="text-sm font-medium text-slate-500">{label}</span><span className={`flex h-9 w-9 items-center justify-center rounded-xl ring-1 ${styles[tone]}`}><Icon size={18} /></span></div><div className="mt-3 text-3xl font-bold tracking-tight text-slate-900">{value}</div></div>
}

const LoadingState = ({ label }: { label: string }) => <div className="flex items-center justify-center gap-3 rounded-2xl border border-slate-200 bg-white py-14 text-sm text-slate-500"><Loader2 size={20} className="animate-spin text-teal-700" />{label}</div>
