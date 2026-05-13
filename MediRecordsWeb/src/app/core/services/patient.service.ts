import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PatientCreateRequest,
  PatientCreateResponse,
  PatientDetails,
  PatientLookup,
  PatientUpdateRequest
} from '../models/patient.models';

@Injectable({ providedIn: 'root' })
export class PatientService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Patient`;

  create(payload: PatientCreateRequest): Observable<PatientCreateResponse> {
    return this.http.post<PatientCreateResponse>(this.base, payload);
  }

  getAll(): Observable<PatientLookup[]> {
    return this.http.get<PatientLookup[]>(`${this.base}/all`);
  }

  getById(id: number): Observable<PatientDetails> {
    return this.http.get<PatientDetails>(`${this.base}/${id}`);
  }

  update(id: number, payload: PatientUpdateRequest): Observable<PatientDetails> {
    return this.http.put<PatientDetails>(`${this.base}/${id}`, payload);
  }
}
