import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ClinicKpiReport,
  DocumentationCompletenessReport,
  NoShowCancellationReport,
  ProviderUtilizationReport
} from '../models/reports.models';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Reports`;

  clinicKpis(fromDate: string, toDate: string): Observable<ClinicKpiReport> {
    const params = new HttpParams().set('fromDate', fromDate).set('toDate', toDate);
    return this.http.get<ClinicKpiReport>(`${this.base}/clinic-kpis`, { params });
  }

  documentationCompleteness(
    startDate: string,
    endDate: string,
    providerId?: number | null
  ): Observable<DocumentationCompletenessReport> {
    let params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    if (providerId) params = params.set('providerId', providerId);
    return this.http.get<DocumentationCompletenessReport>(
      `${this.base}/documentation-completeness`,
      { params }
    );
  }

  noShowCancellation(
    startDate: string,
    endDate: string,
    providerId?: number | null
  ): Observable<NoShowCancellationReport> {
    let params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    if (providerId) params = params.set('providerId', providerId);
    return this.http.get<NoShowCancellationReport>(
      `${this.base}/no-show-cancellation`,
      { params }
    );
  }

  providerUtilization(
    providerId: number,
    startDate: string,
    endDate: string
  ): Observable<ProviderUtilizationReport> {
    const params = new HttpParams()
      .set('providerId', providerId)
      .set('startDate', startDate)
      .set('endDate', endDate);
    return this.http.get<ProviderUtilizationReport>(`${this.base}/provider-utilization`, { params });
  }
}
