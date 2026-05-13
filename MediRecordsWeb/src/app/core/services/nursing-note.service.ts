import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  NursingNoteCreateRequest,
  NursingNoteResponse,
  NursingNoteUpdateRequest
} from '../models/nursing-note.models';

@Injectable({ providedIn: 'root' })
export class NursingNoteService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/NursingNote`;

  create(
    encounterId: number,
    payload: NursingNoteCreateRequest
  ): Observable<NursingNoteResponse> {
    return this.http.post<NursingNoteResponse>(
      `${this.base}/encounters/${encounterId}/nursing-notes`,
      payload
    );
  }

  update(
    noteId: number,
    payload: NursingNoteUpdateRequest
  ): Observable<NursingNoteResponse> {
    return this.http.put<NursingNoteResponse>(
      `${this.base}/${noteId}`,
      payload
    );
  }
}
