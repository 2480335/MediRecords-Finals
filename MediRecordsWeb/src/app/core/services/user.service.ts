import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ProviderLookup,
  RoleOption,
  UserRegisterRequest,
  UserUpdateRequest,
  UserUpdateResponse,
  UserView
} from '../models/user.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/User`;

  getAll(): Observable<UserView[]> {
    return this.http.get<UserView[]>(`${this.base}/GetAll`);
  }

  getRoles(): Observable<RoleOption[]> {
    return this.http.get<RoleOption[]>(`${this.base}/roles`);
  }

  getProviders(): Observable<ProviderLookup[]> {
    return this.http.get<ProviderLookup[]>(`${this.base}/providers`);
  }

  getById(id: number): Observable<UserView> {
    return this.http.get<UserView>(`${this.base}/GetById/${id}`);
  }

  register(payload: UserRegisterRequest): Observable<unknown> {
    return this.http.post(`${this.base}/register`, payload, { responseType: 'text' });
  }

  update(payload: UserUpdateRequest): Observable<{ message: string; data: UserUpdateResponse }> {
    return this.http.put<{ message: string; data: UserUpdateResponse }>(
      `${this.base}/update`,
      payload
    );
  }

  softDelete(id: number): Observable<unknown> {
    return this.http.post(`${this.base}/delete/${id}`, {}, { responseType: 'text' });
  }
}
