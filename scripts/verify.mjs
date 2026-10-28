import { spawn } from 'node:child_process';
import { readdir, mkdir, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('..', import.meta.url));
const node = process.execPath;
const testFiles = (await Promise.all(['tests', 'tests/site'].map(async directory =>
  (await readdir(path.join(root, directory))).filter(name => name.endsWith('.test.mjs')).map(name => `${directory}/${name}`)))).flat();
const steps = [
  ['Restore build tools', 'dotnet', ['tool', 'restore']],
  ['Node regression suite', node, ['--test', ...testFiles]],
  ['Complete site build', node, ['scripts/build.mjs']],
  ['Release solution build', 'dotnet', ['build', 'Ranvier.Docs.slnx', '-c', 'Release', '--nologo']],
  ['Complete example suite', 'dotnet', ['run', '--project', 'tests/examples/Ranvier.Docs.Maps.Tests.fsproj', '-c', 'Release', '--no-build']],
  ['Complete theme suite', 'dotnet', ['run', '--project', 'tests/Nacara.Plugin.Ranvier.Tests/Nacara.Plugin.Ranvier.Tests.fsproj', '-c', 'Release', '--no-build']],
  ['Browser checker metadata', 'dotnet', ['fsi', 'tests/examples/MetadataTests.fsx']],
  ['Published links and assets', node, ['scripts/check-output.mjs']],
  ['Browser interaction suite', node, ['node_modules/@playwright/test/cli.js', 'test']]
];
const env = { ...process.env, RanvierTrace: 'true', CustomBeforeMicrosoftCommonTargets: path.join(root, 'src/SourceDependencies.targets') };
const results = [];
await mkdir(path.join(root, '.artifacts'), { recursive: true });
await writeFile(path.join(root, '.artifacts/verification.json'), JSON.stringify({ complete: false, results }, null, 2));
try {
  if (process.argv.length > 2) throw new Error('verify runs the complete acceptance gate and accepts no partial-run flags');
  for (const [name, command, args] of steps) {
    console.log(`\n${name}`);
    const startedAt = new Date().toISOString();
    const child = spawn(command, args, { cwd: root, stdio: 'inherit', windowsHide: true, env });
    const exitCode = await new Promise((resolve, reject) => { child.once('error', reject); child.once('exit', resolve); });
    results.push({ name, startedAt, completedAt: new Date().toISOString(), exitCode });
    await writeFile(path.join(root, '.artifacts/verification.json'), JSON.stringify({ complete: false, results }, null, 2));
    if (exitCode !== 0) throw new Error(`${name} failed (${exitCode ?? 'terminated'}).`);
  }
  await writeFile(path.join(root, '.artifacts/verification.json'), JSON.stringify({ complete: true, results }, null, 2));
  console.log('\nAll documentation acceptance gates passed.');
} catch (error) { console.error(error.message); process.exitCode = 1; }
