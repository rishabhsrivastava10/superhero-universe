import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { SUPERHERO_API_CONFIG } from '../api-config';
import { AuthResponse } from '../../shared/models/auth.models';
import { AuthService } from './auth.service';
import { TokenStorageService } from './token-storage.service';

const BASE_URL = 'http://test-api';

function authResponse(overrides: Partial<AuthResponse> = {}): AuthResponse {
  return {
    userId: 1,
    username: 'tester',
    email: 'tester@example.com',
    roles: ['User'],
    accessToken: 'access-token-value',
    accessTokenExpiresAt: new Date().toISOString(),
    refreshToken: 'refresh-token-value',
    ...overrides,
  };
}

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;
  let storage: TokenStorageService;

  beforeEach(() => {
    // Each test starts signed out; the service reads storage on construction.
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: SUPERHERO_API_CONFIG, useValue: { baseUrl: BASE_URL } },
      ],
    });

    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    storage = TestBed.inject(TokenStorageService);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('starts unauthenticated when nothing is stored', () => {
    expect(service.isAuthenticated()).toBe(false);
    expect(service.isAdmin()).toBe(false);
    expect(service.user()).toBeNull();
  });

  it('posts credentials to the login endpoint', () => {
    service.login({ username: 'tester', password: 'secret' }).subscribe();

    const req = http.expectOne(`${BASE_URL}/api/auth/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ username: 'tester', password: 'secret' });
    req.flush(authResponse());
  });

  it('marks the user authenticated after a successful login', () => {
    service.login({ username: 'tester', password: 'secret' }).subscribe();
    http.expectOne(`${BASE_URL}/api/auth/login`).flush(authResponse());

    expect(service.isAuthenticated()).toBe(true);
    expect(service.user()?.username).toBe('tester');
  });

  it('persists the tokens so the session survives a reload', () => {
    service.login({ username: 'tester', password: 'secret' }).subscribe();
    http.expectOne(`${BASE_URL}/api/auth/login`).flush(authResponse());

    expect(storage.getAccessToken()).toBe('access-token-value');
    expect(storage.getRefreshToken()).toBe('refresh-token-value');
  });

  it('never stores tokens on the in-memory user object', () => {
    service.login({ username: 'tester', password: 'secret' }).subscribe();
    http.expectOne(`${BASE_URL}/api/auth/login`).flush(authResponse());

    const user = service.user() as Record<string, unknown> | null;
    expect(user?.['accessToken']).toBeUndefined();
    expect(user?.['refreshToken']).toBeUndefined();
  });

  it('reports isAdmin only when the Admin role is present', () => {
    service.login({ username: 'tester', password: 'secret' }).subscribe();
    http.expectOne(`${BASE_URL}/api/auth/login`).flush(authResponse({ roles: ['User'] }));
    expect(service.isAdmin()).toBe(false);

    service.clearSession();

    service.login({ username: 'admin', password: 'secret' }).subscribe();
    http.expectOne(`${BASE_URL}/api/auth/login`).flush(authResponse({ roles: ['User', 'Admin'] }));
    expect(service.isAdmin()).toBe(true);
  });

  it('leaves the user signed out when login fails', () => {
    service.login({ username: 'tester', password: 'wrong' }).subscribe({ error: () => undefined });

    http.expectOne(`${BASE_URL}/api/auth/login`).flush(
      { statusCode: 401, message: 'Invalid username or password.' },
      { status: 401, statusText: 'Unauthorized' },
    );

    expect(service.isAuthenticated()).toBe(false);
    expect(storage.getAccessToken()).toBeNull();
  });

  it('sends both tokens when refreshing', () => {
    service.login({ username: 'tester', password: 'secret' }).subscribe();
    http.expectOne(`${BASE_URL}/api/auth/login`).flush(authResponse());

    service.refresh().subscribe();

    const req = http.expectOne(`${BASE_URL}/api/auth/refresh`);
    expect(req.request.body).toEqual({
      accessToken: 'access-token-value',
      refreshToken: 'refresh-token-value',
    });
    req.flush(authResponse({ accessToken: 'new-access', refreshToken: 'new-refresh' }));

    expect(storage.getAccessToken()).toBe('new-access');
  });

  it('clears the session immediately on logout, before the server replies', () => {
    service.login({ username: 'tester', password: 'secret' }).subscribe();
    http.expectOne(`${BASE_URL}/api/auth/login`).flush(authResponse());

    service.logout().subscribe({ error: () => undefined });

    // The UI must sign out even if the network call never completes.
    expect(service.isAuthenticated()).toBe(false);
    expect(storage.getAccessToken()).toBeNull();

    http.expectOne(`${BASE_URL}/api/auth/logout`).flush({});
  });

  it('restores a stored session when the service is constructed', () => {
    storage.save('stored-access', 'stored-refresh', {
      userId: 9,
      username: 'returning',
      email: 'returning@example.com',
      roles: ['User', 'Admin'],
    });

    // A fresh injector, as if the page had just been reloaded.
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: SUPERHERO_API_CONFIG, useValue: { baseUrl: BASE_URL } },
      ],
    });

    const revived = TestBed.inject(AuthService);
    expect(revived.isAuthenticated()).toBe(true);
    expect(revived.user()?.username).toBe('returning');
    expect(revived.isAdmin()).toBe(true);

    TestBed.inject(HttpTestingController).verify();
  });

  it('recovers from a corrupt stored user rather than crashing', () => {
    localStorage.setItem('shu.user', '{ not valid json');

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: SUPERHERO_API_CONFIG, useValue: { baseUrl: BASE_URL } },
      ],
    });

    const revived = TestBed.inject(AuthService);
    expect(revived.isAuthenticated()).toBe(false);

    TestBed.inject(HttpTestingController).verify();
  });
});
