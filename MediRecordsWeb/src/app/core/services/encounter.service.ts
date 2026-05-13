import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  EncounterDetail,
  EncounterLookup,
  EncounterStatusName,
  EncounterStatusResponse,
  EncounterSummary,
  ENCOUNTER_STATUS_VALUE
} from '../models/encounter.models';

@Injectable({ providedIn: 'root' })
export class EncounterService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Encounter`;

  workspace(date?: string | null): Observable<EncounterSummary[]> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);
    return this.http.get<EncounterSummary[]>(`${this.base}/workspace`, { params });
  }

  getAll(): Observable<EncounterLookup[]> {
    return this.http.get<EncounterLookup[]>(`${this.base}/all`);
  }

  getById(encounterId: number): Observable<EncounterDetail> {
    return this.http.get<EncounterDetail>(`${this.base}/${encounterId}`);
  }

  updateStatus(
    encounterId: number,
    status: EncounterStatusName
  ): Observable<EncounterStatusResponse> {
    return this.http.patch<EncounterStatusResponse>(
      `${this.base}/${encounterId}/status`,
      { status: ENCOUNTER_STATUS_VALUE[status] }
    );
  }
}
