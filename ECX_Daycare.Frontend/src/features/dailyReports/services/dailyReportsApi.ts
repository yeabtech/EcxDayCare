/**
 * Daily Reports API service
 */
import { apiClient } from '@services/api'
import type {
  DailyReport,
  ChildDailyReports,
  DailyLogsSummary,
  CreateDailyReportRequest,
  UpdateDailyReportRequest,
} from '../types'

const BASE = '/dailyreports'

function normalizeReport(r: any): DailyReport {
  if (!r) return r
  return {
    id: r.Id ?? r.id ?? '',
    childId: r.ChildId ?? r.childId ?? '',
    childName: r.ChildName ?? r.childName ?? null,
    date: r.Date ?? r.date ?? '',
    mood: r.Mood ?? r.mood ?? null,
    mealsSummary: r.MealsSummary ?? r.mealsSummary ?? null,
    napSummary: r.NapSummary ?? r.napSummary ?? null,
    activitiesSummary: r.ActivitiesSummary ?? r.activitiesSummary ?? null,
    diaperSummary: r.DiaperSummary ?? r.diaperSummary ?? null,
    milestones: r.Milestones ?? r.milestones ?? null,
    concerns: r.Concerns ?? r.concerns ?? null,
    staffId: r.StaffId ?? r.staffId ?? '',
    sentToParent: r.SentToParent ?? r.sentToParent ?? false,
    sentAt: r.SentAt ?? r.sentAt ?? null,
    createdAt: r.CreatedAt ?? r.createdAt ?? '',
    updatedAt: r.UpdatedAt ?? r.updatedAt ?? '',
  }
}

function normalizeChildReports(d: any): ChildDailyReports {
  return {
    childId: d.ChildId ?? d.childId ?? '',
    childName: d.ChildName ?? d.childName ?? null,
    totalCount: d.TotalCount ?? d.totalCount ?? 0,
    page: d.Page ?? d.page ?? 1,
    pageSize: d.PageSize ?? d.pageSize ?? 30,
    items: (d.Items ?? d.items ?? []).map(normalizeReport),
  }
}

export const dailyReportsApi = {
  async getDailyLogs(childId: string, date: string): Promise<DailyLogsSummary> {
    const response = await apiClient.get<any>(`/dailyreports/logs/child/${childId}`, { params: { date } })
    const data = response.data?.data ?? response.data
    return {
      childId: data.ChildId ?? data.childId,
      childName: data.ChildName ?? data.childName ?? null,
      date: data.Date ?? data.date ?? date,
      feedings: (data.Feedings ?? data.feedings ?? []).map((item: any) => ({
        id: item.Id ?? item.id ?? '', time: item.Time ?? item.time ?? '',
        feedingTypeName: item.FeedingTypeName ?? item.feedingTypeName,
        foodItems: item.FoodItems ?? item.foodItems ?? null, amount: item.Amount ?? item.amount ?? null,
        portionConsumedName: item.PortionConsumedName ?? item.portionConsumedName,
        notes: item.Notes ?? item.notes ?? null,
      })),
      naps: (data.Naps ?? data.naps ?? []).map((item: any) => ({
        id: item.Id ?? item.id ?? '', startTime: item.StartTime ?? item.startTime ?? '',
        endTime: item.EndTime ?? item.endTime ?? null, durationMinutes: item.DurationMinutes ?? item.durationMinutes ?? null,
        qualityName: item.QualityName ?? item.qualityName, notes: item.Notes ?? item.notes ?? null,
      })),
      activities: (data.Activities ?? data.activities ?? []).map((item: any) => ({
        id: item.Id ?? item.id ?? '', time: item.Time ?? item.time ?? '', title: item.Title ?? item.title ?? '',
        durationMinutes: item.DurationMinutes ?? item.durationMinutes ?? null,
        activityTypeName: item.ActivityTypeName ?? item.activityTypeName,
        participationName: item.ParticipationName ?? item.participationName,
        description: item.Description ?? item.description ?? null,
      })),
      suggestedMealsSummary: data.SuggestedMealsSummary ?? data.suggestedMealsSummary ?? '',
      suggestedNapSummary: data.SuggestedNapSummary ?? data.suggestedNapSummary ?? '',
      suggestedActivitiesSummary: data.SuggestedActivitiesSummary ?? data.suggestedActivitiesSummary ?? '',
    }
  },
  /** GET /api/dailyreports/{id} */
  async getById(id: string): Promise<DailyReport> {
    const res = await apiClient.get<any>(`${BASE}/${id}`)
    const payload = res.data?.data ?? res.data
    return normalizeReport(payload)
  },

  /** GET /api/dailyreports/child/{childId} */
  async getByChild(
    childId: string,
    params?: { startDate?: string; endDate?: string; page?: number; pageSize?: number },
  ): Promise<ChildDailyReports> {
    const query: Record<string, any> = {
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 30,
    }
    if (params?.startDate) query.startDate = params.startDate
    if (params?.endDate) query.endDate = params.endDate
    const res = await apiClient.get<any>(`${BASE}/child/${childId}`, { params: query })
    const payload = res.data?.data ?? res.data
    return normalizeChildReports(payload)
  },

  /** GET /api/dailyreports/daily?date=... */
  async getDailyFeed(date?: string): Promise<DailyReport[]> {
    const query: Record<string, any> = {}
    if (date) query.date = date
    const res = await apiClient.get<any>(`${BASE}/daily`, { params: query })
    const payload = res.data?.data ?? res.data
    return (Array.isArray(payload) ? payload : []).map(normalizeReport)
  },

  /** POST /api/dailyreports */
  async create(data: CreateDailyReportRequest): Promise<DailyReport> {
    const res = await apiClient.post<any>(BASE, {
      childId: data.childId,
      date: data.date,
      mood: data.mood ?? null,
      mealsSummary: data.mealsSummary ?? null,
      napSummary: data.napSummary ?? null,
      activitiesSummary: data.activitiesSummary ?? null,
      diaperSummary: data.diaperSummary ?? null,
      milestones: data.milestones ?? null,
      concerns: data.concerns ?? null,
    })
    const payload = res.data?.data ?? res.data
    return normalizeReport(payload)
  },

  /** PUT /api/dailyreports/{id} */
  async update(id: string, data: UpdateDailyReportRequest): Promise<DailyReport> {
    const res = await apiClient.put<any>(`${BASE}/${id}`, {
      mood: data.mood ?? null,
      mealsSummary: data.mealsSummary ?? null,
      napSummary: data.napSummary ?? null,
      activitiesSummary: data.activitiesSummary ?? null,
      diaperSummary: data.diaperSummary ?? null,
      milestones: data.milestones ?? null,
      concerns: data.concerns ?? null,
    })
    const payload = res.data?.data ?? res.data
    return normalizeReport(payload)
  },

  /** POST /api/dailyreports/{id}/send */
  async sendToParent(id: string): Promise<DailyReport> {
    const res = await apiClient.post<any>(`${BASE}/${id}/send`)
    const payload = res.data?.data ?? res.data
    return normalizeReport(payload)
  },

  /** DELETE /api/dailyreports/{id} */
  async delete(id: string): Promise<void> {
    await apiClient.delete(`${BASE}/${id}`)
  },
}
