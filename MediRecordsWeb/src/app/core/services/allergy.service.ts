import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AllergyCreateRequest } from '../models/allergy.models';

@Injectable({ providedIn: 'root' })
export class AllergyService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Allergy`;

  recordForPatient(patientId: number, payload: AllergyCreateRequest): Observable<unknown> {
    return this.http.post(`${this.base}/${patientId}/allergies`, payload, {
      responseType: 'text'
    });
  }
}
