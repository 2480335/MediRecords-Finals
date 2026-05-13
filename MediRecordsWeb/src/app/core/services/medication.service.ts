import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  MedicationFilter,
  MedicationListItem
} from '../models/medication.models';

@Injectable({ providedIn: 'root' })
export class MedicationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Medication`;

  list(filter: MedicationFilter = {}): Observable<MedicationListItem[]> {
    let params = new HttpParams();
    if (filter.patientId != null) params = params.set('PatientId', filter.patientId);
    if (filter.medId != null) params = params.set('MedId', filter.medId);
    if (filter.patientName) params = params.set('PatientName', filter.patientName);
    if (filter.drugName) params = params.set('DrugName', filter.drugName);
    if (filter.dose) params = params.set('Dose', filter.dose);
    if (filter.frequency) params = params.set('Frequency', filter.frequency);
    if (filter.route) params = params.set('Route', filter.route);
    if (filter.startDate) params = params.set('StartDate', filter.startDate);
    if (filter.endDate) params = params.set('EndDate', filter.endDate);
    if (filter.status) params = params.set('Status', filter.status);
    return this.http.get<MedicationListItem[]>(this.base, { params });
  }
}
