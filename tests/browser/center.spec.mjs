import { test, expect } from '@playwright/test';

test('the imported center directive preserves the original table layout without JavaScript', async ({ browser, baseURL }) => {
  const context = await browser.newContext({ javaScriptEnabled: false });
  const page = await context.newPage();
  try {
    await page.goto(new URL('core/guide/installation/', baseURL).href);
    const table = page.locator('main table').first();
    await expect(table).toBeVisible();
    await expect(table).toContainText('net10');
    const wrapper = table.locator('xpath=ancestor::div[@style][1]');
    await expect(wrapper).toHaveCSS('display', 'flex');
    await expect(wrapper).toHaveCSS('justify-content', 'center');
  } finally { await context.close(); }
});
