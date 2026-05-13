export interface LabResultRequest {
  labOrderId?: number | null;
  resultJson?: string | null;
  resultDate?: string | null;
  status?: boolean | null;
}

export interface LabResultResponse {
  resultId: number;
  labOrderId: number;
  resultJson?: string | null;
  resultDate: string;
  status: boolean; // false = Preliminary, true = Final
}

export interface LabResultEntry {
  testName: string;
  value: string;
  unit?: string;
  referenceRange?: string;
  flag?: 'Normal' | 'Low' | 'High' | 'Critical';
}

export function parseResultJson(json: string | null | undefined): LabResultEntry[] {
  if (!json) return [];
  const trimmed = json.trim();
  if (!trimmed) return [];
  try {
    const parsed = JSON.parse(trimmed);
    if (Array.isArray(parsed)) {
      return parsed
        .map((entry) => {
          if (!entry || typeof entry !== 'object') return null;
          const testName = (entry.testName ?? entry.test ?? entry.name ?? '').toString();
          if (!testName) return null;
          return {
            testName,
            value: (entry.value ?? entry.result ?? '').toString(),
            unit: entry.unit ? String(entry.unit) : undefined,
            referenceRange: entry.referenceRange ?? entry.range ?? undefined,
            flag: entry.flag ?? undefined
          } as LabResultEntry;
        })
        .filter((e): e is LabResultEntry => !!e);
    }
  } catch {
    /* fall through */
  }
  return [];
}
