export type PrescriptionStatusName = 'Draft' | 'Issued';

export const PRESCRIPTION_STATUSES: { value: PrescriptionStatusName; label: string; tone: string; icon: string }[] = [
  { value: 'Draft', label: 'Draft', tone: 'muted', icon: 'bi-pencil' },
  { value: 'Issued', label: 'Issued', tone: 'success', icon: 'bi-check2-circle' }
];

export interface PrescriptionItem {
  drugName: string;
  dose?: string | null;
  frequency?: string | null;
  route?: string | null;
  durationDays: number;
  instructions?: string | null;
}

export interface PrescriptionItemResponse extends PrescriptionItem {
  itemId: number;
  prescriptionId: number;
}

export interface PrescriptionWithItemsCreateRequest {
  encounterId: number;
  providerId: number;
  status: PrescriptionStatusName;
  prescriptionItems: PrescriptionItem[];
}

export interface PrescriptionWithItemsUpdateRequest {
  providerId: number;
  status: PrescriptionStatusName;
  prescriptionItems: PrescriptionItem[];
}

export interface PrescriptionWithItems {
  prescriptionId: number;
  encounterId: number;
  providerId: number;
  createdDate: string;
  status: PrescriptionStatusName | string;
  prescriptionItems: PrescriptionItemResponse[];
}

export const COMMON_ROUTES = ['PO', 'IV', 'IM', 'SC', 'Topical', 'Inhaled', 'PR', 'SL'];

export const COMMON_FREQUENCIES = [
  'Once daily',
  'Twice daily',
  'Three times daily',
  'Four times daily',
  'Every 4 hours',
  'Every 6 hours',
  'Every 8 hours',
  'As needed (PRN)',
  'At bedtime'
];
