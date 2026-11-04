import type { Page } from '@playwright/test';

/** What the fake API answers to one `POST /Chat`, in the order the requests arrive. */
export interface FakeReply {
  answer: string;
  conversationId: string;
}

/** The body of a request the page sent, recorded so the journey can check it (for example the `conversationId`). */
export interface RecordedRequest {
  message: string;
  conversationId?: string;
}

/**
 * Answers `POST /Chat` from a script instead of the real API, so the journey is the same every time and spends no
 * calls of the real API's limit. It also answers the browser's CORS preflight. Returns the requests it received.
 */
export async function installFakeChatApi(page: Page, replies: FakeReply[] = []) {
  const requests: RecordedRequest[] = [];
  const cors = {
    'access-control-allow-origin': '*',
    'access-control-allow-headers': '*',
    'access-control-allow-methods': 'POST, OPTIONS',
  };

  await page.route('**/Chat', async (route) => {
    if (route.request().method() === 'OPTIONS') {
      await route.fulfill({ status: 204, headers: cors });
      return;
    }
    requests.push(route.request().postDataJSON() as RecordedRequest);
    const reply = replies[requests.length - 1];
    if (!reply) {
      await route.fulfill({ status: 500, headers: cors, json: { status: 500, title: 'No scripted reply', detail: 'No scripted reply', traceId: 'fake' } });
      return;
    }
    await route.fulfill({ status: 200, headers: cors, json: { answer: reply.answer, sources: [], conversationId: reply.conversationId } });
  });

  return requests;
}
