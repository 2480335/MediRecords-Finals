import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CarePlanCreateRequest,
  CarePlanDetails,
  CarePlanFilter,
  CarePlanResponse
} from '../models/care-plan.models';

@Injectable({ providedIn: 'root' })
export class CarePlanService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/CarePlan`;

  create(payload: CarePlanCreateRequest): Observable<CarePlanResponse> {
    return this.http.post<CarePlanResponse>(this.base, payload);
  }

  list(filter: CarePlanFilter = {}): Observable<CarePlanDetails[]> {
    let params = new HttpParams();
    if (filter.patientId != null) params = params.set('patientId', filter.patientId);
    if (filter.patientName) params = params.set('patientName', filter.patientName);
    if (filter.status != null) params = params.set('status', filter.status);
    return this.http.get<CarePlanDetails[]>(this.base, { params });
  }

  getById(id: number): Observable<CarePlanDetails> {
    return this.http.get<CarePlanDetails>(`${this.base}/${id}`);
  }
}
