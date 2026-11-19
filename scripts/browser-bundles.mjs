import { spawn } from 'node:child_process';
import { mkdir, readFile, writeFile, access } from 'node:fs/promises';
import { createHash } from 'node:crypto';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { loadRegistry, loadSourceConfiguration } from './import.mjs';

const workspace = fileURLToPath(new URL('..', import.meta.url));
export const standardViewBundles = [
  { path: 'examples/Fable.Ranvier.Playground/dist', route: 'examples/playground' },
  { path: 'examples/Ranvier.Motion/dist', route: 'examples/motion' },
  { path: 'examples/Ranvier.Hydration/dist', route: 'examples/hydration' }
];

async function exists(file) {
  try { await access(file); return true; }
  catch (error) { if (error.code === 'ENOENT') return false; throw error; }
}

async function execute(command, args, options) {
  let shell = false;
  if (command === 'npm' && process.platform === 'win32') {
    const npm = process.env.npm_execpath ?? path.join(path.dirname(process.execPath), 'node_modules/npm/bin/npm-cli.js');
    if (await exists(npm)) { command = process.execPath; args = [npm, ...args]; }
    else { command = 'npm.cmd'; shell = true; }
  }
  const child = spawn(command, args, { cwd: options.cwd, env: options.env, windowsHide: true,
    shell, stdio: options.capture ? ['ignore', 'pipe', 'pipe'] : 'inherit' });
  let output = '', errors = '';
  child.stdout?.on('data', chunk => { output += chunk; });
  child.stderr?.on('data', chunk => { errors = (errors + chunk).slice(-131072); });
  return new Promise((resolve, reject) => {
    child.once('error', reject);
    child.once('close', (code, signal) => code === 0 ? resolve(output)
      : reject(new Error(`${command} ${args.join(' ')} failed in ${options.cwd} (${signal ?? code})${errors ? `:\n${errors}` : ''}`)));
  });
}

export function browserBuildEnvironment(root, roots, inherited = process.env) {
  return { ...inherited, RanvierTrace: 'true', RanvierDocsRoot: root,
    RANVIER_DOCS_ROOT: root,
    RANVIER_DOCS_CORE_ROOT: roots.core, RANVIER_DOCS_VIEW_ROOT: roots.view, RANVIER_DOCS_ANIME_ROOT: roots.anime, RANVIER_DOCS_CSS_ROOT: roots.css,
    RanvierRoot: roots.core, RanvierViewRoot: roots.view, RanvierAnimeRoot: roots.anime, RanvierCssRoot: roots.css,
    CustomBeforeMicrosoftCommonTargets: path.join(root, 'src/SourceDependencies.targets'),
    MSBUILDDISABLENODEREUSE: '1', DOTNET_CLI_USE_MSBUILD_SERVER: '0' };
}

async function fingerprint(root, roots, sourceId, bundle, run, env) {
  const hash = createHash('sha256');
  hash.update(JSON.stringify({ version: 1, node: process.versions.node, roots, sourceId, bundle }));
  for (const file of ['scripts/browser-bundles.mjs', 'src/SourceDependencies.targets', 'src/DependencyRoots.props', '.config/dotnet-tools.json']) {
    hash.update(file).update(await readFile(path.join(root, file)));
  }
  for (const [id, checkout] of Object.entries(roots)) {
    const paths = ['src', 'Directory.Build.props', 'Directory.Build.targets', 'Local.Build.props', 'nuget.config', 'global.json'];
    if (id === sourceId) paths.push(path.posix.dirname(bundle.path), 'examples/page-build.mjs', '.config/dotnet-tools.json');
    const files = (await run('git', ['ls-files', '--cached', '--others', '--exclude-standard', '-z', '--', ...paths],
      { cwd: checkout, env, capture: true })).split('\0').filter(Boolean).sort();
    for (const file of files) hash.update(id).update(file).update(await readFile(path.join(checkout, file)));
  }
  return hash.digest('hex');
}

export async function planBrowserBundles(options = {}) {
  const root = path.resolve(options.root ?? workspace);
  const registry = await loadRegistry(root, options.registryFile);
  const docsSources = registry.sources.filter(source => source.kind !== 'dataset');
  const candidateEntries = [];
  for (const source of docsSources) {
    const configuration = await loadSourceConfiguration(root, source, options);
    const requested = configuration.manifest.browserBundles ?? [];
    for (const bundle of requested) {
      const isStandardView = source.id === 'view' && standardViewBundles.some(standard => standard.path === bundle.path);
      if (!isStandardView && !bundle.build) continue;
      const build = bundle.build ?? { kind: 'fickl-vite' };
      candidateEntries.push({ source, checkout: configuration.root, bundle: { ...bundle, build } });
    }
  }
  if (!candidateEntries.length) return [];
  const baseRoots = {};
  for (const id of ['core', 'view', 'anime', 'css']) {
    const source = registry.sources.find(source => source.id === id);
    if (!source) throw new Error(`Standard View browser examples require registered source '${id}'.`);
    baseRoots[id] = (await loadSourceConfiguration(root, source, options)).root;
  }
  const run = options.run ?? execute;
  const cache = path.join(root, '.nacara/browser-bundles');
  const outputs = [];
  for (const { source, checkout, bundle } of candidateEntries) {
    const roots = source.id in baseRoots ? baseRoots : { ...baseRoots, [source.id]: checkout };
    const env = browserBuildEnvironment(root, roots, options.env);
    const projectRoot = path.resolve(checkout, path.posix.dirname(bundle.path));
    const outputRoot = path.resolve(checkout, bundle.path);
    const markerName = source.id === 'view' ? `${path.basename(projectRoot)}.json` : `${source.id}-${path.basename(projectRoot)}.json`;
    const marker = path.join(cache, markerName);
    const wanted = await fingerprint(root, roots, source.id, bundle, run, env);
    let cached = false;
    if (await exists(marker) && await exists(path.join(outputRoot, 'index.html'))) {
      try { cached = JSON.parse(await readFile(marker, 'utf8')).fingerprint === wanted; }
      catch (error) { if (!(error instanceof SyntaxError)) throw error; }
    }
    outputs.push({
      sourceId: source.id,
      ...bundle,
      projectRoot,
      outputRoot,
      cached,
      fingerprint: wanted,
      marker,
      env,
      root,
      roots,
      bundle
    });
  }
  return outputs;
}

export async function completeBrowserBundles(plan, options = {}) {
  const run = options.run ?? execute;
  for (const entry of plan) {
    if (!(await exists(path.join(entry.outputRoot, 'index.html')))) throw new Error(`${entry.path}: browser build did not produce dist/index.html`);
    const current = await fingerprint(entry.root, entry.roots, entry.sourceId, entry.bundle, run, entry.env);
    if (current !== entry.fingerprint) throw new Error(`${entry.path}: browser build inputs changed after planning; prepare the build again.`);
    if (!entry.cached) {
      await mkdir(path.dirname(entry.marker), { recursive: true });
      await writeFile(entry.marker, JSON.stringify({ fingerprint: entry.fingerprint, sourceRoot: entry.roots[entry.sourceId] }) + '\n');
    }
  }
  return plan;
}

export async function prepareBrowserBundles(options = {}) {
  const plan = await planBrowserBundles(options);
  const run = options.run ?? execute;
  for (const entry of plan) if (!entry.cached) {
    const scriptName = entry.build?.script ?? 'build';
    options.log?.(`Building ${entry.sourceId} browser example ${path.basename(entry.projectRoot)}...`);
    await run('npm', ['ci', '--no-audit', '--no-fund'], { cwd: entry.projectRoot, env: entry.env });
    await run('npm', ['run', scriptName], { cwd: entry.projectRoot, env: entry.env });
  }
  await completeBrowserBundles(plan, { run });
  return plan.map(({ sourceId, path: bundlePath, route, projectRoot, outputRoot, cached }) => ({ sourceId, path: bundlePath, route, projectRoot, outputRoot, cached }));
}
