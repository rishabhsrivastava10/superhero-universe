import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { SUPERHERO_API_CONFIG } from '../api-config';
import { adminGuard, authGuard, guestGuard } from './auth.guard';

const route = {} as ActivatedRouteSnapshot;
const stateFor = (url: string) => ({ url }) as RouterStateSnapshot;

/**
 * Seeds a session directly into storage. This must happen BEFORE the guard runs, because
 * AuthService reads storage once when it is constructed - and the guard is what first injects it.
 */
function seedSession(roles: string[]): void {
  localStorage.setItem('shu.accessToken', 'access');
  localStorage.setItem('shu.refreshToken', 'refresh');
  localStorage.setItem(
    'shu.user',
    JSON.stringify({ userId: 1, username: 'tester', email: 'tester@example.com', roles }),
  );
}

function configure(): void {
  TestBed.resetTestingModule();
  TestBed.configureTestingModule({
    providers: [
      provideRouter([]),
      provideHttpClient(),
      provideHttpClientTesting(),
      { provide: SUPERHERO_API_CONFIG, useValue: { baseUrl: 'http://test-api' } },
    ],
  });
}

/** Guards run inside an injection context. */
const run = <T>(fn: () => T): T => TestBed.runInInjectionContext(fn);

describe('auth guards', () => {
  beforeEach(() => {
    localStorage.clear();
    configure();
  });

  afterEach(() => localStorage.clear());

  describe('authGuard', () => {
    it('blocks an anonymous visitor', () => {
      expect(run(() => authGuard(route, stateFor('/superheroes')))).toBeInstanceOf(UrlTree);
    });

    it('redirects an anonymous visitor to login', () => {
      const result = run(() => authGuard(route, stateFor('/superheroes'))) as UrlTree;

      expect(result.toString()).toContain('/login');
    });

    it('remembers where the visitor was headed', () => {
      const result = run(() => authGuard(route, stateFor('/superheroes/42'))) as UrlTree;

      // returnUrl is what lets login send them back afterwards.
      expect(result.queryParams['returnUrl']).toBe('/superheroes/42');
    });

    it('allows a signed-in user through', () => {
      seedSession(['User']);
      configure();

      expect(run(() => authGuard(route, stateFor('/superheroes')))).toBe(true);
    });
  });

  describe('adminGuard', () => {
    it('sends an anonymous visitor to login rather than to forbidden', () => {
      const result = run(() => adminGuard(route, stateFor('/superheroes/new'))) as UrlTree;

      // An anonymous user should be asked to sign in - telling them "forbidden" would be
      // misleading, since signing in as an admin would in fact grant access.
      expect(result.toString()).toContain('/login');
    });

    it('blocks a signed-in NON-admin with forbidden', () => {
      seedSession(['User']);
      configure();

      const result = run(() => adminGuard(route, stateFor('/superheroes/new'))) as UrlTree;

      expect(result.toString()).toContain('/forbidden');
    });

    it('allows an admin through', () => {
      seedSession(['User', 'Admin']);
      configure();

      expect(run(() => adminGuard(route, stateFor('/superheroes/new')))).toBe(true);
    });
  });

  describe('guestGuard', () => {
    it('lets an anonymous visitor reach the login page', () => {
      expect(run(() => guestGuard(route, stateFor('/login')))).toBe(true);
    });

    it('redirects an already signed-in user away from the login page', () => {
      seedSession(['User']);
      configure();

      const result = run(() => guestGuard(route, stateFor('/login'))) as UrlTree;

      expect(result).toBeInstanceOf(UrlTree);
      expect(result.toString()).toContain('/superheroes');
    });
  });
});
