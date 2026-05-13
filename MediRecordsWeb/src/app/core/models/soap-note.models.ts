export interface SOAPNoteSaveRequest {
  encounterId: number;
  hpi: string;
  ros?: string | null;
  examFindings?: string | null;
  observations?: string | null;
  assessment?: string | null;
  plan?: string | null;
  isDraft: boolean;
}

export interface SOAPNoteResponse {
  noteId: number;
  encounterId: number;
  hpi: string;
  ros?: string | null;
  examFindings?: string | null;
  observations?: string | null;
  assessment?: string | null;
  plan?: string | null;
  status: string;
  createdDate: string;
}
