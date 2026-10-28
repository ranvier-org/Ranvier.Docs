import { spawn } from 'node:child_process';
import { mkdir, writeFile } from 'node:fs/promises';
import { existsSync, watch } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { prepareSite, parseArguments } from './import.mjs';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const watching = process.argv.includes('--watch');
const importerArgs = process.argv.slice(2).filter(arg => arg !== '--watch');
const xml = value => String(value).replaceAll('&', '&amp;').replaceAll('"', '&quot;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');

export async function writeBuildInputs(metadata, destination = root) {
  const roots = [['core', 'RanvierRoot'], ['view', 'RanvierViewRoot'], ['css', 'RanvierCssRoot']];
  const properties = roots.map(([id, property]) => {
    const source = metadata.sources.find(source => source.id === id);
    if (!source?.root) throw new Error(`The shared example preset requires registered source '${id}'.`);
    return `<${property}>${xml(source.root)}</${property}>`;
  });
  await writeFile(path.join(destination, '.generated/DependencyRoots.props'), `<Project><PropertyGroup>${properties.join('')}</PropertyGroup></Project>\n`);
  const projects = [path.join(destination, 'src/Ranvier.Docs.Examples/Ranvier.Docs.Examples.fsproj'), ...metadata.sources.flatMap(source => source.examplesProject ? [source.examplesProject] : [])];
  const packages = new Map([['animejs', '4.5.0']]);
  for (const source of metadata.sources) for (const [name, version] of source.packages ?? []) {
    if (packages.has(name) && packages.get(name) !== version) throw new Error(`Conflicting versions for example package '${name}'.`);
    packages.set(name, version);
  }
  const project = `<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies><RanvierTrace>true</RanvierTrace></PropertyGroup><ItemGroup>${[...new Set(projects)].map(project => `<ProjectReference Include="${xml(path.resolve(destination, project))}" />`).join('')}</ItemGroup></Project>\n`;
  await writeFile(path.join(destination, '.generated/Examples.fsproj'), project);
  const assemblies = ['Fable.Core', 'Xantham.Fable.Core', 'Xantham.Fable.Core.TS', 'Ranvier', 'Ranvier.View', 'Ranvier.View.Dom', 'Fable.Ranvier.Anime', 'Ranvier.View.Anime', 'Ranvier.Css', 'Ranvier.Css.Bindings', 'Ranvier.Css.View', 'Ranvier.Docs.MapModel', 'Ranvier.Docs.Maps', 'Ranvier.Docs.Examples'];
  const helper = assemblies.map(name => `#r @"${path.join(destination, '.generated/bin/Release/net10.0', `${name}.dll`).replaceAll('"', '""')}"`).join('\n') + '\n';
  await mkdir(path.join(destination, '.generated/content'), { recursive: true });
  await writeFile(path.join(destination, '.generated/content/literate.fsx'), helper);
  return metadata;
}

export async function prepareBuild(options = {}) {
  const { prepareBrowserBundles } = await import('./browser-bundles.mjs');
  const bundles = await prepareBrowserBundles({ ...options, root });
  const { preparePerformanceData } = await import('./performance-data.mjs');
  const performanceData = await preparePerformanceData({ ...options, root });
  const metadata = await prepareSite({ ...options, root });
  metadata.browserBundleRoots = bundles.map(bundle => bundle.projectRoot);
  metadata.performanceWatchRoots = performanceData?.watchRoots ?? [];
  await writeBuildInputs(metadata);
  const { prepareLiveExampleAssets } = await import('./live-example-assets.mjs');
  await prepareLiveExampleAssets({ root, metadata });
  return metadata;
}

export function watchFolders(metadata, workspace = root) {
  const folders = new Set([path.join(workspace, 'src'), path.join(workspace, '.config')]);
  for (const folder of metadata.browserBundleRoots ?? []) folders.add(folder);
  for (const folder of metadata.performanceWatchRoots ?? []) folders.add(folder);
  for (const source of metadata.sources) {
    for (const folder of source.watchRoots ?? [path.join(source.root, 'docs'), path.resolve(source.root, source.originalContent ?? 'docs/content')]) folders.add(folder);
    if (['core', 'view', 'css'].includes(source.id)) folders.add(path.join(source.root, 'src'));
  }
  return [...folders];
}

async function main() {
  const options = parseArguments(importerArgs);
  let child;
  let watchers = [];
  let timer;
  let refreshing = false;
  let pending = false;
  let stopping = false;
  const run = args => {
    child = spawn('dotnet', args, { cwd: root, stdio: 'inherit', windowsHide: true, env: { ...process.env, RANVIER_DOCS_ROOT: root, RanvierTrace: 'true', CustomBeforeMicrosoftCommonTargets: path.join(root, 'src/SourceDependencies.targets') } });
    const active = child;
    return new Promise((resolve, reject) => { active.once('error', reject); active.once('exit', resolve); });
  };
  const stopChild = async () => {
    if (!child || child.exitCode !== null || child.signalCode !== null) return;
    const active = child;
    const exited = new Promise(resolve => active.once('exit', resolve));
    active.kill('SIGTERM');
    await exited;
  };
  const queueRefresh = () => {
    pending = true;
    clearTimeout(timer);
    timer = setTimeout(refresh, 300);
  };
  const observe = metadata => {
    for (const watcher of watchers) watcher.close();
    watchers = [];
    for (const folder of watchFolders(metadata)) if (existsSync(folder)) watchers.push(watch(folder, { recursive: true }, (_event, name) => {
      if (!name || !/(^|[/\\])(bin|obj|node_modules|output|dist|generated|generated-tests|\.nacara)([/\\]|$)/.test(String(name))) queueRefresh();
    }));
    watchers.push(watch(root, (_event, name) => { if (['sources.json', 'sources.lock.json', 'Directory.Build.props', 'Directory.Build.targets'].includes(String(name))) queueRefresh(); }));
    if (options.registryFile) {
      const registry = path.resolve(root, options.registryFile);
      watchers.push(watch(path.dirname(registry), (_event, name) => { if (String(name) === path.basename(registry)) queueRefresh(); }));
    }
  };
  async function refresh() {
    if (refreshing || stopping) return;
    refreshing = true;
    do {
      pending = false;
      try {
        await stopChild();
        const metadata = await prepareBuild(options);
        if (watching) observe(metadata);
        const code = await run(['build', 'src/Ranvier.Docs.Site/Ranvier.Docs.Site.fsproj', '-c', 'Release', '--nologo']);
        if (code !== 0) throw new Error(`Site compilation failed (${code}).`);
        if (stopping) break;
        const completion = run(['src/Ranvier.Docs.Site/bin/Release/net10.0/Ranvier.Docs.Site.dll', watching ? 'watch' : 'build']);
        if (watching) completion.then(code => { if (!refreshing && !stopping && code !== 0) console.error(`Preview stopped (${code}). Edit a source file to retry.`); });
        else process.exitCode = (await completion) ?? 1;
      } catch (error) { console.error(error.message); if (!watching || watchers.length === 0) process.exitCode = 1; }
    } while (pending && !stopping);
    refreshing = false;
  }
  const stop = async () => {
    stopping = true;
    clearTimeout(timer);
    for (const watcher of watchers) watcher.close();
    await stopChild();
  };
  process.once('SIGINT', stop);
  process.once('SIGTERM', stop);
  await refresh();
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) main().catch(error => { console.error(error.message); process.exitCode = 1; });
