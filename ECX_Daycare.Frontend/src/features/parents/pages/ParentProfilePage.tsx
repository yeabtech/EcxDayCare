import { useEffect, useState } from 'react'
import { AlertCircle, CheckCircle2, Eye, EyeOff, KeyRound, LoaderCircle, Save, ShieldCheck, UserRound } from 'lucide-react'
import { authApi } from '@features/auth/api/authApi'
import { useAuth } from '@features/auth'

const fieldClass = 'mt-1.5 w-full rounded-xl border border-slate-200 bg-white px-3.5 py-3 text-sm text-slate-800 outline-none transition focus:border-[color:var(--app-primary)] focus:ring-4 focus:ring-[color:var(--app-primary-soft)]'
const getError = (error: any, fallback: string) => error?.response?.data?.error ?? error?.response?.data?.message ?? error?.response?.data?.Message ?? fallback

export const ParentProfilePage = () => {
  const { user, updateUser } = useAuth()
  const [profile, setProfile] = useState<Awaited<ReturnType<typeof authApi.getParentProfile>> | null>(null)
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [address, setAddress] = useState('')
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
    authApi.getParentProfile().then((loaded) => {
      if (!active) return
      setProfile(loaded)
      setFirstName(loaded.firstName)
      setLastName(loaded.lastName)
      setEmail(loaded.email)
      setPhoneNumber(loaded.phoneNumber)
      setAddress(loaded.address)
    }).catch((loadError) => {
      if (active) setError(getError(loadError, 'Your parent profile could not be loaded.'))
    }).finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [])

  const saveProfile = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')
    setNotice('')
    setSavingProfile(true)
    try {
      const updated = await authApi.updateParentProfile({
        firstName: firstName.trim(), lastName: lastName.trim(), email: email.trim(),
        phoneNumber: phoneNumber.trim(), address: address.trim(),
      })
      setProfile(updated)
      updateUser({ FirstName: updated.firstName, LastName: updated.lastName, Email: updated.email })
      setNotice('Your profile and sign-in details have been updated.')
    } catch (saveError) {
      setError(getError(saveError, 'Your profile could not be saved.'))
    } finally { setSavingProfile(false) }
  }

  const changePassword = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')
    setNotice('')
    if (newPassword.length < 8) return setError('Choose a new password with at least 8 characters.')
    if (newPassword !== confirmPassword) return setError('The new password and confirmation do not match.')
    setSavingPassword(true)
    try {
      await authApi.changeParentPassword({ currentPassword, newPassword, confirmPassword })
      setCurrentPassword('')
      setNewPassword('')
      setConfirmPassword('')
      setNotice('Your password has been changed.')
    } catch (passwordError) {
      setError(getError(passwordError, 'Your password could not be changed. Check your current password and try again.'))
    } finally { setSavingPassword(false) }
  }

  return <main className="brand-gradient-canvas -m-3 min-h-[calc(100vh-4rem)] p-4 sm:-m-4 sm:p-6 lg:-m-6 lg:p-8">
    <div className="mx-auto max-w-5xl space-y-6">
      <header className="rounded-3xl border border-white/80 bg-white/85 p-6 shadow-sm backdrop-blur sm:p-8">
        <div className="flex items-start gap-4"><span className="rounded-2xl bg-[color:var(--app-primary-soft)] p-3 text-[color:var(--app-primary-deep)]"><UserRound size={24}/></span><div><p className="text-xs font-bold uppercase tracking-[.16em] text-[color:var(--app-primary-deep)]">Family account</p><h1 className="mt-1 text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">My profile</h1><p className="mt-2 text-sm text-slate-600">Keep your contact details current and manage your sign-in security.</p></div></div>
      </header>

      {error && <div role="alert" className="flex items-start gap-2 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800"><AlertCircle className="mt-0.5 shrink-0" size={17}/>{error}</div>}
      {notice && <div role="status" className="flex items-start gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"><CheckCircle2 className="mt-0.5 shrink-0" size={17}/>{notice}</div>}
      {loading ? <div className="flex min-h-48 items-center justify-center gap-2 rounded-2xl bg-white/80 text-sm text-slate-500"><LoaderCircle className="animate-spin" size={18}/> Loading your profile…</div> : !profile ? <div className="rounded-2xl border border-amber-200 bg-amber-50 p-5 text-sm text-amber-900">Your login is not linked to a parent record. Please contact the daycare administrator.</div> : <div className="grid items-start gap-5 lg:grid-cols-[1.1fr_.9fr]">
        <form onSubmit={(event) => void saveProfile(event)} className="space-y-5 rounded-2xl border border-white/80 bg-white/90 p-5 shadow-sm sm:p-6">
          <div className="flex items-center justify-between gap-3 border-b border-slate-100 pb-4"><div><h2 className="font-semibold text-slate-900">Contact details</h2><p className="mt-1 text-xs text-slate-500">Changes are saved to your parent and sign-in records.</p></div><span className="rounded-full bg-[color:var(--app-primary-soft)] px-3 py-1 text-xs font-semibold capitalize text-[color:var(--app-primary-deep)]">{profile.status}</span></div>
          <div className="grid gap-4 sm:grid-cols-2"><label className="text-sm font-medium text-slate-700">First name<input className={fieldClass} value={firstName} onChange={event => setFirstName(event.target.value)} autoComplete="given-name" maxLength={100} required/></label><label className="text-sm font-medium text-slate-700">Last name<input className={fieldClass} value={lastName} onChange={event => setLastName(event.target.value)} autoComplete="family-name" maxLength={100} required/></label></div>
          <label className="block text-sm font-medium text-slate-700">Email<input className={fieldClass} type="email" value={email} onChange={event => setEmail(event.target.value)} autoComplete="email" maxLength={256} required/><span className="mt-1.5 block text-xs text-slate-500">This is also the email you use to sign in.</span></label>
          <label className="block text-sm font-medium text-slate-700">Phone number<input className={fieldClass} type="tel" value={phoneNumber} onChange={event => setPhoneNumber(event.target.value)} autoComplete="tel" maxLength={30} required/></label>
          <label className="block text-sm font-medium text-slate-700">Address<textarea className={`${fieldClass} resize-y`} value={address} onChange={event => setAddress(event.target.value)} autoComplete="street-address" rows={3} maxLength={300} required/></label>
          <div className="flex justify-end border-t border-slate-100 pt-4"><button type="submit" disabled={savingProfile} className="inline-flex items-center gap-2 rounded-xl bg-[color:var(--app-primary)] px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:brightness-95 disabled:opacity-60">{savingProfile ? <LoaderCircle className="animate-spin" size={17}/> : <Save size={17}/>} {savingProfile ? 'Saving…' : 'Save profile'}</button></div>
        </form>

        <form onSubmit={(event) => void changePassword(event)} className="space-y-5 rounded-2xl border border-white/80 bg-white/90 p-5 shadow-sm sm:p-6">
          <div className="flex items-center gap-3 border-b border-slate-100 pb-4"><span className="rounded-xl bg-[color:var(--app-primary-soft)] p-2.5 text-[color:var(--app-primary-deep)]"><ShieldCheck size={20}/></span><div><h2 className="font-semibold text-slate-900">Change password</h2><p className="mt-1 text-xs text-slate-500">Confirm your current password before updating it.</p></div></div>
          <label className="block text-sm font-medium text-slate-700">Current password<input className={fieldClass} type={showPasswords ? 'text' : 'password'} value={currentPassword} onChange={event => setCurrentPassword(event.target.value)} autoComplete="current-password" required/></label>
          <label className="block text-sm font-medium text-slate-700">New password<input className={fieldClass} type={showPasswords ? 'text' : 'password'} value={newPassword} onChange={event => setNewPassword(event.target.value)} autoComplete="new-password" minLength={8} required/><span className="mt-1.5 block text-xs text-slate-500">At least 8 characters.</span></label>
          <label className="block text-sm font-medium text-slate-700">Confirm new password<input className={fieldClass} type={showPasswords ? 'text' : 'password'} value={confirmPassword} onChange={event => setConfirmPassword(event.target.value)} autoComplete="new-password" minLength={8} required/></label>
          <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 pt-4"><button type="button" onClick={() => setShowPasswords(value => !value)} className="inline-flex items-center gap-2 rounded-lg px-2 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-50">{showPasswords ? <EyeOff size={15}/> : <Eye size={15}/>} {showPasswords ? 'Hide passwords' : 'Show passwords'}</button><button type="submit" disabled={savingPassword} className="inline-flex items-center gap-2 rounded-xl border border-[color:var(--app-primary)] px-5 py-2.5 text-sm font-semibold text-[color:var(--app-primary-deep)] transition hover:bg-[color:var(--app-primary-soft)] disabled:opacity-60">{savingPassword ? <LoaderCircle className="animate-spin" size={17}/> : <KeyRound size={17}/>} {savingPassword ? 'Updating…' : 'Update password'}</button></div>
        </form>
      </div>}
      <p className="px-1 text-xs text-slate-500">Signed in as {user?.FirstName ?? 'Parent'} · Profile access is limited to your own account.</p>
    </div>
  </main>
}
