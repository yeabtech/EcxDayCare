/**
 * Staff Daily Report Creation/Editing Page
 * Rich form for creating or editing a child's daily activity report
 */
import { useState, useEffect, useCallback } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import {
  ArrowLeft,
  FileText,
  Smile,
  Utensils,
  Moon,
  Activity,
  Baby,
  Star,
  AlertTriangle,
  Save,
  Send,
  Loader2,
  CheckCircle,
  X,
  Search,
  ChevronDown,
} from 'lucide-react'
import { useDailyReports } from '../hooks/useDailyReports'
import { apiClient } from '@services/api'
import type { CreateDailyReportRequest, UpdateDailyReportRequest } from '../types'
import { useAuth } from '@features/auth'
import { staffApi } from '@features/staff/services/staffApi'
import { classroomsApi } from '@features/classrooms/services/classroomsApi'
import { dailyReportsApi } from '../services/dailyReportsApi'
import type { DailyLogsSummary } from '../types'

const MOOD_OPTIONS = ['Happy', 'Calm', 'Fussy', 'Tired', 'Excited', 'Sad', 'Sick']
const ACTIVITY_CHOICES = [
  { title: 'Outdoor play', type: 'OutdoorPlay' }, { title: 'Sensory play', type: 'Sensory' },
  { title: 'Arts and crafts', type: 'ArtsAndCrafts' }, { title: 'Story time', type: 'StoryTime' },
  { title: 'Music and movement', type: 'MusicAndMovement' }, { title: 'Circle time', type: 'CircleTime' },
  { title: 'Tummy time', type: 'TummyTime' }, { title: 'Motor skills practice', type: 'MotorSkills' },
  { title: 'Free play', type: 'FreePlay' }, { title: 'Other classroom activity', type: 'Other' },
]
const FOOD_CHOICES = ['Injera and shiro', 'Genfo porridge', 'Oatmeal porridge', 'Egg and bread', 'Rice and vegetables', 'Mixed vegetables', 'Banana', 'Avocado', 'Fruit pieces', 'Yogurt', 'Milk', 'Formula', 'Breast milk', 'Water', 'Other']
const AMOUNT_CHOICES = ['A few bites', 'Small serving', 'Half serving', 'Full serving', '120 ml', '180 ml', '240 ml']
const NAP_DURATIONS = [30, 45, 60, 75, 90, 120]
const PORTION_CHOICES = [
  { value: 'All', label: 'All' }, { value: 'Most', label: 'Most' }, { value: 'Half', label: 'About half' },
  { value: 'Some', label: 'A little' }, { value: 'None', label: 'None' },
]
const OBSERVATION_CHOICES = ['Needed help', 'Tried something new', 'Refused at first', 'Settled with support']
const newRowId = () => `${Date.now()}-${Math.random().toString(36).slice(2)}`

const localDate = () => {
  const now = new Date()
  return new Date(now.getTime() - now.getTimezoneOffset() * 60_000).toISOString().slice(0, 10)
}
const localDateTime = (date = localDate()) => {
  const now = new Date()
  const time = new Date(now.getTime() - now.getTimezoneOffset() * 60_000).toISOString().slice(11, 16)
  return `${date}T${time}`
}

interface FeedingEntryDraft {
  id: string; time: string; feedingType: string; foodItems: string; amount: string; portion: string; notes: string
}
interface NapEntryDraft {
  id: string; startTime: string; quality: string; durationMinutes: string; notes: string
}
interface ActivityEntryDraft {
  id: string; time: string; title: string; activityType: string; participation: string; durationMinutes: string; notes: string
}

const newFeedingDraft = (date?: string): FeedingEntryDraft => ({ id: newRowId(), time: localDateTime(date), feedingType: '', foodItems: '', amount: 'Small serving', portion: 'Most', notes: '' })
const newNapDraft = (date?: string): NapEntryDraft => ({ id: newRowId(), startTime: localDateTime(date), quality: '', durationMinutes: '', notes: '' })
const newActivityDraft = (date?: string): ActivityEntryDraft => ({ id: newRowId(), time: localDateTime(date), title: '', activityType: '', participation: 'Engaged', durationMinutes: '30', notes: '' })

interface FormState {
  childId: string
  date: string
  mood: string
  mealsSummary: string
  napSummary: string
  activitiesSummary: string
  diaperSummary: string
  milestones: string
  concerns: string
}

export const StaffReportFormPage = () => {
  const navigate = useNavigate()
  const { id: reportId } = useParams<{ id?: string }>()
  const [searchParams] = useSearchParams()
  const preselectedChildId = searchParams.get('childId') ?? ''
  const { user, isStaff } = useAuth()

  const isEditing = Boolean(reportId)

  const { createReport, updateReport, sendToParent, fetchById, loading, error } = useDailyReports()

  const [form, setForm] = useState<FormState>({
    childId: preselectedChildId,
    date: localDate(),
    mood: '',
    mealsSummary: '',
    napSummary: '',
    activitiesSummary: '',
    diaperSummary: '',
    milestones: '',
    concerns: '',
  })

  const [children, setChildren] = useState<Array<{ id: string; name: string }>>([])
  const [childSearch, setChildSearch] = useState('')
  const [childPickerOpen, setChildPickerOpen] = useState(false)
  const [childrenLoading, setChildrenLoading] = useState(true)
  const [childrenError, setChildrenError] = useState<string | null>(null)
  const [classroomName, setClassroomName] = useState('')
  const [submitted, setSubmitted] = useState(false)
  const [savedReportId, setSavedReportId] = useState<string | null>(null)
  const [reportAlreadySent, setReportAlreadySent] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [dailyLogs, setDailyLogs] = useState<DailyLogsSummary | null>(null)
  const [dailyLogsLoading, setDailyLogsLoading] = useState(false)
  const [dailyLogsError, setDailyLogsError] = useState<string | null>(null)
  const [savingLogEntries, setSavingLogEntries] = useState(false)
  const [feedingDrafts, setFeedingDrafts] = useState<FeedingEntryDraft[]>([newFeedingDraft()])
  const [napDrafts, setNapDrafts] = useState<NapEntryDraft[]>([newNapDraft()])
  const [activityDrafts, setActivityDrafts] = useState<ActivityEntryDraft[]>([newActivityDraft()])

  // Staff can report only on children assigned to their own classroom.
  useEffect(() => {
    let active = true
    const loadChildren = async () => {
      setChildrenLoading(true)
      setChildrenError(null)
      try {
        if (isStaff) {
          if (!user?.StaffId) throw new Error('This login is not linked to a staff record. Ask an administrator to link your account.')
          const staff = await staffApi.getStaffMember(user.StaffId)
          if (!staff.classroomId) throw new Error('Your staff record is not assigned to a classroom yet. Ask an administrator to assign one.')
          const [classroom, classroomChildren] = await Promise.all([
            classroomsApi.getClassroom(staff.classroomId),
            classroomsApi.getClassroomChildren(staff.classroomId),
          ])
          const scopedChildren = classroomChildren
            .filter((child) => ['enrolled', 'active'].includes(child.Status.toLowerCase()))
            .map((child) => ({
              id: child.Id,
              name: `${child.FirstName} ${child.LastName}`.trim(),
            }))
            .filter((child) => child.id && child.name)
          if (active) {
            setClassroomName(classroom.name)
            setChildren(scopedChildren)
          }
          return
        }

        const allChildren: Array<{ id: string; name: string }> = []
        let skip = 0
        let total = Number.POSITIVE_INFINITY

        while (skip < total) {
          const res = await apiClient.get<any>('/children', { params: { skip, take: 100 } })
          const payload = res.data?.data ?? res.data
          const rows = Array.isArray(payload)
            ? payload
            : payload?.children ?? payload?.Children ?? payload?.items ?? payload?.Items ?? []
          const page = rows.map((child: any) => ({
            id: child.Id ?? child.id ?? '',
            name: `${child.FirstName ?? child.firstName ?? ''} ${child.LastName ?? child.lastName ?? ''}`.trim(),
          })).filter((child: { id: string; name: string }) => child.id && child.name)

          allChildren.push(...page)
          total = Number(payload?.total ?? payload?.Total ?? page.length)
          if (page.length === 0) break
          skip += page.length
        }

        if (active) setChildren(allChildren)
      } catch (loadError) {
        if (active) setChildrenError(loadError instanceof Error ? loadError.message : 'Children or classroom could not be loaded. Please try again.')
      } finally {
        if (active) setChildrenLoading(false)
      }
    }

    void loadChildren()
    return () => { active = false }
  }, [isStaff, user?.StaffId])

  const selectedChild = children.find((child) => child.id === form.childId)
  const filteredChildren = children.filter((child) =>
    child.name.toLocaleLowerCase().includes(childSearch.trim().toLocaleLowerCase()),
  )

  useEffect(() => {
    if (selectedChild && !childPickerOpen) setChildSearch(selectedChild.name)
  }, [selectedChild, childPickerOpen])

  // Load existing source records so the report snapshot includes saved classroom logs.
  useEffect(() => {
    if (!form.childId || !form.date) {
      setDailyLogs(null)
      return
    }

    let active = true
    setDailyLogsLoading(true)
    setDailyLogsError(null)
    dailyReportsApi.getDailyLogs(form.childId, form.date)
      .then((logs) => {
        if (!active) return
        setDailyLogs(logs)
        // Start a new report with the real log-derived summaries. Existing report
        // summaries remain editable and are not overwritten while editing.
        setForm((previous) => ({
          ...previous,
          mealsSummary: logs.suggestedMealsSummary || (isEditing ? previous.mealsSummary : ''),
          napSummary: logs.suggestedNapSummary || (isEditing ? previous.napSummary : ''),
          activitiesSummary: logs.suggestedActivitiesSummary || (isEditing ? previous.activitiesSummary : ''),
        }))
      })
      .catch((loadError) => {
        if (active) setDailyLogsError(loadError instanceof Error ? loadError.message : 'Daily log records could not be loaded.')
      })
      .finally(() => { if (active) setDailyLogsLoading(false) })

    return () => { active = false }
  }, [form.childId, form.date, isEditing])

  // Load report if editing
  useEffect(() => {
    if (reportId && !childrenLoading && !childrenError) {
      fetchById(reportId).then((report) => {
        if (report) {
          if (isStaff && !children.some((child) => child.id === report.childId)) {
            setChildrenError('This report is for a child outside your assigned classroom.')
            return
          }
          setForm({
            childId: report.childId,
            date: report.date.split('T')[0],
            mood: report.mood ?? '',
            mealsSummary: report.mealsSummary ?? '',
            napSummary: report.napSummary ?? '',
            activitiesSummary: report.activitiesSummary ?? '',
            diaperSummary: report.diaperSummary ?? '',
            milestones: report.milestones ?? '',
            concerns: report.concerns ?? '',
          })
          const reportDate = report.date.split('T')[0]
          setFeedingDrafts([newFeedingDraft(reportDate)])
          setNapDrafts([newNapDraft(reportDate)])
          setActivityDrafts([newActivityDraft(reportDate)])
          setChildSearch(report.childName ?? '')
          setSavedReportId(report.id)
          setReportAlreadySent(report.sentToParent)
        }
      })
    }
  }, [children, childrenError, childrenLoading, fetchById, isStaff, reportId])

  const handleField = useCallback(
    (field: keyof FormState, value: string) => {
      setForm((prev) => ({ ...prev, [field]: value }))
      if (field === 'date') {
        const alignTime = (entry: { time?: string; startTime?: string }) => {
          const current = entry.time ?? entry.startTime ?? ''
          return `${value}T${current.split('T')[1] || '12:00'}`
        }
        setFeedingDrafts((entries) => entries.map((entry) => ({ ...entry, time: alignTime(entry) })))
        setNapDrafts((entries) => entries.map((entry) => ({ ...entry, startTime: alignTime(entry) })))
        setActivityDrafts((entries) => entries.map((entry) => ({ ...entry, time: alignTime(entry) })))
      }
      setFormError(null)
    },
    [],
  )

  const handleSubmit = async (e: React.FormEvent, andSend = false) => {
    e.preventDefault()
    setFormError(null)

    if (!form.childId) {
      setFormError('Please select a child.')
      return
    }
    if (!children.some((child) => child.id === form.childId)) {
      setFormError('You can only create or update reports for children in your assigned classroom.')
      return
    }

    const incompleteFeeding = feedingDrafts.some((entry) => Boolean(entry.feedingType || entry.foodItems) && !(entry.feedingType && entry.foodItems))
    if (incompleteFeeding) {
      setFormError('For each feeding entry, choose both a meal type and a food or drink.')
      return
    }

    setSavingLogEntries(true)
    try {
      // Save selected source records first. The API attaches them to an existing
      // daily report automatically; report creation below links today's records.
      for (const entry of feedingDrafts.filter((item) => item.feedingType && item.foodItems)) {
        await apiClient.post('/feedinglogs', {
          childId: form.childId,
          time: new Date(entry.time).toISOString(),
          feedingType: entry.feedingType,
          foodItems: entry.foodItems,
          amount: entry.amount || null,
          portionConsumed: entry.portion,
          notes: entry.notes || null,
        })
        setFeedingDrafts((current) => current.filter((item) => item.id !== entry.id))
      }

      for (const entry of napDrafts.filter((item) => item.quality)) {
        const startTime = new Date(entry.startTime).toISOString()
        const endTime = entry.quality === 'Refused'
          ? startTime
          : entry.durationMinutes
            ? new Date(new Date(startTime).getTime() + Number(entry.durationMinutes) * 60_000).toISOString()
            : null
        await apiClient.post('/naplogs', {
          childId: form.childId,
          startTime,
          endTime,
          quality: entry.quality,
          notes: entry.notes || null,
        })
        setNapDrafts((current) => current.filter((item) => item.id !== entry.id))
      }

      for (const entry of activityDrafts.filter((item) => item.title && item.activityType)) {
        await apiClient.post('/activitylogs', {
          childId: form.childId,
          time: new Date(entry.time).toISOString(),
          durationMinutes: Number(entry.durationMinutes),
          activityType: entry.activityType,
          title: entry.title,
          description: null,
          participation: entry.participation,
          notes: entry.notes || null,
        })
        setActivityDrafts((current) => current.filter((item) => item.id !== entry.id))
      }

      const sourceLogs = await dailyReportsApi.getDailyLogs(form.childId, form.date)
      setDailyLogs(sourceLogs)
      const payload = {
        mood: form.mood || undefined,
        mealsSummary: sourceLogs.suggestedMealsSummary || undefined,
        napSummary: sourceLogs.suggestedNapSummary || undefined,
        activitiesSummary: sourceLogs.suggestedActivitiesSummary || undefined,
        diaperSummary: form.diaperSummary || undefined,
        milestones: form.milestones || undefined,
        concerns: form.concerns || undefined,
      }

      let resultId = savedReportId
      if (savedReportId) {
        const updated = await updateReport(savedReportId, payload as UpdateDailyReportRequest)
        if (!updated) return
      } else {
        const req: CreateDailyReportRequest = { childId: form.childId, date: form.date, ...payload }
        const created = await createReport(req)
        if (!created) return
        resultId = created.id
        setSavedReportId(created.id)
      }

      if (andSend && resultId) {
        const sent = await sendToParent(resultId)
        if (!sent) return
        setReportAlreadySent(true)
      }

      setSubmitted(true)
    } catch (submitError: any) {
      setFormError(submitError?.response?.data?.error ?? submitError?.response?.data?.message ?? submitError?.response?.data?.Message ?? 'Some report data could not be saved. Any saved log entries have been kept; review the remaining entries and try again.')
      try {
        setDailyLogs(await dailyReportsApi.getDailyLogs(form.childId, form.date))
      } catch { /* Keep the last loaded log list visible. */ }
    } finally {
      setSavingLogEntries(false)
    }
  }

  if (submitted) {
    return (
      <div className="brand-gradient-canvas -m-3 flex min-h-[calc(100vh-4rem)] flex-col items-center justify-center gap-6 p-6 sm:-m-4 lg:-m-6">
        <div className="w-20 h-20 rounded-full bg-green-100 flex items-center justify-center">
          <CheckCircle className="text-green-600" size={44} />
        </div>
        <div className="text-center">
          <h2 className="text-2xl font-bold text-gray-900 mb-2">
            Report {isEditing ? 'Updated' : 'Created'}!
          </h2>
          <p className="text-gray-500">
            {reportAlreadySent
              ? 'The report has also been sent to the parent.'
              : 'The report was saved successfully.'}
          </p>
        </div>
        <div className="flex gap-3">
          <button
            onClick={() => navigate('/reports')}
            className="px-5 py-2.5 rounded-xl border border-gray-200 text-gray-700 hover:bg-gray-50 font-medium transition-colors"
          >
            Back to Reports
          </button>
          <button
            onClick={() => {
              setSubmitted(false)
              if (!isEditing) {
                setForm({
                  childId: '',
                  date: localDate(),
                  mood: '',
                  mealsSummary: '',
                  napSummary: '',
                  activitiesSummary: '',
                  diaperSummary: '',
                  milestones: '',
                  concerns: '',
                })
                setChildSearch('')
                setFeedingDrafts([newFeedingDraft()])
                setNapDrafts([newNapDraft()])
                setActivityDrafts([newActivityDraft()])
                setSavedReportId(null)
                setReportAlreadySent(false)
              }
            }}
            className="px-5 py-2.5 rounded-xl bg-[color:var(--app-primary)] text-white font-medium transition-colors hover:brightness-95"
          >
            {isEditing ? 'Continue Editing' : isStaff ? 'Next Child Report' : 'Create Another Report'}
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="brand-gradient-canvas -m-3 min-h-[calc(100vh-4rem)] p-3 sm:-m-4 sm:p-4 lg:-m-6 lg:p-7">
    <div className="mx-auto max-w-6xl space-y-6 pb-12">
      {/* Header */}
      <div className="flex items-start gap-3 rounded-3xl border border-white/70 bg-white/80 p-5 shadow-sm backdrop-blur sm:p-7">
        <button
          onClick={() => navigate(-1)}
          className="mt-0.5 rounded-xl border border-gray-200 bg-white p-2.5 text-gray-600 transition-colors hover:bg-gray-50"
        >
          <ArrowLeft size={20} />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            {isEditing ? 'Edit Daily Report' : 'New Daily Report'}
          </h1>
          {isStaff && classroomName && <span className="mt-3 inline-flex items-center gap-2 rounded-full bg-[color:var(--app-primary-soft)] px-3 py-1.5 text-xs font-semibold text-[color:var(--app-primary-deep)]"><Baby size={14} /> {classroomName} classroom</span>}
          <p className="text-sm text-gray-500 mt-0.5">
            Document the child's day — activities, meals, mood, and more
          </p>
        </div>
      </div>

      {/* Global errors */}
      {(error || formError) && (
        <div className="flex items-start gap-3 p-4 rounded-xl bg-red-50 border border-red-200 text-red-700">
          <AlertTriangle size={18} className="mt-0.5 shrink-0" />
          <span className="text-sm">{formError ?? error}</span>
          <button
            onClick={() => setFormError(null)}
            className="ml-auto text-red-400 hover:text-red-600"
          >
            <X size={16} />
          </button>
        </div>
      )}

      <form onSubmit={(e) => handleSubmit(e, false)} className="space-y-5">
        {/* Child + Date */}
        <div className="rounded-2xl border border-white/80 bg-white/90 p-5 shadow-sm backdrop-blur sm:p-6">
          <div className="flex items-center gap-2 mb-4">
            <FileText size={18} className="text-[color:var(--app-primary)]" />
            <h2 className="font-semibold text-gray-800">Report Details</h2>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Child <span className="text-red-500">*</span>
              </label>
              <div
                className="relative"
                onBlur={(event) => {
                  if (!event.currentTarget.contains(event.relatedTarget as Node | null)) {
                    setChildPickerOpen(false)
                  }
                }}
              >
                <div className="relative">
                  <Search size={17} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-gray-400" />
                  <input
                    type="search"
                    role="combobox"
                    aria-label="Search children"
                    aria-expanded={childPickerOpen && !isEditing}
                    aria-controls="daily-report-child-options"
                    aria-autocomplete="list"
                    autoComplete="off"
                    value={isEditing ? (selectedChild?.name ?? childSearch) : childSearch}
                    onFocus={() => !isEditing && setChildPickerOpen(true)}
                    onChange={(event) => {
                      if (form.childId) {
                        setFeedingDrafts([newFeedingDraft(form.date)])
                        setNapDrafts([newNapDraft(form.date)])
                        setActivityDrafts([newActivityDraft(form.date)])
                      }
                      setChildSearch(event.target.value)
                      handleField('childId', '')
                      setChildPickerOpen(true)
                    }}
                    disabled={isEditing || childrenLoading}
                    placeholder={childrenLoading ? 'Loading children…' : 'Search by child name'}
                    className="w-full pl-10 pr-10 py-2.5 rounded-xl border border-gray-200 bg-white text-sm focus:outline-none focus:ring-2 focus:ring-[color:var(--app-primary)] disabled:bg-gray-50 disabled:text-gray-500"
                    required={!form.childId}
                  />
                  <ChevronDown size={16} className="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" />
                </div>
                {childPickerOpen && !isEditing && (
                  <div
                    id="daily-report-child-options"
                    role="listbox"
                    className="absolute z-20 mt-2 max-h-60 w-full overflow-y-auto rounded-xl border border-gray-200 bg-white p-1.5 shadow-lg"
                  >
                    {childrenError ? (
                      <p className="px-3 py-2 text-sm text-red-600">{childrenError}</p>
                    ) : childrenLoading ? (
                      <p className="px-3 py-2 text-sm text-gray-500">Loading children…</p>
                    ) : filteredChildren.length ? filteredChildren.map((child) => (
                      <button
                        key={child.id}
                        type="button"
                        role="option"
                        aria-selected={form.childId === child.id}
                        onClick={() => {
                          handleField('childId', child.id)
                          setChildSearch(child.name)
                          setChildPickerOpen(false)
                        }}
                        className={`w-full rounded-lg px-3 py-2.5 text-left text-sm transition-colors ${form.childId === child.id ? 'bg-[color:var(--app-primary-soft)] font-semibold text-[color:var(--app-primary-deep)]' : 'text-gray-700 hover:bg-gray-50'}`}
                      >
                        {child.name}
                      </button>
                    )) : (
                      <p className="px-3 py-2 text-sm text-gray-500">No children match that name.</p>
                    )}
                  </div>
                )}
              </div>
              {childrenError && !childPickerOpen && <p className="mt-1.5 text-xs text-red-600">{childrenError}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">
                Date <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                value={form.date}
                onChange={(e) => handleField('date', e.target.value)}
                disabled={isEditing}
                max={localDate()}
                className="w-full px-3.5 py-2.5 rounded-xl border border-gray-200 bg-white text-sm focus:outline-none focus:ring-2 focus:ring-[color:var(--app-primary)] disabled:bg-gray-50 disabled:text-gray-400"
                required
              />
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
        {dailyLogsError && (
          <div className="lg:col-span-2 rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
            Structured daily logs could not be loaded: {dailyLogsError}
          </div>
        )}
        {/* Mood */}
        <div className="bg-white rounded-2xl border border-gray-200 p-6 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <Smile size={18} className="text-yellow-500" />
            <h2 className="font-semibold text-gray-800">Mood</h2>
          </div>
          <div className="flex flex-wrap gap-2">
            {MOOD_OPTIONS.map((mood) => (
              <button
                key={mood}
                type="button"
                onClick={() => handleField('mood', form.mood === mood ? '' : mood)}
                className={`px-4 py-2 rounded-xl text-sm font-medium transition-all border ${
                  form.mood === mood
                    ? 'bg-yellow-500 border-yellow-500 text-white shadow-sm'
                    : 'bg-white border-gray-200 text-gray-600 hover:border-yellow-300 hover:bg-yellow-50'
                }`}
              >
                {mood}
              </button>
            ))}
          </div>
        </div>

        {/* Meals */}
        <div className="bg-white rounded-2xl border border-gray-200 p-6 shadow-sm">
          <div className="mb-4 flex items-center justify-between gap-3">
            <div className="flex items-center gap-2"><Utensils size={18} className="text-orange-500" /><h2 className="font-semibold text-gray-800">Meals</h2></div>
            <button type="button" disabled={!form.childId} onClick={() => navigate(`/feeding?childId=${encodeURIComponent(form.childId)}&date=${form.date}`)} className="text-xs font-semibold text-[color:var(--app-primary-deep)] hover:underline disabled:opacity-50">Manage feeding logs</button>
          </div>
          <div className="space-y-3">
            {feedingDrafts.map((entry, index) => (
              <div key={entry.id} className="rounded-xl border border-orange-100 bg-orange-50/40 p-3">
                <div className="mb-3 flex items-center justify-between"><span className="text-xs font-semibold text-orange-800">Meal {index + 1}</span><button type="button" onClick={() => setFeedingDrafts((current) => current.filter((item) => item.id !== entry.id))} className="text-xs font-medium text-gray-500 hover:text-red-600">Remove</button></div>
                <div className="grid gap-3 sm:grid-cols-2">
                  <label className="text-xs font-medium text-gray-600">Meal type<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.feedingType} onChange={(e) => setFeedingDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, feedingType: e.target.value } : item))}><option value="">Choose meal type</option>{[['Breakfast','Breakfast'],['Lunch','Lunch'],['Snack','Snack'],['Bottle','Bottle'],['BreastMilk','Breast milk'],['Formula','Formula'],['SolidFood','Solid food'],['Water','Water']].map(([value,label]) => <option key={value} value={value}>{label}</option>)}</select></label>
                  <label className="text-xs font-medium text-gray-600">Food or drink<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.foodItems} onChange={(e) => setFeedingDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, foodItems: e.target.value } : item))}><option value="">Choose food or drink</option>{FOOD_CHOICES.map((choice) => <option key={choice}>{choice}</option>)}</select></label>
                  <label className="text-xs font-medium text-gray-600">Serving amount<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.amount} onChange={(e) => setFeedingDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, amount: e.target.value } : item))}>{AMOUNT_CHOICES.map((choice) => <option key={choice}>{choice}</option>)}</select></label>
                  <label className="text-xs font-medium text-gray-600">Amount eaten<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.portion} onChange={(e) => setFeedingDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, portion: e.target.value } : item))}>{PORTION_CHOICES.map((choice) => <option key={choice.value} value={choice.value}>{choice.label}</option>)}</select></label>
                  <label className="text-xs font-medium text-gray-600">Time<input type="datetime-local" className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.time} onChange={(e) => setFeedingDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, time: e.target.value } : item))} /></label>
                  <label className="text-xs font-medium text-gray-600">Observation<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.notes} onChange={(e) => setFeedingDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, notes: e.target.value } : item))}><option value="">No additional note</option>{OBSERVATION_CHOICES.map((choice) => <option key={choice}>{choice}</option>)}</select></label>
                </div>
              </div>
            ))}
            <button type="button" onClick={() => setFeedingDrafts((current) => [...current, newFeedingDraft(form.date)])} className="rounded-lg px-2 py-1 text-xs font-semibold text-orange-700 hover:bg-orange-50">+ Add another meal</button>
          </div>
          <div className="mt-4 border-t border-orange-100 pt-3">
            <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
              Feeding records {dailyLogsLoading ? '· Loading…' : `· ${dailyLogs?.feedings.length ?? 0}`}
            </p>
            {dailyLogs?.feedings.length ? (
              <ul className="space-y-2">
                {dailyLogs.feedings.map((log) => (
                  <li key={log.id} className="rounded-lg bg-orange-50/70 px-3 py-2 text-xs text-gray-700">
                    <span className="font-semibold">{new Date(log.time).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })}</span>
                    {' · '}{log.foodItems || log.feedingTypeName || 'Feeding'}
                    {log.amount ? ` · ${log.amount}` : ''}
                    {log.portionConsumedName ? ` · ${log.portionConsumedName}` : ''}
                  </li>
                ))}
              </ul>
            ) : <p className="text-xs text-gray-500">No feeding logs for this child and date.</p>}
          </div>
        </div>

        {/* Nap */}
        <div className="bg-white rounded-2xl border border-gray-200 p-6 shadow-sm">
          <div className="mb-4 flex items-center justify-between gap-3">
            <div className="flex items-center gap-2"><Moon size={18} className="text-blue-500" /><h2 className="font-semibold text-gray-800">Nap / Rest</h2></div>
            <button type="button" disabled={!form.childId} onClick={() => navigate(`/nap?childId=${encodeURIComponent(form.childId)}&date=${form.date}`)} className="text-xs font-semibold text-[color:var(--app-primary-deep)] hover:underline disabled:opacity-50">Manage nap logs</button>
          </div>
          <div className="space-y-3">
            {napDrafts.map((entry, index) => (
              <div key={entry.id} className="rounded-xl border border-blue-100 bg-blue-50/40 p-3">
                <div className="mb-3 flex items-center justify-between"><span className="text-xs font-semibold text-blue-800">Rest session {index + 1}</span><button type="button" onClick={() => setNapDrafts((current) => current.filter((item) => item.id !== entry.id))} className="text-xs font-medium text-gray-500 hover:text-red-600">Remove</button></div>
                <div className="grid gap-3 sm:grid-cols-2">
                  <label className="text-xs font-medium text-gray-600">Rest outcome<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.quality} onChange={(e) => setNapDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, quality: e.target.value } : item))}><option value="">Choose outcome</option>{[['Peaceful','Rested peacefully'],['Restless','Rested restlessly'],['Fussy','Needed comfort'],['Refused','Did not nap']].map(([value,label]) => <option key={value} value={value}>{label}</option>)}</select></label>
                  <label className="text-xs font-medium text-gray-600">Nap length<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.durationMinutes} onChange={(e) => setNapDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, durationMinutes: e.target.value } : item))}><option value="">Still resting</option>{NAP_DURATIONS.map((minutes) => <option key={minutes} value={minutes}>{minutes < 60 ? `${minutes} minutes` : `${minutes / 60} hour${minutes > 60 ? 's' : ''}`}</option>)}</select></label>
                  <label className="text-xs font-medium text-gray-600">Start time<input type="datetime-local" className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.startTime} onChange={(e) => setNapDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, startTime: e.target.value } : item))} /></label>
                  <label className="text-xs font-medium text-gray-600">Observation<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.notes} onChange={(e) => setNapDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, notes: e.target.value } : item))}><option value="">No additional note</option>{OBSERVATION_CHOICES.map((choice) => <option key={choice}>{choice}</option>)}</select></label>
                </div>
              </div>
            ))}
            <button type="button" onClick={() => setNapDrafts((current) => [...current, newNapDraft(form.date)])} className="rounded-lg px-2 py-1 text-xs font-semibold text-blue-700 hover:bg-blue-50">+ Add another rest session</button>
          </div>
          <div className="mt-4 border-t border-blue-100 pt-3">
            <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
              Nap records {dailyLogsLoading ? '· Loading…' : `· ${dailyLogs?.naps.length ?? 0}`}
            </p>
            {dailyLogs?.naps.length ? (
              <ul className="space-y-2">
                {dailyLogs.naps.map((log) => (
                  <li key={log.id} className="rounded-lg bg-blue-50/70 px-3 py-2 text-xs text-gray-700">
                    <span className="font-semibold">{new Date(log.startTime).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })}</span>
                    {' – '}{log.endTime ? new Date(log.endTime).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' }) : 'In progress'}
                    {log.durationMinutes != null ? ` · ${log.durationMinutes} min` : ''}
                    {log.qualityName ? ` · ${log.qualityName}` : ''}
                  </li>
                ))}
              </ul>
            ) : <p className="text-xs text-gray-500">No nap logs for this child and date.</p>}
          </div>
        </div>

        {/* Activities */}
        <div className="bg-white rounded-2xl border border-gray-200 p-6 shadow-sm">
          <div className="mb-4 flex items-center justify-between gap-3">
            <div className="flex items-center gap-2"><Activity size={18} className="text-green-500" /><h2 className="font-semibold text-gray-800">Activities</h2></div>
            <button type="button" disabled={!form.childId} onClick={() => navigate(`/activities?childId=${encodeURIComponent(form.childId)}&date=${form.date}`)} className="text-xs font-semibold text-[color:var(--app-primary-deep)] hover:underline disabled:opacity-50">Manage activity logs</button>
          </div>
          <div className="space-y-3">
            {activityDrafts.map((entry, index) => (
              <div key={entry.id} className="rounded-xl border border-green-100 bg-green-50/40 p-3">
                <div className="mb-3 flex items-center justify-between"><span className="text-xs font-semibold text-green-800">Activity {index + 1}</span><button type="button" onClick={() => setActivityDrafts((current) => current.filter((item) => item.id !== entry.id))} className="text-xs font-medium text-gray-500 hover:text-red-600">Remove</button></div>
                <div className="grid gap-3 sm:grid-cols-2">
                  <label className="text-xs font-medium text-gray-600">Activity<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.title} onChange={(e) => {
                    const selected = ACTIVITY_CHOICES.find((choice) => choice.title === e.target.value)
                    setActivityDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, title: selected?.title ?? '', activityType: selected?.type ?? '' } : item))
                  }}><option value="">Choose activity</option>{ACTIVITY_CHOICES.map((choice) => <option key={choice.title} value={choice.title}>{choice.title}</option>)}</select></label>
                  <label className="text-xs font-medium text-gray-600">Participation<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.participation} onChange={(e) => setActivityDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, participation: e.target.value } : item))}>{[['Engaged','Joined in'],['Excited','Very enthusiastic'],['Observing','Watched the group'],['Hesitant','Needed encouragement'],['SoloPlay','Played independently']].map(([value,label]) => <option key={value} value={value}>{label}</option>)}</select></label>
                  <label className="text-xs font-medium text-gray-600">Duration<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.durationMinutes} onChange={(e) => setActivityDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, durationMinutes: e.target.value } : item))}>{['10','15','20','30','45','60'].map((minutes) => <option key={minutes} value={minutes}>{minutes} minutes</option>)}</select></label>
                  <label className="text-xs font-medium text-gray-600">Time<input type="datetime-local" className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.time} onChange={(e) => setActivityDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, time: e.target.value } : item))} /></label>
                  <label className="text-xs font-medium text-gray-600 sm:col-span-2">Observation<select className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm" value={entry.notes} onChange={(e) => setActivityDrafts((current) => current.map((item) => item.id === entry.id ? { ...item, notes: e.target.value } : item))}><option value="">No additional note</option>{OBSERVATION_CHOICES.map((choice) => <option key={choice}>{choice}</option>)}</select></label>
                </div>
              </div>
            ))}
            <button type="button" onClick={() => setActivityDrafts((current) => [...current, newActivityDraft(form.date)])} className="rounded-lg px-2 py-1 text-xs font-semibold text-green-700 hover:bg-green-50">+ Add another activity</button>
          </div>
          <div className="mt-4 border-t border-green-100 pt-3">
            <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
              Activity records {dailyLogsLoading ? '· Loading…' : `· ${dailyLogs?.activities.length ?? 0}`}
            </p>
            {dailyLogs?.activities.length ? (
              <ul className="space-y-2">
                {dailyLogs.activities.map((log) => (
                  <li key={log.id} className="rounded-lg bg-green-50/70 px-3 py-2 text-xs text-gray-700">
                    <span className="font-semibold">{new Date(log.time).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })}</span>
                    {' · '}{log.title}{log.durationMinutes != null ? ` · ${log.durationMinutes} min` : ''}
                    {log.participationName ? ` · ${log.participationName}` : ''}
                    {log.description ? <span className="mt-1 block text-gray-500">{log.description}</span> : null}
                  </li>
                ))}
              </ul>
            ) : <p className="text-xs text-gray-500">No activity logs for this child and date.</p>}
          </div>
        </div>

        {/* Diapers */}
        <div className="bg-white rounded-2xl border border-gray-200 p-6 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <Baby size={18} className="text-purple-500" />
            <h2 className="font-semibold text-gray-800">Diaper Changes</h2>
          </div>
          <textarea
            value={form.diaperSummary}
            onChange={(e) => handleField('diaperSummary', e.target.value)}
            placeholder="Record change times, count, and any relevant observations."
            maxLength={500}
            rows={2}
            className="w-full px-3.5 py-2.5 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-purple-400 resize-none"
          />
        </div>

        {/* Milestones */}
        <div className="bg-white rounded-2xl border border-gray-200 p-6 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <Star size={18} className="text-amber-500" />
            <h2 className="font-semibold text-gray-800">Milestones</h2>
          </div>
          <textarea
            value={form.milestones}
            onChange={(e) => handleField('milestones', e.target.value)}
            placeholder="Record skills, progress, or notable achievements observed today."
            maxLength={1000}
            rows={2}
            className="w-full px-3.5 py-2.5 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-amber-400 resize-none"
          />
        </div>

        {/* Concerns */}
        <div className="bg-white rounded-2xl border border-gray-200 p-6 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <AlertTriangle size={18} className="text-red-500" />
            <h2 className="font-semibold text-gray-800">Concerns / Notes for Parent</h2>
          </div>
          <textarea
            value={form.concerns}
            onChange={(e) => handleField('concerns', e.target.value)}
            placeholder="Record concerns or follow-up information the parent should know."
            maxLength={1000}
            rows={3}
            className="w-full px-3.5 py-2.5 rounded-xl border border-gray-200 text-sm focus:outline-none focus:ring-2 focus:ring-red-400 resize-none"
          />
        </div>

        </div>

        {/* Action buttons */}
        <div className="flex flex-col sm:flex-row gap-3">
          <button
            type="submit"
            disabled={loading || savingLogEntries || dailyLogsLoading}
            className="flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-[color:var(--app-primary)] text-white font-semibold hover:brightness-95 disabled:opacity-60 disabled:cursor-not-allowed transition-all shadow-sm"
          >
            {loading || savingLogEntries ? <Loader2 size={18} className="animate-spin" /> : <Save size={18} />}
            {savingLogEntries ? 'Saving report and entries…' : isEditing ? 'Save Changes' : 'Save Report'}
          </button>

          {!reportAlreadySent && (
            <button
              type="button"
              disabled={loading || savingLogEntries || dailyLogsLoading}
              onClick={(e) => handleSubmit(e as any, true)}
              className="flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-emerald-600 text-white font-semibold hover:bg-emerald-700 disabled:opacity-60 disabled:cursor-not-allowed transition-all shadow-sm"
            >
              {loading || savingLogEntries ? <Loader2 size={18} className="animate-spin" /> : <Send size={18} />}
              {savingLogEntries ? 'Saving report and entries…' : 'Save & Send to Parent'}
            </button>
          )}

          {reportAlreadySent && (
            <span className="flex items-center gap-2 px-4 py-3 rounded-xl bg-green-50 text-green-700 text-sm font-medium border border-green-200">
              <CheckCircle size={16} />
              Already sent to parent
            </span>
          )}

          <button
            type="button"
            onClick={() => navigate(-1)}
            className="flex items-center justify-center gap-2 px-6 py-3 rounded-xl border border-gray-200 text-gray-700 font-medium hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
    </div>
  )
}
