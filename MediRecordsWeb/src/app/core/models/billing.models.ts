export interface UnbilledEncounter {
  encounterId: number;
  patientName: string;
  patientId: number;
  providerName: string;
  providerId: number;
  visitType: string;
  date: string;
  totalChargeAmount: number;
  unbilledChargeCount: number;
}

export interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface VisitChargeResponse {
  chargeId: number;
  encounterId: number;
  codeId: number;
  code: string;
  description: string;
  amount: number;
  status: string;
}

export interface MarkChargesBilledRequest {
  chargeIds: number[];
}

export interface MarkBilledResponse {
  markedAsBilled: number[];
  alreadyBilled: number[];
  notFound: number[];
  message: string;
}

export interface UnbilledEncountersFilter {
  fromDate?: string | null;
  toDate?: string | null;
  providerId?: number | null;
  page: number;
  pageSize: number;
}

export type ExportFormat = 'csv' | 'json';
export type ExportStatus = 'All' | 'Billed' | 'Unbilled';

export interface AssignVisitChargeRequest {
  encounterId: number;
  codeId: number;
  amount?: number | null;
}

export interface UpdateVisitChargeRequest {
  amount: number;
}
