import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { readFile, writeFile, mkdir, readdir, stat } from 'node:fs/promises';
import { createHash } from 'node:crypto';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { loadRegistry, loadSourceConfiguration } from './import.mjs';

const workspace = fileURLToPath(new URL('..', import.meta.url));
const executeFile = promisify(execFile);
const digest = value => createHash('sha256').update(value).digest('hex');

async function execute(command, args, options) {
  await executeFile(command, args, { ...options, windowsHide: true, maxBuffer: 4 * 1024 * 1024 });
}

async function required(file, directory = false) {
  try {
    const info = await stat(file);
    if (directory ? !info.isDirectory() : !info.isFile()) throw new Error(`Expected ${directory ? 'directory' : 'file'}: ${file}`);
  } catch (error) {
    if (error.code === 'ENOENT') throw new Error(`Performance data input missing: ${file}; run dotnet fsi build.fsx -- sources first.`, { cause: error });
    throw error;
  }
}

async function readOutputs(outputDirectory, groups) {
  const hashes = {};
  let commits = 0;
  for (const name of ['index', ...groups]) {
    const file = path.join(outputDirectory, name + '.json');
    const text = await readFile(file, 'utf8');
    const data = JSON.parse(text);
    if (!Array.isArray(data.commits) || data.commits.length === 0) throw new Error(`Performance data has no commits: ${file}`);
    hashes[name] = digest(text);
    if (name === 'index') commits = data.commits.length;
  }
  return { commits, hashes };
}

async function inputFingerprint({ coreRoot, datasetRoot, generator, groupFile, runsDirectory }) {
  const files = (await readdir(runsDirectory)).filter(file => file.endsWith('.json')).sort();
  if (!files.length) throw new Error(`No performance records found in ${runsDirectory}`);
  const hash = createHash('sha256').update(JSON.stringify({ version: 1, coreRoot, datasetRoot }));
  hash.update(await readFile(groupFile));
  hash.update(await readFile(generator));
  for (const file of files) hash.update(file).update(await readFile(path.join(runsDirectory, file)));
  return hash.digest('hex');
}

export async function planPerformanceData(options = {}) {
  const root = path.resolve(options.root ?? workspace);
  const registry = await loadRegistry(root, options.registryFile);
  const dataset = registry.sources.find(source => source.id === 'core-performance' && source.kind === 'dataset');
  if (!dataset) return null;
  const core = registry.sources.find(source => source.id === 'core');
  if (!core) throw new Error("Performance data requires registered source 'core'.");
  const coreRoot = (await loadSourceConfiguration(root, core, options)).root;
  const datasetRoot = (await loadSourceConfiguration(root, dataset, options)).root;
  const generator = path.join(coreRoot, '.github/scripts/perf-site-data.cjs');
  const groupFile = path.join(coreRoot, '.github/scripts/perf-groups.json');
  const runsDirectory = path.join(datasetRoot, 'runs');
  for (const file of [generator, groupFile]) await required(file);
  await required(runsDirectory, true);
  const groupsText = await readFile(groupFile, 'utf8');
  const groups = Object.keys(JSON.parse(groupsText));
  if (!groups.length || groups.some(name => !/^[a-z0-9-]+$/.test(name) || name === 'index')) {
    throw new Error(`Performance groups must contain nonempty, safe group names: ${groupFile}`);
  }
  const fingerprint = await inputFingerprint({ coreRoot, datasetRoot, generator, groupFile, runsDirectory });
  const outputDirectory = path.join(coreRoot, 'docs/static/perf-data');
  const outputFile = path.join(outputDirectory, 'index.json');
  const marker = path.join(root, '.nacara/performance-data/core-performance.json');
  const metadata = { sourceId: core.id, datasetId: dataset.id, coreRoot, datasetRoot, generator, groupFile,
    runsDirectory, outputDirectory, outputFile, groups, watchRoots: [runsDirectory, path.dirname(generator)],
    fingerprint, marker, env: options.env ?? process.env, commits: 0 };
  try {
    const cached = JSON.parse(await readFile(marker, 'utf8'));
    if (cached.fingerprint === fingerprint) {
      const output = await readOutputs(outputDirectory, groups);
      if (JSON.stringify(cached.hashes) === JSON.stringify(output.hashes)) return { ...metadata, commits: output.commits, cached: true };
    }
  } catch (error) {
    if (error.code !== 'ENOENT' && !(error instanceof SyntaxError) && !error.message.startsWith('Performance data has no commits:')) throw error;
  }
  return { ...metadata, cached: false };
}

export async function completePerformanceData(plan) {
  if (!plan) return null;
  const output = await readOutputs(plan.outputDirectory, plan.groups);
  if (await inputFingerprint(plan) !== plan.fingerprint) throw new Error('Performance data inputs changed after planning; prepare the build again.');
  if (!plan.cached) {
    await mkdir(path.dirname(plan.marker), { recursive: true });
    await writeFile(plan.marker, JSON.stringify({ fingerprint: plan.fingerprint, hashes: output.hashes }) + '\n');
  }
  return { ...plan, commits: output.commits };
}

export async function preparePerformanceData(options = {}) {
  const plan = await planPerformanceData(options);
  if (!plan || plan.cached) return plan;
  options.log?.(`Preparing performance charts from ${plan.datasetId}...`);
  try {
    await (options.run ?? execute)(process.execPath, [plan.generator, plan.runsDirectory, plan.outputFile], { cwd: plan.coreRoot, env: plan.env });
  } catch (error) {
    throw new Error(`Performance data generator failed: ${error.message}`, { cause: error });
  }
  return completePerformanceData(plan);
}
