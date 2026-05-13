import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  SOAPNoteResponse,
  SOAPNoteSaveRequest
} from '../models/soap-note.models';

@Injectable({ providedIn: 'root' })
export class SOAPNoteService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/SOAPNote`;

  save(payload: SOAPNoteSaveRequest): Observable<SOAPNoteResponse> {
    return this.http.post<SOAPNoteResponse>(`${this.base}/soap`, payload);
  }
}
