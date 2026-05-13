export type PatientStatusName = 'Inactive' | 'Active' | 'Deceased';

export const PATIENT_STATUS_VALUE: Record<PatientStatusName, number> = {
  Inactive: 0,
  Active: 1,
  Deceased: 2
};

export const PATIENT_STATUSES: { value: PatientStatusName; label: string; tone: string; icon: string }[] = [
  { value: 'Active', label: 'Active', tone: 'success', icon: 'bi-circle-fill' },
  { value: 'Inactive', label: 'Inactive', tone: 'muted', icon: 'bi-pause-circle' },
  { value: 'Deceased', label: 'Deceased', tone: 'danger', icon: 'bi-x-octagon' }
];

export type GenderOption = 'Male' | 'Female' | 'Other' | 'Prefer not to say';

export const GENDER_OPTIONS: GenderOption[] = ['Male', 'Female', 'Other', 'Prefer not to say'];

export interface PatientLookup {
  patientId: number;
  name: string;
  mrn: string;
  dob: string;
  gender?: string | null;
  phoneNo: string;
  status: string;
}

export interface PatientCreateRequest {
  name: string;
  dob: string;          // backend expects DateOnly, ISO date string YYYY-MM-DD works
  gender: string;
  phoneNo: string;
  contactInfo?: string | null;
  addressJSON?: string | null;
  primaryProviderId?: number | null;
}

export interface PatientCreateResponse {
  patientId: number;
}

export interface PatientUpdateRequest {
  name: string;
  dob: string;
  gender?: string | null;
  phoneNo: string;
  addressJSON?: string | null;
  primaryProviderId?: number | null;
  status: number; // PatientStatus enum int
}

export interface PatientDetails {
  patientId: number;
  mrn: string;
  name: string;
  dob: string;          // DateOnly serialized as ISO date
  gender?: string | null;
  phoneNo: string;
  addressJSON?: string | null;
  status: string;
  problems: string[];
  allergies: string[];
  medicalHistory: string[];
}

export interface PatientAddress {
  line1?: string;
  line2?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
}

export function parseAddress(json: string | null | undefined): PatientAddress | null {
  if (!json) return null;
  try {
    const parsed = JSON.parse(json);
    if (parsed && typeof parsed === 'object') return parsed as PatientAddress;
  } catch { /* fall through */ }
  return null;
}

export function formatAddress(addr: PatientAddress | null): string {
  if (!addr) return '';
  const parts = [
    addr.line1,
    addr.line2,
    [addr.city, addr.state].filter(Boolean).join(', '),
    addr.postalCode,
    addr.country
  ].filter(Boolean);
  return parts.join(' · ');
}
