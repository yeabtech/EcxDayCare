import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import {
  AlertTriangle,
  BellRing,
  CheckCircle2,
  ClipboardList,
  Clock3,
  Loader2,
  MapPin,
  Plus,
  Search,
  ShieldAlert,
  X,
  type LucideIcon,
} from 'lucide-react'
import type { FormEvent, ReactNode } from 'react'
import { useLocation } from 'react-router-dom'
import { useAuth } from '@features/auth/hooks/useAuth'
import { ChildSearchSelect } from '../components/ChildSearchSelect'
import { healthRecordsApi } from '../services/healthRecordsApi'
import { incidentRecordsApi } from '../services/incidentRecordsApi'
import type { ChildOption, IncidentRecord } from '../types'

const INJURY_TYPES = [
  { value: 1, label: 'Minor' },
  { value: 2, label: 'Moderate' },
  { value: 3, label: 'Severe' },
  { value: 4, label: 'Head injury' },
  { value: 5, label: 'Allergic reaction' },
  { value: 6, label: 'Other' },
]

const BODY_PARTS = [
  'Head', 'Neck', 'Shoulder', 'Arms', 'Elbows', 'Wrists', 'Hands', 'Fingers',
  'Chest', 'Abdomen', 'Back', 'Hips', 'Legs', 'Knees', 'Ankles', 'Feet', 'Toes', 'Other',
].map((label, index) => ({ value: index + 1, label }))

interface IncidentForm {
  incidentTime: string
  location: string
  description: string
  injuryType: number
  affectedBodyPart: number
  firstAidGiven: string
  followUp: string
  notes: string
}

const localDateTimeValue = (date: Date) => {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
  return local.toISOString().slice(0, 16)
}

const blankForm = (): IncidentForm => ({
  incidentTime: localDateTimeValue(new Date()),
  location: '',
  description: '',
  injuryType: 1,
  affectedBodyPart: 18,
  firstAidGiven: '',
  followUp: '',
  notes: '',
})

const errorMessage = (error: any) =>
  error?.response?.data?.error ?? error?.response?.data?.message ?? error?.message ?? 'Something went wrong. Please try again.'

const formatDateTime = (value: string) =>
  new Date(value).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

const severityTone = (type: number) => type === 3 || type === 4 || type === 5
  ? 'border-rose-200 bg-rose-50 text-rose-800'
  : type === 2
    ? 'border-amber-200 bg-amber-50 text-amber-800'
    : 'border-slate-200 bg-slate-100 text-slate-700'

export const IncidentsPage = () => {
  const location = useLocation()
  const parentView = location.pathname.startsWith('/parent/')
  const { user } = useAuth()
  const [children, setChildren] = useState<ChildOption[]>([])
  const [childId, setChildId] = useState('')
  const [historyChildId, setHistoryChildId] = useState('all')
  const [incidents, setIncidents] = useState<IncidentRecord[]>([])
  const [childrenLoading, setChildrenLoading] = useState(true)
  const [incidentsLoading, setIncidentsLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [notifyingId, setNotifyingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<IncidentForm>(blankForm)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState<'all' | 'pending' | 'notified'>('all')
  const [error, setError] = useState<string | null>(null)
  const [childrenError, setChildrenError] = useState<string | null>(null)
  const [saved, setSaved] = useState(false)
  const [savedMessage, setSavedMessage] = useState('Incident report saved.')
  const incidentRequest = useRef(0)

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

  const loadIncidents = useCallback(async () => {
    if (childrenLoading) return

    const selectedChildren = historyChildId === 'all'
      ? children
      : children.filter((child) => child.id === historyChildId)

    if (!selectedChildren.length) {
      incidentRequest.current += 1
      setIncidents([])
      setIncidentsLoading(false)
      return
    }
    const requestId = ++incidentRequest.current
    setIncidentsLoading(true)
    setError(null)
    try {
      const incidentGroups = await Promise.all(
        selectedChildren.map((child) => incidentRecordsApi.getByChild(child.id)),
      )
      const loaded = incidentGroups
        .flat()
        .sort((first, second) => new Date(second.incidentTime).getTime() - new Date(first.incidentTime).getTime())
      if (requestId === incidentRequest.current) setIncidents(loaded)
    } catch (loadError) {
      if (requestId === incidentRequest.current) {
        setError(errorMessage(loadError))
        setIncidents([])
      }
    } finally {
      if (requestId === incidentRequest.current) setIncidentsLoading(false)
    }
  }, [children, childrenLoading, historyChildId])

  useEffect(() => { void loadIncidents() }, [loadIncidents])

  const filteredIncidents = useMemo(() => incidents.filter((incident) => {
    const matchesStatus = statusFilter === 'all' || (statusFilter === 'notified' ? incident.parentNotified : !incident.parentNotified)
    const text = `${incident.childName ?? ''} ${incident.location} ${incident.description} ${incident.injuryTypeName} ${incident.affectedBodyPartName} ${incident.firstAidGiven ?? ''} ${incident.followUp ?? ''} ${incident.notes ?? ''}`.toLocaleLowerCase()
    return matchesStatus && (!search.trim() || text.includes(search.trim().toLocaleLowerCase()))
  }), [incidents, search, statusFilter])

  const pendingCount = incidents.filter((incident) => !incident.parentNotified).length
  const severeCount = incidents.filter((incident) => incident.injuryType === 3 || incident.injuryType === 4 || incident.injuryType === 5).length

  const openCreate = () => {
    setEditingId(null)
    setForm(blankForm())
    setError(null)
    setSaved(false)
    setSavedMessage('Incident report saved.')
    setFormOpen(true)
  }

  const openEdit = (incident: IncidentRecord) => {
    setEditingId(incident.id)
    setForm({
      incidentTime: localDateTimeValue(new Date(incident.incidentTime)),
      location: incident.location,
      description: incident.description,
      injuryType: incident.injuryType,
      affectedBodyPart: incident.affectedBodyPart,
      firstAidGiven: incident.firstAidGiven ?? '',
      followUp: incident.followUp ?? '',
      notes: incident.notes ?? '',
    })
    setError(null)
    setSaved(false)
    setSavedMessage('Incident report saved.')
    setFormOpen(true)
  }

  const saveIncident = async (submitEvent: FormEvent) => {
    submitEvent.preventDefault()
    if (!childId) {
      setError('Choose a child before creating an incident report.')
      return
    }
    setSaving(true)
    setError(null)
    setSaved(false)
    const payload = {
      ...(!editingId ? { childId } : {}),
      incidentTime: new Date(form.incidentTime).toISOString(),
      location: form.location.trim(),
      description: form.description.trim(),
      injuryType: form.injuryType,
      affectedBodyPart: form.affectedBodyPart,
      firstAidGiven: form.firstAidGiven.trim() || null,
      followUp: form.followUp.trim() || null,
      notes: form.notes.trim() || null,
    }
    try {
      if (editingId) {
        await incidentRecordsApi.update(editingId, payload)
        setSavedMessage('Incident report updated.')
      } else {
        const message = await incidentRecordsApi.create(payload)
        setSavedMessage(message)
      }
      setSaved(true)
      setFormOpen(false)
      await loadIncidents()
    } catch (saveError) {
      setError(errorMessage(saveError))
    } finally {
      setSaving(false)
    }
  }

  const markNotified = async (incidentId: string) => {
    setNotifyingId(incidentId)
    setError(null)
    try {
      await incidentRecordsApi.markParentNotified(incidentId)
      await loadIncidents()
    } catch (notifyError) {
      setError(errorMessage(notifyError))
    } finally {
      setNotifyingId(null)
    }
  }

  return (
    <div className="space-y-6 pb-12">
      <section className="overflow-hidden rounded-3xl bg-gradient-to-br from-rose-800 via-rose-700 to-orange-700 p-6 text-white shadow-lg sm:p-8">
        <div className="flex flex-col gap-6 md:flex-row md:items-end md:justify-between">
          <div className="max-w-2xl">
            <span className="inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-rose-50"><ShieldAlert size={14} /> Safety & care</span>
            <h1 className="mt-4 text-3xl font-bold tracking-tight sm:text-4xl">Incident reports</h1>
            <p className="mt-2 max-w-xl text-sm leading-6 text-rose-50/90 sm:text-base">Document what happened, the care provided, and any follow-up needed for a child.</p>
          </div>
          {!parentView && <button onClick={openCreate} disabled={!childId} className="inline-flex shrink-0 items-center justify-center gap-2 rounded-xl bg-white px-5 py-3 text-sm font-bold text-rose-800 shadow-sm transition hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-50"><Plus size={18} />Create incident report</button>}
        </div>
      </section>

      {(error || childrenError) && <div role="alert" className="flex items-start gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"><AlertTriangle size={18} className="mt-0.5 shrink-0" /><span>{error ?? childrenError}</span>{error && <button type="button" onClick={() => setError(null)} className="ml-auto"><X size={16} /></button>}</div>}
      {saved && <div role="status" className="flex items-center gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-800"><CheckCircle2 size={17} /> {savedMessage}</div>}

      <section className="grid gap-4 sm:grid-cols-3">
        <Metric icon={ClipboardList} label="Incident reports" value={incidents.length} tone="rose" />
        <Metric icon={BellRing} label="Parent follow-up pending" value={pendingCount} tone="amber" />
        <Metric icon={AlertTriangle} label="Higher attention" value={severeCount} tone="orange" />
      </section>

      <section className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
        <label className="mb-2 block text-sm font-semibold text-slate-700">Child for new reports</label>
        <ChildSearchSelect children={children} value={childId} onChange={setChildId} disabled={childrenLoading || children.length === 0} placeholder={childrenLoading ? 'Loading children…' : 'Search children by name'} />
        {!childrenLoading && !children.length && <p className="mt-3 text-sm text-slate-500">No linked children are available.</p>}
      </section>

      {formOpen && !parentView && (
        <section className="rounded-2xl border border-rose-200 bg-white p-5 shadow-sm sm:p-6">
          <div className="mb-5 flex items-start justify-between gap-4"><div><p className="text-xs font-bold uppercase tracking-wider text-rose-700">Incident details</p><h2 className="mt-1 text-xl font-bold text-slate-900">{editingId ? 'Update incident report' : 'Create incident report'}</h2></div><button type="button" onClick={() => setFormOpen(false)} className="rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700" aria-label="Close form"><X size={19} /></button></div>
          <form onSubmit={saveIncident} className="grid gap-4 md:grid-cols-2">
            <Field label="Incident date and time" required><input type="datetime-local" required min={localDateTimeValue(new Date(Date.now() - 60 * 24 * 60 * 60 * 1000))} max={localDateTimeValue(new Date())} value={form.incidentTime} onChange={(event) => setForm((previous) => ({ ...previous, incidentTime: event.target.value }))} className={inputClass} /></Field>
            <Field label="Location" required><input required maxLength={255} value={form.location} onChange={(event) => setForm((previous) => ({ ...previous, location: event.target.value }))} placeholder="Room, playground, or other location" className={inputClass} /></Field>
            <Field label="Incident type" required><select required value={form.injuryType} onChange={(event) => setForm((previous) => ({ ...previous, injuryType: Number(event.target.value) }))} className={inputClass}>{INJURY_TYPES.map((type) => <option key={type.value} value={type.value}>{type.label}</option>)}</select></Field>
            <Field label="Affected body part" required><select required value={form.affectedBodyPart} onChange={(event) => setForm((previous) => ({ ...previous, affectedBodyPart: Number(event.target.value) }))} className={inputClass}>{BODY_PARTS.map((part) => <option key={part.value} value={part.value}>{part.label}</option>)}</select></Field>
            <Field label="What happened" required><textarea required maxLength={1000} rows={4} value={form.description} onChange={(event) => setForm((previous) => ({ ...previous, description: event.target.value }))} placeholder="Describe the incident clearly and factually." className={`${inputClass} resize-y`} /></Field>
            <Field label="First aid provided"><textarea maxLength={500} rows={4} value={form.firstAidGiven} onChange={(event) => setForm((previous) => ({ ...previous, firstAidGiven: event.target.value }))} placeholder="Record care provided and who provided it." className={`${inputClass} resize-y`} /></Field>
            <Field label="Follow-up plan"><textarea maxLength={1000} rows={3} value={form.followUp} onChange={(event) => setForm((previous) => ({ ...previous, followUp: event.target.value }))} placeholder="Add planned monitoring or next steps." className={`${inputClass} resize-y`} /></Field>
            <Field label="Additional notes"><textarea maxLength={1000} rows={3} value={form.notes} onChange={(event) => setForm((previous) => ({ ...previous, notes: event.target.value }))} placeholder="Add other relevant details." className={`${inputClass} resize-y`} /></Field>
            <p className="rounded-xl bg-amber-50 px-4 py-3 text-sm leading-5 text-amber-900 md:col-span-2">Parent notification is recorded after the parent has actually been contacted. This form does not send a notification.</p>
            <div className="flex flex-wrap items-center justify-end gap-3 md:col-span-2"><button type="button" onClick={() => setFormOpen(false)} className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50">Cancel</button><button type="submit" disabled={saving} className="inline-flex items-center gap-2 rounded-xl bg-rose-700 px-5 py-2.5 text-sm font-bold text-white hover:bg-rose-800 disabled:opacity-60">{saving && <Loader2 size={16} className="animate-spin" />}{saving ? 'Saving…' : editingId ? 'Save changes' : 'Save incident report'}</button></div>
          </form>
        </section>
      )}

      <section className="space-y-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div><h2 className="text-xl font-bold text-slate-900">Incident history</h2><p className="mt-1 text-sm text-slate-500">Reports across all children, newest first.</p></div>
          <div className="flex flex-col gap-2 sm:flex-row">
            <select aria-label="Filter incidents by child" value={historyChildId} onChange={(event) => setHistoryChildId(event.target.value)} className="rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm outline-none focus:border-rose-500"><option value="all">All children</option>{children.map((child) => <option key={child.id} value={child.id}>{child.name}</option>)}</select>
            <div className="relative"><Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search incident records" className="w-full rounded-xl border border-slate-200 bg-white py-2.5 pl-9 pr-3 text-sm outline-none focus:border-rose-500 sm:w-60" /></div>
            <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value as typeof statusFilter)} className="rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm outline-none focus:border-rose-500"><option value="all">All reports</option><option value="pending">Parent follow-up pending</option><option value="notified">Parent notified</option></select>
          </div>
        </div>

        {incidentsLoading ? <LoadingState /> : filteredIncidents.length ? (
          <div className="space-y-3">
            {filteredIncidents.map((incident) => (
              <article key={incident.id} className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
                <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
                  <div className="flex min-w-0 flex-1 gap-3">
                    <span className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border ${severityTone(incident.injuryType)}`}><ShieldAlert size={20} /></span>
                    <div className="min-w-0 flex-1">
                      <div className="flex flex-wrap items-center gap-2"><h3 className="font-bold text-slate-900">{incident.injuryTypeName || 'Incident'} · {incident.affectedBodyPartName || 'Other'}</h3><span className={`rounded-full border px-2.5 py-1 text-xs font-semibold ${severityTone(incident.injuryType)}`}>{INJURY_TYPES.find((type) => type.value === incident.injuryType)?.label ?? incident.injuryTypeName}</span>{historyChildId === 'all' && incident.childName && <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-700">{incident.childName}</span>}</div>
                      <div className="mt-2 flex flex-wrap gap-x-4 gap-y-1 text-xs text-slate-500"><span className="inline-flex items-center gap-1"><Clock3 size={13} />{formatDateTime(incident.incidentTime)}</span><span className="inline-flex items-center gap-1"><MapPin size={13} />{incident.location}</span></div>
                      <p className="mt-3 whitespace-pre-line text-sm leading-6 text-slate-700">{incident.description}</p>
                      {incident.firstAidGiven && <p className="mt-3 rounded-xl bg-sky-50 px-3 py-2.5 text-sm leading-5 text-sky-900"><strong>First aid: </strong>{incident.firstAidGiven}</p>}
                      {incident.followUp && <p className="mt-2 rounded-xl bg-violet-50 px-3 py-2.5 text-sm leading-5 text-violet-900"><strong>Follow-up: </strong>{incident.followUp}</p>}
                      {incident.notes && <p className="mt-2 text-sm leading-5 text-slate-600"><strong className="text-slate-700">Notes: </strong>{incident.notes}</p>}
                      {incident.parentSignature && <p className="mt-3 rounded-xl border border-emerald-200 bg-emerald-50 px-3 py-2.5 text-sm text-emerald-800"><strong>Parent acknowledgement: </strong>{incident.parentSignature}</p>}
                      <div className="mt-4 flex flex-wrap items-center gap-2"><span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${incident.parentNotified ? 'bg-emerald-50 text-emerald-700' : 'bg-amber-50 text-amber-800'}`}><BellRing size={13} />{incident.parentNotified ? 'Parent notified' : 'Parent notification pending'}</span><span className="text-xs text-slate-400">Recorded by daycare staff</span></div>
                    </div>
                  </div>
                  {!parentView && <div className="flex shrink-0 flex-wrap gap-2 sm:flex-col"><button onClick={() => openEdit(incident)} className="rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50">Edit report</button>{!incident.parentNotified && <button onClick={() => markNotified(incident.id)} disabled={notifyingId === incident.id} className="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-700 px-3 py-2 text-sm font-semibold text-white hover:bg-emerald-800 disabled:opacity-60">{notifyingId === incident.id && <Loader2 size={14} className="animate-spin" />}Mark parent notified</button>}</div>}
                </div>
              </article>
            ))}
          </div>
        ) : (
          <div className="rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-14 text-center"><span className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-50 text-rose-700"><ShieldAlert size={26} /></span><h3 className="mt-4 font-bold text-slate-900">No incident reports to show</h3><p className="mx-auto mt-1 max-w-md text-sm text-slate-500">{search || statusFilter !== 'all' ? 'Try changing your search or notification filter.' : `Incident reports for ${historyChildId === 'all' ? 'these children' : 'this child'} will appear here when staff records them.`}</p>{!parentView && childId && !search && statusFilter === 'all' && <button onClick={openCreate} className="mt-5 inline-flex items-center gap-2 rounded-xl bg-rose-700 px-4 py-2.5 text-sm font-bold text-white hover:bg-rose-800"><Plus size={16} />Create first report</button>}</div>
        )}
      </section>
    </div>
  )
}

const inputClass = 'w-full rounded-xl border border-slate-200 bg-white px-3.5 py-3 text-sm text-slate-800 outline-none transition placeholder:text-slate-400 focus:border-rose-500 focus:ring-4 focus:ring-rose-500/10'

const Field = ({ label, required, children }: { label: string; required?: boolean; children: ReactNode }) => (
  <label className="block text-sm font-semibold text-slate-700"><span>{label}{required && <span className="ml-1 text-rose-500">*</span>}</span><span className="mt-1.5 block">{children}</span></label>
)

const Metric = ({ icon: Icon, label, value, tone }: { icon: LucideIcon; label: string; value: number; tone: 'rose' | 'amber' | 'orange' }) => {
  const styles = { rose: 'bg-rose-50 text-rose-700 ring-rose-100', amber: 'bg-amber-50 text-amber-700 ring-amber-100', orange: 'bg-orange-50 text-orange-700 ring-orange-100' }
  return <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"><div className="flex items-center justify-between"><span className="text-sm font-medium text-slate-500">{label}</span><span className={`flex h-9 w-9 items-center justify-center rounded-xl ring-1 ${styles[tone]}`}><Icon size={18} /></span></div><div className="mt-3 text-3xl font-bold tracking-tight text-slate-900">{value}</div></div>
}

const LoadingState = () => <div className="flex items-center justify-center gap-3 rounded-2xl border border-slate-200 bg-white py-14 text-sm text-slate-500"><Loader2 size={20} className="animate-spin text-rose-700" />Loading incident reports…</div>
