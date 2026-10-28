import assert from 'node:assert/strict';
import { test } from 'node:test';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { readFile } from 'node:fs/promises';

const exec = promisify(execFile);
const workspace = new URL('../..', import.meta.url);
const root = await import('node:url').then(({ fileURLToPath }) => fileURLToPath(workspace));
const explain = async args => {
  const { stdout } = await exec('dotnet', ['fsi', 'build.fsx', '--', ...args, '--explain', '--json'], {
    cwd: root, windowsHide: true, maxBuffer: 2 * 1024 * 1024, env: { ...process.env, CI: 'true' }
  });
  return JSON.parse(stdout.trim());
};

function stages(tree) {
  return tree.pipelines.flatMap(pipeline => pipeline.stages.flatMap(function visit(stage) {
    return [stage, ...stage.steps.flatMap(step => step.stage ? visit(step.stage) : [])];
  }));
}

test('the actual Partas command accepts repeated source paths and previews without test stages', async () => {
  const tree = await explain(['docs', '--watch', '--source', 'view=C:/A Checkout/View', '--source', 'css=C:/Another Checkout/Css']);
  const resolved = stages(tree);
  assert.equal(resolved.find(stage => stage.name === 'preview').status, 'active');
  assert.ok(!resolved.some(stage => stage.name.startsWith('test ')));
  const preparation = resolved.find(stage => stage.name === 'prepare source content and runnable assets');
  assert.ok(preparation.steps[0].label.includes('"view=C:/A Checkout/View"'));
  assert.ok(preparation.steps[0].label.includes('"css=C:/Another Checkout/Css"'));
});

test('full verification and focused tests share the Partas tree used by CI', async () => {
  const full = stages(await explain(['verify']));
  for (const suite of ['node', 'examples', 'theme', 'metadata', 'browser']) assert.ok(full.some(stage => stage.name === `test ${suite}`));
  const focused = stages(await explain(['test', '--suite', 'theme']));
  assert.deepEqual(focused.filter(stage => stage.name.startsWith('test ')).map(stage => stage.name), ['test theme']);
  const workflow = await readFile(new URL('.github/workflows/docs.yml', workspace), 'utf8');
  assert.ok(workflow.includes('dotnet fsi build.fsx -- verify'));
  assert.ok(!/run: (?:npm|npx)/.test(workflow));
});

test('compiler and test stages carry their source environment when the script is invoked elsewhere', async () => {
  const script = new URL('tests/site/PartasEnvironment.fsx', workspace);
  const { fileURLToPath } = await import('node:url');
  const { stdout } = await exec('dotnet', ['fsi', fileURLToPath(script)], { cwd: new URL('..', workspace), windowsHide: true });
  assert.ok(stdout.includes('Coordinated compilation environment verified.'));
});
