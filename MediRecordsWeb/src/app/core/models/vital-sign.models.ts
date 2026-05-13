export interface VitalSignCaptureRequest {
  encounterId: number;
  bp?: string | null;
  hr?: string | null;
  temp?: string | null;
  spO2?: string | null;
  height?: number | null;   // cm
  weight?: number | null;   // kg
}

export interface VitalSignCaptureResponse {
  vitalId: number;
  bmi?: number | null;
}

/**
 * BMI classification per WHO:
 *   <18.5  Underweight
 *   18.5–24.9  Normal
 *   25–29.9  Overweight
 *   ≥30  Obese
 */
export interface BmiCategory {
  label: string;
  tone: 'info' | 'success' | 'warn' | 'danger';
}

export function classifyBmi(bmi: number | null | undefined): BmiCategory | null {
  if (bmi == null || !Number.isFinite(bmi)) return null;
  if (bmi < 18.5) return { label: 'Underweight', tone: 'info' };
  if (bmi < 25) return { label: 'Normal', tone: 'success' };
  if (bmi < 30) return { label: 'Overweight', tone: 'warn' };
  return { label: 'Obese', tone: 'danger' };
}
