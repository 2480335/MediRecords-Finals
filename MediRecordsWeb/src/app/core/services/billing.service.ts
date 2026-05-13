import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AssignVisitChargeRequest,
  ExportFormat,
  ExportStatus,
  MarkBilledResponse,
  MarkChargesBilledRequest,
  PagedResponse,
  UnbilledEncounter,
  UnbilledEncountersFilter,
  UpdateVisitChargeRequest,
  VisitChargeResponse
} from '../models/billing.models';

@Injectable({ providedIn: 'root' })
export class BillingService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Billing`;

  getUnbilledEncounters(
    filter: UnbilledEncountersFilter
  ): Observable<PagedResponse<UnbilledEncounter>> {
    let params = new HttpParams()
      .set('page', filter.page)
      .set('pageSize', filter.pageSize);

    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.providerId != null) params = params.set('providerId', filter.providerId);

    return this.http.get<PagedResponse<UnbilledEncounter>>(
      `${this.base}/unbilled-encounters`,
      { params }
    );
  }

  getChargesByEncounter(encounterId: number): Observable<VisitChargeResponse[]> {
    return this.http.get<VisitChargeResponse[]>(
      `${this.base}/encounters/${encounterId}/charges`
    );
  }

  markBilled(payload: MarkChargesBilledRequest): Observable<MarkBilledResponse> {
    return this.http.put<MarkBilledResponse>(
      `${this.base}/visit-charges/mark-billed`,
      payload
    );
  }

  assignVisitCharge(payload: AssignVisitChargeRequest): Observable<VisitChargeResponse> {
    return this.http.post<VisitChargeResponse>(
      `${this.base}/visit-charges`,
      payload
    );
  }

  updateVisitChargeAmount(
    chargeId: number,
    payload: UpdateVisitChargeRequest
  ): Observable<VisitChargeResponse> {
    return this.http.put<VisitChargeResponse>(
      `${this.base}/visit-charges/${chargeId}`,
      payload
    );
  }

  exportCharges(
    format: ExportFormat,
    status: ExportStatus = 'All',
    fromDate?: string | null,
    toDate?: string | null
  ): Observable<Blob> {
    let params = new HttpParams().set('format', format).set('status', status);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get(`${this.base}/exports`, {
      params,
      responseType: 'blob'
    });
  }
}
