import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';

import * as validator from '../scripts/validate-repo.mjs';

async function repoFixture(t) {
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-validate-repo-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  await mkdir(path.join(root, 'docs/content/guide'), { recursive: true });
  await writeFile(path.join(root, 'docs/content/index.md'), '---\ntitle: Sample\n---\n# Sample\n[Start](guide/intro.md#install)\n');
  await writeFile(path.join(root, 'docs/content/guide/intro.md'), '---\ntitle: Intro\norder: 1\n---\n# Intro\n## Install\n[External](repo:view/index.md)\n');
  return root;
}

test('standalone repository validator passes valid manifests, local anchors and syntactically valid cross-repo links', async t => {
  assert.equal(typeof validator.validateRepository, 'function');
  const root = await repoFixture(t);
  await writeFile(path.join(root, 'docs/ranvier-docs.json'), JSON.stringify({
    version: 1,
    title: 'Sample',
    unlisted: ['index.md'],
    sections: [{ title: 'Guide', items: [{ dir: 'guide' }] }]
  }));
  const report = await validator.validateRepository(root);
  assert.equal(report.pages.length, 2);
  assert.deepEqual(report.problems, []);
});

test('standalone repository validator catches unlisted omissions, broken local links/anchors and malformed repo links', async t => {
  const root = await repoFixture(t);
  await writeFile(path.join(root, 'docs/content/guide/orphan.md'), '# Orphan\n[Broken](missing.md) [BadAnchor](intro.md#nope) [BadRepo](repo:Upper/bad)\n');
  await writeFile(path.join(root, 'docs/ranvier-docs.json'), JSON.stringify({
    version: 1,
    sections: [{ title: 'Guide', pages: ['index.md', 'guide/intro.md'] }]
  }));
  await assert.rejects(() => validator.validateRepository(root), /orphan\.md.*missing from navigation/);

  await writeFile(path.join(root, 'docs/ranvier-docs.json'), JSON.stringify({
    version: 1,
    sections: [{ title: 'Guide', pages: ['index.md', 'guide/intro.md', 'guide/orphan.md'] }]
  }));
  const report = await validator.validateRepository(root);
  assert.equal(report.problems.some(p => p.includes('missing.md')), true);
  assert.equal(report.problems.some(p => p.includes('#nope')), true);
  assert.equal(report.problems.some(p => p.includes('repo:Upper/bad')), true);
});
