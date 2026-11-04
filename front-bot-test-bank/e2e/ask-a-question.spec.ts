import { expect, test } from '@playwright/test';

// These tests use the real API at VITE_API_BASE_URL, whose CORS setting must allow E2E_BASE_URL.
// The API allows 5 requests per minute per IP, so this file sends only one to it.
// A blank question is checked by the unit tests in src/app/App.test.tsx: it never reaches the API.
// The error test talks to a second API instance (E2E_BROKEN_API_URL) that playwright.config.ts starts
// with its vector search pointed at a wrong host, so it answers a real error with its real CORS headers.

test('a question reaches the API and its answer shows on the page', async ({ page }) => {
  const consoleErrors: string[] = [];
  page.on('console', (message) => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });

  await page.goto('/');
  await page.getByRole('textbox', { name: 'Your question' }).fill('How do I reset my password?');
  const [response] = await Promise.all([
    page.waitForResponse((r) => r.url().endsWith('/Chat') && r.request().method() === 'POST'),
    page.getByRole('button', { name: 'Send' }).click(),
  ]);

  expect(response.status()).toBe(200);
  const body = await response.json();
  await expect(page.getByText(body.answer, { exact: false }).first()).toBeVisible();
  expect(consoleErrors.filter((text) => /CORS/i.test(text))).toEqual([]);
});

test('when the API answers an error, the page shows its message and trace id, not a CORS error', async ({ page }) => {
  const consoleErrors: string[] = [];
  page.on('console', (message) => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });
  // Only this test: the chat call goes to the broken API instead of the normal one.
  const brokenApiURL = process.env.E2E_BROKEN_API_URL!;
  await page.route('**/Chat', (route) => route.continue({ url: `${brokenApiURL}/Chat` }));

  await page.goto('/');
  await page.getByRole('textbox', { name: 'Your question' }).fill('How do I reset my password?');
  await page.getByRole('button', { name: 'Send' }).click();

  const alert = page.getByRole('alert');
  // The browser could read the error JSON (message and trace id), so the CORS header came with the error.
  await expect(alert).toContainText('reference:');
  await expect(alert).not.toContainText('could not be reached');
  expect(consoleErrors.filter((text) => /CORS/i.test(text))).toEqual([]);
});
