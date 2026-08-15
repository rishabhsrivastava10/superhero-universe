import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import {
  SUPERHERO_API_CONFIG,
  errorInterceptor,
  jwtInterceptor,
} from 'superhero-universe';

import { environment } from '../environments/environment';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),

    // withComponentInputBinding lets route params bind straight to component input() signals,
    // so components don't need to inject ActivatedRoute just to read an :id.
    provideRouter(routes, withComponentInputBinding()),

    // Interceptor order matters: jwt runs first so it can attach the token and retry a 401,
    // and error runs outside it so a request that succeeds after refresh never shows a toast.
    provideHttpClient(withInterceptors([jwtInterceptor, errorInterceptor])),

    { provide: SUPERHERO_API_CONFIG, useValue: { baseUrl: environment.apiBaseUrl } },
  ],
};
