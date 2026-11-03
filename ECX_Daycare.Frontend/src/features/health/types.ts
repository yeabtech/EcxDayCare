export interface ChildOption {
  id: string
  name: string
}

export interface HealthLog {
  id: string
  childId: string
  childName: string | null
  time: string
  eventType: number
  eventTypeName: string
  description: string | null
  staffId: string
  notes: string | null
  createdAt: string
  updatedAt: string | null
}

export interface IncidentRecord {
  id: string
  childId: string
  childName: string | null
  incidentTime: string
  location: string
  description: string
  injuryType: number
  injuryTypeName: string
  affectedBodyPart: number
  affectedBodyPartName: string
  firstAidGiven: string | null
  staffId: string
  parentNotified: boolean
  parentSignature: string | null
  followUp: string | null
  notes: string | null
  createdAt: string
  updatedAt: string | null
}
