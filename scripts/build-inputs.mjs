import { mkdir, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
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

