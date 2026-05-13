import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ImmunizationCreateRequest,
  ImmunizationCreateResponse,
  ImmunizationDetails,
  ImmunizationFilter
} from '../models/immunization.models';

@Injectable({ providedIn: 'root' })
export class ImmunizationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Immunization`;

  create(
    patientId: number,
    payload: ImmunizationCreateRequest
  ): Observable<ImmunizationCreateResponse> {
    return this.http.post<ImmunizationCreateResponse>(
      `${this.base}/${patientId}`,
      payload
    );
  }

  list(filter: ImmunizationFilter = {}): Observable<ImmunizationDetails[]> {
    let params = new HttpParams();
    if (filter.patientId != null) params = params.set('patientId', filter.patientId);
    if (filter.patientName) params = params.set('patientName', filter.patientName);
    if (filter.vaccine) params = params.set('vaccine', filter.vaccine);
    if (filter.status != null) params = params.set('status', filter.status);
    return this.http.get<ImmunizationDetails[]>(this.base, { params });
  }

  getById(id: number): Observable<ImmunizationDetails> {
    return this.http.get<ImmunizationDetails>(`${this.base}/${id}`);
  }
}
