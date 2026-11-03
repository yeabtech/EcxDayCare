/**
 * AttendanceTrendChart Component
 * Displays attendance trend over time using simple bar chart
 */
import React from 'react'
import type { AttendanceDay } from '@features/dashboards/hooks'

export interface AttendanceTrendChartProps {
  data: AttendanceDay[]
  isLoading?: boolean
  title?: string
  height?: string
}

export const AttendanceTrendChart: React.FC<AttendanceTrendChartProps> = ({
  data,
  isLoading = false,
  title = 'Attendance Trend (7 Days)',
  height = '300px',
}) => {
  if (isLoading)
    return (
      <div
        className="bg-gray-50 rounded-lg flex items-center justify-center"
        style={{ height }}
      >
        <p className="text-gray-500">Loading chart...</p>
      </div>
    )

  if (!data.length)
    return (
      <div
        className="bg-gray-50 rounded-lg flex items-center justify-center"
        style={{ height }}
      >
        <p className="text-gray-500">No data available</p>
      </div>
    )

  // Find min and max for scaling
  const maxValue = Math.max(...data.map((d) => d.totalCount))
  const chartHeight = 150

  return (
    <div className="space-y-4">
      <h3 className="text-sm font-medium text-gray-900">{title}</h3>
      <div className="flex items-end justify-between space-x-2" style={{ minHeight: height }}>
        {data.map((day, index) => (
          <div key={index} className="flex-1 flex flex-col items-center space-y-2">
            {/* Bars */}
            <div className="flex items-end space-x-1 h-40">
              {/* Present bar */}
              <div
                className="bg-green-500 rounded-t"
                style={{
                  width: '50%',
                  height: `${(day.presentCount / maxValue) * chartHeight}px`,
                }}
                title={`Present: ${day.presentCount}`}
              />
              {/* Absent bar */}
              <div
                className="bg-red-500 rounded-t"
                style={{
                  width: '50%',
                  height: `${(day.absentCount / maxValue) * chartHeight}px`,
                }}
                title={`Absent: ${day.absentCount}`}
              />
            </div>
            {/* Date label */}
            <span className="text-xs text-gray-600 text-center">
              {new Date(day.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
            </span>
            {/* Rate label */}
            <span className="text-xs font-semibold text-gray-900">{day.rate}%</span>
          </div>
        ))}
      </div>

      {/* Legend */}
      <div className="flex justify-center space-x-4 text-xs">
        <div className="flex items-center space-x-2">
          <div className="w-3 h-3 bg-green-500 rounded"></div>
          <span className="text-gray-600">Present</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-3 h-3 bg-red-500 rounded"></div>
          <span className="text-gray-600">Absent</span>
        </div>
      </div>
    </div>
  )
}

export default AttendanceTrendChart
