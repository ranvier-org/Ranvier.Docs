import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';
import { Script } from 'node:vm';

test('the first-paint theme bootstrap runs as an inline classic script with no module dependency', async () => {
  const source = await readFile(new URL('../src/Nacara.Plugin.Ranvier.Theme/assets/theme-bootstrap.js', import.meta.url), 'utf8');
  const script = new Script(source);
  for (const [choice, prefersDark, theme] of [['system', true, 'dark'], ['system', false, 'light'], ['light', true, 'light'], ['dark', false, 'dark']]) {
    const document = { documentElement: { dataset: {} } };
    script.runInNewContext({ document, matchMedia: () => ({ matches: prefersDark }), localStorage: { getItem: () => choice } });
    assert.equal(document.documentElement.dataset.themeSetting, choice);
    assert.equal(document.documentElement.dataset.theme, theme);
  }
  const document = { documentElement: { dataset: {} } };
  script.runInNewContext({ document, matchMedia: () => ({ matches: true }), localStorage: { getItem: () => { throw new Error('storage unavailable'); } } });
  assert.equal(document.documentElement.dataset.themeSetting, 'system');
  assert.equal(document.documentElement.dataset.theme, 'dark');
});
