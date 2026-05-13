export interface ImmunizationCreateRequest {
  vaccine: string;
  dose: string;
  givenDate: string;
  status: boolean; // false = Pending, true = Administered
}

export interface ImmunizationCreateResponse {
  immunizationId: number;
}

export interface ImmunizationDetails {
  immunizationId: number;
  patientId: number;
  patientName: string;
  vaccine: string;
  dose: string;
  givenDate: string;
  status: boolean;
}

export interface ImmunizationFilter {
  patientId?: number | null;
  patientName?: string | null;
  vaccine?: string | null;
  status?: boolean | null;
}

export const COMMON_VACCINES: string[] = [
  'COVID-19',
  'Influenza',
  'Tdap',
  'MMR',
  'Hepatitis B',
  'HPV',
  'Pneumococcal',
  'Varicella',
  'Polio (IPV)',
  'Meningococcal'
];
