export interface LabOrderCreateRequest {
  encounterId: number;
  testJson?: string | null;
  orderDate?: string | null;
}

export interface LabOrderResponse {
  labOrderId: number;
  encounterId: number;
  orderedBy: number;
  testJson: string;
  orderDate: string;
  status: boolean; // false = Ordered, true = Completed
}

export interface LabOrderFilter {
  encounterId?: number | null;
  orderedBy?: number | null;
  testJson?: string | null;
  orderDate?: string | null;
  status?: boolean | null;
}

export interface LabTestItem {
  testName: string;
  notes?: string;
}

export const COMMON_LAB_TESTS: string[] = [
  'CBC (Complete Blood Count)',
  'CMP (Comprehensive Metabolic Panel)',
  'BMP (Basic Metabolic Panel)',
  'Lipid Panel',
  'Liver Function (LFT)',
  'Thyroid (TSH)',
  'HbA1c',
  'Urinalysis',
  'PT/INR',
  'D-Dimer',
  'Cardiac Troponin',
  'CRP',
  'ESR'
];

export function parseTestJson(testJson: string | null | undefined): LabTestItem[] {
  if (!testJson) return [];
  const trimmed = testJson.trim();
  if (!trimmed) return [];
  try {
    const parsed = JSON.parse(trimmed);
    if (Array.isArray(parsed)) {
      return parsed
        .map((t) => {
          if (typeof t === 'string') return { testName: t };
          if (t && typeof t === 'object') {
            return {
              testName: (t.testName ?? t.name ?? t.test ?? '').toString(),
              notes: t.notes ? String(t.notes) : undefined
            };
          }
          return null;
        })
        .filter((t): t is LabTestItem => !!t && !!t.testName);
    }
  } catch {
    /* fall through */
  }
  // Fallback: comma / newline split.
  return trimmed
    .split(/[\n,;|]+/)
    .map((s) => s.trim())
    .filter(Boolean)
    .map((t) => ({ testName: t }));
}
