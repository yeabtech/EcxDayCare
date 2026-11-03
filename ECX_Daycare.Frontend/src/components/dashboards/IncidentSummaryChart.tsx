/**
 * IncidentSummaryChart Component
 * Displays incident distribution by severity and type
 */
import React from 'react'
import type { RecentIncident } from '@features/dashboards/hooks'
import { AlertCircle, AlertTriangle } from 'lucide-react'

export interface IncidentSummaryChartProps {
  data: RecentIncident[]
  isLoading?: boolean
  title?: string
}

export const IncidentSummaryChart: React.FC<IncidentSummaryChartProps> = ({
  data,
  isLoading = false,
  title = 'Incident Summary',
}) => {
  if (isLoading)
    return (
      <div className="bg-gray-50 rounded-lg p-6 flex items-center justify-center min-h-48">
        <p className="text-gray-500">Loading chart...</p>
      </div>
    )

  // Calculate statistics
  const bySeverity = {
    high: data.filter((i) => i.severity === 'high').length,
    medium: data.filter((i) => i.severity === 'medium').length,
    low: data.filter((i) => i.severity === 'low').length,
  }

  const byType = {
    injury: data.filter((i) => i.type === 'injury').length,
    behavior: data.filter((i) => i.type === 'behavior').length,
    health: data.filter((i) => i.type === 'health').length,
    allergy: data.filter((i) => i.type === 'allergy').length,
    other: data.filter((i) => i.type === 'other').length,
  }

  const openCount = data.filter((i) => i.status === 'open').length
  const closedCount = data.filter((i) => i.status === 'closed').length

  return (
    <div className="space-y-6">
      <h3 className="text-sm font-medium text-gray-900">{title}</h3>

      {data.length === 0 ? (
        <div className="text-center py-8">
          <AlertCircle size={32} className="text-gray-400 mx-auto mb-2" />
          <p className="text-gray-500">No incidents recorded</p>
        </div>
      ) : (
        <>
          {/* Status Overview */}
          <div className="grid grid-cols-2 gap-4">
            <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
              <div className="flex items-center space-x-3">
                <div className="bg-yellow-100 rounded-full p-2">
                  <AlertTriangle size={20} className="text-yellow-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-600">Open Incidents</p>
                  <p className="text-2xl font-bold text-gray-900">{openCount}</p>
                </div>
              </div>
            </div>
            <div className="bg-green-50 border border-green-200 rounded-lg p-4">
              <div className="flex items-center space-x-3">
                <div className="bg-green-100 rounded-full p-2">
                  <AlertCircle size={20} className="text-green-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-600">Resolved</p>
                  <p className="text-2xl font-bold text-gray-900">{closedCount}</p>
                </div>
              </div>
            </div>
          </div>

          {/* Severity Distribution */}
          <div className="space-y-3">
            <p className="text-sm font-medium text-gray-900">By Severity</p>
            {[
              { severity: 'high', count: bySeverity.high, textColor: 'text-red-600', label: 'High', bgColor: 'bg-red-500' },
              { severity: 'medium', count: bySeverity.medium, textColor: 'text-yellow-600', label: 'Medium', bgColor: 'bg-yellow-500' },
              { severity: 'low', count: bySeverity.low, textColor: 'text-blue-600', label: 'Low', bgColor: 'bg-blue-500' },
            ].map(({ severity, count, textColor, label, bgColor }) => (
              <div key={severity} className="flex items-center space-x-3">
                <span className="text-sm text-gray-600 w-12">{label}</span>
                <div className="flex-1 bg-gray-200 rounded-full h-2">
                  <div
                    className={`h-2 rounded-full ${bgColor}`}
                    style={{ width: `${data.length > 0 ? (count / data.length) * 100 : 0}%` }}
                  />
                </div>
                <span className={`text-sm font-semibold ${textColor} w-8 text-right`}>{count}</span>
              </div>
            ))}
          </div>

          {/* Type Distribution */}
          <div className="space-y-3">
            <p className="text-sm font-medium text-gray-900">By Type</p>
            <div className="grid grid-cols-5 gap-2">
              {[
                { type: 'Injury', count: byType.injury },
                { type: 'Behavior', count: byType.behavior },
                { type: 'Health', count: byType.health },
                { type: 'Allergy', count: byType.allergy },
                { type: 'Other', count: byType.other },
              ].map(({ type, count }) => (
                <div key={type} className="text-center">
                  <div className="bg-gray-100 rounded-lg p-3 mb-2">
                    <p className="text-lg font-bold text-gray-900">{count}</p>
                  </div>
                  <p className="text-xs text-gray-600">{type}</p>
                </div>
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  )
}

export default IncidentSummaryChart
