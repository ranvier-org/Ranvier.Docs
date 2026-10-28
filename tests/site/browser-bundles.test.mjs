import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, writeFile, readFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { prepareBrowserBundles, planBrowserBundles, completeBrowserBundles } from '../../scripts/browser-bundles.mjs';

async function fixture(t) {
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-browser-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  for (const name of ['scripts/browser-bundles.mjs', 'src/SourceDependencies.targets', 'src/DependencyRoots.props', '.config/dotnet-tools.json']) {
    await mkdir(path.dirname(path.join(root, name)), { recursive: true });
    await writeFile(path.join(root, name), name);
  }
  const sources = [];
  for (const id of ['core', 'view', 'css']) {
    const local = path.join(root, 'inputs', id);
    await mkdir(path.join(local, 'src'), { recursive: true });
    await mkdir(path.join(local, 'docs'), { recursive: true });
    await writeFile(path.join(local, 'src/Library.fs'), 'module Library\nlet value = 1');
    sources.push({ id, repository: `fixture/${id}`, local });
  }
  const app = path.join(sources[1].local, 'examples/Ranvier.Motion');
  await mkdir(app, { recursive: true });
  await writeFile(path.join(app, 'App.fs'), 'module App');
  await writeFile(path.join(sources[1].local, 'docs/ranvier-docs.json'), JSON.stringify({ version: 1,
    browserBundles: [{ path: 'examples/Ranvier.Motion/dist', route: 'examples/motion' },
      { path: 'custom/dist', route: 'custom' }] }));
  await writeFile(path.join(root, 'sources.json'), JSON.stringify({ version: 1, sources }));
  const calls = [];
  const run = async (command, args, options) => {
    calls.push({ command, args, ...options });
    if (command === 'git') return options.cwd === sources[1].local ? 'src/Library.fs\0examples/Ranvier.Motion/App.fs\0' : 'src/Library.fs\0';
    if (args[0] === 'run') {
      await mkdir(path.join(app, 'dist'), { recursive: true });
      await writeFile(path.join(app, 'dist/index.html'), '<!doctype html><h1>Real build output fixture</h1>');
    }
    return '';
  };
  return { root, app, sources, calls, options: { root, preferLocal: true, run } };
}

test('standard View bundles build with selected source roots, cache successful outputs and track library changes', async t => {
  const f = await fixture(t);
  const [first] = await prepareBrowserBundles(f.options);
  assert.equal(first.projectRoot, f.app);
  assert.equal(first.cached, false);
  assert.equal(first.route, 'examples/motion');
  assert.deepEqual(f.calls.filter(call => call.command === 'npm').map(call => call.args), [['ci', '--no-audit', '--no-fund'], ['run', 'build']]);
  for (const call of f.calls) {
    assert.equal(call.env.RANVIER_DOCS_CORE_ROOT, f.sources[0].local);
    assert.equal(call.env.RANVIER_DOCS_VIEW_ROOT, f.sources[1].local);
    assert.equal(call.env.RanvierTrace, 'true');
  }
  f.calls.length = 0;
  assert.equal((await prepareBrowserBundles(f.options))[0].cached, true);
  assert.equal(f.calls.some(call => call.command === 'npm'), false);
  await writeFile(path.join(f.sources[0].local, 'src/Library.fs'), 'module Library\nlet value = 2');
  assert.equal((await prepareBrowserBundles(f.options))[0].cached, false);
});

test('a failed browser build leaves no successful cache marker and missing dist output fails explicitly', async t => {
  const f = await fixture(t);
  await assert.rejects(prepareBrowserBundles({ ...f.options, run: async (command, args, options) => {
    if (command === 'git') return 'src/Library.fs\0';
    if (args[0] === 'run') throw new Error('compiler failed');
    return '';
  } }), /compiler failed/);
  await assert.rejects(readFile(path.join(f.root, '.nacara/browser-bundles/Ranvier.Motion.json')), /ENOENT/);
  await assert.rejects(prepareBrowserBundles({ ...f.options, run: async command => command === 'git' ? 'src/Library.fs\0' : '' }), /did not produce dist\/index.html/);
});

test('browser planning only reads inputs and completion accepts a serialized plan', async t => {
  const f = await fixture(t);
  const plan = await planBrowserBundles(f.options);
  assert.equal(plan.length, 1);
  assert.equal(plan[0].cached, false);
  assert.ok(plan[0].fingerprint);
  assert.equal(f.calls.every(call => call.command === 'git'), true);
  await assert.rejects(readFile(plan[0].marker), /ENOENT/);
  await assert.rejects(completeBrowserBundles(plan, { run: f.options.run }), /did not produce dist\/index.html/);
  await mkdir(plan[0].outputRoot, { recursive: true });
  await writeFile(path.join(plan[0].outputRoot, 'index.html'), '<h1>Externally built</h1>');
  await completeBrowserBundles(JSON.parse(JSON.stringify(plan)), { run: f.options.run });
  assert.equal((await planBrowserBundles(f.options))[0].cached, true);
});

test('browser completion refuses to cache a build whose inputs changed after planning', async t => {
  const f = await fixture(t);
  const plan = await planBrowserBundles(f.options);
  await mkdir(plan[0].outputRoot, { recursive: true });
  await writeFile(path.join(plan[0].outputRoot, 'index.html'), '<h1>Externally built</h1>');
  await writeFile(path.join(f.sources[0].local, 'src/Library.fs'), 'module Library\nlet value = 99');
  await assert.rejects(completeBrowserBundles(plan, { run: f.options.run }), /inputs changed/);
  await assert.rejects(readFile(plan[0].marker), /ENOENT/);
});
