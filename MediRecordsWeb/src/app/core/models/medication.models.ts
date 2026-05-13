export interface MedicationFilter {
  patientId?: number | null;
  medId?: number | null;
  patientName?: string | null;
  drugName?: string | null;
  dose?: string | null;
  frequency?: string | null;
  route?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  status?: 'Active' | 'Inactive' | null;
}

export interface MedicationListItem {
  medId: number;
  patientId: number;
  patientName?: string | null;
  drugName: string;
  dose?: string | null;
  frequency?: string | null;
  route?: string | null;
  startDate: string;
  endDate: string;
  status: string;
}
