import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  VitalSignCaptureRequest,
  VitalSignCaptureResponse
} from '../models/vital-sign.models';

@Injectable({ providedIn: 'root' })
export class VitalSignService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/VitalSign`;

  capture(payload: VitalSignCaptureRequest): Observable<VitalSignCaptureResponse> {
    return this.http.post<VitalSignCaptureResponse>(
      `${this.base}/capture`,
      payload
    );
  }
}
