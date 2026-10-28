import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, writeFile, readFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';

const exec = promisify(execFile);
const properties = fileURLToPath(new URL('../../Directory.Build.props', import.meta.url));
const xml = value => value.replaceAll('&', '&amp;').replaceAll('"', '&quot;');

test('a Release solution keeps a referenced project outside the solution in Release', async t => {
  const root = await mkdtemp(path.join(tmpdir(), 'ranvier-solution-config-'));
  t.after(() => rm(root, { recursive: true, force: true }));
  await mkdir(path.join(root, 'app'));
  await mkdir(path.join(root, 'dependency'));
  await mkdir(path.join(root, 'leaf'));
  await writeFile(path.join(root, 'Directory.Build.props'), `<Project><Import Project="${xml(properties)}" /><ItemGroup><PackageReference Remove="FSharp.Core" /></ItemGroup></Project>`);
  await writeFile(path.join(root, 'app/App.csproj'), '<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><ProjectReference Include="../dependency/Dependency.csproj" /></ItemGroup></Project>');
  await writeFile(path.join(root, 'dependency/Directory.Build.props'), '<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>');
  await writeFile(path.join(root, 'dependency/Dependency.csproj'), '<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><ProjectReference Include="../leaf/Leaf.csproj" /></ItemGroup><Target Name="RecordConfiguration" AfterTargets="Build"><WriteLinesToFile File="$(MSBuildProjectDirectory)/observed.txt" Lines="$(Configuration)" Overwrite="true" /></Target></Project>');
  await writeFile(path.join(root, 'leaf/Leaf.csproj'), '<Project Sdk="Microsoft.NET.Sdk"><Target Name="RecordConfiguration" AfterTargets="Build"><WriteLinesToFile File="$(MSBuildProjectDirectory)/observed.txt" Lines="$(Configuration)" Overwrite="true" /></Target></Project>');
  await writeFile(path.join(root, 'Fixture.slnx'), '<Solution><Project Path="app/App.csproj" /></Solution>');
  await exec('dotnet', ['build', path.join(root, 'Fixture.slnx'), '-c', 'Release', '--nologo', '-v', 'quiet'],
    { windowsHide: true, maxBuffer: 1024 * 1024, env: { ...process.env, MSBUILDDISABLENODEREUSE: '1', DOTNET_CLI_USE_MSBUILD_SERVER: '0' } });
  assert.equal((await readFile(path.join(root, 'dependency/observed.txt'), 'utf8')).trim(), 'Release');
  await readFile(path.join(root, 'dependency/bin/Release/net10.0/Dependency.dll'));
  assert.equal((await readFile(path.join(root, 'leaf/observed.txt'), 'utf8')).trim(), 'Release');
  await readFile(path.join(root, 'leaf/bin/Release/net10.0/Leaf.dll'));
});
