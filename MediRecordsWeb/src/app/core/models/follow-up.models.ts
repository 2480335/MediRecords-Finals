export interface FollowUpCreateRequest {
  recommendedDate: string;
  notes: string;
}

export interface FollowUpCreateResponse {
  followupId: number;
}

export interface FollowUpDetails {
  followupId: number;
  encounterId: number;
  patientId: number;
  patientName: string;
  recommendedDate: string;
  notes: string;
  createdDate: string;
}

export interface FollowUpFilter {
  patientId?: number | null;
  encounterId?: number | null;
}
