import { useCallback, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { AlertTriangle, ArrowRight, Baby, BookOpen, CalendarCheck, CalendarDays, CheckCircle2, Heart, HeartPulse, LoaderCircle, MessageCircle, Moon, Palette, RefreshCw, School, Sparkles, Utensils, UsersRound, Bell } from 'lucide-react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { apiClient } from '@services/api'
import { useAuth } from '@features/auth'
import { useParentNotifications } from '@features/notifications/ParentNotificationContext'

type Child = { id: string; firstName: string; lastName: string; dateOfBirth: string; age?: number; gender?: string | null; photoUrl?: string | null; enrollmentDate?: string; status?: string; createdAt?: string; updatedAt?: string; allergies?: string | null; medicalNotes?: string | null; classroomId?: string | null; classroomName?: string | null }
type Overview = { child: Child; classroom?: { id: string; name: string; description?: string | null; status?: string; minAgeMonths: number; maxAgeMonths: number; leadTeacher?: { firstName: string; lastName: string; email: string; phone: string } | null } | null; todayAttendance?: any; attendanceHistory: any[]; dailyReports: any[]; incidents: any[]; healthEvents: any[]; meals: any[]; naps: any[]; activities: any[] }
const unwrap = (r: any) => r?.data?.data ?? r?.data?.Data ?? r?.data
const val = (o: any, a: string, b: string) => o?.[a] ?? o?.[b]
const normalizeGender = (value: unknown): string | null => {
  if (typeof value === 'number') return ({ 1: 'Male', 2: 'Female', 3: 'Other', 4: 'PreferNotToSay' } as Record<number, string>)[value] ?? null
  if (typeof value !== 'string' || !value.trim()) return null
  if (/^\d+$/.test(value.trim())) return normalizeGender(Number(value))
  return value.trim()
}
const normalizeChild = (o: any): Child => ({ id: String(val(o,'id','Id') ?? ''), firstName: val(o,'firstName','FirstName') ?? '', lastName: val(o,'lastName','LastName') ?? '', dateOfBirth: val(o,'dateOfBirth','DateOfBirth') ?? '', age: val(o,'age','Age'), gender: normalizeGender(val(o,'gender','Gender')), photoUrl: val(o,'photoUrl','PhotoUrl'), enrollmentDate: val(o,'enrollmentDate','EnrollmentDate'), status: val(o,'status','Status'), createdAt: val(o,'createdAt','CreatedAt'), updatedAt: val(o,'updatedAt','UpdatedAt'), allergies: val(o,'allergies','Allergies'), medicalNotes: val(o,'medicalNotes','MedicalNotes'), classroomId: val(o,'classroomId','ClassroomId'), classroomName: val(o,'classroomName','ClassroomName') })
const normalize = (o: any): Overview => {
  const get = (a: string, b: string) => val(o,a,b)
  const rows = (a: string,b: string) => (get(a,b) ?? []).map((item: any) => Object.fromEntries(Object.entries(item).map(([k,v]) => [k[0].toLowerCase()+k.slice(1),v])))
  const c = get('child','Child'), cr = get('classroom','Classroom')
  const lead = cr && val(cr,'leadTeacher','LeadTeacher')
  return { child: normalizeChild(c), classroom: cr ? { id: val(cr,'id','Id'), name: val(cr,'name','Name'), description: val(cr,'description','Description'), status: val(cr,'status','Status'), minAgeMonths: val(cr,'minAgeMonths','MinAgeMonths'), maxAgeMonths: val(cr,'maxAgeMonths','MaxAgeMonths'), leadTeacher: lead ? { firstName: val(lead,'firstName','FirstName'), lastName: val(lead,'lastName','LastName'), email: val(lead,'email','Email'), phone: val(lead,'phone','Phone') } : null } : null, todayAttendance: get('todayAttendance','TodayAttendance'), attendanceHistory: rows('attendanceHistory','AttendanceHistory'), dailyReports: rows('dailyReports','DailyReports'), incidents: rows('incidents','Incidents'), healthEvents: rows('healthEvents','HealthEvents'), meals: rows('meals','Meals'), naps: rows('naps','Naps'), activities: rows('activities','Activities') }
}
const prettyDate = (d?: string | null) => d ? new Date(d).toLocaleDateString([], { year: 'numeric', month: 'short', day: 'numeric' }) : '—'
const prettyTime = (d?: string | null) => d ? new Date(d).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' }) : '—'
const friendlyLabel = (value?: string | null) => value ? value.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/[_-]/g, ' ').toLowerCase().replace(/^./, letter => letter.toUpperCase()) : ''
const todayForApi = () => new Date().toISOString().slice(0,10)
const card = 'rounded-2xl border border-white/80 bg-white/90 p-5 shadow-sm'
const title: Record<string,string> = { dashboard: 'Family dashboard', children: 'My children', classroom: 'Classrooms & teachers', notifications: 'Notifications' }

const ParentChildrenDashboard = () => {
  const { user } = useAuth()
  const [parentName, setParentName] = useState('')
  const [children, setChildren] = useState<Child[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState('')

  const loadChildren = useCallback(async (quiet = false) => {
    quiet ? setRefreshing(true) : setLoading(true)
    setError('')
    try {
      const response = await apiClient.get<any>('/parent-portal/children')
      const payload = unwrap(response)
      const items: any[] = Array.isArray(payload) ? payload : val(payload, 'children', 'Children') ?? []
      setParentName(val(payload, 'parentName', 'ParentName') ?? '')
      setChildren(items.map(normalizeChild))
    } catch (e: any) {
      setError(e?.response?.data?.message ?? 'We could not load your children. Please try again.')
    } finally {
      setLoading(false)
      setRefreshing(false)
    }
  }, [])

  useEffect(() => { void loadChildren() }, [loadChildren])

  return (
    <main className="brand-gradient-canvas -m-3 min-h-[calc(100vh-4rem)] p-4 sm:-m-4 sm:p-6 lg:-m-6 lg:p-8">
      <div className="mx-auto max-w-7xl space-y-6">
        <header className="flex flex-col gap-4 rounded-3xl border border-white/80 bg-gradient-to-r from-amber-100 via-rose-50 to-sky-100 p-5 shadow-sm sm:flex-row sm:items-center sm:justify-between sm:p-7">
          <div>
            <p className="text-xs font-bold uppercase tracking-[.16em] text-[color:var(--app-primary-deep)]">Family space</p>
            <h1 className="mt-1 text-2xl font-bold text-slate-900 sm:text-3xl">Family dashboard</h1>
            <p className="mt-2 text-sm text-slate-700">Hello, {parentName || `${user?.FirstName ?? ''} ${user?.LastName ?? ''}`.trim() || 'Parent'}! Here are the children linked to your account.</p>
          </div>
          <button type="button" onClick={() => void loadChildren(true)} disabled={refreshing} className="inline-flex h-11 items-center gap-2 self-start rounded-xl border border-white/80 bg-white/85 px-4 text-sm font-semibold shadow-sm transition hover:bg-white disabled:opacity-60 sm:self-auto">
            <RefreshCw size={16} className={refreshing ? 'animate-spin' : ''} /> Refresh
          </button>
        </header>

        {error && <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-800">{error}</div>}
        {loading ? (
          <div className="flex min-h-56 items-center justify-center gap-2 rounded-2xl bg-white/80 text-sm text-slate-500">
            <LoaderCircle className="animate-spin" size={19} /> Loading children...
          </div>
        ) : children.length === 0 ? (
          <article className={`${card} text-center`}>
            <Baby className="mx-auto text-slate-400" size={34} />
            <h2 className="mt-3 font-bold">No child is linked to this account</h2>
            <p className="mt-1 text-sm text-slate-600">Please contact the daycare administrator to connect your parent account.</p>
          </article>
        ) : (
          <section aria-label="Children linked to your account" className="space-y-5">
            {children.map(child => (
              <article key={child.id} className="group overflow-hidden rounded-[2rem] border border-white/80 bg-white shadow-xl shadow-slate-200/60 transition-shadow hover:shadow-2xl">
                <div className="grid lg:grid-cols-[minmax(16rem,0.8fr)_1.2fr]">
                  <div className="relative min-h-64 overflow-hidden bg-gradient-to-br from-amber-100 via-rose-100 to-sky-100 lg:min-h-80">
                    {child.photoUrl
                      ? <img src={child.photoUrl} alt={`${child.firstName} ${child.lastName}`} className="absolute inset-0 h-full w-full object-cover transition-transform duration-500 group-hover:scale-105" />
                      : <div className="absolute inset-0 flex items-center justify-center"><Baby size={76} strokeWidth={1.2} className="text-slate-500/60" /></div>}
                    <div className="absolute inset-0 bg-gradient-to-t from-slate-950/60 via-transparent to-transparent" />
                    <div className="absolute inset-x-0 bottom-0 p-6 text-white sm:p-8">
                      <p className="text-xs font-bold uppercase tracking-[.18em] text-white/80">Child profile</p>
                      <h2 className="mt-2 text-3xl font-black tracking-tight sm:text-4xl">{child.firstName} {child.lastName}</h2>
                    </div>
                  </div>
                  <div className="flex flex-col justify-center p-6 sm:p-8 lg:p-10">
                    <p className="text-sm font-semibold uppercase tracking-[.14em] text-[color:var(--app-primary-deep)]">A little about {child.firstName}</p>
                    <p className="mt-2 max-w-xl text-sm leading-6 text-slate-600">Here are the important profile details we have on file for your child.</p>
                    <div className="mt-7 grid gap-4 sm:grid-cols-2">
                      <div className="rounded-2xl border border-slate-100 bg-slate-50 p-5">
                        <p className="text-xs font-bold uppercase tracking-wider text-slate-500">Date of birth</p>
                        <p className="mt-2 text-lg font-bold text-slate-900">{prettyDate(child.dateOfBirth)}</p>
                      </div>
                      <div className="rounded-2xl border border-slate-100 bg-slate-50 p-5">
                        <p className="text-xs font-bold uppercase tracking-wider text-slate-500">Gender</p>
                        <p className="mt-2 text-lg font-bold text-slate-900">{child.gender ? friendlyLabel(child.gender) : 'N/A'}</p>
                      </div>
                      <div className="rounded-2xl border border-amber-100 bg-amber-50/70 p-5">
                        <p className="text-xs font-bold uppercase tracking-wider text-amber-800">Allergies</p>
                        <p className="mt-2 text-lg font-bold text-slate-900">
                          {child.allergies?.trim() || 'N/A'}
                        </p>
                      </div>
                      <div className="rounded-2xl border border-slate-100 bg-slate-50 p-5 sm:col-span-2">
                        <p className="text-xs font-bold uppercase tracking-wider text-slate-500">Medical notes</p>
                        <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-slate-800">{child.medicalNotes?.trim() || 'N/A'}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </article>
            ))}
          </section>
        )}
      </div>
    </main>
  )
}

export const ParentPortalPage = ({ view = 'dashboard' }: { view?: string }) =>
  view === 'dashboard' ? <ParentChildrenDashboard /> : <ParentPortalContent view={view} />

const ParentPortalContent = ({ view }: { view: string }) => {
  const { user } = useAuth()
  const { notifications, incidentNotifications, refreshNotifications, markAsRead, markIncidentAsRead } = useParentNotifications()
  const location = useLocation()
  const navigate = useNavigate()
  const navigationState = location.state as { childId?: string; reportDate?: string } | null
  const [children,setChildren] = useState<Child[]>([]), [selectedId,setSelectedId] = useState(navigationState?.childId ?? ''), [overview,setOverview] = useState<Overview|null>(null)
  const [parentName,setParentName] = useState('')
  const [summaries,setSummaries] = useState<Array<{child:Child;overview:Overview}>>([])
  const [selectedDate,setSelectedDate] = useState(() => navigationState?.reportDate?.slice(0,10) ?? todayForApi()), [loading,setLoading] = useState(true), [refreshing,setRefreshing] = useState(false), [error,setError] = useState('')
  const load = useCallback(async (quiet = false) => {
    quiet ? setRefreshing(true) : setLoading(true); setError('')
    try {
      const [childResponse] = await Promise.all([
        apiClient.get<any>('/parent-portal/children'),
        refreshNotifications(),
      ])
      const childPayload = unwrap(childResponse)
      const list: any[] = Array.isArray(childPayload) ? childPayload : val(childPayload, 'children', 'Children') ?? []
      setParentName(val(childPayload, 'parentName', 'ParentName') ?? '')
      const normalized: Child[] = list.map(normalizeChild); setChildren(normalized)
      const id = normalized.some(c => c.id === selectedId) ? selectedId : normalized[0]?.id ?? ''; setSelectedId(id)
      if (!id) { setOverview(null); setSummaries([]); return }
      const params = new URLSearchParams(); if (view === 'children') { params.set('from',selectedDate); params.set('to',selectedDate) }
      const details: Overview[] = await Promise.all(normalized.map(async (c: Child) => normalize(unwrap(await apiClient.get<any>(`/parent-portal/children/${c.id}${params.size ? `?${params}` : ''}`)))))
      setSummaries(normalized.map((c,i)=>({child:c,overview:details[i]})))
      setOverview(details[normalized.findIndex(c=>c.id===id)])
    } catch (e: any) { setError(e?.response?.data?.message ?? 'We could not load your family information. Please try again.'); setOverview(null) }
    finally { setLoading(false); setRefreshing(false) }
  },[selectedId,selectedDate,view,refreshNotifications])
  useEffect(() => { void load() },[load])
  const selectChild = async (id: string) => { setSelectedId(id); setLoading(true); setError(''); try { const p=new URLSearchParams();if(view==='children'){p.set('from',selectedDate);p.set('to',selectedDate)}setOverview(normalize(unwrap(await apiClient.get<any>(`/parent-portal/children/${id}${p.size?`?${p}`:''}`)))) } catch { setError('Could not load this child information.') } finally { setLoading(false) } }
  const openReportNotification = async (reportId: string) => {
    const notification = notifications.find(item => item.dailyReportId === reportId)
    if (!notification) { setError('This notification is no longer available. Refresh and try again.'); return }
    try {
      await apiClient.post(`/parent-portal/reports/${notification.dailyReportId}/read`)
      markAsRead(notification.dailyReportId)
      setSelectedId(notification.childId)
      setSelectedDate(notification.reportDate.slice(0,10))
      navigate('/parent/children', { state: { childId: notification.childId, reportDate: notification.reportDate } })
    } catch { setError('Could not open this report notification. Please try again.') }
  }
  const openIncidentNotification = async (incidentId: string) => {
    try {
      await apiClient.post(`/parent-portal/incidents/${incidentId}/read`)
      markIncidentAsRead(incidentId)
    } catch {
      setError('Could not mark this incident report as reviewed. Please try again.')
    }
  }
  const childrenSummaries = summaries.map(({child:c,overview:o})=>({...c,classroomName:o.classroom?.name??c.classroomName,attendance:o.todayAttendance,latest:[...o.meals.map(x=>({date:x.time,label:`Meal · ${x.feedingType}`})),...o.naps.map(x=>({date:x.startTime,label:`Nap · ${x.quality}`})),...o.activities.map(x=>({date:x.time,label:x.title}))].sort((a,b)=>Date.parse(b.date)-Date.parse(a.date))[0],report:o.dailyReports[0]}))
  const reportNotifications = notifications.filter(notification => !notification.isRead).map(notification => ({
    child: { firstName: notification.childName ?? 'your child' },
    report: { id: notification.dailyReportId, date: notification.reportDate, mood: null, mealsSummary: null, activitiesSummary: null, concerns: null },
  }))
  const filtered = (rows:any[],dateField:string) => rows.filter(x => String(x[dateField] ?? '').slice(0,10) === selectedDate)
  const child = overview?.child
  const section = (heading:string, childrenContent:ReactNode) => { const Icon=heading==='Attendance'?CalendarCheck:heading==='Daily reports'?BookOpen:heading==='Meals'?Utensils:heading==='Nap'?Moon:heading==='Activities'?Palette:HeartPulse; const colors:Record<string,string>={'Attendance':'from-sky-100 to-cyan-50','Daily reports':'from-violet-100 to-fuchsia-50','Meals':'from-amber-100 to-orange-50','Nap':'from-indigo-100 to-blue-50','Activities':'from-emerald-100 to-teal-50','Health & incidents':'from-rose-100 to-pink-50'}; const hasRecords=Array.isArray(childrenContent)?childrenContent.length>0:Boolean(childrenContent); return <article className={`${card} overflow-hidden`}><div className={`-mx-5 -mt-5 mb-3 flex items-center gap-3 bg-gradient-to-r ${colors[heading] ?? 'from-slate-100 to-white'} px-5 py-4`}><span className="flex h-10 w-10 items-center justify-center rounded-xl bg-white/80 text-[color:var(--app-primary-deep)]"><Icon size={19}/></span><div><h2 className="font-bold text-slate-900">{heading}</h2><p className="mt-0.5 text-xs font-medium text-slate-600">Records for {prettyDate(selectedDate)}</p></div></div><div className="divide-y divide-slate-100">{hasRecords?childrenContent:<p className="py-4 text-center text-sm text-slate-500">No records for this date.</p>}</div></article> }
  return <main className="brand-gradient-canvas -m-3 min-h-[calc(100vh-4rem)] p-4 sm:-m-4 sm:p-6 lg:-m-6 lg:p-8"><div className="mx-auto max-w-7xl space-y-6">
    <header className={`relative flex flex-col gap-4 overflow-hidden rounded-3xl border p-5 shadow-sm backdrop-blur sm:flex-row sm:items-center sm:justify-between sm:p-7 ${view==='dashboard'?'border-white/80 bg-gradient-to-r from-amber-100 via-rose-50 to-sky-100 shadow-amber-100/60':'border-white/70 bg-white/80'}`}><Sparkles aria-hidden="true" className={`pointer-events-none absolute -right-2 -top-3 h-24 w-24 rotate-12 ${view==='dashboard'?'text-amber-300/70':'hidden'}`} strokeWidth={1.5}/><div className="relative min-w-0"><p className="text-xs font-bold uppercase tracking-[.16em] text-[color:var(--app-primary-deep)]">{view==='dashboard'?'A happy day at ECX':'Family space'}</p><h1 className="mt-1 text-2xl font-bold text-slate-900 sm:text-3xl">{title[view] ?? title.dashboard}</h1><p className="mt-2 text-sm text-slate-700">{view==='dashboard'?`Hello, ${parentName || `${user?.FirstName ?? ''} ${user?.LastName ?? ''}`.trim() || 'Parent'}! ${child?.firstName ? `Let's see what ${child.firstName} is up to today.` : 'Here is your family’s latest update.'}`:'Private information for children linked to your account.'}</p>{view==='dashboard'&&<span className="mt-3 inline-flex items-center rounded-full border border-white/70 bg-white/75 px-3 py-1 text-xs font-bold text-slate-700">{prettyDate(new Date().toISOString())}</span>}</div><div className="relative flex flex-wrap gap-2">{children.length>0&&<label className="flex h-11 items-center gap-2 rounded-xl border border-white/80 bg-white/85 px-3 text-sm shadow-sm"><Baby size={17} className="text-[color:var(--app-primary)]"/><select aria-label="Choose your child" className="bg-transparent" value={selectedId} onChange={e=>void selectChild(e.target.value)}>{children.map(c=><option key={c.id} value={c.id}>{c.firstName} {c.lastName}</option>)}</select></label>}<button type="button" onClick={()=>void load(true)} disabled={refreshing} className="inline-flex h-11 items-center gap-2 rounded-xl border border-white/80 bg-white/85 px-4 text-sm font-semibold shadow-sm transition hover:bg-white disabled:opacity-60"><RefreshCw size={16} className={refreshing?'animate-spin':''}/> Refresh</button></div></header>
    {view==='children'&&<section className="flex flex-col gap-4 rounded-3xl border border-white/80 bg-gradient-to-r from-sky-100 via-indigo-50 to-violet-100 p-4 shadow-sm sm:flex-row sm:items-center sm:justify-between sm:px-6"><div className="flex items-center gap-3"><span className="flex h-11 w-11 items-center justify-center rounded-2xl bg-white/80 text-indigo-700 shadow-sm"><CalendarDays size={20}/></span><div><h2 className="font-extrabold text-slate-900">Pick a day to explore</h2><p className="text-sm text-slate-600">Showing care records for one date.</p></div></div><div className="flex flex-wrap items-center gap-2"><label htmlFor="care-date" className="sr-only">Choose care history date</label><input id="care-date" type="date" value={selectedDate} max={todayForApi()} onChange={e=>setSelectedDate(e.target.value || todayForApi())} className="h-11 rounded-xl border border-white/80 bg-white px-3 text-sm font-semibold text-slate-800 shadow-sm"/><button type="button" onClick={()=>setSelectedDate(todayForApi())} className="h-11 rounded-xl bg-indigo-700 px-4 text-sm font-bold text-white shadow-sm transition hover:bg-indigo-800 focus:outline-none focus:ring-2 focus:ring-indigo-400 focus:ring-offset-2">Today</button></div></section>}
    {error&&<div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-800">{error}</div>}
    {loading?<div className="flex min-h-56 items-center justify-center gap-2 rounded-2xl bg-white/80 text-sm text-slate-500"><LoaderCircle className="animate-spin" size={19}/> Loading family information...</div>:view==='notifications'?<section className="space-y-4">{reportNotifications.length===0&&incidentNotifications.length===0?<article className={`${card} text-center`}><Bell className="mx-auto text-slate-400" size={30}/><h2 className="mt-2 font-bold">You’re all caught up!</h2><p className="mt-1 text-sm text-slate-600">New reports shared by your daycare will appear here.</p></article>:<>{incidentNotifications.map(incident=><article key={incident.id} className={`rounded-2xl border p-5 shadow-sm ${incident.isRead?'border-slate-200 bg-white':'border-rose-300 bg-gradient-to-br from-rose-50 to-amber-50'}`}><div className="flex flex-col gap-4 sm:flex-row sm:items-start"><div className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-white shadow-sm ${incident.isRead?'text-slate-500':'text-rose-700'}`}>{incident.isRead?<CheckCircle2 size={22}/>:<AlertTriangle size={22}/>}</div><div className="min-w-0 flex-1"><div className="flex flex-wrap items-center gap-2"><p className="text-xs font-extrabold uppercase tracking-wide text-rose-800">Incident report · {prettyDate(incident.incidentTime)}</p>{!incident.isRead&&<span className="rounded-full bg-rose-700 px-2 py-1 text-[10px] font-bold uppercase text-white">Needs review</span>}</div><h2 className="mt-1 text-lg font-extrabold text-slate-900">{incident.childName} · {friendlyLabel(incident.injuryType)}</h2><dl className="mt-4 grid gap-3 sm:grid-cols-2"><div><dt className="text-xs font-bold uppercase tracking-wide text-slate-500">Location</dt><dd className="mt-1 text-sm text-slate-800">{incident.location}</dd></div><div><dt className="text-xs font-bold uppercase tracking-wide text-slate-500">Affected area</dt><dd className="mt-1 text-sm text-slate-800">{friendlyLabel(incident.affectedBodyPart)}</dd></div><div className="sm:col-span-2"><dt className="text-xs font-bold uppercase tracking-wide text-slate-500">What happened</dt><dd className="mt-1 whitespace-pre-wrap text-sm leading-6 text-slate-800">{incident.description}</dd></div>{incident.firstAidGiven&&<div className="sm:col-span-2"><dt className="text-xs font-bold uppercase tracking-wide text-slate-500">Care provided</dt><dd className="mt-1 whitespace-pre-wrap text-sm leading-6 text-slate-800">{incident.firstAidGiven}</dd></div>}{incident.followUp&&<div className="sm:col-span-2"><dt className="text-xs font-bold uppercase tracking-wide text-slate-500">Follow-up</dt><dd className="mt-1 whitespace-pre-wrap text-sm leading-6 text-slate-800">{incident.followUp}</dd></div>}{incident.notes&&<div className="sm:col-span-2"><dt className="text-xs font-bold uppercase tracking-wide text-slate-500">Additional notes</dt><dd className="mt-1 whitespace-pre-wrap text-sm leading-6 text-slate-800">{incident.notes}</dd></div>}</dl></div><button type="button" disabled={incident.isRead} onClick={()=>void openIncidentNotification(incident.incidentId)} className="min-h-11 shrink-0 rounded-full bg-rose-700 px-5 py-2 text-sm font-extrabold text-white shadow-sm transition hover:bg-rose-800 disabled:cursor-default disabled:bg-slate-200 disabled:text-slate-600"> {incident.isRead?'Reviewed':'Mark as reviewed'}</button></div></article>)}{reportNotifications.map(({child:c,report})=><article key={report.id} className="rounded-2xl border border-amber-200 bg-gradient-to-br from-amber-50 to-rose-50 p-5 shadow-sm"><div className="flex flex-col gap-4 sm:flex-row sm:items-start"><div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-white text-amber-600 shadow-sm"><Bell size={22}/></div><div className="min-w-0 flex-1"><p className="text-xs font-extrabold uppercase tracking-wide text-amber-800">Daily report · {prettyDate(report.date)}</p><h2 className="mt-1 text-lg font-extrabold text-slate-900">A new update for {c.firstName}!</h2></div><button type="button" onClick={()=>void openReportNotification(report.id)} className="min-h-11 shrink-0 rounded-full bg-amber-700 px-5 py-2 text-sm font-extrabold text-white shadow-sm transition hover:bg-amber-800 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:ring-offset-2">Open report <ArrowRight className="ml-1 inline" size={15}/></button></div></article>)}</>}</section>:!overview?<article className={`${card} text-center`}><Baby className="mx-auto text-slate-400" size={34}/><h2 className="mt-3 font-bold">No child is linked to this account</h2><p className="mt-1 text-sm text-slate-600">Please contact the daycare administrator to connect your parent account.</p></article>:view==='dashboard'?<div className="space-y-6"><section className="grid gap-5 sm:grid-cols-2">{childrenSummaries.map((c,index)=><article key={c.id} className={`group relative overflow-hidden rounded-[1.75rem] border border-white/80 bg-gradient-to-br ${['from-rose-100 via-orange-50 to-amber-100','from-sky-100 via-cyan-50 to-teal-100','from-violet-100 via-fuchsia-50 to-pink-100'][index%3]} p-5 shadow-lg shadow-slate-200/50 transition duration-300 hover:-translate-y-1 hover:shadow-xl`}><div className="absolute -right-8 -top-8 h-36 w-36 rounded-full bg-white/35 blur-2xl"/><div className="relative flex items-center gap-4"><div className="relative h-20 w-20 shrink-0 overflow-hidden rounded-[1.35rem] border-4 border-white/90 bg-white/60 shadow-md">{c.photoUrl?<img src={c.photoUrl} alt={`${c.firstName} ${c.lastName}`} className="h-full w-full object-cover"/>:<div className="flex h-full w-full items-center justify-center text-3xl"><Baby size={32} className="text-[color:var(--app-primary)]"/></div>}</div><div className="min-w-0"><p className="text-xs font-extrabold uppercase tracking-[.15em] text-slate-600">Your little one</p><h2 className="truncate text-2xl font-black tracking-tight text-slate-900">{c.firstName} {c.lastName}</h2><p className="mt-1 truncate text-sm font-semibold text-slate-700">{c.classroomName ?? 'Classroom to be assigned'}</p></div><span className="ml-auto hidden rounded-full bg-white/80 px-3 py-1 text-xs font-extrabold capitalize text-emerald-800 shadow-sm sm:inline-flex">{c.attendance?.status ?? 'Today'}</span></div><div className="relative mt-5 grid gap-2 sm:grid-cols-2"><div className="rounded-2xl bg-white/75 p-3 backdrop-blur"><p className="text-[11px] font-extrabold uppercase tracking-wide text-slate-500">Today’s attendance</p><p className="mt-1 text-sm font-bold text-slate-800">{c.attendance?.status ?? 'Not recorded'}{c.attendance?.checkInTime?` · In ${prettyTime(c.attendance.checkInTime)}`:''}{c.attendance?.checkOutTime?` · Out ${prettyTime(c.attendance.checkOutTime)}`:''}</p></div><div className="rounded-2xl bg-white/75 p-3 backdrop-blur"><p className="text-[11px] font-extrabold uppercase tracking-wide text-slate-500">Latest little moment</p><p className="mt-1 truncate text-sm font-bold text-slate-800">{c.latest?.label ?? 'Care updates will show here'}</p></div></div>{c.report&&<div className="relative mt-2 rounded-2xl bg-white/75 p-3 backdrop-blur"><p className="text-[11px] font-extrabold uppercase tracking-wide text-slate-500">Latest daily note</p><p className="mt-1 text-sm font-semibold text-slate-800">{prettyDate(c.report.date)}{c.report.mood?` · Feeling ${c.report.mood}`:''}{c.report.concerns?` · ${c.report.concerns}`:''}</p></div>}<Link className="relative mt-4 inline-flex min-h-11 items-center gap-2 rounded-full bg-slate-900 px-5 py-2.5 text-sm font-extrabold text-white shadow-md transition hover:scale-[1.03] hover:bg-slate-700 focus:outline-none focus:ring-4 focus:ring-white/80" to="/parent/children">See {c.firstName}’s day <ArrowRight aria-hidden="true" size={16}/></Link><span className="absolute bottom-4 right-5 text-3xl transition-transform group-hover:rotate-12 group-hover:scale-110" aria-hidden="true"><Sparkles size={26} className="text-amber-500"/></span></article>)}</section><div className="grid gap-4 sm:grid-cols-3"><Link className="group flex min-h-28 items-center gap-4 rounded-3xl border border-white/70 bg-gradient-to-br from-amber-200 to-orange-100 p-5 shadow-md transition hover:-translate-y-1 hover:shadow-lg" to="/notifications"><span className="flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-white/80 text-2xl shadow-sm"><MessageCircle size={25} className="text-amber-700"/></span><span><b className="block text-base font-extrabold text-slate-900">Messages</b><span className="mt-1 block text-sm text-slate-600">Visit your family inbox</span></span><span className="ml-auto text-xl transition group-hover:translate-x-1"><ArrowRight size={17}/></span></Link><Link className="group flex min-h-28 items-center gap-4 rounded-3xl border border-white/70 bg-gradient-to-br from-emerald-200 to-lime-100 p-5 shadow-md transition hover:-translate-y-1 hover:shadow-lg" to="/parent/classroom"><span className="flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-white/80 text-2xl shadow-sm"><UsersRound size={25} className="text-emerald-700"/></span><span><b className="block text-base font-extrabold text-slate-900">Care team</b><span className="mt-1 block text-sm text-slate-600">Meet teachers & classroom</span></span><span className="ml-auto text-xl transition group-hover:translate-x-1"><ArrowRight size={17}/></span></Link><Link className="group flex min-h-28 items-center gap-4 rounded-3xl border border-white/70 bg-gradient-to-br from-sky-200 to-indigo-100 p-5 shadow-md transition hover:-translate-y-1 hover:shadow-lg" to="/parent/children"><span className="flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-white/80 text-2xl shadow-sm"><Heart size={25} className="text-sky-700"/></span><span><b className="block text-base font-extrabold text-slate-900">My children</b><span className="mt-1 block text-sm text-slate-600">Explore their care history</span></span><span className="ml-auto text-xl transition group-hover:translate-x-1"><ArrowRight size={17}/></span></Link></div></div>:view==='classroom'?<div className="grid gap-4 lg:grid-cols-2"><article className={card}><div className="flex items-center gap-2"><School size={18}/><h2 className="font-bold">{overview.classroom?.name ?? 'No classroom assigned'}</h2></div><p className="mt-2 text-sm text-slate-600">{overview.classroom?.description || (overview.classroom ? `Status: ${overview.classroom.status ?? 'Current'}` : 'No active classroom assignment or current enrollment was found.')}</p>{overview.classroom&&<p className="mt-1 text-sm text-slate-500">Age range: {overview.classroom.minAgeMonths}-{overview.classroom.maxAgeMonths} months</p>}</article><article className={card}><h2 className="font-bold">Lead teacher</h2>{overview.classroom?.leadTeacher?<div className="mt-3 space-y-2 text-sm"><p className="font-semibold">{overview.classroom.leadTeacher.firstName} {overview.classroom.leadTeacher.lastName}</p>{overview.classroom.leadTeacher.email&&<a className="block text-[color:var(--app-primary-deep)]" href={`mailto:${overview.classroom.leadTeacher.email}`}>{overview.classroom.leadTeacher.email}</a>}{overview.classroom.leadTeacher.phone&&<a className="block text-[color:var(--app-primary-deep)]" href={`tel:${overview.classroom.leadTeacher.phone}`}>{overview.classroom.leadTeacher.phone}</a>}</div>:<p className="mt-2 text-sm text-slate-500">No lead teacher is currently assigned.</p>}</article></div>:<div className="space-y-4"><article className="relative overflow-hidden rounded-[1.75rem] border border-white/80 bg-gradient-to-br from-fuchsia-100 via-rose-50 to-amber-100 p-5 shadow-md sm:p-6"><Sparkles aria-hidden="true" className="pointer-events-none absolute -right-1 -top-2 h-20 w-20 rotate-12 text-rose-300/70" strokeWidth={1.5}/><div className="relative flex flex-col gap-4 sm:flex-row sm:items-center"><div className="flex h-24 w-24 shrink-0 items-center justify-center overflow-hidden rounded-[1.5rem] border-4 border-white/90 bg-white/70 shadow-md">{child?.photoUrl?<img src={child.photoUrl} alt={`${child.firstName} ${child.lastName}`} className="h-full w-full object-cover"/>:<Baby size={38} className="text-rose-500"/>}</div><div className="min-w-0 flex-1"><p className="text-xs font-extrabold uppercase tracking-[.15em] text-rose-700">My little one</p><h2 className="mt-1 text-2xl font-black tracking-tight text-slate-900">{child?.firstName} {child?.lastName}</h2><p className="mt-1 text-sm font-semibold text-slate-700">{overview.classroom?.name ?? 'Classroom not assigned'}</p><div className="mt-3 flex flex-wrap gap-2"><span className="inline-flex items-center gap-1.5 rounded-full bg-white/80 px-3 py-1 text-xs font-bold text-slate-700"><CalendarDays size={14}/>{prettyDate(child?.dateOfBirth)}</span>{overview.todayAttendance&&<span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-100/90 px-3 py-1 text-xs font-bold capitalize text-emerald-800"><CalendarCheck size={14}/>{overview.todayAttendance.status}</span>}</div></div><div className="rounded-2xl bg-white/75 px-4 py-3 text-sm shadow-sm"><p className="text-[11px] font-bold uppercase tracking-wide text-slate-500">Viewing</p><p className="mt-1 font-extrabold text-slate-800">{prettyDate(selectedDate)}</p></div></div></article><div className="grid gap-4 lg:grid-cols-2">
      {section('Attendance',filtered(overview.attendanceHistory,'date').map((x:any,i:number)=><div key={i} className="flex flex-wrap items-center justify-between gap-3 rounded-xl bg-sky-50/80 px-3 py-3"><div><p className="text-base font-extrabold text-sky-900">📅 {prettyDate(x.date)}</p><p className="mt-1 text-sm font-medium text-slate-600">Check-in <b className="font-extrabold text-sky-800">{prettyTime(x.checkInTime)}</b><span className="mx-2 text-slate-400">·</span>Pickup <b className="font-extrabold text-sky-800">{prettyTime(x.checkOutTime)}</b></p></div><span className="rounded-full bg-white px-3 py-1.5 text-sm font-bold capitalize text-sky-800 shadow-sm">{friendlyLabel(x.status)}</span></div>))}
      {section('Meals',filtered(overview.meals,'time').map((x:any,i:number)=><div key={i} className="flex gap-3 rounded-xl bg-amber-50/80 px-3 py-3"><span aria-hidden="true" className="mt-0.5 flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-amber-100 text-amber-700"><Utensils size={18}/></span><div className="min-w-0"><p className="text-base font-extrabold text-amber-900">🍽️ {friendlyLabel(x.feedingType)} <span className="text-amber-800">at {prettyTime(x.time)}</span></p>{x.foodItems&&<p className="mt-1 text-base leading-relaxed text-slate-700">On the menu: {x.foodItems}</p>}{(x.amount||x.portionConsumed)&&<p className="mt-1 text-sm font-semibold text-amber-800">{x.amount?`Amount: ${x.amount}`:''}{x.amount&&x.portionConsumed?' · ':''}{x.portionConsumed?`Eaten: ${friendlyLabel(x.portionConsumed)}`:''}</p>}</div></div>))}
      {section('Activities',filtered(overview.activities,'time').map((x:any,i:number)=><div key={i} className="flex gap-3 rounded-xl bg-emerald-50/80 px-3 py-3"><span aria-hidden="true" className="mt-0.5 flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-emerald-100 text-emerald-700"><Palette size={18}/></span><div className="min-w-0"><p className="text-base font-extrabold text-emerald-900">🎨 {x.title}</p><p className="mt-1 text-base font-bold text-emerald-800">At {prettyTime(x.time)}</p>{x.description&&<p className="mt-1 text-base leading-relaxed text-slate-700">{x.description}</p>}<p className="mt-2 inline-flex rounded-full bg-white px-3 py-1 text-sm font-bold text-emerald-800">Joined in: {friendlyLabel(x.participation)}</p></div></div>))}
      {section('Nap',filtered(overview.naps,'startTime').map((x:any,i:number)=><div key={i} className="rounded-xl bg-indigo-50/80 px-3 py-3"><p className="text-base font-extrabold text-indigo-900">😴 Nap time <span className="text-indigo-800">at {prettyTime(x.startTime)}</span></p><p className="mt-1 text-base font-bold text-indigo-800">{x.endTime?`Woke at ${prettyTime(x.endTime)}`:'End time not recorded'}{x.durationMinutes?` · ${x.durationMinutes} minutes`:''}</p><span className="mt-2 inline-flex rounded-full bg-white px-3 py-1 text-sm font-bold text-indigo-800">{friendlyLabel(x.quality)}</span></div>))}
      {section('Health & incidents',[...filtered(overview.healthEvents,'time').map((x:any)=><div key={`h${x.id}`} className="rounded-xl bg-rose-50/80 px-3 py-3"><p className="text-base font-extrabold text-rose-900">💗 {friendlyLabel(x.eventType)} <span className="text-rose-800">at {prettyTime(x.time)}</span></p><p className="mt-1 text-sm font-medium text-slate-700">{x.description||x.notes}</p></div>),...filtered(overview.incidents,'incidentTime').map((x:any)=><div key={`i${x.id}`} className="rounded-xl bg-orange-50/80 px-3 py-3"><p className="text-base font-extrabold text-orange-900">🩹 Incident at {prettyTime(x.incidentTime)} · {x.location}</p><p className="mt-1 text-base leading-relaxed text-slate-700">{x.description}</p><p className="mt-2 text-sm font-bold text-orange-800">{x.parentNotified?'Your family was notified':'Family notification not recorded'}</p></div>)])}
    </div></div>}
    <p className="px-1 text-xs text-slate-500">Signed in as {user?.FirstName ?? 'Parent'} · Family information is limited to your linked children.</p>
  </div></main>
}
