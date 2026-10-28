import { cp, mkdir, readFile, access, writeFile } from 'node:fs/promises';
import path from 'node:path';

export async function prepareLiveExampleAssets({ root, metadata }) {
  root = path.resolve(root);
  const baseUrl = metadata.baseUrl ?? '/';
  if (!/^\/(?:[A-Za-z0-9._~-]+\/)*$/.test(baseUrl)) throw new Error('Live examples require a deployment base with leading and trailing slashes.');
  const packageRoot = path.join(root, 'node_modules/animejs');
  let manifest;
  try { manifest = JSON.parse(await readFile(path.join(packageRoot, 'package.json'), 'utf8')); }
  catch (error) { if (error.code !== 'ENOENT') throw error; throw new Error('Live examples require animejs@4.5.0; run dotnet fsi build.fsx -- docs to restore internal dependencies.', { cause: error }); }
  if (manifest.version !== '4.5.0') throw new Error(`Live examples require animejs@4.5.0; found ${manifest.version}. Run dotnet fsi build.fsx -- docs to restore internal dependencies.`);
  const modules = path.join(packageRoot, 'dist/modules');
  await access(path.join(modules, 'index.js'));
  const generated = path.join(root, '.generated');
  const vendor = path.join(generated, 'static/assets/live-example/vendor/animejs');
  await mkdir(path.dirname(vendor), { recursive: true });
  await cp(modules, vendor, { recursive: true });
  await cp(path.join(packageRoot, 'LICENSE.md'), path.join(vendor, 'LICENSE.md'));
  const imports = { imports: { animejs: `${baseUrl}assets/live-example/vendor/animejs/index.js` } };
  const template = `<!doctype html><html><head><meta charset="utf-8">
<script type="importmap">${JSON.stringify(imports)}</script>
<style>body { margin: 0; background: var(--rv-canvas); color: var(--rv-text); font-family: system-ui, sans-serif; }</style>
</head><body></body></html>
`;
  const templatePath = path.join(generated, 'live/snippet.html');
  await mkdir(path.dirname(templatePath), { recursive: true });
  await writeFile(templatePath, template);
  return templatePath;
}
