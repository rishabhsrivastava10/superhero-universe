import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * Blocks routes that require a signed-in user.
 *
 * This is a convenience/UX guard only - it stops an unauthenticated user navigating to a page
 * that would just show errors. It is NOT security: the API re-checks authorization on every
 * request, because anything enforced only in the browser can be bypassed.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  // Remember where they were headed so login can send them back afterwards.
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/** Same idea, additionally requiring the Admin role. */
export const adminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  return auth.isAdmin() ? true : router.createUrlTree(['/forbidden']);
};

/** Keeps a signed-in user away from the login/register pages. */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.isAuthenticated() ? router.createUrlTree(['/superheroes']) : true;
};
