import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PrescriptionWithItems,
  PrescriptionWithItemsCreateRequest,
  PrescriptionWithItemsUpdateRequest
} from '../models/prescription.models';

@Injectable({ providedIn: 'root' })
export class PrescriptionService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/PrescriptionWithItems`;

  create(payload: PrescriptionWithItemsCreateRequest): Observable<PrescriptionWithItems> {
    return this.http.post<PrescriptionWithItems>(this.base, payload);
  }

  list(): Observable<PrescriptionWithItems[]> {
    return this.http.get<PrescriptionWithItems[]>(this.base);
  }

  getById(id: number): Observable<PrescriptionWithItems> {
    return this.http.get<PrescriptionWithItems>(`${this.base}/${id}`);
  }

  update(
    id: number,
    payload: PrescriptionWithItemsUpdateRequest
  ): Observable<PrescriptionWithItems> {
    return this.http.put<PrescriptionWithItems>(`${this.base}/${id}`, payload);
  }

  remove(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }
}
