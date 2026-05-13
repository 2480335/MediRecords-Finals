export type AllergySeverity = 'Mild' | 'Moderate' | 'Severe' | 'Life-threatening';

export interface AllergyCreateRequest {
  allergen: string;
  reaction?: string | null;
  severity?: AllergySeverity | string | null;
}

export const SEVERITY_OPTIONS: { value: AllergySeverity; label: string; tone: string }[] = [
  { value: 'Mild', label: 'Mild', tone: 'success' },
  { value: 'Moderate', label: 'Moderate', tone: 'warning' },
  { value: 'Severe', label: 'Severe', tone: 'danger' },
  { value: 'Life-threatening', label: 'Life-threatening', tone: 'critical' }
];
