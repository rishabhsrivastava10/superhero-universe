import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ApiErrorResponse } from '../../shared/models/api-error.model';
import { NotificationService } from '../services/notification.service';

/**
 * Turns any failed HTTP call into a single user-facing toast, so components never repeat
 * error-handling boilerplate. The error is still re-thrown, so a component that genuinely needs
 * to react (e.g. show inline form errors) still can.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        // 401s are handled by the JWT interceptor (refresh or redirect); toasting them here
        // would show a spurious error during a refresh the user never sees.
        if (error.status !== 401) {
          notifications.error(toMessage(error));
        }
      }

      return throwError(() => error);
    }),
  );
};

function toMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'Cannot reach the server. Is the API running?';
  }

  const body = error.error as ApiErrorResponse | null;

  // Field-level validation failures are the most useful thing to surface, when present.
  if (body?.errors) {
    const first = Object.values(body.errors).flat()[0];
    if (first) {
      return first;
    }
  }

  if (body?.message) {
    return body.message;
  }

  return error.status === 403
    ? 'You do not have permission to do that.'
    : 'Something went wrong. Please try again.';
}
