import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';

const exec = promisify(execFile);
const extension = fileURLToPath(new URL('../../src/SourceDependencies.targets', import.meta.url));
const xml = text => text.replaceAll('&', '&amp;').replaceAll('"', '&quot;');

test('the source bridge replaces traced and ordinary preset packages with one coherent source graph', async () => {
  const directory = await mkdtemp(path.join(tmpdir(), 'ranvier-bridge-'));
  const packages = ['Ranvier', 'Ranvier.View', 'Ranvier.View.Dom', 'Ranvier.View.Anime',
    'Fable.Ranvier.Anime', 'Ranvier.Css', 'Ranvier.Css.Bindings', 'Ranvier.Css.View', 'Ranvier.Elmish', 'Ranvier.Wire', 'Fable.Ranvier'];
  try {
    for (const suffix of ['', '.Traced', 'project']) {
      const project = path.join(directory, 'Probe.proj');
      const references = packages.map(name => suffix === 'project'
        ? `<ProjectReference Include="${xml(path.join(directory, 'wrong-checkout', name, `${name}.fsproj`))}" />`
        : `<PackageReference Include="${name}${name === 'Fable.Ranvier' ? '' : suffix}" />`).join('');
      await writeFile(project, `<Project><PropertyGroup><RanvierDocsRoot>${xml(directory)}</RanvierDocsRoot><RanvierRoot>${xml(path.join(directory, 'core'))}</RanvierRoot><RanvierViewRoot>${xml(path.join(directory, 'view'))}</RanvierViewRoot><RanvierCssRoot>${xml(path.join(directory, 'css'))}</RanvierCssRoot></PropertyGroup><ItemGroup>${references}<PackageReference Include="Third.Party" /></ItemGroup><Import Project="${xml(extension)}" /></Project>`);
      const { stdout } = await exec('dotnet', ['msbuild', project, '-getItem:ProjectReference,PackageReference', '-getProperty:RanvierTrace'], { windowsHide: true });
      const result = JSON.parse(stdout);
      assert.equal(result.Properties.RanvierTrace, 'true');
      assert.deepEqual(result.Items.PackageReference.map(item => item.Identity), ['Third.Party']);
      assert.equal(result.Items.ProjectReference.length, packages.length);
      for (const item of result.Items.ProjectReference) {
        const name = path.basename(item.Identity, '.fsproj');
        const id = name.startsWith('Ranvier.Css') ? 'css' : ['Ranvier', 'Ranvier.Elmish', 'Ranvier.Wire'].includes(name) ? 'core' : 'view';
        assert.equal(path.resolve(item.Identity), path.join(directory, id, 'src', name, `${name}.fsproj`));
      }
    }
  } finally { await rm(directory, { recursive: true, force: true }); }
});
