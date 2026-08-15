import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

/** Endpoints that must never carry a token or trigger a refresh - they ARE the auth flow. */
const AUTH_ENDPOINTS = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh'];

// Shared across requests so that N concurrent 401s trigger ONE refresh, not N refreshes.
let isRefreshing = false;
const refreshedToken$ = new BehaviorSubject<string | null>(null);

/**
 * Attaches the bearer token to outgoing requests, and transparently refreshes it once on a 401.
 *
 * Access tokens are deliberately short-lived (15 minutes), so without this the user would be
 * kicked out mid-session. Doing it here means no component ever has to think about tokens.
 */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const isAuthCall = AUTH_ENDPOINTS.some((endpoint) => req.url.includes(endpoint));
  const token = auth.getAccessToken();

  const request = !isAuthCall && token ? withToken(req, token) : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      const is401 = error instanceof HttpErrorResponse && error.status === 401;

      // Only a genuine, refreshable session gets a retry.
      if (!is401 || isAuthCall || !auth.hasRefreshToken()) {
        return throwError(() => error);
      }

      if (isRefreshing) {
        // Queue behind the in-flight refresh, then replay with the new token.
        return refreshedToken$.pipe(
          filter((newToken): newToken is string => newToken !== null),
          take(1),
          switchMap((newToken) => next(withToken(req, newToken))),
        );
      }

      isRefreshing = true;
      refreshedToken$.next(null);

      return auth.refresh().pipe(
        switchMap((response) => {
          isRefreshing = false;
          refreshedToken$.next(response.accessToken);
          return next(withToken(req, response.accessToken));
        }),
        catchError((refreshError: unknown) => {
          // The refresh token is gone or revoked - the session is genuinely over.
          isRefreshing = false;
          auth.clearSession();
          void router.navigate(['/login']);
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};

function withToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

/** Exposed for tests, which need the module-level refresh state reset between cases. */
export function resetJwtInterceptorState(): void {
  isRefreshing = false;
  refreshedToken$.next(null);
}

export type { Observable };
