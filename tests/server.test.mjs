import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
const implementation = await import('../scripts/serve.mjs').catch(() => ({}));

test('the built-site server serves project-base pages and rejects paths outside output', async t => {
  assert.equal(typeof implementation.startServer, 'function');
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-server-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  await mkdir(path.join(root, 'output/guide'), { recursive: true });
  await writeFile(path.join(root, 'output/guide/index.html'), '<h1>Guide</h1>');
  await writeFile(path.join(root, 'secret.txt'), 'private');
  const server = await implementation.startServer({ root: path.join(root, 'output'), baseUrl: '/docs/', port: 0 });
  t.after(() => new Promise(resolve => server.close(resolve)));
  const url = `http://127.0.0.1:${server.address().port}`;
  const page = await fetch(`${url}/docs/guide/`);
  assert.equal(page.status, 200);
  assert.match(page.headers.get('content-type'), /text\/html/);
  assert.equal(await page.text(), '<h1>Guide</h1>');
  assert.equal((await fetch(`${url}/guide/`)).status, 404);
  assert.equal((await fetch(`${url}/docs/%2e%2e%2fsecret.txt`)).status, 403);
});
