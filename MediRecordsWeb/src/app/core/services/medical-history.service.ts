import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MedicalHistoryCreateRequest } from '../models/medical-history.models';

@Injectable({ providedIn: 'root' })
export class MedicalHistoryService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/MedicalHistory`;

  recordForPatient(
    patientId: number,
    payload: MedicalHistoryCreateRequest
  ): Observable<unknown> {
    return this.http.post(
      `${this.base}/${patientId}/medical-history`,
      payload,
      { responseType: 'text' }
    );
  }
}
