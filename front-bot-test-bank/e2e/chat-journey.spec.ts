import { expect, test } from '@playwright/test';
import { installFakeChatApi } from './support/fakeChatApi';

// One journey that grows with each phase of spec 002: a customer opens the page, talks, and starts another inquiry.
// The API is a scripted fake (see support/fakeChatApi.ts); the real one is only used by ask-a-question.spec.ts.
// The screenshots are attached to the report so the developer can compare them with design/figma-frame-2-679.png.
test.use({ viewport: { width: 1440, height: 1000 } });

test('a customer holds a conversation and starts another', async ({ page }, testInfo) => {
  await installFakeChatApi(page);
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
});
