import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ProcedureCodeCreateRequest,
  ProcedureCodeUpdateRequest,
  ProcedureCodeView
} from '../models/procedure-code.models';

@Injectable({ providedIn: 'root' })
export class ProcedureCodeService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/Billing/procedure-codes`;

  getAll(filterCode?: string | null): Observable<ProcedureCodeView[]> {
    let params = new HttpParams();
    if (filterCode) {
      params = params.set('filterCode', filterCode);
    }
    return this.http.get<ProcedureCodeView[]>(this.base, { params });
  }

  create(payload: ProcedureCodeCreateRequest): Observable<unknown> {
    return this.http.post(this.base, payload, { responseType: 'text' });
  }

  update(id: number, payload: ProcedureCodeUpdateRequest): Observable<unknown> {
    return this.http.patch(`${this.base}/${id}`, payload, { responseType: 'text' });
  }
}
