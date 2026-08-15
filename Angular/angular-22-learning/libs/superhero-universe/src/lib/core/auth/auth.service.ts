import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { SUPERHERO_API_CONFIG } from '../api-config';
import {
  AuthResponse,
  CurrentUser,
  LoginRequest,
  RegisterRequest,
  ROLE_ADMIN,
  UserProfile,
} from '../../shared/models/auth.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(TokenStorageService);
  private readonly baseUrl = `${inject(SUPERHERO_API_CONFIG).baseUrl}/api/auth`;

  // Signals rather than BehaviorSubjects: this is synchronous state with a current value,
  // which is exactly what signals model well, and templates read them without the async pipe.
  private readonly currentUser = signal<CurrentUser | null>(this.storage.getUser());

  readonly user = this.currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUser() !== null);
  readonly isAdmin = computed(() => this.currentUser()?.roles.includes(ROLE_ADMIN) ?? false);

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/register`, request)
      .pipe(tap((response) => this.applySession(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, request)
      .pipe(tap((response) => this.applySession(response)));
  }

  /** Exchanges an expired access token + valid refresh token for a fresh pair. */
  refresh(): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/refresh`, {
        accessToken: this.storage.getAccessToken() ?? '',
        refreshToken: this.storage.getRefreshToken() ?? '',
      })
      .pipe(tap((response) => this.applySession(response)));
  }

  profile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/me`);
  }

  logout(): Observable<void> {
    const refreshToken = this.storage.getRefreshToken() ?? '';
    // Clear locally first, so the UI signs out immediately even if the network call fails.
    this.clearSession();
    return this.http.post<void>(`${this.baseUrl}/logout`, { refreshToken });
  }

  /** Drops the local session without calling the API - used when a refresh fails. */
  clearSession(): void {
    this.storage.clear();
    this.currentUser.set(null);
  }

  getAccessToken(): string | null {
    return this.storage.getAccessToken();
  }

  hasRefreshToken(): boolean {
    return !!this.storage.getRefreshToken();
  }

  private applySession(response: AuthResponse): void {
    const user: CurrentUser = {
      userId: response.userId,
      username: response.username,
      email: response.email,
      roles: response.roles,
    };

    this.storage.save(response.accessToken, response.refreshToken, user);
    this.currentUser.set(user);
  }
}
