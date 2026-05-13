import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthUser,
  ForgotPasswordRequest,
  LoginRequest,
  LoginResponse,
  UserRole
} from '../models/auth.models';
import { TokenService } from './token.service';

interface DecodedJwt {
  nameid?: string;
  unique_name?: string;
  email?: string;
  role?: string;
  sub?: string;
  exp?: number;
  [key: string]: unknown;
}

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const NAMEID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

/**
 * Normalize a role string from the JWT to one of our canonical UserRole
 * literals. Tolerant to case, whitespace, dashes/underscores so that DB
 * values like "frontdesk", "Front Desk", or "FRONT_DESK" all work.
 */
function normalizeRole(raw: unknown): UserRole | null {
  if (typeof raw !== 'string') return null;
  const k = raw.replace(/[\s_\-]+/g, '').toLowerCase();
  switch (k) {
    case 'admin':
    case 'administrator':
      return 'Admin';
    case 'physician':
    case 'doctor':
      return 'Physician';
    case 'nurse':
      return 'Nurse';
    case 'labtech':
    case 'labtechnician':
    case 'lab':
      return 'LabTech';
    case 'frontdesk':
    case 'reception':
    case 'receptionist':
      return 'FrontDesk';
    default:
      return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);

  private readonly currentUser = signal<AuthUser | null>(this.loadUserFromToken());
  readonly user = this.currentUser.asReadonly();
  readonly isAuthenticated = computed(() => {
    const u = this.currentUser();
    return !!u && u.expiresAt > Date.now();
  });
  readonly role = computed(() => this.currentUser()?.role ?? null);

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiBaseUrl}/Auth/login`, payload)
      .pipe(
        tap((res) => {
          this.tokenService.saveTokens(res.accessToken, res.refreshToken, res.expires);
          this.currentUser.set(this.loadUserFromToken());
        })
      );
  }

  forgotPassword(payload: ForgotPasswordRequest): Observable<unknown> {
    return this.http.post(`${environment.apiBaseUrl}/User/forgotpassword`, payload);
  }

  logout(redirect = true): void {
    this.tokenService.clear();
    this.currentUser.set(null);
    if (redirect) {
      this.router.navigate(['/login']);
    }
  }

  dashboardRouteForRole(role: UserRole | null): string {
    switch (role) {
      case 'Admin':
        return '/admin';
      case 'Physician':
        return '/physician';
      case 'Nurse':
        return '/nurse';
      case 'LabTech':
        return '/labtech';
      case 'FrontDesk':
        return '/frontdesk';
      default:
        return '/login';
    }
  }

  private loadUserFromToken(): AuthUser | null {
    const token = this.tokenService.getAccessToken();
    const expires = this.tokenService.getExpires();
    if (!token || !expires) {
      return null;
    }
    try {
      const decoded = jwtDecode<DecodedJwt>(token);
      const userIdRaw =
        decoded[NAMEID_CLAIM] ?? decoded.nameid ?? decoded.sub ?? '0';
      const rawRole = decoded[ROLE_CLAIM] ?? decoded.role ?? null;
      const role = normalizeRole(rawRole);
      const name = (decoded[NAME_CLAIM] ?? decoded.unique_name ?? '') as string;
      const email = (decoded[EMAIL_CLAIM] ?? decoded.email ?? '') as string;

      if (!role) {
        // eslint-disable-next-line no-console
        console.warn(
          '[Auth] Could not resolve a known role from the JWT.',
          { rawRole, decoded }
        );
      }

      return {
        userId: Number(userIdRaw) || 0,
        name,
        email,
        role,
        expiresAt: new Date(expires).getTime()
      };
    } catch (err) {
      // eslint-disable-next-line no-console
      console.error('[Auth] Failed to decode access token', err);
      this.tokenService.clear();
      return null;
    }
  }
}
