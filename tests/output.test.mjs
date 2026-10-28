import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
const implementation = await import('../scripts/check-output.mjs').catch(() => ({}));

test('the output checker resolves deployment bases and verifies page anchors and assets', async t => {
  assert.equal(typeof implementation.checkOutput, 'function');
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-output-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  await mkdir(path.join(root, 'guide'));
  await writeFile(path.join(root, 'guide/index.html'), '<h1 id="start">Start</h1><a href="../#home">Home</a><a href="#/gallery">Client route</a>');
  await writeFile(path.join(root, 'site.css'), 'body{}');
  await writeFile(path.join(root, 'index.html'), '<h1 id="home">Home</h1><a href="/docs/guide/#start">Guide</a><link href="/docs/site.css"><a href="https://example.invalid">External</a><code>&lt;img src="/literal-example.svg"&gt;</code><!-- <a href="/comment-literal/"> --><script>const example = \'<a href="/script-literal/">\';</script>');
  assert.deepEqual(await implementation.checkOutput({ root, baseUrl: '/docs/' }), []);
  await writeFile(path.join(root, 'index.html'), '<h1 id="home">Home</h1><a href="/docs/guide/#missing">Bad anchor</a><img src="/docs/missing.svg">');
  const problems = await implementation.checkOutput({ root, baseUrl: '/docs/' });
  assert.equal(problems.length, 2);
  assert.equal(problems.some(p => p.includes('missing.svg')), true);
  assert.equal(problems.some(p => p.includes('#missing')), true);
});
