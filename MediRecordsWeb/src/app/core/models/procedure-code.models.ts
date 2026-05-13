export type ProcedureCodeKey =
  | 'Consult'
  | 'Xray'
  | 'CT'
  | 'MRI'
  | 'UltraSound'
  | 'BloodTest';

export interface ProcedureCodeView {
  codeId: number;
  code: string;
  description: string;
  price: number;
}

export interface ProcedureCodeCreateRequest {
  code: ProcedureCodeKey;
  description: string;
  price: number;
}

export interface ProcedureCodeUpdateRequest {
  description: string;
  price: number;
}

export const PROCEDURE_KEYS: ProcedureCodeKey[] = [
  'Consult',
  'Xray',
  'CT',
  'MRI',
  'UltraSound',
  'BloodTest'
];
