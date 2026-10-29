import {
  Injectable,
  Inject,
  PLATFORM_ID
} from '@angular/core';

import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface LoginRequest {
  email: string;
  password: string;
  selectedRole?: string;
}

export interface LoginResponse {
  requiresRoleSelection: boolean;
  roles?: string[];
  token?: string;
  userId?: number;
  empNo?: string;
  email?: string;
  fullName?: string;
  department?: string;
  position?: string;
  role?: string;
  expiresAt?: string;
}

export interface LoggedInUser {
  userId?: number;
  empNo?: string;
  email?: string;
  fullName?: string;
  department?: string;
  position?: string;
  role: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl =
    'https://localhost:7103/api/Auth';

  private logoutTimer: ReturnType<typeof setTimeout> | null =
    null;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: object
  ) {
    if (isPlatformBrowser(this.platformId)) {
      this.checkTokenExpiration();
    }
  }

  login(data: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(
        `${this.apiUrl}/login`,
        data
      )
      .pipe(
        tap(response => {
          // A role-selection response does not contain a token.
          if (
            response.requiresRoleSelection ||
            !response.token ||
            !response.role
          ) {
            return;
          }

          if (!isPlatformBrowser(this.platformId)) {
            return;
          }

          const loggedInUser: LoggedInUser = {
            userId: response.userId,
            empNo: response.empNo,
            email: response.email,
            fullName: response.fullName,
            department: response.department,
            position: response.position,
            role: response.role,
            roles: response.roles ?? [response.role]
          };

          localStorage.setItem(
            'loggedInUser',
            JSON.stringify(loggedInUser)
          );

          localStorage.setItem('token', response.token);

          this.startLogoutTimer(response.token);
        })
      );
  }

  getLoggedInUser(): LoggedInUser | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }

    const storedUser =
      localStorage.getItem('loggedInUser');

    if (!storedUser) {
      return null;
    }

    try {
      return JSON.parse(storedUser) as LoggedInUser;
    } catch {
      return null;
    }
  }

  getUserId(): number | null {
    return this.getLoggedInUser()?.userId ?? null;
  }

  getToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }

    return localStorage.getItem('token');
  }

  getCurrentRole(): string | null {
    return this.getLoggedInUser()?.role ?? null;
  }

  hasRole(role: string): boolean {
    return this.getCurrentRole()?.toLowerCase() ===
      role.toLowerCase();
  }

  isLoggedIn(): boolean {
    const token = this.getToken();

    if (!token) {
      return false;
    }

    try {
      const payload = this.decodeToken(token);

      return typeof payload.exp === 'number' &&
        payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  logout(): void {
    if (this.logoutTimer) {
      clearTimeout(this.logoutTimer);
      this.logoutTimer = null;
    }

    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('loggedInUser');
      localStorage.removeItem('token');
    }
  }

  private checkTokenExpiration(): void {
    const token = this.getToken();

    if (token) {
      this.startLogoutTimer(token);
    }
  }

  private startLogoutTimer(token: string): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    if (this.logoutTimer) {
      clearTimeout(this.logoutTimer);
      this.logoutTimer = null;
    }

    try {
      const payload = this.decodeToken(token);

      if (typeof payload.exp !== 'number') {
        this.logout();
        return;
      }

      const remaining =
        payload.exp * 1000 - Date.now();

      if (remaining <= 0) {
        this.logout();
        return;
      }

      this.logoutTimer = setTimeout(() => {
        this.logout();
        window.location.href = '/login';
      }, remaining);
    } catch {
      this.logout();
    }
  }

  private decodeToken(token: string): any {
    const parts = token.split('.');

    if (parts.length !== 3) {
      throw new Error('Invalid JWT format.');
    }

    const base64 = parts[1]
      .replace(/-/g, '+')
      .replace(/_/g, '/');

    const padded = base64 +
      '='.repeat((4 - base64.length % 4) % 4);

    return JSON.parse(atob(padded));
  }
}