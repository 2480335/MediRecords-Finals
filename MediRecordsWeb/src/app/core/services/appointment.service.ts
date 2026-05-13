import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AppointmentFilter,
  AppointmentRequest,
  AppointmentResponse,
  AppointmentStatusName,
  AppointmentUpdateRequest
} from '../models/appointment.models';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Appointments`;

  book(payload: AppointmentRequest): Observable<AppointmentResponse> {
    return this.http.post<AppointmentResponse>(this.base, payload);
  }

  list(filter: AppointmentFilter = {}): Observable<AppointmentResponse[]> {
    let params = new HttpParams();
    if (filter.id != null) params = params.set('id', filter.id);
    if (filter.patientId != null) params = params.set('patientId', filter.patientId);
    if (filter.providerId != null) params = params.set('providerId', filter.providerId);
    if (filter.date) params = params.set('date', filter.date);
    return this.http.get<AppointmentResponse[]>(this.base, { params });
  }

  updateStatus(
    id: number,
    status: AppointmentStatusName,
    payload: AppointmentUpdateRequest = {}
  ): Observable<AppointmentResponse> {
    return this.http.put<AppointmentResponse>(`${this.base}/${id}/${status}`, payload);
  }
}
