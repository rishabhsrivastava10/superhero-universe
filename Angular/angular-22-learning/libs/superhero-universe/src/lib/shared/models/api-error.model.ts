/** Mirrors ModelErrorResponse - the single error shape every failing endpoint returns. */
export interface ApiErrorResponse {
  statusCode: number;
  message: string;
  timestamp: string;
  errors?: Record<string, string[]>;
}
