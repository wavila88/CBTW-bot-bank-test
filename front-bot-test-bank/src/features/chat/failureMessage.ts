import { ApiError, NetworkError } from '../../shared/api/client';

/** What the customer reads when a call to the API fails: the API's own message with its reference, or a generic line. */
export function failureMessage(error: unknown): string {
  if (error instanceof ApiError) return `${error.message} (reference: ${error.traceId})`;
  if (error instanceof NetworkError) return 'The service could not be reached. Please try again.';
  return 'The service answered something unexpected. Please try again.';
}
