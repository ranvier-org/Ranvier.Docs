import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, readFile, rm } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { writeBuildInputs, watchFolders } from '../../scripts/build.mjs';

test('build inputs preserve portable dependency roots and project extensions without treating npm packages as NuGet', async () => {
  const root = await mkdtemp(path.join(os.tmpdir(), 'ranvier-build-inputs-'));
  try {
    await mkdir(path.join(root, '.generated'));
    const metadata = { sources: [
      { id: 'core', root: path.join(root, 'core & sources') },
      { id: 'view', root: path.join(root, 'view') },
      { id: 'css', root: path.join(root, 'css'), examplesProject: 'examples/Custom.fsproj', packages: [['animejs', '4.5.0']] }
    ] };
    await writeBuildInputs(metadata, root);
    const properties = await readFile(path.join(root, '.generated/DependencyRoots.props'), 'utf8');
    const project = await readFile(path.join(root, '.generated/Examples.fsproj'), 'utf8');
    const helper = await readFile(path.join(root, '.generated/content/literate.fsx'), 'utf8');
    assert.match(properties, /core &amp; sources/);
    assert.match(properties, /<RanvierViewRoot>/);
    assert.match(properties, /<RanvierCssRoot>/);
    assert.ok(project.includes(path.join(root, 'examples/Custom.fsproj')));
    assert.match(project, /Ranvier.Docs.Examples.fsproj/);
    assert.doesNotMatch(project, /PackageReference|animejs/);
    assert.match(project, /<RanvierTrace>true/);
    assert.match(project, /<CopyLocalLockFileAssemblies>true/);
    assert.match(helper, /Ranvier.Docs.MapModel.dll/);
    assert.match(helper, /Ranvier.View.Anime.dll/);
    assert.ok(helper.includes(path.join(root, '.generated/bin/Release/net10.0/Ranvier.dll')));
  } finally { await rm(root, { recursive: true, force: true }); }
});

test('build inputs fail clearly for missing shared libraries or conflicting browser packages', async () => {
  await assert.rejects(writeBuildInputs({ sources: [] }), /requires registered source 'core'/);
  const metadata = { sources: [
    { id: 'core', root: '/core', packages: [['animejs', '4.5.0']] },
    { id: 'view', root: '/view', packages: [['animejs', '5.0.0']] },
    { id: 'css', root: '/css' }
  ] };
  const root = await mkdtemp(path.join(os.tmpdir(), 'ranvier-conflict-'));
  try {
    await mkdir(path.join(root, '.generated'));
    await assert.rejects(writeBuildInputs(metadata, root), /Conflicting versions/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test('preview watches library APIs, central infrastructure and arbitrary manifest asset roots', () => {
  const workspace = path.resolve('/workspace');
  const core = path.join(workspace, '.sources/core');
  const customAssets = path.join(workspace, 'custom-assets');
  const browserProject = path.join(workspace, '.sources/view/examples/Ranvier.Motion');
  const dataset = path.join(workspace, '.sources/core-performance');
  const folders = watchFolders({ browserBundleRoots: [browserProject], performanceWatchRoots: [dataset], sources: [
    { id: 'core', root: core, watchRoots: [path.join(core, 'docs'), customAssets] },
    { id: 'view', root: path.join(workspace, '.sources/view') },
    { id: 'css', root: path.join(workspace, '.sources/css') }
  ] }, workspace);
  assert.ok(folders.includes(path.join(core, 'src')));
  assert.ok(folders.includes(path.join(workspace, '.sources/view/src')));
  assert.ok(folders.includes(path.join(workspace, '.sources/css/src')));
  assert.ok(folders.includes(path.join(workspace, 'src')));
  assert.ok(folders.includes(path.join(workspace, '.config')));
  assert.ok(folders.includes(customAssets));
  assert.ok(folders.includes(browserProject));
  assert.ok(folders.includes(dataset));
});
