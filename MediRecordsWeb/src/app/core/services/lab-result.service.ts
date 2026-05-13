import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LabResultRequest,
  LabResultResponse
} from '../models/lab-result.models';

@Injectable({ providedIn: 'root' })
export class LabResultService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/LabResult`;

  create(payload: LabResultRequest): Observable<LabResultResponse> {
    return this.http.post<LabResultResponse>(this.base, payload);
  }

  list(filter: LabResultRequest = {}): Observable<LabResultResponse[]> {
    let params = new HttpParams();
    if (filter.labOrderId != null) params = params.set('LabOrderId', filter.labOrderId);
    if (filter.resultJson) params = params.set('ResultJson', filter.resultJson);
    if (filter.resultDate) params = params.set('ResultDate', filter.resultDate);
    if (filter.status != null) params = params.set('Status', filter.status);
    return this.http.get<LabResultResponse[]>(this.base, { params });
  }

  getById(id: number): Observable<LabResultResponse> {
    return this.http.get<LabResultResponse>(`${this.base}/${id}`);
  }

  getByLabOrder(labOrderId: number): Observable<LabResultResponse[]> {
    return this.http.get<LabResultResponse[]>(
      `${this.base}/laborder/${labOrderId}`
    );
  }
}
