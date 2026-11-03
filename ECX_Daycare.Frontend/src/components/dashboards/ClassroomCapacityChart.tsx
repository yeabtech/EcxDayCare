/**
 * ClassroomCapacityChart Component
 * Displays classroom capacity distribution using donut-style visualization
 */
import React from 'react'
import type { ClassroomUtilization } from '@features/dashboards/hooks'

export interface ClassroomCapacityChartProps {
  data: ClassroomUtilization[]
  isLoading?: boolean
  title?: string
}

export const ClassroomCapacityChart: React.FC<ClassroomCapacityChartProps> = ({
  data,
  isLoading = false,
  title = 'Classroom Capacity Distribution',
}) => {
  if (isLoading)
    return (
      <div className="bg-gray-50 rounded-lg p-6 flex items-center justify-center min-h-64">
        <p className="text-gray-500">Loading chart...</p>
      </div>
    )

  if (!data.length)
    return (
      <div className="bg-gray-50 rounded-lg p-6 flex items-center justify-center min-h-64">
        <p className="text-gray-500">No data available</p>
      </div>
    )

  const getColorByOccupancy = (rate: number): string => {
    if (rate >= 90) return 'bg-red-100 text-red-600'
    if (rate >= 75) return 'bg-yellow-100 text-yellow-600'
    return 'bg-green-100 text-green-600'
  }

  const sortedByOccupancy = [...data].sort((a, b) => b.occupancyRate - a.occupancyRate)

  return (
    <div className="space-y-4">
      <h3 className="text-sm font-medium text-gray-900">{title}</h3>
      <div className="space-y-3">
        {sortedByOccupancy.map((classroom) => {
          const rate = Math.round(classroom.occupancyRate)
          const emptySeats = classroom.capacity - classroom.enrolled
          return (
            <div key={classroom.classroomId} className="space-y-1">
              {/* Classroom name and stats */}
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-900">{classroom.classroomName}</span>
                <span className={`text-xs font-semibold px-2 py-1 rounded ${getColorByOccupancy(rate)}`}>
                  {rate}% • {classroom.enrolled}/{classroom.capacity}
                </span>
              </div>

              {/* Capacity bar */}
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full ${
                    rate >= 90
                      ? 'bg-red-500'
                      : rate >= 75
                        ? 'bg-yellow-500'
                        : 'bg-green-500'
                  }`}
                  style={{ width: `${Math.min(rate, 100)}%` }}
                />
              </div>

              {/* Details */}
              <div className="flex justify-between text-xs text-gray-600">
                <span>{classroom.present} present today</span>
                <span>{emptySeats} empty seats</span>
              </div>
            </div>
          )
        })}
      </div>

      {/* Capacity status legend */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mt-4">
        <p className="text-xs font-medium text-gray-900 mb-2">Capacity Status</p>
        <div className="grid grid-cols-3 gap-2 text-xs">
          <div className="flex items-center space-x-1">
            <div className="w-2 h-2 bg-green-500 rounded-full"></div>
            <span className="text-gray-600">Good (&lt;75%)</span>
          </div>
          <div className="flex items-center space-x-1">
            <div className="w-2 h-2 bg-yellow-500 rounded-full"></div>
            <span className="text-gray-600">Fair (75-89%)</span>
          </div>
          <div className="flex items-center space-x-1">
            <div className="w-2 h-2 bg-red-500 rounded-full"></div>
            <span className="text-gray-600">Full (&ge;90%)</span>
          </div>
        </div>
      </div>
    </div>
  )
}

export default ClassroomCapacityChart
