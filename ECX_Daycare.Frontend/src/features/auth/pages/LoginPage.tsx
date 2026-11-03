import { useEffect, useState, type FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import {
  AlertCircle,
  ArrowRight,
  Eye,
  EyeOff,
  Loader2,
  LockKeyhole,
  Mail,
  ShieldCheck,
  Sparkles,
} from 'lucide-react'
import ecxLogo from '@/logo/EcxLogoWide.png'
import { useAuth } from '../hooks/useAuth'

export const LoginPage = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const { login, isAuthenticated, isLoading: authLoading } = useAuth()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const from = (location.state as any)?.from?.pathname || '/dashboard'

  useEffect(() => {
    if (isAuthenticated && !authLoading) navigate(from, { replace: true })
  }, [isAuthenticated, authLoading, navigate, from])

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)

    if (!email.trim()) {
      setError('Please enter your email address.')
      return
    }

    if (!password) {
      setError('Please enter your password.')
      return
    }

    setIsSubmitting(true)
    try {
      await login({ Email: email.trim(), Password: password })
      navigate(from, { replace: true })
    } catch (err: any) {
      const serverMessage =
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        (typeof err?.response?.data === 'string' ? err.response.data : null) ||
        'Invalid email or password. Please try again.'
      setError(serverMessage)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-page brand-gradient-canvas">
      <section className="login-shell">
        <aside className="login-visual" aria-label="ECX Daycare">
          <div className="login-visual__top">
            <span className="login-visual__logo">
              <img src={ecxLogo} alt="ECX Daycare" />
            </span>
            <span className="login-visual__badge"><Sparkles size={15} /> Your little corner of ECX</span>
          </div>
          <div className="login-visual__copy">
            <p className="login-visual__eyebrow">CARE THAT GROWS WITH THEM</p>
            <h2>Big care for<br />little moments.</h2>
            <p>Good things happen when families and caregivers grow together.</p>
          </div>
          <span className="login-visual__footnote">Made for the moments that matter.</span>
        </aside>

        <section className="login-card" aria-labelledby="login-title">
          <div className="login-form-panel">
            <div className="login-form-heading">
              <span className="login-form-heading__badge"><Sparkles size={15} /> Welcome to your family hub</span>
              <h1 id="login-title">Welcome back</h1>
              <p>Sign in to pick up right where you left off.</p>
            </div>

            {error && (
              <div role="alert" className="login-error">
                <AlertCircle size={18} className="shrink-0" />
                <span>{error}</span>
              </div>
            )}

            <form onSubmit={handleSubmit} className="login-form">
              <div>
                <label htmlFor="login-email">Email address</label>
                <div className="login-field">
                  <Mail size={18} aria-hidden="true" />
                  <input
                    id="login-email"
                    type="email"
                    required
                    autoComplete="email"
                    autoCapitalize="none"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    placeholder="you@example.com"
                  />
                </div>
              </div>

              <div>
                <label htmlFor="login-password">Password</label>
                <div className="login-field">
                  <LockKeyhole size={18} aria-hidden="true" />
                  <input
                    id="login-password"
                    type={showPassword ? 'text' : 'password'}
                    required
                    autoComplete="current-password"
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    placeholder="Enter your password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((visible) => !visible)}
                    className="login-field__toggle"
                    aria-label={showPassword ? 'Hide password' : 'Show password'}
                    aria-pressed={showPassword}
                  >
                    {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>

              <button type="submit" disabled={isSubmitting} className="login-submit">
                {isSubmitting ? <><Loader2 size={18} className="animate-spin" />Signing in…</> : <>Sign in <ArrowRight size={17} /></>}
              </button>
            </form>

            <div className="login-security-note">
              <ShieldCheck size={18} />
              <p>Secure access for daycare staff, families, and administrators.</p>
            </div>
          </div>
        </section>
      </section>
      <p className="login-page__footer">If you need access, contact your daycare administrator.</p>
    </main>
  )
}
