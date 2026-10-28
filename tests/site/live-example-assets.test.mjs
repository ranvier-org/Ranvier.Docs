import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, writeFile, readFile, rm } from 'node:fs/promises';
import path from 'node:path';
import { tmpdir } from 'node:os';
import { prepareLiveExampleAssets } from '../../scripts/live-example-assets.mjs';

test('live examples use a pinned local runtime and retain relative module imports beneath the deployment base', async t => {
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-live-assets-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  const packageRoot = path.join(root, 'node_modules/animejs');
  await mkdir(path.join(packageRoot, 'dist/modules/animation'), { recursive: true });
  await writeFile(path.join(packageRoot, 'package.json'), JSON.stringify({ version: '4.5.0' }));
  await writeFile(path.join(packageRoot, 'LICENSE.md'), 'MIT license fixture');
  await writeFile(path.join(packageRoot, 'dist/modules/index.js'), 'export * from "./animation/index.js";');
  await writeFile(path.join(packageRoot, 'dist/modules/animation/index.js'), 'export const animate = () => {};');
  const template = await prepareLiveExampleAssets({ root, metadata: { baseUrl: '/custom/docs/' } });
  assert.equal(template, path.join(root, '.generated/live/snippet.html'));
  const html = await readFile(template, 'utf8');
  assert.match(html, /"animejs":"\/custom\/docs\/assets\/live-example\/vendor\/animejs\/index.js"/);
  assert.doesNotMatch(html, /https?:\/\//);
  assert.match(html, /<head>.*<meta charset="utf-8">/);
  assert.match(html, /<\/head><body><\/body><\/html>/);
  const output = path.join(root, '.generated/static/assets/live-example/vendor/animejs');
  assert.equal(await readFile(path.join(output, 'index.js'), 'utf8'), 'export * from "./animation/index.js";');
  assert.equal(await readFile(path.join(output, 'animation/index.js'), 'utf8'), 'export const animate = () => {};');
  assert.equal(await readFile(path.join(output, 'LICENSE.md'), 'utf8'), 'MIT license fixture');
});

test('live runtime preparation rejects missing or mismatched dependencies and malformed deployment bases', async t => {
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-live-missing-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  await assert.rejects(prepareLiveExampleAssets({ root, metadata: {} }), /animejs@4\.5\.0.*npm ci/);
  await mkdir(path.join(root, 'node_modules/animejs'), { recursive: true });
  await writeFile(path.join(root, 'node_modules/animejs/package.json'), '{"version":"5.0.0"}');
  await assert.rejects(prepareLiveExampleAssets({ root, metadata: {} }), /found 5\.0\.0/);
  await assert.rejects(prepareLiveExampleAssets({ root, metadata: { baseUrl: 'relative' } }), /deployment base/);
});
