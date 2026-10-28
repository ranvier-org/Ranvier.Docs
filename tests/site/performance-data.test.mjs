import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, writeFile, readFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { preparePerformanceData } from '../../scripts/performance-data.mjs';

async function fixture(t) {
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-performance-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  const sources = [
    { id: 'core', repository: 'fixture/core', local: 'local/core' },
    { id: 'core-performance', kind: 'dataset', repository: 'fixture/core', ref: 'perf-data', local: 'local/data' }
  ];
  const coreRoot = path.join(root, 'local/core');
  const datasetRoot = path.join(root, 'local/data');
  await mkdir(path.join(coreRoot, '.github/scripts'), { recursive: true });
  await mkdir(path.join(datasetRoot, 'runs'), { recursive: true });
  await writeFile(path.join(datasetRoot, 'runs/fixture.json'), '{}');
  await writeFile(path.join(coreRoot, '.github/scripts/perf-site-data.cjs'), '// fixture generator');
  await writeFile(path.join(coreRoot, '.github/scripts/perf-groups.json'), JSON.stringify({ signals: ['chain'], memos: ['diamond'] }));
  await writeFile(path.join(root, 'sources.json'), JSON.stringify({ version: 1, sources }));
  const calls = [];
  const outputFile = path.join(coreRoot, 'docs/static/perf-data/index.json');
  const run = async (command, args, options) => {
    calls.push({ command, args, options });
    await mkdir(path.dirname(outputFile), { recursive: true });
    for (const name of ['index', 'signals', 'memos']) {
      await writeFile(path.join(path.dirname(outputFile), name + '.json'), JSON.stringify({ commits: [{ Commit: 'fixture-commit' }] }));
    }
  };
  return { root, coreRoot, datasetRoot, outputFile, calls, run, options: { root, preferLocal: true, run } };
}

test('performance data uses the selected core generator and dataset without fetching sources', async t => {
  const f = await fixture(t);
  const result = await preparePerformanceData(f.options);
  assert.deepEqual(f.calls.map(call => [call.command, call.args, call.options.cwd]), [[process.execPath,
    [path.join(f.coreRoot, '.github/scripts/perf-site-data.cjs'), path.join(f.datasetRoot, 'runs'), f.outputFile], f.coreRoot]]);
  assert.equal(result.commits, 1);
  assert.deepEqual(result.groups, ['signals', 'memos']);
  assert.equal(result.outputFile, f.outputFile);
  assert.equal(result.datasetRoot, f.datasetRoot);
  assert.deepEqual(JSON.parse(await readFile(f.outputFile, 'utf8')).commits, [{ Commit: 'fixture-commit' }]);
  assert.equal((await preparePerformanceData(f.options)).cached, true);
  assert.equal(f.calls.length, 1, 'unchanged inputs and output must not rerun the generator during preview');
  await writeFile(path.join(f.datasetRoot, 'runs/fixture.json'), '{"changed":true}');
  assert.equal((await preparePerformanceData(f.options)).cached, false);
  assert.equal(f.calls.length, 2);
});

test('source root overrides take precedence over local preference', async t => {
  const f = await fixture(t);
  const override = path.join(f.root, 'overrides/dataset');
  await mkdir(path.join(override, 'runs'), { recursive: true });
  await writeFile(path.join(override, 'runs/fixture.json'), '{}');
  const result = await preparePerformanceData({ ...f.options, sourceRoots: { 'core-performance': override } });
  assert.equal(result.datasetRoot, override);
  assert.equal(f.calls[0].args[1], path.join(override, 'runs'));
});

test('generator failures and incomplete or empty output fail explicitly', async t => {
  const f = await fixture(t);
  await assert.rejects(preparePerformanceData({ ...f.options, run: async () => { throw new Error('bad performance record'); } }), /bad performance record/);
  await assert.rejects(preparePerformanceData({ ...f.options, run: async () => {} }), /index\.json/);
  await f.run();
  await writeFile(f.outputFile, JSON.stringify({ commits: [] }));
  await assert.rejects(preparePerformanceData({ ...f.options, run: async () => {} }), /no commits/);
  await f.run();
  await rm(path.join(path.dirname(f.outputFile), 'memos.json'));
  await assert.rejects(preparePerformanceData({ ...f.options, run: async () => {} }), /memos\.json/);
});

test('registries without the performance dataset skip generation', async t => {
  const f = await fixture(t);
  await writeFile(path.join(f.root, 'sources.json'), JSON.stringify({ version: 1, sources: [{ id: 'docs', repository: 'fixture/docs' }] }));
  assert.equal(await preparePerformanceData({ root: f.root, run: async () => assert.fail('must not run') }), null);
});
