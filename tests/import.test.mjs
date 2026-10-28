import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, writeFile, readFile, access, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';

const implementation = await import('../scripts/import.mjs').catch(() => ({}));
const plainBody = text => text.replace(/^---\ntitle: [^\n]*\n---\n\n/, '');

async function fixture(t, sources = ['core', 'view']) {
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-docs-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  const registry = { version: 1, title: 'Ranvier', baseUrl: '/docs/', sources: [] };
  for (const id of sources) {
    const checkout = path.join(root, 'inputs', id);
    await mkdir(path.join(checkout, 'docs/content'), { recursive: true });
    await writeFile(path.join(checkout, 'docs/content/index.md'), `---\ntitle: ${id}\n---\n# ${id}\n`);
    registry.sources.push({ id, repository: `ranvier-org/${id}`, ref: 'main', local: checkout });
  }
  await writeFile(path.join(root, 'sources.json'), JSON.stringify(registry));
  return { root, registry, checkout: id => path.join(root, 'inputs', id),
    put: async (id, name, content) => {
      const file = path.join(root, 'inputs', id, name);
      await mkdir(path.dirname(file), { recursive: true });
      await writeFile(file, content);
    },
    prepare: () => implementation.prepareSite({ root, preferLocal: true, allowUnversioned: true }) };
}

test('the importer exposes its prepare and registry validation operations', () => {
  assert.equal(typeof implementation.prepareSite, 'function');
  assert.equal(typeof implementation.validateRegistry, 'function');
});

test('default imports preserve source files and create navigation and source metadata', async t => {
  const f = await fixture(t);
  await f.put('core', 'docs/content/guide/start.md', '---\ntitle: Start here\n---\n# Start\nA page.');
  const site = await f.prepare();
  assert.equal(site.baseUrl, '/docs/');
  assert.equal(site.sources[0].route, 'core');
  assert.equal(site.sources[0].sections[0].pages.some(p => p.path === 'guide/start.md' && p.title === 'Start here'), true);
  assert.equal(await readFile(path.join(f.root, '.generated/content/core/guide/start.md'), 'utf8'),
    await readFile(path.join(f.checkout('core'), 'docs/content/guide/start.md'), 'utf8'));
  assert.equal(site.sources[0].repository, 'ranvier-org/core');
  assert.equal(site.sources[0].revision, null);
});

test('a manifest controls section labels, ordering, landing pages and static files', async t => {
  const f = await fixture(t, ['core']);
  await f.put('core', 'docs/content/tutorial.md', '# Tutorial');
  await f.put('core', 'docs/content/reference.md', '# Reference');
  await f.put('core', 'docs/static/logo.svg', '<svg/>');
  await f.put('core', 'docs/ranvier-docs.json', JSON.stringify({ version: 1, title: 'Reactive core', index: 'tutorial.md',
    sections: [{ title: 'Learn', pages: [{ path: 'tutorial.md', title: 'Begin' }] },
      { title: 'API', pages: ['reference.md', 'index.md'] }] }));
  const site = await f.prepare();
  assert.equal(site.sources[0].title, 'Reactive core');
  assert.equal(site.sources[0].index, 'tutorial.md');
  assert.deepEqual(site.sources[0].sections.map(s => s.title), ['Learn', 'API']);
  assert.deepEqual(site.sources[0].sections[0].pages, [{ path: 'tutorial.md', title: 'Begin' }]);
  assert.equal(await readFile(path.join(f.root, '.generated/static/core/logo.svg'), 'utf8'), '<svg/>');
});

test('cross repository links resolve to relative staged Markdown with anchors and titles', async t => {
  const f = await fixture(t);
  await f.put('view', 'docs/content/guide/motion.md', '# Motion\n## Reactive timeline\n');
  await f.put('core', 'docs/content/guide/start.md', '# Start\n[Motion](repo:view/guide/motion.md#reactive-timeline "Animate")\n');
  await f.prepare();
  assert.match(await readFile(path.join(f.root, '.generated/content/core/guide/start.md'), 'utf8'),
    /\[Motion\]\(\.\.\/\.\.\/view\/guide\/motion\.md#reactive-timeline "Animate"\)/);
});

test('code fences and inline code containing repository links stay byte-for-byte unchanged', async t => {
  const f = await fixture(t);
  const markdown = '# Guide\n```markdown\n[Missing](repo:unknown/no.md)\n```\n~~~fsharp\n// repo:unknown/no.md\n~~~\n`[Missing](repo:unknown/no.md)`\n';
  await f.put('core', 'docs/content/code.md', markdown);
  await f.prepare();
  assert.equal(plainBody(await readFile(path.join(f.root, '.generated/content/core/code.md'), 'utf8')), markdown);
});

test('unresolved repository page and heading links fail with the source location', async t => {
  const f = await fixture(t);
  await f.put('core', 'docs/content/bad.md', '# Bad\n[Missing](repo:view/no.md)');
  await assert.rejects(f.prepare(), /core.*bad\.md:2.*view\/no\.md/);
  await f.put('core', 'docs/content/bad.md', '# Bad\n[Missing](repo:view/index.md#absent)');
  await assert.rejects(f.prepare(), /core.*bad\.md:2.*absent/);
});

test('a manifest cannot silently omit an existing page from navigation', async t => {
  const f = await fixture(t, ['core']);
  await f.put('core', 'docs/content/forgotten.md', '# Forgotten');
  await f.put('core', 'docs/ranvier-docs.json', JSON.stringify({ version: 1, sections: [{ title: 'Guide', pages: ['index.md'] }] }));
  await assert.rejects(f.prepare(), /forgotten\.md.*navigation/);
});

test('unlisted pages are an explicit escape hatch for hidden landing or reference pages', async t => {
  const f = await fixture(t, ['core']);
  await f.put('core', 'docs/content/hidden.md', '# Hidden');
  await f.put('core', 'docs/ranvier-docs.json', JSON.stringify({ version: 1, unlisted: ['hidden.md'], sections: [{ title: 'Guide', pages: ['index.md'] }] }));
  const site = await f.prepare();
  assert.deepEqual(site.sources[0].unlisted, ['hidden.md']);
  await access(path.join(f.root, '.generated/content/core/hidden.md'));
});

test('invalid registries and escaping paths are rejected before staging', async t => {
  assert.throws(() => implementation.validateRegistry({ version: 1, sources: [
    { id: 'a', repository: 'ranvier-org/a', route: 'same' },
    { id: 'b', repository: 'ranvier-org/b', route: 'same' }] }), /route.*same/);
  assert.throws(() => implementation.validateRegistry({ version: 1, sources: [{ id: '../a', repository: 'ranvier-org/a' }] }), /id/);
  const f = await fixture(t, ['core']);
  await f.put('core', 'docs/ranvier-docs.json', JSON.stringify({ version: 1, content: '../private' }));
  await assert.rejects(f.prepare(), /content.*relative|escape/);
});

test('missing checkouts produce an actionable acquisition diagnostic', async t => {
  const f = await fixture(t, ['core']);
  await rm(f.checkout('core'), { recursive: true });
  await assert.rejects(f.prepare(), /core.*sources:fetch|core.*checkout/);
});

test('regeneration drops deleted content and keeps the previous generation on failure', async t => {
  const f = await fixture(t, ['core']);
  await f.put('core', 'docs/content/old.md', '# Old');
  await f.prepare();
  await rm(path.join(f.checkout('core'), 'docs/content/old.md'));
  await f.prepare();
  await assert.rejects(access(path.join(f.root, '.generated/content/core/old.md')));
  await f.put('core', 'docs/content/bad.md', '[Bad](repo:missing/no.md)');
  await assert.rejects(f.prepare());
  await access(path.join(f.root, '.generated/site.json'));
  await assert.rejects(access(path.join(f.root, '.generated/content/core/bad.md')));
});

test('custom content roots and route prefixes preserve local asset links', async t => {
  const f = await fixture(t, ['core']);
  await f.put('core', 'manual/guide.md', '# Guide\n![Diagram](images/diagram.svg)');
  await f.put('core', 'manual/images/diagram.svg', '<svg/>');
  await f.put('core', 'docs/ranvier-docs.json', JSON.stringify({ version: 1, content: 'manual', index: 'guide.md' }));
  f.registry.sources[0].route = 'reactivity';
  await writeFile(path.join(f.root, 'sources.json'), JSON.stringify(f.registry));
  const site = await f.prepare();
  assert.equal(site.sources[0].route, 'reactivity');
  await access(path.join(f.root, '.generated/content/reactivity/images/diagram.svg'));
  assert.equal(plainBody(await readFile(path.join(f.root, '.generated/content/reactivity/guide.md'), 'utf8')), '# Guide\n![Diagram](images/diagram.svg)');
});

test('heading anchors retain inline code and formatting in cross repository targets', async t => {
  const f = await fixture(t);
  await f.put('view', 'docs/content/api.md', '# API\n## Call `createSignal` safely\n## **Important** values\n');
  await f.put('core', 'docs/content/index.md', '[Call](repo:view/api.md#call-createsignal-safely)\n[Values](repo:view/api.md#important-values)');
  await f.prepare();
});

test('all CommonMark code contexts preserve literal repository link examples', async t => {
  const f = await fixture(t, ['core']);
  const markdown = '# Code\n\n    [Indented](repo:missing/no.md)\n\n> ```markdown\n> [Quoted](repo:missing/no.md)\n> ```\n\n- ```markdown\n  [Listed](repo:missing/no.md)\n  ```\n\n`multi\n[span](repo:missing/no.md)`\n';
  await f.put('core', 'docs/content/index.md', markdown);
  await f.prepare();
  assert.equal(plainBody(await readFile(path.join(f.root, '.generated/content/core/index.md'), 'utf8')), markdown);
});

test('literate F# links transform only documentation comments and preserve executable text', async t => {
  const f = await fixture(t);
  const code = 'let ordinary = "[Literal](repo:missing/no.md)"\n// [Comment](repo:missing/no.md)\nlet multiline = """\n(** [Not docs](repo:missing/no.md) *)\n"""\nlet multiply = (*)\n(**\n# Literate guide\n[Views](repo:view/index.md)\n*)\nlet after = 1\n';
  await f.put('core', 'docs/content/literate.fsx', code);
  await f.prepare();
  const result = await readFile(path.join(f.root, '.generated/content/core/literate.fsx'), 'utf8');
  assert.equal(result, code.replace('[Views](repo:view/index.md)', '[Views](../view/index.md)'));
});

test('plain Markdown receives staged title metadata without changing its source or body', async t => {
  const f = await fixture(t, ['core']);
  const markdown = '# A "quoted" guide\n\nA regular Markdown file.\n';
  await f.put('core', 'docs/content/plain.md', markdown);
  await f.prepare();
  const staged = await readFile(path.join(f.root, '.generated/content/core/plain.md'), 'utf8');
  assert.match(staged, /^---\ntitle: "A \\"quoted\\" guide"\n---\n\n/);
  assert.equal(staged.endsWith(markdown), true);
  assert.equal(await readFile(path.join(f.checkout('core'), 'docs/content/plain.md'), 'utf8'), markdown);
});

test('legacy repository-site URLs in real HTML rebase while displayed examples stay unchanged', async t => {
  const f = await fixture(t, ['core']);
  f.registry.sources[0].repository = 'ranvier-org/Ranvier';
  await writeFile(path.join(f.root, 'sources.json'), JSON.stringify(f.registry));
  const body = '# Assets\n<a href="/Ranvier/guide/start/#example">Start</a>\n<img src="/Ranvier/images/example.svg">\n\n![Markdown diagram](/Ranvier/images/chart.svg)\n[Open demo](/Ranvier/examples/demo/index.html)\n\n```html\n<img src="/Ranvier/literal.svg">\n```\n\n`<a href="/Ranvier/literal/">`\n';
  await f.put('core', 'docs/content/index.md', body);
  await f.prepare();
  const staged = await readFile(path.join(f.root, '.generated/content/core/index.md'), 'utf8');
  assert.match(staged, /href="\/docs\/core\/guide\/start\/#example"/);
  assert.match(staged, /src="\/docs\/core\/images\/example.svg"/);
  assert.match(staged, /\[Markdown diagram\]\(\/docs\/core\/images\/chart.svg\)/);
  assert.match(staged, /\[Open demo\]\(\/docs\/core\/examples\/demo\/index.html\)/);
  assert.match(staged, /src="\/Ranvier\/literal.svg"/);
  assert.match(staged, /`<a href="\/Ranvier\/literal\/">`/);
});

test('host registration defaults bootstrap a source until its own manifest overrides them', async t => {
  const f = await fixture(t, ['core']);
  f.registry.sources[0].defaults = { title: 'Bootstrap label', sections: [{ title: 'Start', pages: ['index.md'] }] };
  await writeFile(path.join(f.root, 'sources.json'), JSON.stringify(f.registry));
  assert.equal((await f.prepare()).sources[0].title, 'Bootstrap label');
  await f.put('core', 'docs/ranvier-docs.json', JSON.stringify({ version: 1, title: 'Owned by source' }));
  assert.equal((await f.prepare()).sources[0].title, 'Owned by source');
});

test('locked dataset inputs have provenance without becoming documentation collections', async t => {
  const f = await fixture(t, ['core', 'history']);
  f.registry.sources[1].kind = 'dataset';
  await writeFile(path.join(f.root, 'sources.json'), JSON.stringify(f.registry));
  const site = await f.prepare();
  assert.deepEqual(site.sources.map(source => source.id), ['core']);
  assert.equal(site.datasets[0].id, 'history');
  assert.equal(site.datasets[0].root, f.checkout('history'));
  await assert.rejects(access(path.join(f.root, '.generated/content/history/index.md')));
});
