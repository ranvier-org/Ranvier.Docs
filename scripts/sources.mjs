import { mkdir, readFile, writeFile, rename } from 'node:fs/promises';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { exists, loadRegistry, parseArguments } from './import.mjs';

const exec = promisify(execFile);
async function git(root, args) {
  try { return (await exec('git', ['-C', root, ...args], { windowsHide: true, maxBuffer: 8 * 1024 * 1024 })).stdout.trim(); }
  catch (error) { throw new Error(`git ${args.join(' ')} failed in ${root}: ${error.stderr?.trim() ?? error.message}`, { cause: error }); }
}

export async function acquireSources(options = {}) {
  const root = path.resolve(options.root ?? fileURLToPath(new URL('..', import.meta.url)));
  const registry = await loadRegistry(root, options.registryFile);
  for (const id of options.refreshIds ?? []) if (!registry.sources.some(s => s.id === id && !s.self)) throw new Error(`Unknown acquired source to refresh: ${id}`);
  const lockFile = path.join(root, 'sources.lock.json');
  const oldLock = await exists(lockFile) ? JSON.parse(await readFile(lockFile, 'utf8')) : { version: 1, sources: {} };
  if (oldLock.version !== 1 || !oldLock.sources) throw new Error('Unsupported sources.lock.json format');
  const lock = { version: 1, sources: {} };
  for (const source of registry.sources.filter(s => !s.self)) {
    const previous = oldLock.sources[source.id];
    if (Object.hasOwn(options.sourceRoots ?? {}, source.id) || (options.preferLocal && source.local)) {
      if (previous) lock.sources[source.id] = previous;
      options.onProgress?.(`${source.id}: using selected local checkout`);
      continue;
    }
    const refresh = options.refresh === true || (options.refreshIds ?? []).includes(source.id);
    const directory = path.join(root, '.sources', source.id);
    const remote = options.repositoryUrls?.[source.id] ?? `https://github.com/${source.repository}.git`;
    const existingCheckout = await exists(directory);
    if (existingCheckout) {
      if (!(await exists(path.join(directory, '.git')))) throw new Error(`${source.id}: acquired checkout is not a Git repository: ${directory}`);
      if (await git(directory, ['status', '--porcelain'])) throw new Error(`${source.id}: acquired checkout is dirty; preserve or commit uncommitted changes before fetching`);
      const configured = await git(directory, ['remote', 'get-url', 'origin']);
      if (configured !== remote) throw new Error(`${source.id}: acquired checkout origin differs from ${remote}`);
    } else {
      await mkdir(directory, { recursive: true });
      await git(directory, ['init']);
      await git(directory, ['remote', 'add', 'origin', remote]);
    }
    if (previous && (previous.repository !== source.repository || previous.ref !== source.ref) && !refresh) {
      throw new Error(`${source.id}: registry differs from lock; run dotnet fsi build.fsx -- sources --refresh ${source.id} to accept the new repository/ref`);
    }
    const ref = !refresh && previous ? previous.revision : source.ref;
    if (!ref || typeof ref !== 'string' || ref.startsWith('-')) throw new Error(`${source.id}: invalid source ref`);
    const currentRevision = !refresh && previous && existingCheckout
      ? await git(directory, ['rev-parse', '--verify', 'HEAD']).catch(() => null) : null;
    if (!refresh && previous && currentRevision === previous.revision) {
      lock.sources[source.id] = previous;
      options.onProgress?.(`${source.id}: ${previous.revision.slice(0, 12)} (locked checkout ready)`);
      continue;
    }
    await git(directory, ['fetch', '--depth=1', 'origin', ref]);
    await git(directory, ['checkout', '--detach', 'FETCH_HEAD']);
    const revision = await git(directory, ['rev-parse', 'HEAD']);
    if (!refresh && previous && revision !== previous.revision) throw new Error(`${source.id}: fetched revision does not match sources.lock.json`);
    lock.sources[source.id] = { repository: source.repository, ref: source.ref, revision };
    options.onProgress?.(`${source.id}: ${revision.slice(0, 12)}`);
  }
  const temporary = `${lockFile}.next-${process.pid}`;
  await writeFile(temporary, JSON.stringify(lock, null, 2) + '\n');
  await rename(temporary, lockFile);
  return lock;
}

if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
  try {
    const args = process.argv.slice(2);
    const options = { onProgress: console.log };
    const sourceArgs = [];
    for (let i = 0; i < args.length; i++) {
      if (args[i] !== '--refresh') { sourceArgs.push(args[i]); continue; }
      if (args[i + 1] && !args[i + 1].startsWith('--')) options.refreshIds = [...(options.refreshIds ?? []), ...args[++i].split(',')];
      else options.refresh = true;
    }
    Object.assign(options, parseArguments(sourceArgs));
    const lock = await acquireSources(options);
    console.log(`Acquired ${Object.keys(lock.sources).length} repositories; exact revisions recorded in sources.lock.json`);
  } catch (error) { console.error(error.message); process.exitCode = 1; }
}
