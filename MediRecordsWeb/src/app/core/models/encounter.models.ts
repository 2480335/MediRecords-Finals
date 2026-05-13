export type EncounterStatusName = 'Open' | 'Closed' | 'Locked';

export const ENCOUNTER_STATUS_VALUE: Record<EncounterStatusName, number> = {
  Open: 1,
  Closed: 2,
  Locked: 3
};

export const ENCOUNTER_STATUS_NAME: Record<number, EncounterStatusName> = {
  1: 'Open',
  2: 'Closed',
  3: 'Locked'
};

export const ENCOUNTER_STATUSES: { value: EncounterStatusName; label: string; tone: string; icon: string }[] = [
  { value: 'Open', label: 'Open', tone: 'success', icon: 'bi-circle' },
  { value: 'Closed', label: 'Closed', tone: 'muted', icon: 'bi-check2-circle' },
  { value: 'Locked', label: 'Locked', tone: 'danger', icon: 'bi-lock-fill' }
];

export interface EncounterSummary {
  encounterId: number;
  patientName: string;
  visitType: string;
  status: string;
  date: string;
}

export interface EncounterDetail {
  encounterId: number;
  patientId: number;
  patientName: string;
  providerId: number;
  providerName: string;
  visitType: string;
  status: string;
  date: string;
}

export interface EncounterLookup {
  encounterId: number;
  patientId: number;
  patientName: string;
  date: string;
  status: string;
}

export interface EncounterStatusUpdateRequest {
  status: EncounterStatusName | number;
}

export interface EncounterStatusResponse {
  encounterId: number;
  status: string;
  message: string;
}
