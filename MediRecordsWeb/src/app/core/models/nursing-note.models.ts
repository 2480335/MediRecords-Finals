export interface NursingNoteCreateRequest {
  notes: string;
}

export interface NursingNoteUpdateRequest {
  notes: string;
}

export interface NursingNoteResponse {
  nursingNoteId: number;
  encounterId: number;
  notes: string;
  recordedBy: string;
  recordedDate: string;
}
