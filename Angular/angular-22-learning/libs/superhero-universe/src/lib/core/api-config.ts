import { InjectionToken } from '@angular/core';

export interface SuperheroApiConfig {
  /** Base URL of the API, without a trailing slash. e.g. http://localhost:5024 */
  baseUrl: string;
}

/**
 * Provided by the host application rather than hardcoded in the library, so the same library
 * can point at a different API per environment (or per app consuming it).
 */
export const SUPERHERO_API_CONFIG = new InjectionToken<SuperheroApiConfig>('SUPERHERO_API_CONFIG');
