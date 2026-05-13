import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LabOrderCreateRequest,
  LabOrderFilter,
  LabOrderResponse
} from '../models/lab-order.models';

@Injectable({ providedIn: 'root' })
export class LabOrderService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/LabOrder`;

  create(payload: LabOrderCreateRequest): Observable<LabOrderResponse> {
    return this.http.post<LabOrderResponse>(this.base, payload);
  }

  list(filter: LabOrderFilter = {}): Observable<LabOrderResponse[]> {
    let params = new HttpParams();
    if (filter.encounterId != null) params = params.set('EncounterId', filter.encounterId);
    if (filter.orderedBy != null) params = params.set('OrderedBy', filter.orderedBy);
    if (filter.testJson) params = params.set('TestJson', filter.testJson);
    if (filter.orderDate) params = params.set('OrderDate', filter.orderDate);
    if (filter.status != null) params = params.set('Status', filter.status);
    return this.http.get<LabOrderResponse[]>(this.base, { params });
  }

  getById(id: number): Observable<LabOrderResponse> {
    return this.http.get<LabOrderResponse>(`${this.base}/${id}`);
  }

  getByEncounter(encounterId: number): Observable<LabOrderResponse[]> {
    return this.http.get<LabOrderResponse[]>(
      `${this.base}/encounter/${encounterId}`
    );
  }

  updateStatus(id: number, status: boolean): Observable<LabOrderResponse> {
    return this.http.put<LabOrderResponse>(`${this.base}/${id}/status`, status);
  }
}
