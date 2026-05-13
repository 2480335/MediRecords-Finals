export type ImagingStudyType = 'Xray' | 'CT' | 'MRI' | 'UltraSound';

export const IMAGING_STUDY_TYPES: { value: ImagingStudyType; label: string; icon: string; tone: string }[] = [
  { value: 'Xray', label: 'X-ray', icon: 'bi-radioactive', tone: 'primary' },
  { value: 'CT', label: 'CT scan', icon: 'bi-bullseye', tone: 'violet' },
  { value: 'MRI', label: 'MRI', icon: 'bi-soundwave', tone: 'pink' },
  { value: 'UltraSound', label: 'Ultrasound', icon: 'bi-mic', tone: 'success' }
];

export interface ImagingOrderCreateRequest {
  studyType: ImagingStudyType;
  notes: string;
}

export interface ImagingReportShort {
  reportId: number;
  imagingOrderId: number;
  findings?: string;
  impression: string;
  reportDate: string;
  status: boolean;
}

export interface ImagingOrderResponse {
  imagingOrderId: number;
  encounterId: number;
  studyType: ImagingStudyType | null;
  notes: string;
  orderedDate: string;
  status: boolean;
  reports: ImagingReportShort[];
}

export interface ImagingOrderFilter {
  imagingOrderID?: number | null;
  encounterID?: number | null;
  studyType?: ImagingStudyType | null;
  notes?: string | null;
  orderedDate?: string | null;
  status?: boolean | null;
}

export interface ImagingReportFull {
  reportId: number;
  imagingOrderId: number;
  findings: string;
  impression: string;
  reportDate: string;
  status: string;
}

export interface ImagingReportUploadInput {
  imagingOrderId: number;
  findings: string;
  impression: string;
  reportFile?: File | null;
}
