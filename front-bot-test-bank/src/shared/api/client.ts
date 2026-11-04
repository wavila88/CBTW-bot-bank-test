/** The API answered with its error JSON; `message` is already meant for the customer. */
export class ApiError extends Error {
  readonly kind = 'api';
  readonly status: number;
  readonly traceId: string;

  constructor(status: number, message: string, traceId: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.traceId = traceId;
  }
}

/** The API could not be reached. Each feature decides what to tell the customer. */
export class NetworkError extends Error {
  readonly kind = 'network';

  constructor(message = 'The API could not be reached.') {
    super(message);
    this.name = 'NetworkError';
  }
}

/** The API answered something that is not the JSON we expect. */
export class UnexpectedError extends Error {
  readonly kind = 'unexpected';

  constructor(message = 'The API answered something unexpected.') {
    super(message);
    this.name = 'UnexpectedError';
  }
}

export type ClientError = ApiError | NetworkError | UnexpectedError;

export interface ApiClient {
  post<T>(path: string, body: unknown): Promise<T>;
}

interface ApiErrorBody {
  status: number;
  detail: string;
  traceId: string;
}

function isApiErrorBody(value: unknown): value is ApiErrorBody {
  if (typeof value !== 'object' || value === null) return false;
  const body = value as Record<string, unknown>;
  return typeof body.status === 'number' && typeof body.detail === 'string' && typeof body.traceId === 'string';
}

export function createApiClient({
  baseUrl,
  fetchFn = fetch,
}: {
  baseUrl: string;
  fetchFn?: typeof fetch;
}): ApiClient {
  return {
    async post<T>(path: string, body: unknown): Promise<T> {
      let response: Response;
      try {
        response = await fetchFn(`${baseUrl}${path}`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(body),
        });
      } catch {
        throw new NetworkError();
      }

      let parsed: unknown;
      try {
        parsed = await response.json();
      } catch {
        throw new UnexpectedError();
      }

      if (!response.ok) {
        if (isApiErrorBody(parsed)) throw new ApiError(parsed.status, parsed.detail, parsed.traceId);
        throw new UnexpectedError();
      }
      return parsed as T;
    },
  };
}
