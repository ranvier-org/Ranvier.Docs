import { readFile, writeFile, mkdir } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { prepareSite, parseArguments } from './import.mjs';
import { writeBuildInputs, watchFolders } from './build-inputs.mjs';
import { planBrowserBundles, completeBrowserBundles } from './browser-bundles.mjs';
import { planPerformanceData, completePerformanceData } from './performance-data.mjs';
import { prepareLiveExampleAssets } from './live-example-assets.mjs';

const root = fileURLToPath(new URL('..', import.meta.url));
const [mode, ...args] = process.argv.slice(2);
const options = { ...parseArguments(args), root };
const planFile = path.join(root, '.nacara/build-plan.json');

try {
  if (mode === 'plan') {
    const bundles = await planBrowserBundles(options);
    const performance = await planPerformanceData(options);
    await mkdir(path.dirname(planFile), { recursive: true });
    await writeFile(planFile, JSON.stringify({ bundles, performance }));
  } else if (mode === 'finish') {
    const plan = JSON.parse(await readFile(planFile, 'utf8'));
    await completeBrowserBundles(plan.bundles);
    const performance = await completePerformanceData(plan.performance);
    const metadata = await prepareSite(options);
    metadata.browserBundleRoots = plan.bundles.map(bundle => bundle.projectRoot);
    metadata.performanceWatchRoots = performance?.watchRoots ?? [];
    await writeBuildInputs(metadata);
    await prepareLiveExampleAssets({ root, metadata });
    await writeFile(path.join(root, '.generated/watch-roots.json'), JSON.stringify([...watchFolders(metadata), root]));
    console.log(`Prepared ${metadata.sources.length} repository collections through Partas.Build.`);
  } else throw new Error('Preparation is an internal Partas.Build helper; use dotnet fsi build.fsx -- docs.');
} catch (error) { console.error(error.message); process.exitCode = 1; }
