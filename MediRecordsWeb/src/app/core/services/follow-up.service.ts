import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FollowUpCreateRequest,
  FollowUpCreateResponse,
  FollowUpDetails,
  FollowUpFilter
} from '../models/follow-up.models';

@Injectable({ providedIn: 'root' })
export class FollowUpService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/FollowUp`;

  create(
    encounterId: number,
    payload: FollowUpCreateRequest
  ): Observable<FollowUpCreateResponse> {
    return this.http.post<FollowUpCreateResponse>(
      `${this.base}/${encounterId}`,
      payload
    );
  }

  list(filter: FollowUpFilter = {}): Observable<FollowUpDetails[]> {
    let params = new HttpParams();
    if (filter.patientId != null) params = params.set('patientId', filter.patientId);
    if (filter.encounterId != null) params = params.set('encounterId', filter.encounterId);
    return this.http.get<FollowUpDetails[]>(this.base, { params });
  }

  getById(id: number): Observable<FollowUpDetails> {
    return this.http.get<FollowUpDetails>(`${this.base}/${id}`);
  }
}
