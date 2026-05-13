import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ImagingOrderCreateRequest,
  ImagingOrderFilter,
  ImagingOrderResponse,
  ImagingReportFull,
  ImagingReportUploadInput
} from '../models/imaging.models';

@Injectable({ providedIn: 'root' })
export class ImagingOrderService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/ImagingOrder`;
  private readonly imagingBase = `${environment.apiBaseUrl}/Imaging`;
  private readonly reportBase = `${environment.apiBaseUrl}/ImagingReport`;

  create(
    encounterId: number,
    payload: ImagingOrderCreateRequest
  ): Observable<unknown> {
    return this.http.post(`${this.base}/${encounterId}`, payload, {
      responseType: 'text'
    });
  }

  list(filter: ImagingOrderFilter = {}): Observable<ImagingOrderResponse[]> {
    let params = new HttpParams();
    if (filter.imagingOrderID != null) params = params.set('ImagingOrderID', filter.imagingOrderID);
    if (filter.encounterID != null) params = params.set('EncounterID', filter.encounterID);
    if (filter.studyType) params = params.set('StudyType', filter.studyType);
    if (filter.notes) params = params.set('Notes', filter.notes);
    if (filter.orderedDate) params = params.set('OrderedDate', filter.orderedDate);
    if (filter.status != null) params = params.set('Status', filter.status);
    return this.http.get<ImagingOrderResponse[]>(this.base, { params });
  }

  /** GET /Imaging/orders/{orderId}/reports */
  getReportsByOrderId(orderId: number): Observable<ImagingReportFull[]> {
    return this.http.get<ImagingReportFull[]>(
      `${this.imagingBase}/orders/${orderId}/reports`
    );
  }

  /** POST /ImagingReport/{ImagingOrderID} (multipart) */
  uploadReport(input: ImagingReportUploadInput): Observable<unknown> {
    const form = new FormData();
    form.append('Findings', input.findings);
    form.append('Impression', input.impression);
    if (input.reportFile) {
      form.append('ReportFile', input.reportFile, input.reportFile.name);
    }
    return this.http.post(
      `${this.reportBase}/${input.imagingOrderId}`,
      form,
      { responseType: 'text' }
    );
  }
}
