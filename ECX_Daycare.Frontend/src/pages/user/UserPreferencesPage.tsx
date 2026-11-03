/**
 * User Preferences Page
 * Personal settings for the logged-in user
 */
import React, { useState, useEffect } from 'react'
import { apiClient } from '@services/api'
import { handleApiError } from '@utils/index'
import { LoadingSpinner, ErrorAlert } from '@components/shared'
import { Save, Lock, Bell } from 'lucide-react'

export interface UserProfile {
  id: string
  email: string
  firstName: string
  lastName: string
  phone?: string
  profileImage?: string
  department?: string
  role: string
}

export interface UserPreferences {
  userId: string
  emailNotifications: boolean
  smsNotifications: boolean
  pushNotifications: boolean
  newsAndUpdates: boolean
  securityAlerts: boolean
  twoFactorEnabled: boolean
  language: string
  timeFormat: '12h' | '24h'
  theme: 'light' | 'dark' | 'auto'
  timezone: string
}

export const UserPreferencesPage: React.FC = () => {
  const [profile, setProfile] = useState<UserProfile | null>(null)
  const [preferences, setPreferences] = useState<UserPreferences | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [activeTab, setActiveTab] = useState<'profile' | 'notifications' | 'security' | 'appearance'>('profile')
  const [passwordData, setPasswordData] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' })

  // Load profile and preferences on mount
  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    setIsLoading(true)
    setError(null)

    try {
      const [profileRes, prefRes] = await Promise.all([
        apiClient.get('/users/profile'),
        apiClient.get('/users/preferences'),
      ])

      setProfile(profileRes.data as UserProfile)
      setPreferences(prefRes.data as UserPreferences)
    } catch (err) {
      const apiError = handleApiError(err)
      setError(apiError.message)
    } finally {
      setIsLoading(false)
    }
  }

  const handleProfileChange = <K extends keyof UserProfile>(key: K, value: UserProfile[K]) => {
    if (profile) {
      setProfile({ ...profile, [key]: value })
    }
  }

  const handlePreferencesChange = <K extends keyof UserPreferences>(
    key: K,
    value: UserPreferences[K]
  ) => {
    if (preferences) {
      setPreferences({ ...preferences, [key]: value })
    }
  }

  const handleSaveProfile = async () => {
    if (!profile) return

    setIsSaving(true)
    setError(null)
    setSuccessMessage(null)

    try {
      await apiClient.put('/users/profile', profile)
      setSuccessMessage('Profile updated successfully!')
      setTimeout(() => setSuccessMessage(null), 3000)
    } catch (err) {
      const apiError = handleApiError(err)
      setError(apiError.message)
    } finally {
      setIsSaving(false)
    }
  }

  const handleSavePreferences = async () => {
    if (!preferences) return

    setIsSaving(true)
    setError(null)
    setSuccessMessage(null)

    try {
      await apiClient.put('/users/preferences', preferences)
      setSuccessMessage('Preferences updated successfully!')
      setTimeout(() => setSuccessMessage(null), 3000)
    } catch (err) {
      const apiError = handleApiError(err)
      setError(apiError.message)
    } finally {
      setIsSaving(false)
    }
  }

  const handleChangePassword = async () => {
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setError('Passwords do not match')
      return
    }

    if (passwordData.newPassword.length < 8) {
      setError('Password must be at least 8 characters long')
      return
    }

    setIsSaving(true)
    setError(null)
    setSuccessMessage(null)

    try {
      await apiClient.post('/users/change-password', {
        currentPassword: passwordData.currentPassword,
        newPassword: passwordData.newPassword,
      })

      setSuccessMessage('Password changed successfully!')
      setPasswordData({ currentPassword: '', newPassword: '', confirmPassword: '' })
      setTimeout(() => setSuccessMessage(null), 3000)
    } catch (err) {
      const apiError = handleApiError(err)
      setError(apiError.message)
    } finally {
      setIsSaving(false)
    }
  }

  if (isLoading) return <LoadingSpinner />

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">My Preferences</h1>
        <p className="text-gray-600 mt-2">Manage your profile and account settings</p>
      </div>

      {/* Error display */}
      {error && <ErrorAlert message={error} onDismiss={() => setError(null)} />}

      {/* Success message */}
      {successMessage && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-4">
          <p className="text-green-800 text-sm">{successMessage}</p>
        </div>
      )}

      {profile && preferences && (
        <div className="space-y-6">
          {/* Tabs */}
          <div className="border-b border-gray-200">
            <div className="flex space-x-8">
              {['profile', 'notifications', 'security', 'appearance'].map((tab) => (
                <button
                  key={tab}
                  onClick={() => setActiveTab(tab as any)}
                  className={`py-4 px-1 border-b-2 font-medium text-sm ${
                    activeTab === tab
                      ? 'border-blue-600 text-blue-600'
                      : 'border-transparent text-gray-600 hover:text-gray-900'
                  }`}
                >
                  {tab.charAt(0).toUpperCase() + tab.slice(1)}
                </button>
              ))}
            </div>
          </div>

          {/* Tab Content */}
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            {/* Profile Tab */}
            {activeTab === 'profile' && (
              <div className="space-y-4">
                <FormInput
                  label="Email"
                  value={profile.email}
                  onChange={(value) => handleProfileChange('email', value)}
                  type="email"
                  disabled
                />
                <div className="grid grid-cols-2 gap-4">
                  <FormInput
                    label="First Name"
                    value={profile.firstName}
                    onChange={(value) => handleProfileChange('firstName', value)}
                  />
                  <FormInput
                    label="Last Name"
                    value={profile.lastName}
                    onChange={(value) => handleProfileChange('lastName', value)}
                  />
                </div>
                <FormInput
                  label="Phone"
                  value={profile.phone || ''}
                  onChange={(value) => handleProfileChange('phone', value)}
                  type="tel"
                />
                <FormInput
                  label="Department"
                  value={profile.department || ''}
                  onChange={(value) => handleProfileChange('department', value)}
                  disabled
                />
                <div className="flex justify-end">
                  <button
                    onClick={handleSaveProfile}
                    disabled={isSaving}
                    className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                  >
                    <Save size={18} />
                    {isSaving ? 'Saving...' : 'Save Changes'}
                  </button>
                </div>
              </div>
            )}

            {/* Notifications Tab */}
            {activeTab === 'notifications' && (
              <div className="space-y-4">
                <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <Bell size={20} className="text-blue-600" />
                    <div>
                      <p className="font-medium text-gray-900">Email Notifications</p>
                      <p className="text-sm text-gray-600">Receive important updates via email</p>
                    </div>
                  </div>
                  <input
                    type="checkbox"
                    checked={preferences.emailNotifications}
                    onChange={(e) => handlePreferencesChange('emailNotifications', e.target.checked)}
                    className="w-5 h-5 rounded cursor-pointer"
                  />
                </div>

                <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <Bell size={20} className="text-green-600" />
                    <div>
                      <p className="font-medium text-gray-900">SMS Notifications</p>
                      <p className="text-sm text-gray-600">Receive urgent updates via SMS</p>
                    </div>
                  </div>
                  <input
                    type="checkbox"
                    checked={preferences.smsNotifications}
                    onChange={(e) => handlePreferencesChange('smsNotifications', e.target.checked)}
                    className="w-5 h-5 rounded cursor-pointer"
                  />
                </div>

                <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <Bell size={20} className="text-purple-600" />
                    <div>
                      <p className="font-medium text-gray-900">Push Notifications</p>
                      <p className="text-sm text-gray-600">Receive browser notifications</p>
                    </div>
                  </div>
                  <input
                    type="checkbox"
                    checked={preferences.pushNotifications}
                    onChange={(e) => handlePreferencesChange('pushNotifications', e.target.checked)}
                    className="w-5 h-5 rounded cursor-pointer"
                  />
                </div>

                <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <Bell size={20} className="text-orange-600" />
                    <div>
                      <p className="font-medium text-gray-900">News & Updates</p>
                      <p className="text-sm text-gray-600">Latest features and improvements</p>
                    </div>
                  </div>
                  <input
                    type="checkbox"
                    checked={preferences.newsAndUpdates}
                    onChange={(e) => handlePreferencesChange('newsAndUpdates', e.target.checked)}
                    className="w-5 h-5 rounded cursor-pointer"
                  />
                </div>

                <div className="flex justify-end">
                  <button
                    onClick={handleSavePreferences}
                    disabled={isSaving}
                    className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                  >
                    <Save size={18} />
                    {isSaving ? 'Saving...' : 'Save Changes'}
                  </button>
                </div>
              </div>
            )}

            {/* Security Tab */}
            {activeTab === 'security' && (
              <div className="space-y-4">
                <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-6">
                  <p className="text-yellow-800 text-sm">
                    For security reasons, we recommend changing your password regularly.
                  </p>
                </div>

                <div className="space-y-4 p-6 bg-gray-50 rounded-lg">
                  <h3 className="text-lg font-medium text-gray-900 flex items-center gap-2">
                    <Lock size={20} />
                    Change Password
                  </h3>

                  <FormInput
                    label="Current Password"
                    value={passwordData.currentPassword}
                    onChange={(value) => setPasswordData({ ...passwordData, currentPassword: value })}
                    type="password"
                  />
                  <FormInput
                    label="New Password"
                    value={passwordData.newPassword}
                    onChange={(value) => setPasswordData({ ...passwordData, newPassword: value })}
                    type="password"
                  />
                  <FormInput
                    label="Confirm New Password"
                    value={passwordData.confirmPassword}
                    onChange={(value) => setPasswordData({ ...passwordData, confirmPassword: value })}
                    type="password"
                  />

                  <button
                    onClick={handleChangePassword}
                    disabled={isSaving}
                    className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                  >
                    <Save size={18} />
                    {isSaving ? 'Updating...' : 'Update Password'}
                  </button>
                </div>

                <div className="p-6 bg-gray-50 rounded-lg">
                  <h3 className="text-lg font-medium text-gray-900 mb-4">
                    Two-Factor Authentication
                  </h3>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-gray-600">
                        {preferences.twoFactorEnabled ? 'Enabled' : 'Disabled'}
                      </p>
                    </div>
                    <button className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
                      {preferences.twoFactorEnabled ? 'Disable' : 'Enable'}
                    </button>
                  </div>
                </div>
              </div>
            )}

            {/* Appearance Tab */}
            {activeTab === 'appearance' && (
              <div className="space-y-4">
                <FormSelect
                  label="Theme"
                  value={preferences.theme}
                  onChange={(value) => handlePreferencesChange('theme', value as 'light' | 'dark' | 'auto')}
                  options={[
                    { value: 'light', label: 'Light' },
                    { value: 'dark', label: 'Dark' },
                    { value: 'auto', label: 'Auto (System)' },
                  ]}
                />
                <FormSelect
                  label="Time Format"
                  value={preferences.timeFormat}
                  onChange={(value) => handlePreferencesChange('timeFormat', value as '12h' | '24h')}
                  options={[
                    { value: '12h', label: '12-hour' },
                    { value: '24h', label: '24-hour' },
                  ]}
                />
                <FormSelect
                  label="Language"
                  value={preferences.language}
                  onChange={(value) => handlePreferencesChange('language', value)}
                  options={[
                    { value: 'en', label: 'English' },
                    { value: 'es', label: 'Spanish' },
                    { value: 'fr', label: 'French' },
                  ]}
                />
                <FormSelect
                  label="Timezone"
                  value={preferences.timezone}
                  onChange={(value) => handlePreferencesChange('timezone', value)}
                  options={[
                    { value: 'EST', label: 'Eastern Standard Time' },
                    { value: 'CST', label: 'Central Standard Time' },
                    { value: 'MST', label: 'Mountain Standard Time' },
                    { value: 'PST', label: 'Pacific Standard Time' },
                  ]}
                />

                <div className="flex justify-end">
                  <button
                    onClick={handleSavePreferences}
                    disabled={isSaving}
                    className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                  >
                    <Save size={18} />
                    {isSaving ? 'Saving...' : 'Save Changes'}
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

/**
 * Form Input Component
 */
interface FormInputProps {
  label: string
  value: string
  onChange: (value: string) => void
  type?: string
  disabled?: boolean
}

const FormInput: React.FC<FormInputProps> = ({ label, value, onChange, type = 'text', disabled = false }) => (
  <div>
    <label className="block text-sm font-medium text-gray-700 mb-2">{label}</label>
    <input
      type={type}
      value={value}
      onChange={(e) => onChange(e.target.value)}
      disabled={disabled}
      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
    />
  </div>
)

/**
 * Form Select Component
 */
interface FormSelectProps {
  label: string
  value: string
  onChange: (value: string) => void
  options: { value: string; label: string }[]
}

const FormSelect: React.FC<FormSelectProps> = ({ label, value, onChange, options }) => (
  <div>
    <label className="block text-sm font-medium text-gray-700 mb-2">{label}</label>
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
    >
      {options.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </select>
  </div>
)

export default UserPreferencesPage
