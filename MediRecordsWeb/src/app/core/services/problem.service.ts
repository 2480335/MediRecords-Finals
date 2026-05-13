import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProblemCreateRequest } from '../models/problem.models';

@Injectable({ providedIn: 'root' })
export class ProblemService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/ProblemList`;

  recordForPatient(
    patientId: number,
    payload: ProblemCreateRequest
  ): Observable<unknown> {
    return this.http.post(`${this.base}/${patientId}/problems`, payload, {
      responseType: 'text'
    });
  }
}
