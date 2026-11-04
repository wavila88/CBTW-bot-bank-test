import { expect, test, type Page } from '@playwright/test';

// One integration journey that grows with each phase of spec 002: a customer opens the page and talks.
// It uses the real API at VITE_API_BASE_URL (no mocks), so that API must be running with its CORS setting
// allowing E2E_BASE_URL. The model answers differently each time, so the journey checks that an answer arrived and that
// the conversation id is chained, never the exact text. The API allows 5 requests per minute per IP: this journey sends 3.
// The screenshots are attached to the report so the developer can compare them with design/figma-frame-2-679.png.
test.use({ viewport: { width: 1440, height: 1000 } });

interface ChatCall {
  sent: { message: string; conversationId?: string };
  received: { answer: string; conversationId: string };
}

/** Types a message, sends it with Enter and waits for the real API's answer to that call. */
async function askAndWaitForAnswer(page: Page, message: string): Promise<ChatCall> {
  const field = page.getByRole('main').getByRole('textbox', { name: 'Your question' });
  await field.fill(message);
  const [response] = await Promise.all([
    page.waitForResponse((r) => r.url().endsWith('/Chat') && r.request().method() === 'POST'),
    field.press('Enter'),
  ]);
  expect(response.status()).toBe(200);
  return { sent: response.request().postDataJSON(), received: await response.json() };
}

test('a customer holds a conversation', async ({ page }, testInfo) => {
  let chatRequests = 0;
  page.on('request', (request) => {
    if (request.url().endsWith('/Chat') && request.method() === 'POST') chatRequests += 1;
  });
  await page.goto('/');

  // Phase 1: the page structure as in the mock-up.
  const header = page.getByRole('banner');
  await expect(header.getByRole('heading', { name: 'Bank Test' })).toBeVisible();
  await expect(header.getByText('Customer support')).toBeVisible();
  await expect(header.getByText('Official and secure channel')).toBeVisible();
  await expect(header.getByText('Guest')).toBeVisible();
  await expect(header.getByText('General inquiry')).toBeVisible();

  const menu = page.getByRole('navigation');
  await expect(menu.getByRole('button', { name: 'Virtual assistant', exact: true })).toHaveAttribute('aria-current', 'page');
  for (const name of ['My inquiries', 'FAQs', 'Contact channels']) {
    await expect(menu.getByRole('button', { name, exact: true })).toBeVisible();
  }
  await expect(menu.getByText('CURRENT CONVERSATION')).toBeVisible();

  const help = page.getByRole('complementary');
  await expect(help.getByRole('heading', { name: 'Your help center' })).toBeVisible();
  for (const topic of ['Transfers', 'Cards', 'Accounts and transactions', 'Digital banking']) {
    await expect(help.getByText(topic, { exact: true })).toBeVisible();
  }
  await expect(help.getByRole('button', { name: 'Privacy' })).toBeVisible();
  await expect(help.getByRole('button', { name: 'Accessibility' })).toBeVisible();

  await testInfo.attach('1-page-structure', { body: await page.screenshot(), contentType: 'image/png' });

  // Phase 2: the real chat.
  const chat = page.getByRole('main');
  await expect(chat.getByText('Bank Test Assistant')).toBeVisible();
  await expect(chat.getByText('Online · Available 24/7')).toBeVisible();
  await expect(chat.getByText('Hello, welcome to Bank Test.')).toBeVisible();

  // A suggested button fills the field and sends nothing.
  const field = chat.getByRole('textbox', { name: 'Your question' });
  await chat.getByRole('button', { name: 'To Bank Test', exact: true }).click();
  await expect(field).toHaveValue('To Bank Test');
  expect(chatRequests).toBe(0);

  // A blank message sends nothing and says so.
  await field.fill('   ');
  await field.press('Enter');
  await expect(chat.getByRole('alert')).toHaveText('Please type your question.');
  expect(chatRequests).toBe(0);

  // The first question: no conversation id goes out, the API gives one back and its answer shows.
  const first = await askAndWaitForAnswer(page, 'How do I pay with my card abroad?');
  expect(first.sent.conversationId).toBeUndefined();
  expect(first.received.conversationId).toBeTruthy();
  await expect(chat.getByText('How do I pay with my card abroad?')).toBeVisible();
  await expect(chat.getByText(first.received.answer, { exact: false }).first()).toBeVisible();
  await expect(chat.getByRole('alert')).toHaveCount(0);

  // Phase 3: while the follow-up is pending (the real call takes a few seconds) the typing dots show, sending is
  // blocked, and pressing Enter again sends nothing.
  const typing = chat.getByRole('status', { name: 'The assistant is typing' });
  await field.fill('and in dollars?');
  const followUpResponse = page.waitForResponse((r) => r.url().endsWith('/Chat') && r.request().method() === 'POST');
  await field.press('Enter');
  await expect(typing).toBeVisible();
  await expect(chat.getByRole('button', { name: 'Send' })).toBeDisabled();
  await field.fill('this must not be sent');
  await field.press('Enter');

  // The follow-up carries the conversation id the first answer returned, and the API continues that conversation.
  const response = await followUpResponse;
  expect(response.status()).toBe(200);
  const followUp: ChatCall = { sent: response.request().postDataJSON(), received: await response.json() };
  expect(followUp.sent.conversationId).toBe(first.received.conversationId);
  expect(followUp.received.conversationId).toBe(first.received.conversationId);
  await expect(chat.getByText(followUp.received.answer, { exact: false }).first()).toBeVisible();
  expect(chatRequests).toBe(2);

  // The answer is here: the dots are gone, sending works again, and both customer messages show two checks.
  await expect(typing).toBeHidden();
  await expect(chat.getByRole('button', { name: 'Send' })).toBeEnabled();
  await expect(chat.getByRole('img', { name: 'Answered' })).toHaveCount(2);
  await expect(chat.getByRole('img', { name: 'Sent' })).toHaveCount(0);

  // A message in Spanish: the API answers and the reply is shown as it comes.
  const spanish = await askAndWaitForAnswer(page, '¿Cómo bloqueo mi tarjeta?');
  expect(spanish.sent.conversationId).toBe(first.received.conversationId);
  await expect(chat.getByText(spanish.received.answer, { exact: false }).first()).toBeVisible();
  expect(chatRequests).toBe(3);

  await testInfo.attach('2-chat', { body: await page.screenshot(), contentType: 'image/png' });
});
