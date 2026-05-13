export interface MedicalHistoryCreateRequest {
  condition: string;
  notes?: string | null;
}

export const COMMON_CONDITIONS: string[] = [
  'Hypertension',
  'Type 2 Diabetes',
  'Asthma',
  'Hyperlipidemia',
  'Coronary Artery Disease',
  'COPD',
  'Hypothyroidism',
  'Migraine',
  'GERD',
  'Anxiety',
  'Depression',
  'Osteoarthritis'
];
