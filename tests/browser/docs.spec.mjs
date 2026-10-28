import { test, expect } from '@playwright/test';

test('the core homepage retains its original hero, state presentation and responsive layout', async ({ page }) => {
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto('core/');
  await expect(page.locator('.nacara-layout--splash')).toBeVisible();
  const hero = page.locator('.rv-hero');
  await expect(hero).toHaveCSS('display', 'grid');
  const copy = await page.locator('.rv-hero__copy').boundingBox();
  const demo = await page.locator('.rv-demo').boundingBox();
  expect(demo.x).toBeGreaterThan(copy.x + copy.width);
  await expect(page.locator('.rv-hero__title')).toHaveCSS('font-size', '80px');
  await expect(page.locator('.rv-btn--primary').first()).toHaveCSS('align-items', 'center');
  await expect(page.locator('.rv-demo__frames > li.is-active')).toHaveCSS('opacity', '1');
  await expect(page.locator('#rv-demo-fsharp')).toHaveCSS('visibility', 'hidden');
  await expect(page.locator('.rv-dial')).toHaveCSS('display', 'grid');
  await expect(page.locator('.rv-cards')).toHaveCSS('display', 'grid');
  const stateIcon = await page.locator('.rv-state--fallback').first().evaluate(element =>
    getComputedStyle(element, '::before').backgroundImage);
  expect(stateIcon).toContain('/Ranvier.Docs/core/brand/states/fallback-light.svg');
  await page.setViewportSize({ width: 390, height: 844 });
  const mobileCopy = await page.locator('.rv-hero__copy').boundingBox();
  const mobileDemo = await page.locator('.rv-demo').boundingBox();
  expect(mobileDemo.y).toBeGreaterThan(mobileCopy.y + mobileCopy.height);
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  await page.goto('docs/');
  expect(await page.locator('style').allTextContents()).not.toEqual(expect.arrayContaining([expect.stringContaining('.rv-hero {')]));
  expect(errors).toEqual([]);
});

test('the core homepage demo cycles, pauses and supports language tabs with the keyboard', async ({ page }) => {
  await page.clock.install();
  await page.goto('core/');
  const demo = page.locator('[data-rv-demo]');
  const activeFrame = demo.locator('.rv-demo__frames > li.is-active');
  const csharp = demo.getByRole('tab', { name: 'C#', exact: true });
  const fsharp = demo.getByRole('tab', { name: 'F#', exact: true });
  await expect(activeFrame).toHaveAttribute('data-step', '0');
  await page.clock.fastForward(2400);
  await expect(activeFrame).toHaveAttribute('data-step', '1');
  await expect(activeFrame).toContainText('Total 12');
  await page.clock.runFor(7200);
  await expect(activeFrame).toHaveAttribute('data-step', '0');
  await expect(fsharp).toHaveAttribute('aria-selected', 'true');
  await expect(demo.locator('#rv-demo-fsharp')).not.toHaveAttribute('hidden');
  await csharp.click();
  await expect(csharp).toHaveAttribute('aria-selected', 'true');
  await csharp.press('ArrowRight');
  await expect(fsharp).toBeFocused();
  await expect(fsharp).toHaveAttribute('aria-selected', 'true');
  await expect(demo.locator('#rv-demo-csharp')).toHaveAttribute('hidden');
  const heldStep = await activeFrame.getAttribute('data-step');
  await page.clock.runFor(4800);
  await expect(activeFrame).toHaveAttribute('data-step', heldStep);
  await demo.locator('#rv-demo-fsharp [data-step="2"]').hover();
  await expect(activeFrame).toHaveAttribute('data-step', '2');
  await expect(activeFrame).toContainText('Unavailable: feed offline');
});

test('the Ranvier theme and initial examples are readable without JavaScript', async ({ browser, baseURL }) => {
  const context = await browser.newContext({ javaScriptEnabled: false });
  const page = await context.newPage();
  await page.goto(new URL('docs/runnable-examples/', baseURL).href);
  await expect(page.locator('main')).toContainText('Shared runnable examples');
  await expect(page.locator('button.rv-map-fixture__count')).toHaveText('0');
  await expect(page.locator('.rv-map').first()).toBeAttached();
  await expect(page.locator('.nacara-navbar')).toBeVisible();
  await expect(page.locator('link[rel="stylesheet"]').first()).toBeAttached();
  await context.close();
});

test('shared counter, signal map and motion hydrate and react in the same page', async ({ page }) => {
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('docs/runnable-examples/');
  const counter = page.locator('button.rv-map-fixture__count');
  await expect(counter).toHaveText('0');
  await expect(page.locator('[data-ranvier-map-cell="shared-counter"]')).toHaveAttribute('data-ranvier-ready', 'true');
  await counter.click();
  await expect(counter).toHaveText('1');
  const map = page.locator('.rv-map').first();
  await expect(page.locator('[data-ranvier-map-cell="shared-map"]')).toHaveAttribute('data-ranvier-ready', 'true');
  await expect(map.locator('.rv-map-node').first()).toBeVisible();
  await page.getByRole('button', { name: 'Increment map', exact: true }).click();
  await expect(map.locator('.rv-map__log')).toContainText('write');
  await expect(page.locator('[data-ranvier-map-cell="shared-motion"]')).toHaveAttribute('data-ranvier-ready', 'true');
  await page.getByRole('button', { name: 'Move', exact: true }).click();
  await expect.poll(() => page.locator('.motion-target').evaluate(element =>
    new DOMMatrix(getComputedStyle(element).transform).m41)).toBe(40);
  expect(errors).toEqual([]);
});

test('the live editor compiles, mounts and recompiles an edited component', async ({ page }) => {
  await page.goto('docs/runnable-examples/');
  const run = page.getByRole('button', { name: 'Run', exact: true });
  await run.click();
  await page.getByRole('button', { name: 'Result', exact: true }).click();
  const result = page.frameLocator('iframe.nacara-live__frame');
  const counter = result.locator('button.rv-map-fixture__count');
  await expect(counter).toHaveText('0', { timeout: 90_000 });
  await counter.click();
  await expect(counter).toHaveText('1');
  const editor = page.locator('.cm-content');
  const source = await editor.innerText();
  await editor.fill(source + '\nprintfn "Edited through the shared documentation editor"\n');
  await run.click();
  await expect(counter).toHaveText('0', { timeout: 90_000 });
  await counter.click();
  await expect(counter).toHaveText('1');
});

test('repository navigation and original source edit links retain their identities', async ({ page }) => {
  await page.goto('docs/');
  const view = page.locator('main').getByRole('link', { name: 'Views and motion', exact: true });
  await view.click();
  await expect(page).toHaveURL(/\/view\/$/);
  await page.goto('view/view/getting-started/');
  await expect(page.locator('a[href*="github.com/ranvier-org/Ranvier.View/edit/"]')).toHaveCount(1);
  await expect(page.locator('a[href*="github.com/ranvier-org/Ranvier.View/edit/"]')).toHaveAttribute('href', /docs\/content\/view\/getting-started\.md$/);
  await expect(page.locator('body')).toHaveAttribute('data-section', 'view');
  await page.setViewportSize({ width: 390, height: 844 });
  await expect(page.locator('main')).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true);
});

test('global search can find a page from another repository collection', async ({ page }) => {
  await page.goto('docs/');
  await page.locator('[data-nacara-search]').click();
  const modal = page.locator('pagefind-modal');
  const input = modal.getByRole('searchbox');
  await expect(input).toBeVisible();
  await input.fill('Persistence');
  await expect(modal.locator('a[href*="/persistence/"]').first()).toBeVisible();
});

test('the shared colour preference applies and survives navigation', async ({ page }) => {
  await page.goto('docs/');
  await page.getByRole('combobox', { name: 'Colour scheme' }).selectOption('dark');
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await page.goto('docs/theme/');
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
});

test('core performance charts load the recorded dataset and render measurements', async ({ page, request }) => {
  const response = page.waitForResponse(response => /\/core\/perf-data\/[^/]+\.json$/.test(response.url()) && response.status() === 200);
  await page.goto('core/benchmarks/counters/');
  const dataset = await request.get((await response).url());
  expect(dataset.ok()).toBe(true);
  const history = await dataset.json();
  expect(history.commits.length).toBeGreaterThan(0);
  await expect(page.locator('.rv-perf-provenance').first()).toBeVisible();
  await expect(page.locator('.rv-perf svg, .rv-perf-tabs svg').first()).toBeVisible();
});

test('the real standalone View demos load their rendered pages and browser modules', async ({ page }) => {
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  for (const app of ['playground', 'motion', 'hydration']) {
    const response = await page.goto(`view/examples/${app}/index.html`);
    expect(response.status()).toBe(200);
    await expect(page.locator('body')).toContainText(/\S/);
  }
  expect(errors).toEqual([]);
});
