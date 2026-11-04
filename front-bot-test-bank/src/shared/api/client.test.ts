import { describe, expect, it, vi } from 'vitest';
import { ApiError, NetworkError, UnexpectedError, createApiClient } from './client';

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

describe('api client', () => {
  it('sends JSON to the base URL and returns the parsed body', async () => {
    const fetchFn = vi.fn().mockResolvedValue(jsonResponse({ answer: 'Hi', conversationId: 'c1' }));
    const client = createApiClient({ baseUrl: 'http://localhost:5082', fetchFn });

    const result = await client.post<{ answer: string; conversationId: string }>('/Chat', { message: 'Hello' });

    expect(result).toEqual({ answer: 'Hi', conversationId: 'c1' });
    const [url, init] = fetchFn.mock.calls[0];
    expect(url).toBe('http://localhost:5082/Chat');
    expect(init.method).toBe('POST');
    expect(init.headers['Content-Type']).toBe('application/json');
    expect(JSON.parse(init.body)).toEqual({ message: 'Hello' });
  });

  it('turns the API error JSON into an ApiError with status, customer message and trace id', async () => {
    const error = { status: 429, title: 'Too many', detail: 'Please wait a minute.', traceId: 'trace-1' };
    const client = createApiClient({ baseUrl: 'http://x', fetchFn: vi.fn().mockResolvedValue(jsonResponse(error, 429)) });

    const failure = await client.post('/Chat', {}).catch((e: unknown) => e);

    expect(failure).toBeInstanceOf(ApiError);
    expect(failure).toMatchObject({ kind: 'api', status: 429, message: 'Please wait a minute.', traceId: 'trace-1' });
  });

  it('turns a connection failure into a NetworkError', async () => {
    const client = createApiClient({ baseUrl: 'http://x', fetchFn: vi.fn().mockRejectedValue(new TypeError('fetch failed')) });

    const failure = await client.post('/Chat', {}).catch((e: unknown) => e);

    expect(failure).toBeInstanceOf(NetworkError);
    expect(failure).toMatchObject({ kind: 'network' });
  });

  it('turns an answer that is not JSON into an UnexpectedError', async () => {
    const html = new Response('<html>Bad gateway</html>', { status: 502 });
    const client = createApiClient({ baseUrl: 'http://x', fetchFn: vi.fn().mockResolvedValue(html) });

    const failure = await client.post('/Chat', {}).catch((e: unknown) => e);

    expect(failure).toBeInstanceOf(UnexpectedError);
    expect(failure).toMatchObject({ kind: 'unexpected' });
  });
});
