import { Injectable } from '@angular/core';
import { ConnexionResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class TokenStorage {
  private readonly ACCESS_TOKEN_KEY = 'hra_jwt';
  private readonly REFRESH_TOKEN_KEY = 'hra_refresh';
  private readonly USER_INFO_KEY = 'hra_user';

  saveTokens(jwt: string, refreshToken: string): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, jwt);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  saveUser(user: Partial<ConnexionResponse>): void {
    localStorage.setItem(this.USER_INFO_KEY, JSON.stringify(user));
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getUser(): Partial<ConnexionResponse> | null {
    const raw = localStorage.getItem(this.USER_INFO_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  clear(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_INFO_KEY);
  }
}
