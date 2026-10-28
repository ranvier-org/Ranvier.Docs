import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, writeFile, readFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';

const implementation = await import('../scripts/sources.mjs').catch(() => ({}));
const exec = promisify(execFile);
const git = (cwd, args) => exec('git', ['-C', cwd, ...args], { windowsHide: true });

test('source acquisition exposes a reusable fetch operation', () => {
  assert.equal(typeof implementation.acquireSources, 'function');
});

test('source acquisition locks revisions and refuses to overwrite dirty acquired checkouts', async t => {
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-source-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  const upstream = path.join(root, 'upstream');
  await mkdir(upstream);
  await git(upstream, ['init', '-b', 'main']);
  await git(upstream, ['config', 'user.email', 'test@example.invalid']);
  await git(upstream, ['config', 'user.name', 'Fixture']);
  await writeFile(path.join(upstream, 'page.md'), '# One');
  await git(upstream, ['add', 'page.md']);
  await git(upstream, ['commit', '-m', 'one']);
  const first = (await git(upstream, ['rev-parse', 'HEAD'])).stdout.trim();
  await writeFile(path.join(root, 'sources.json'), JSON.stringify({ version: 1,
    sources: [{ id: 'core', repository: 'ranvier-org/fixture', ref: 'main' }, { id: 'view', repository: 'ranvier-org/fixture', ref: 'main' }] }));
  const options = { root, repositoryUrls: { core: upstream, view: upstream } };
  await implementation.acquireSources(options);
  assert.equal(JSON.parse(await readFile(path.join(root, 'sources.lock.json'), 'utf8')).sources.core.revision, first);
  await writeFile(path.join(upstream, 'page.md'), '# Two');
  await git(upstream, ['commit', '-am', 'two']);
  await implementation.acquireSources(options);
  assert.equal((await git(path.join(root, '.sources/core'), ['rev-parse', 'HEAD'])).stdout.trim(), first);
  await implementation.acquireSources({ ...options, refreshIds: ['core'] });
  assert.notEqual((await git(path.join(root, '.sources/core'), ['rev-parse', 'HEAD'])).stdout.trim(), first);
  assert.equal((await git(path.join(root, '.sources/view'), ['rev-parse', 'HEAD'])).stdout.trim(), first);
  await implementation.acquireSources({ ...options, refresh: true });
  assert.notEqual((await git(path.join(root, '.sources/core'), ['rev-parse', 'HEAD'])).stdout.trim(), first);
  await writeFile(path.join(root, '.sources/core/page.md'), '# Local change');
  await assert.rejects(implementation.acquireSources(options), /dirty|uncommitted/);
  assert.equal(await readFile(path.join(root, '.sources/core/page.md'), 'utf8'), '# Local change');
});
