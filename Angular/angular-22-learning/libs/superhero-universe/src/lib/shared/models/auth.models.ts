export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RefreshRequest {
  accessToken: string;
  refreshToken: string;
}

/** Mirrors ModelAuthResponse from the API. */
export interface AuthResponse {
  userId: number;
  username: string;
  email: string;
  roles: string[];
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
}

export interface UserProfile {
  userId: number;
  username: string;
  email: string;
  roles: string[];
  createdAt: string;
}

/** The signed-in user as held in memory. Never includes tokens. */
export interface CurrentUser {
  userId: number;
  username: string;
  email: string;
  roles: string[];
}

export const ROLE_ADMIN = 'Admin';
export const ROLE_USER = 'User';
