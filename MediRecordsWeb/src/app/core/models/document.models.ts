export type DocType = 'Referral' | 'Consent' | 'Report' | 'Photo';

export const DOC_TYPES: { value: DocType; label: string; icon: string; tone: string }[] = [
  { value: 'Referral', label: 'Referral', icon: 'bi-arrow-right-circle', tone: 'primary' },
  { value: 'Consent', label: 'Consent form', icon: 'bi-file-check', tone: 'violet' },
  { value: 'Report', label: 'Report', icon: 'bi-clipboard-data', tone: 'success' },
  { value: 'Photo', label: 'Photo', icon: 'bi-camera', tone: 'pink' }
];

export interface DocumentUploadResponse {
  documentId: number;
  patientId: number;
  encounterId: number;
  docType: string;
  fileName: string;
  fileType: string;
  uploadedBy: string;
  uploadedDate: string;
  status: string;
  message: string;
}

export interface DocumentListItem {
  documentId: number;
  patientId: number;
  encounterId: number;
  docType: string;
  fileName: string;
  uploadedBy: string;
  uploadedDate: string;
  status: string;
}

export interface DocumentListFilter {
  patientId?: number | null;
  encounterId?: number | null;
  uploadedDate?: string | null;
}

export interface DocumentDownloadFile {
  blob: Blob;
  fileName: string;
}
