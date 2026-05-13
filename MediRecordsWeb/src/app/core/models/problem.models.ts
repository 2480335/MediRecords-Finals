export type ProblemStatus = 'Active' | 'Resolved' | 'Chronic' | 'Discontinued';

export const PROBLEM_STATUSES: { value: ProblemStatus; label: string; tone: string; icon: string }[] = [
  { value: 'Active', label: 'Active', tone: 'warn', icon: 'bi-exclamation-circle' },
  { value: 'Chronic', label: 'Chronic', tone: 'danger', icon: 'bi-arrow-repeat' },
  { value: 'Resolved', label: 'Resolved', tone: 'success', icon: 'bi-check2-circle' },
  { value: 'Discontinued', label: 'Discontinued', tone: 'muted', icon: 'bi-x-octagon' }
];

export interface ProblemCreateRequest {
  diagnosis: string;
  startDate: string;
  status: ProblemStatus;
  endDate?: string | null;
}

export const COMMON_DIAGNOSES: string[] = [
  'Hypertension',
  'Type 2 Diabetes',
  'Asthma',
  'COPD',
  'Hyperlipidemia',
  'Coronary Artery Disease',
  'Atrial Fibrillation',
  'Migraine',
  'GERD',
  'Hypothyroidism',
  'Anxiety',
  'Depression',
  'Osteoarthritis',
  'Anemia'
];
