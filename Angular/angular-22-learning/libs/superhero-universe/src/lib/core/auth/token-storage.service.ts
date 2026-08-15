import { Injectable } from '@angular/core';
import { CurrentUser } from '../../shared/models/auth.models';

const ACCESS_TOKEN_KEY = 'shu.accessToken';
const REFRESH_TOKEN_KEY = 'shu.refreshToken';
const USER_KEY = 'shu.user';

/**
 * Persists the session across page reloads.
 *
 * Trade-off worth knowing: localStorage is readable by any script on the page, so it is
 * vulnerable to XSS. The more secure alternative is an httpOnly cookie, which JavaScript cannot
 * read - but that requires the API to set cookies and brings CSRF protection along with it.
 * localStorage is used here because the API is a stateless bearer-token API; if this were
 * handling real user data, httpOnly cookies would be the better default.
 */
@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getAccessToken(): string | null {
    return this.read(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return this.read(REFRESH_TOKEN_KEY);
  }

  getUser(): CurrentUser | null {
    const raw = this.read(USER_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      // Corrupt or tampered-with entry - drop it rather than crashing app startup.
      this.clear();
      return null;
    }
  }

  save(accessToken: string, refreshToken: string, user: CurrentUser): void {
    this.write(ACCESS_TOKEN_KEY, accessToken);
    this.write(REFRESH_TOKEN_KEY, refreshToken);
    this.write(USER_KEY, JSON.stringify(user));
  }

  clear(): void {
    [ACCESS_TOKEN_KEY, REFRESH_TOKEN_KEY, USER_KEY].forEach((key) => {
      try {
        localStorage.removeItem(key);
      } catch {
        /* storage unavailable - nothing to clear */
      }
    });
  }

  private read(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      // localStorage throws in private-browsing modes and during SSR.
      return null;
    }
  }

  private write(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      /* non-fatal: the session simply won't survive a reload */
    }
  }
}
