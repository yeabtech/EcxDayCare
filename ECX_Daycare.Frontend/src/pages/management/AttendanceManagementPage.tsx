import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  AlertCircle,
  CalendarDays,
  Check,
  ChevronRight,
  Clock3,
  LoaderCircle,
  LogIn,
  LogOut,
  RefreshCw,
  Search,
  School,
  Users,
  UserRoundCheck,
  UserRoundX,
} from 'lucide-react'
import { apiClient } from '@services/api'
import { CheckInModal, CheckOutModal, attendanceApi } from '@features/attendance'
import type { AttendanceRecord, DailyAttendanceSummary } from '@features/attendance'
import { useAuth } from '@features/auth'
import { classroomsApi } from '@features/classrooms/services/classroomsApi'
import { staffApi } from '@features/staff/services/staffApi'

interface Classroom {
  id: string
  name: string
  description?: string | null
  maxCapacity?: number
}

interface ActiveEnrollment {
  id: string
  childId: string
  childName: string
  classroomId: string
  classroomName: string
  programType?: string
  daysOfWeek?: string
}

const getPayload = (response: any) => response?.data?.data ?? response?.data?.Data ?? response?.data

const localDate = () => {
  const now = new Date()
  return new Date(now.getTime() - now.getTimezoneOffset() * 60_000).toISOString().slice(0, 10)
}

const formatTime = (value?: string | null) =>
  value ? new Date(value).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' }) : '—'

const STATUS_META: Record<string, { label: string; className: string }> = {
  Present: { label: 'Present', className: 'attendance-status attendance-status--present' },
  Late: { label: 'Late', className: 'attendance-status attendance-status--late' },
  Absent: { label: 'Absent', className: 'attendance-status attendance-status--absent' },
  EarlyPickup: { label: 'Checked out', className: 'attendance-status attendance-status--checkout' },
}

const Stat = ({ label, value, icon, tone }: { label: string; value: number; icon: ReactNode; tone: string }) => (
  <div className="attendance-stat">
    <div><p className="attendance-stat__label">{label}</p><p className="attendance-stat__value">{value}</p></div>
    <span className={`attendance-stat__icon ${tone}`}>{icon}</span>
  </div>
)

export const AttendanceManagementPage = ({ staffMode = false }: { staffMode?: boolean } = {}) => {
  const navigate = useNavigate()
  const { user } = useAuth()
  const today = localDate()
  const [selectedDate, setSelectedDate] = useState(today)
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [selectedRoomId, setSelectedRoomId] = useState('')
  const [roster, setRoster] = useState<ActiveEnrollment[]>([])
  const [attendance, setAttendance] = useState<AttendanceRecord[]>([])
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [initialChildId, setInitialChildId] = useState<string | undefined>()
  const [checkInOpen, setCheckInOpen] = useState(false)
  const [checkoutRecord, setCheckoutRecord] = useState<AttendanceRecord | null>(null)
  const [classroomsLoading, setClassroomsLoading] = useState(true)
  const [rosterLoading, setRosterLoading] = useState(false)
  const [attendanceLoading, setAttendanceLoading] = useState(false)
  const [attendanceLoadFailed, setAttendanceLoadFailed] = useState(false)
  const [checkoutLoading, setCheckoutLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    if (staffMode) {
      setClassroomsLoading(true)
      setRosterLoading(true)
      setError(null)
      if (!user?.StaffId) {
        setClassrooms([])
        setRoster([])
        setSelectedRoomId('')
        setError('This login is not linked to a staff record. Ask an administrator to link your account.')
        setClassroomsLoading(false)
        setRosterLoading(false)
        return () => { active = false }
      }

      staffApi.getStaffMember(user.StaffId)
        .then(async (staff) => {
          if (!active) return
          if (!staff.classroomId) {
            setClassrooms([])
            setRoster([])
            setSelectedRoomId('')
            setError('Your staff record is not assigned to a classroom yet. Ask an administrator to assign one.')
            return
          }
          const [classroom, children] = await Promise.all([
            classroomsApi.getClassroom(staff.classroomId),
            classroomsApi.getClassroomChildren(staff.classroomId),
          ])
          if (!active) return
          setClassrooms([{
            id: classroom.id,
            name: classroom.name,
            description: classroom.description,
            maxCapacity: classroom.maxCapacity,
          }])
          setSelectedRoomId(classroom.id)
          setRoster(children.filter((child) => ['enrolled', 'active'].includes(child.Status.toLowerCase())).map((child) => ({
            id: child.Id,
            childId: child.Id,
            childName: `${child.FirstName} ${child.LastName}`.trim(),
            classroomId: classroom.id,
            classroomName: classroom.name,
            programType: child.Status,
          })))
        })
        .catch((requestError: any) => {
          if (active) {
            setClassrooms([])
            setRoster([])
            setSelectedRoomId('')
            setError(requestError.response?.data?.message || requestError.message || 'Could not load your classroom roster.')
          }
        })
        .finally(() => {
          if (active) {
            setClassroomsLoading(false)
            setRosterLoading(false)
          }
        })
      return () => { active = false }
    }

    apiClient.get<any>('/classrooms', { params: { page: 1, pageSize: 100 } })
      .then((response) => {
        const payload = getPayload(response)
        const items = payload?.items ?? payload?.Items ?? []
        const mapped: Classroom[] = items.map((room: any) => ({
          id: room.id ?? room.Id,
          name: room.name ?? room.Name,
          description: room.description ?? room.Description,
          maxCapacity: room.maxCapacity ?? room.MaxCapacity,
        }))
        if (active) {
          setClassrooms(mapped)
          setSelectedRoomId((current) => current || mapped[0]?.id || '')
        }
      })
      .catch((requestError: any) => {
        if (active) setError(requestError.response?.data?.message || 'Could not load classrooms.')
      })
      .finally(() => { if (active) setClassroomsLoading(false) })
    return () => { active = false }
  }, [staffMode, user?.StaffId])

  useEffect(() => {
    if (staffMode) return
    if (!selectedRoomId) {
      setRoster([])
      return
    }
    let active = true
    setRosterLoading(true)
    setError(null)
    apiClient.get<any>('/enrollments', {
      params: { page: 1, pageSize: 100, status: 'Active', classroomId: selectedRoomId },
    })
      .then((response) => {
        const payload = getPayload(response)
        const items = payload?.items ?? payload?.Items ?? []
        const mapped: ActiveEnrollment[] = items.map((enrollment: any) => ({
          id: enrollment.id ?? enrollment.Id,
          childId: enrollment.childId ?? enrollment.ChildId,
          childName: enrollment.childName ?? enrollment.ChildName ?? 'Unnamed child',
          classroomId: enrollment.classroomId ?? enrollment.ClassroomId ?? selectedRoomId,
          classroomName: enrollment.classroomName ?? enrollment.ClassroomName ?? '',
          programType: enrollment.programType ?? enrollment.ProgramType,
          daysOfWeek: enrollment.daysOfWeek ?? enrollment.DaysOfWeek,
        }))
        if (active) setRoster(mapped)
      })
      .catch((requestError: any) => {
        if (active) {
          setRoster([])
          setError(requestError.response?.data?.message || 'Could not load the selected classroom roster.')
        }
      })
      .finally(() => { if (active) setRosterLoading(false) })
    return () => { active = false }
  }, [selectedRoomId, staffMode])

  const loadAttendance = useCallback(async () => {
    if (!selectedRoomId) {
      setAttendance([])
      setAttendanceLoadFailed(false)
      return
    }
    setAttendanceLoading(true)
    setAttendanceLoadFailed(false)
    setError(null)
    try {
      const result: DailyAttendanceSummary = await attendanceApi.getTodayAttendance({ date: selectedDate, classroomId: selectedRoomId })
      setAttendance(result.items)
    } catch (requestError: any) {
      setAttendanceLoadFailed(true)
      setError(requestError.response?.data?.message || requestError.message || 'Could not load attendance for this date.')
      setAttendance([])
    } finally {
      setAttendanceLoading(false)
    }
  }, [selectedDate, selectedRoomId])

  useEffect(() => { void loadAttendance() }, [loadAttendance])

  const attendanceByChild = useMemo(() => new Map(attendance.map((record) => [record.childId, record])), [attendance])
  const selectedClassroom = classrooms.find((classroom) => classroom.id === selectedRoomId)
  const rosterRows = useMemo(() => {
    const query = search.trim().toLowerCase()
    return roster
      .filter((enrollment) => !query || enrollment.childName.toLowerCase().includes(query))
      .map((enrollment) => ({ enrollment, record: attendanceByChild.get(enrollment.childId) }))
      .filter(({ record }) => {
        if (!statusFilter) return true
        if (statusFilter === 'Absent') {
          return record?.status === 'Absent' || (!record && !attendanceLoadFailed && !attendanceLoading)
        }
        return record?.status === statusFilter
      })
  }, [attendanceByChild, attendanceLoadFailed, attendanceLoading, roster, search, statusFilter])

  const presentCount = roster.filter((item) => ['Present', 'Late', 'EarlyPickup'].includes(attendanceByChild.get(item.childId)?.status ?? '')).length
  const absentCount = roster.filter((item) => {
    const record = attendanceByChild.get(item.childId)
    return record?.status === 'Absent' || (!record && !attendanceLoadFailed && !attendanceLoading)
  }).length
  const checkedInCount = roster.filter((item) => {
    const record = attendanceByChild.get(item.childId)
    return !!record?.checkInTime && !record.checkOutTime
  }).length
  const isSelectedDateToday = selectedDate === today

  const openCheckIn = (childId: string) => {
    setInitialChildId(childId)
    setCheckInOpen(true)
  }

  const finishCheckIn = () => {
    setCheckInOpen(false)
    setInitialChildId(undefined)
    void loadAttendance()
  }

  const finishCheckout = async (pickedUpBy: string, isEarlyPickup: boolean) => {
    if (!checkoutRecord) return
    setCheckoutLoading(true)
    try {
      await attendanceApi.checkOutById(checkoutRecord.id, {
        childId: checkoutRecord.childId,
        pickedUpBy: pickedUpBy || null,
        isEarlyPickup,
      })
      setCheckoutRecord(null)
      await loadAttendance()
    } finally {
      setCheckoutLoading(false)
    }
  }

  return (
    <main className="attendance-page space-y-6">
      <header className="attendance-hero">
        <div className="attendance-hero__copy">
          <span className="attendance-eyebrow"><CalendarDays size={14} /> {staffMode ? 'MY CLASSROOM' : 'CLASSROOM ATTENDANCE'}</span>
          <h1>{staffMode ? 'Class attendance' : 'Daily attendance'}</h1>
          <p>{staffMode ? 'Record attendance for the children assigned to your classroom.' : 'Choose a classroom to open its active roster and record arrivals.'}</p>
        </div>
        <label className="attendance-date-control">
          <span>Date</span>
          <span className="attendance-date-control__input"><CalendarDays size={17} /><input type="date" value={selectedDate} onChange={(event) => setSelectedDate(event.target.value)} /></span>
        </label>
      </header>

      {!staffMode && <section className="attendance-classrooms" aria-label="Choose a classroom">
        <div className="attendance-section-heading"><div><h2>Choose a class</h2><p>Select a room to see its attendance list.</p></div><span>{classrooms.length} classroom{classrooms.length === 1 ? '' : 's'}</span></div>
        {classroomsLoading ? (
          <div className="attendance-loading"><LoaderCircle className="animate-spin" size={20} /> Loading classrooms…</div>
        ) : classrooms.length === 0 ? (
          <div className="attendance-empty attendance-empty--compact">No classrooms are available yet.</div>
        ) : (
          <div className="attendance-room-grid">
            {classrooms.map((room) => {
              const active = room.id === selectedRoomId
              return <button key={room.id} onClick={() => setSelectedRoomId(room.id)} aria-pressed={active} className={`attendance-room ${active ? 'attendance-room--active' : ''}`}>
                <span className="attendance-room__icon"><School size={19} /></span>
                <span className="attendance-room__content"><strong>{room.name}</strong><small>{room.description || (room.maxCapacity ? `Capacity ${room.maxCapacity}` : 'Classroom roster')}</small></span>
                {active && <span className="attendance-room__check"><Check size={15} /></span>}
              </button>
            })}
          </div>
        )}
      </section>}

      <section className="attendance-roster-panel">
        <div className="attendance-roster-heading">
          <div>
            <span className="attendance-eyebrow attendance-eyebrow--green"><School size={14} /> SELECTED CLASS</span>
            <h2>{selectedClassroom?.name ?? 'Select a classroom'}</h2>
            <p>{selectedDate === today ? 'Today’s classroom register' : `Classroom register for ${new Date(`${selectedDate}T12:00:00`).toLocaleDateString([], { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}`}</p>
          </div>
          <button onClick={() => void loadAttendance()} disabled={attendanceLoading || !selectedRoomId} className="attendance-refresh"><RefreshCw size={16} className={attendanceLoading ? 'animate-spin' : ''} /> Refresh</button>
        </div>

        <div className="attendance-stats-grid">
          <Stat label="Class roster" value={roster.length} icon={<Users size={18} />} tone="attendance-stat__icon--neutral" />
          <Stat label="Present" value={presentCount} icon={<UserRoundCheck size={18} />} tone="attendance-stat__icon--present" />
          <Stat label="Absent" value={absentCount} icon={<UserRoundX size={18} />} tone="attendance-stat__icon--absent" />
          <Stat label="Checked in now" value={checkedInCount} icon={<Clock3 size={18} />} tone="attendance-stat__icon--pending" />
        </div>

        <div className="attendance-roster-tools">
          <label className="attendance-search"><Search size={17} /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Find a child in this class" /></label>
          <select aria-label="Filter the class roster by attendance" value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
            <option value="">Everyone in class</option><option value="Present">Present</option><option value="Late">Late</option><option value="Absent">Absent</option><option value="EarlyPickup">Checked out</option>
          </select>
          <span className="attendance-roster-count">{rosterRows.length} of {roster.length} children</span>
        </div>

        {error && <div role="alert" className="attendance-error"><AlertCircle size={17} />{error}</div>}
        {rosterLoading || attendanceLoading ? (
          <div className="attendance-loading attendance-loading--body"><LoaderCircle size={21} className="animate-spin" /> Loading class attendance…</div>
        ) : !selectedRoomId ? (
          <div className="attendance-empty"><School size={30} /><strong>{staffMode ? 'No classroom assigned' : 'Choose a classroom to begin'}</strong><span>{staffMode ? 'Ask an administrator to assign a classroom to your staff record.' : 'The attendance list will show every active enrollment in that class.'}</span></div>
        ) : rosterRows.length === 0 ? (
          <div className="attendance-empty"><Users size={30} /><strong>{roster.length ? 'No children match these filters' : 'No active enrollments in this classroom'}</strong><span>{roster.length ? 'Try a different name or attendance status.' : 'Active classroom enrollments appear here when assigned.'}</span></div>
        ) : (
          <div className="attendance-list">
            {rosterRows.map(({ enrollment, record }) => {
              const checkedIn = !!record?.checkInTime && !record.checkOutTime
              const meta = record
                ? STATUS_META[record.status]
                : attendanceLoadFailed
                  ? { label: 'Unavailable', className: 'attendance-status attendance-status--pending' }
                  : STATUS_META.Absent
              const notRecorded = !record
              return <article key={enrollment.id} className="attendance-child-row">
                <div className="attendance-child-row__action">
                  {checkedIn ? (
                    <button onClick={() => setCheckoutRecord(record!)} disabled={!isSelectedDateToday} className="attendance-row-action attendance-row-action--checkout" aria-label={`Check out ${enrollment.childName}`}><LogOut size={16} /><span>Check out</span></button>
                  ) : notRecorded ? (
                    <button onClick={() => openCheckIn(enrollment.childId)} disabled={!isSelectedDateToday || attendanceLoadFailed} className="attendance-row-action" aria-label={`Check in ${enrollment.childName}`}><LogIn size={16} /><span>Check in</span></button>
                  ) : (
                    <button onClick={() => navigate(`/attendance/child/${enrollment.childId}`)} className="attendance-row-action attendance-row-action--quiet" aria-label={`View ${enrollment.childName} history`}><ChevronRight size={17} /><span>Details</span></button>
                  )}
                </div>
                <div className="attendance-child-row__main">
                  <div className="attendance-child-avatar">{enrollment.childName.split(/\s+/).map((part) => part[0]).slice(0, 2).join('').toUpperCase()}</div>
                  <div className="attendance-child-name"><strong>{enrollment.childName}</strong><span>{enrollment.programType || 'Active enrollment'}{enrollment.daysOfWeek ? ` · ${enrollment.daysOfWeek}` : ''}</span></div>
                </div>
                <div className="attendance-child-row__status">
                  <span className={meta?.className ?? 'attendance-status attendance-status--pending'}><i />{meta?.label ?? 'Not recorded'}</span>
                  {record?.droppedOffBy && <small>Dropped off by {record.droppedOffBy}</small>}
                  {record?.pickedUpBy && <small>Picked up by {record.pickedUpBy}</small>}
                  {record?.notes && <small title={record.notes}>{record.notes}</small>}
                </div>
                <div className="attendance-child-row__times"><span><LogIn size={14} />{formatTime(record?.checkInTime)}</span><span><LogOut size={14} />{formatTime(record?.checkOutTime)}</span></div>
                {record && <button onClick={() => navigate(`/attendance/child/${enrollment.childId}`)} className="attendance-history-link">History <ChevronRight size={15} /></button>}
              </article>
            })}
          </div>
        )}
        {!isSelectedDateToday && <p className="attendance-history-note">Check-in and checkout actions are available for today’s class register.</p>}
        {isSelectedDateToday && checkedInCount > 0 && <p className="attendance-live-count"><span /> {checkedInCount} child{checkedInCount === 1 ? '' : 'ren'} currently in this classroom</p>}
      </section>

      {checkInOpen && <CheckInModal initialChildId={initialChildId} initialChildName={roster.find((child) => child.childId === initialChildId)?.childName} initialClassroomId={selectedRoomId} restrictToClassroom={staffMode} onClose={() => { setCheckInOpen(false); setInitialChildId(undefined) }} onSuccess={finishCheckIn} />}
      {checkoutRecord && <CheckOutModal record={checkoutRecord} onClose={() => setCheckoutRecord(null)} onConfirm={async (pickedUpBy, early) => {
        try { await finishCheckout(pickedUpBy, early) } catch (requestError: any) { throw new Error(requestError.response?.data?.message || requestError.message || 'Check-out failed') }
      }} loading={checkoutLoading} />}
    </main>
  )
}

export default AttendanceManagementPage
