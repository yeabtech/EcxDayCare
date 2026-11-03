/**
 * Daily Reports feature types
 */

export interface DailyReport {
  id: string
  childId: string
  childName: string | null
  date: string
  mood: string | null
  mealsSummary: string | null
  napSummary: string | null
  activitiesSummary: string | null
  diaperSummary: string | null
  milestones: string | null
  concerns: string | null
  staffId: string
  sentToParent: boolean
  sentAt: string | null
  createdAt: string
  updatedAt: string
}

export interface ChildDailyReports {
  childId: string
  childName: string | null
  totalCount: number
  page: number
  pageSize: number
  items: DailyReport[]
}

export interface DailyLogsSummary {
  childId: string
  childName: string | null
  date: string
  feedings: Array<{
    id: string; time: string; feedingTypeName?: string; foodItems?: string | null
    amount?: string | null; portionConsumedName?: string; notes?: string | null
  }>
  naps: Array<{
    id: string; startTime: string; endTime?: string | null; durationMinutes?: number | null
    qualityName?: string; notes?: string | null
  }>
  activities: Array<{
    id: string; time: string; title: string; durationMinutes?: number | null
    activityTypeName?: string; participationName?: string; description?: string | null
  }>
  suggestedMealsSummary: string
  suggestedNapSummary: string
  suggestedActivitiesSummary: string
}

export interface CreateDailyReportRequest {
  childId: string
  date: string
  mood?: string
  mealsSummary?: string
  napSummary?: string
  activitiesSummary?: string
  diaperSummary?: string
  milestones?: string
  concerns?: string
}

export interface UpdateDailyReportRequest {
  mood?: string
  mealsSummary?: string
  napSummary?: string
  activitiesSummary?: string
  diaperSummary?: string
  milestones?: string
  concerns?: string
}
