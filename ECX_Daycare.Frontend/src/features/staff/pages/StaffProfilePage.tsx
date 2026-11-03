import { useEffect, useState } from 'react'
import { AlertCircle, CheckCircle2, Eye, EyeOff, KeyRound, LoaderCircle, Save, ShieldCheck, UserRound } from 'lucide-react'
import { authApi } from '@features/auth/api/authApi'
import type { Staff } from '../types'

const inputClass = 'w-full rounded-xl border border-gray-200 bg-white px-3.5 py-3 text-sm text-gray-800 outline-none transition focus:border-[color:var(--app-primary)] focus:ring-4 focus:ring-[color:var(--app-primary-soft)] disabled:bg-gray-50 disabled:text-gray-500'

const errorMessage = (error: any, fallback: string) =>
  error?.response?.data?.error ?? error?.response?.data?.message ?? error?.response?.data?.Message ?? fallback

export const StaffProfilePage = () => {
  const [profile, setProfile] = useState<Staff | null>(null)
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [certifications, setCertifications] = useState('')
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [showPasswords, setShowPasswords] = useState(false)
  const [loading, setLoading] = useState(true)
  const [savingProfile, setSavingProfile] = useState(false)
  const [savingPassword, setSavingPassword] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')

  useEffect(() => {
    let active = true
    authApi.getStaffProfile()
      .then((data) => {
        if (!active) return
        setProfile(data)
        setFirstName(data.firstName)
        setLastName(data.lastName)
        setPhone(data.phone ?? '')
        setEmail(data.email ?? '')
        setCertifications(data.certifications ?? '')
      })
      .catch((loadError) => { if (active) setError(errorMessage(loadError, 'Your staff profile could not be loaded.')) })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [])

  const saveProfile = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')
    setNotice('')
    setSavingProfile(true)
    try {
      const updated = await authApi.updateStaffProfile({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        phone: phone.trim(),
        email: email.trim(),
        certifications: certifications.trim() || null,
      })
      setProfile(updated)
      setNotice('Your profile details have been updated.')
    } catch (saveError) {
      setError(errorMessage(saveError, 'Your profile could not be saved.'))
    } finally {
      setSavingProfile(false)
    }
  }

  const changePassword = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')
    setNotice('')
    if (newPassword.length < 8) {
      setError('Choose a new password with at least 8 characters.')
      return
    }
    if (newPassword !== confirmPassword) {
      setError('The new password and confirmation do not match.')
      return
    }

    setSavingPassword(true)
    try {
      await authApi.changeStaffPassword({ currentPassword, newPassword, confirmPassword })
      setCurrentPassword('')
      setNewPassword('')
      setConfirmPassword('')
      setNotice('Your password has been changed.')
    } catch (passwordError) {
      setError(errorMessage(passwordError, 'Your password could not be changed. Check your current password and try again.'))
    } finally {
      setSavingPassword(false)
    }
  }

  return (
    <div className="brand-gradient-canvas -m-3 min-h-[calc(100vh-4rem)] p-4 sm:-m-4 sm:p-6 lg:-m-6 lg:p-8">
      <div className="mx-auto max-w-5xl space-y-6">
        <header className="rounded-3xl border border-white/80 bg-white/80 p-6 shadow-sm backdrop-blur sm:p-8">
          <div className="flex items-start gap-4">
            <span className="rounded-2xl bg-[color:var(--app-primary-soft)] p-3 text-[color:var(--app-primary-deep)]"><UserRound size={25} /></span>
            <div>
              <p className="text-xs font-bold uppercase tracking-[.16em] text-[color:var(--app-primary-deep)]">Staff account</p>
              <h1 className="mt-1 text-2xl font-bold tracking-tight text-gray-900 sm:text-3xl">My profile</h1>
              <p className="mt-2 max-w-xl text-sm text-gray-600">Update your contact details and keep your sign-in secure.</p>
            </div>
          </div>
        </header>

        {error && <div role="alert" className="flex items-start gap-2 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"><AlertCircle size={17} className="mt-0.5 shrink-0" />{error}</div>}
        {notice && <div role="status" className="flex items-start gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"><CheckCircle2 size={17} className="mt-0.5 shrink-0" />{notice}</div>}

        {loading ? (
          <div className="flex min-h-52 items-center justify-center gap-2 rounded-2xl bg-white/80 text-sm text-gray-500"><LoaderCircle size={19} className="animate-spin" /> Loading your profile…</div>
        ) : !profile ? (
          <div className="rounded-2xl border border-amber-200 bg-amber-50 p-5 text-sm text-amber-800">Your sign-in is not linked to a staff record. Ask an administrator to link your account.</div>
        ) : (
          <div className="grid items-start gap-5 lg:grid-cols-[1.1fr_.9fr]">
            <form onSubmit={(event) => void saveProfile(event)} className="space-y-5 rounded-2xl border border-white/80 bg-white/90 p-5 shadow-sm sm:p-6">
              <div className="flex items-center justify-between gap-3 border-b border-gray-100 pb-4">
                <div><h2 className="font-semibold text-gray-900">Personal details</h2><p className="mt-1 text-xs text-gray-500">These details are linked to your staff record.</p></div>
                <span className="rounded-full bg-[color:var(--app-primary-soft)] px-3 py-1 text-xs font-semibold text-[color:var(--app-primary-deep)]">{profile.role}</span>
              </div>

              <div className="grid gap-4 sm:grid-cols-2">
                <label className="text-sm font-medium text-gray-700">First name<input className={`${inputClass} mt-1.5`} value={firstName} onChange={(event) => setFirstName(event.target.value)} autoComplete="given-name" required maxLength={100} /></label>
                <label className="text-sm font-medium text-gray-700">Last name<input className={`${inputClass} mt-1.5`} value={lastName} onChange={(event) => setLastName(event.target.value)} autoComplete="family-name" required maxLength={100} /></label>
              </div>
              <label className="block text-sm font-medium text-gray-700">Phone<input className={`${inputClass} mt-1.5`} type="tel" value={phone} onChange={(event) => setPhone(event.target.value)} autoComplete="tel" required maxLength={30} /></label>
              <label className="block text-sm font-medium text-gray-700">Login email<input className={`${inputClass} mt-1.5`} type="email" value={email} onChange={(event) => setEmail(event.target.value)} autoComplete="email" required maxLength={256} /><span className="mt-1.5 block text-xs text-gray-500">Changing this also changes the email you use to sign in.</span></label>
              <label className="block text-sm font-medium text-gray-700">Certifications or qualifications<textarea className={`${inputClass} mt-1.5 resize-y`} value={certifications} onChange={(event) => setCertifications(event.target.value)} rows={3} maxLength={1000} placeholder="Add relevant certifications" /></label>

              <div className="flex flex-wrap items-center justify-between gap-3 border-t border-gray-100 pt-4">
                <p className="text-xs text-gray-500">{profile.classroomName ? `Assigned classroom: ${profile.classroomName}` : 'No classroom assignment'}</p>
                <button type="submit" disabled={savingProfile} className="inline-flex items-center justify-center gap-2 rounded-xl bg-[color:var(--app-primary)] px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:brightness-95 disabled:opacity-60">
                  {savingProfile ? <LoaderCircle size={17} className="animate-spin" /> : <Save size={17} />}{savingProfile ? 'Saving…' : 'Save profile'}
                </button>
              </div>
            </form>

            <form onSubmit={(event) => void changePassword(event)} className="space-y-5 rounded-2xl border border-white/80 bg-white/90 p-5 shadow-sm sm:p-6">
              <div className="flex items-center gap-3 border-b border-gray-100 pb-4">
                <span className="rounded-xl bg-[color:var(--app-primary-soft)] p-2.5 text-[color:var(--app-primary-deep)]"><ShieldCheck size={20} /></span>
                <div><h2 className="font-semibold text-gray-900">Change password</h2><p className="mt-1 text-xs text-gray-500">Use your current password to confirm this change.</p></div>
              </div>

              <label className="block text-sm font-medium text-gray-700">Current password<input className={`${inputClass} mt-1.5`} type={showPasswords ? 'text' : 'password'} value={currentPassword} onChange={(event) => setCurrentPassword(event.target.value)} autoComplete="current-password" required /></label>
              <label className="block text-sm font-medium text-gray-700">New password<input className={`${inputClass} mt-1.5`} type={showPasswords ? 'text' : 'password'} value={newPassword} onChange={(event) => setNewPassword(event.target.value)} autoComplete="new-password" minLength={8} required /><span className="mt-1.5 block text-xs text-gray-500">At least 8 characters.</span></label>
              <label className="block text-sm font-medium text-gray-700">Confirm new password<input className={`${inputClass} mt-1.5`} type={showPasswords ? 'text' : 'password'} value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} autoComplete="new-password" minLength={8} required /></label>

              <div className="flex flex-wrap items-center justify-between gap-3 border-t border-gray-100 pt-4">
                <button type="button" onClick={() => setShowPasswords((shown) => !shown)} className="inline-flex items-center gap-2 rounded-lg px-2 py-2 text-xs font-semibold text-gray-600 hover:bg-gray-50">
                  {showPasswords ? <EyeOff size={15} /> : <Eye size={15} />}{showPasswords ? 'Hide passwords' : 'Show passwords'}
                </button>
                <button type="submit" disabled={savingPassword} className="inline-flex items-center justify-center gap-2 rounded-xl border border-[color:var(--app-primary)] px-5 py-2.5 text-sm font-semibold text-[color:var(--app-primary-deep)] transition hover:bg-[color:var(--app-primary-soft)] disabled:opacity-60">
                  {savingPassword ? <LoaderCircle size={17} className="animate-spin" /> : <KeyRound size={17} />}{savingPassword ? 'Updating…' : 'Update password'}
                </button>
              </div>
            </form>
          </div>
        )}
      </div>
    </div>
  )
}
