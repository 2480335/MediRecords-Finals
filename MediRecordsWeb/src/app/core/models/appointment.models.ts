export type AppointmentStatusName =
  | 'Booked'
  | 'CheckedIn'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow';

export const APPOINTMENT_STATUSES: { value: AppointmentStatusName; label: string; tone: string }[] = [
  { value: 'Booked', label: 'Booked', tone: 'info' },
  { value: 'CheckedIn', label: 'Checked-in', tone: 'primary' },
  { value: 'Completed', label: 'Completed', tone: 'success' },
  { value: 'Cancelled', label: 'Cancelled', tone: 'muted' },
  { value: 'NoShow', label: 'No show', tone: 'danger' }
];

export interface AppointmentRequest {
  patientId: number;
  providerId: number;
  dateTime: string;
  reason?: string | null;
}

export interface AppointmentResponse {
  appointmentId: number;
  patientId: number;
  providerId: number;
  dateTime: string;
  reason?: string | null;
  status: AppointmentStatusName | number;
  encounterId?: number | null;
  message?: string;
}

export interface AppointmentUpdateRequest {
  patientId?: number | null;
  providerId?: number | null;
  dateTime?: string | null;
  reason?: string | null;
}

export interface AppointmentFilter {
  id?: number | null;
  patientId?: number | null;
  providerId?: number | null;
  date?: string | null;
}
