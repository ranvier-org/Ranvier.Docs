import { defineConfig } from '@playwright/test';
import { readFileSync } from 'node:fs';

const site = JSON.parse(readFileSync(new URL('.generated/site.json', import.meta.url), 'utf8'));
export default defineConfig({
  testDir: './tests/browser',
  timeout: 120_000,
  expect: { timeout: 15_000 },
  retries: 0,
  use: { baseURL: `http://127.0.0.1:4173${site.baseUrl}`, trace: 'retain-on-failure' },
  webServer: { command: 'node scripts/serve.mjs', url: `http://127.0.0.1:4173${site.baseUrl}`, reuseExistingServer: false },
  reporter: [['list'], ['html', { open: 'never' }]]
});
