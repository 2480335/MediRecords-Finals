import { Injectable } from '@angular/core';

const ACCESS_KEY = 'mr.accessToken';
const REFRESH_KEY = 'mr.refreshToken';
const EXPIRES_KEY = 'mr.expires';

@Injectable({ providedIn: 'root' })
export class TokenService {
  private storage: Storage = sessionStorage;

  saveTokens(accessToken: string, refreshToken: string, expires: string): void {
    this.storage.setItem(ACCESS_KEY, accessToken);
    this.storage.setItem(REFRESH_KEY, refreshToken);
    this.storage.setItem(EXPIRES_KEY, expires);
  }

  getAccessToken(): string | null {
    return this.storage.getItem(ACCESS_KEY);
  }

  getRefreshToken(): string | null {
    return this.storage.getItem(REFRESH_KEY);
  }

  getExpires(): string | null {
    return this.storage.getItem(EXPIRES_KEY);
  }

  clear(): void {
    this.storage.removeItem(ACCESS_KEY);
    this.storage.removeItem(REFRESH_KEY);
    this.storage.removeItem(EXPIRES_KEY);
  }

  hasValidToken(): boolean {
    const token = this.getAccessToken();
    const expires = this.getExpires();
    if (!token || !expires) {
      return false;
    }
    return new Date(expires).getTime() > Date.now();
  }
}
