import { readFile, writeFile, mkdir, readdir, stat, lstat, realpath, rename, rm, cp } from 'node:fs/promises';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { fromMarkdown } from 'mdast-util-from-markdown';
import { parseFragment } from 'parse5';

const exec = promisify(execFile);
const slash = value => value.replaceAll('\\', '/');
const pageExtension = /\.(md|fsx)$/i;
const ignoredDirectories = new Set(['.git', 'bin', 'obj', 'node_modules', 'output', '.SageFs']);

export async function exists(file) {
  try { await accessPath(file); return true; } catch (error) { if (error.code === 'ENOENT') return false; throw error; }
}

async function accessPath(file) { await stat(file); }

export function relativePath(value, label) {
  if (typeof value !== 'string' || !value || value.includes('\\') || value.includes('\0') ||
      path.posix.isAbsolute(value) || /^[a-z]:/i.test(value) || value.split('/').some(p => p === '..' || p === '.')) {
    throw new Error(`${label} must be a nonempty relative path inside its repository: ${value}`);
  }
  return value;
}

export function validateRegistry(input) {
  if (input.version !== 1 || !Array.isArray(input.sources) || !input.sources.length) {
    throw new Error('sources.json requires version: 1 and a nonempty sources array');
  }
  const ids = new Set(), routes = new Set();
  const sources = input.sources.map(source => {
    if (!/^[a-z][a-z0-9-]*$/.test(source.id)) throw new Error(`Invalid source id: ${source.id}`);
    if (ids.has(source.id)) throw new Error(`Duplicate source id: ${source.id}`);
    ids.add(source.id);
    if (!/^[\w.-]+\/[\w.-]+$/.test(source.repository)) throw new Error(`Invalid repository for ${source.id}: use owner/name`);
    if (source.kind !== undefined && !['docs', 'dataset'].includes(source.kind)) throw new Error(`Invalid source kind for ${source.id}: ${source.kind}`);
    const route = source.route ?? source.id;
    if (!/^[a-z][a-z0-9-]*$/.test(route)) throw new Error(`Invalid route for ${source.id}: ${route}`);
    if (routes.has(route)) throw new Error(`Duplicate route: ${route}`);
    routes.add(route);
    if (source.manifest) relativePath(source.manifest, `${source.id}.manifest`);
    if (source.content) relativePath(source.content, `${source.id}.content`);
    return { ...source, route, ref: source.ref ?? 'HEAD' };
  });
  let baseUrl = input.baseUrl ?? '/';
  if (typeof baseUrl !== 'string' || !/^\/(?:[A-Za-z0-9._~-]+\/)*$/.test(baseUrl)) {
    throw new Error('baseUrl must be / or a URL path with leading and trailing slashes');
  }
  return { ...input, baseUrl, title: input.title ?? 'Ranvier', sources };
}

export async function loadRegistry(root, registryFile = 'sources.json') {
  return validateRegistry(JSON.parse(await readFile(path.resolve(root, registryFile), 'utf8')));
}

async function walk(directory, prefix = '') {
  const result = [];
  for (const entry of (await readdir(directory, { withFileTypes: true })).sort((a, b) => a.name.localeCompare(b.name))) {
    const name = prefix ? `${prefix}/${entry.name}` : entry.name;
    if (entry.isSymbolicLink()) throw new Error(`Symbolic links are not supported in imported content: ${directory}/${name}`);
    if (entry.isDirectory() && !ignoredDirectories.has(entry.name)) result.push(...await walk(path.join(directory, entry.name), name));
    else if (entry.isFile()) result.push(name);
  }
  return result;
}

async function within(root, name, label, optional = false) {
  relativePath(name, label);
  const target = path.resolve(root, name);
  if (!(await exists(target))) {
    if (optional) return null;
    throw new Error(`${label} does not exist: ${target}`);
  }
  const resolvedRoot = await realpath(root), resolvedTarget = await realpath(target);
  const relative = path.relative(resolvedRoot, resolvedTarget);
  if (relative.startsWith('..') || path.isAbsolute(relative)) throw new Error(`${label} escapes repository root: ${name}`);
  return target;
}

function titleOf(text, fallback) {
  const frontMatter = /^---\r?\n([\s\S]*?)\r?\n---(?:\r?\n|$)/.exec(text);
  const title = frontMatter && /^title:\s*(.*?)\s*$/m.exec(frontMatter[1]);
  if (title) return title[1].replace(/^['"]|['"]$/g, '');
  return /^#\s+(.+?)\s*#*\s*$/m.exec(text)?.[1] ?? fallback.replaceAll('-', ' ');
}

function excluded(file, patterns) {
  return patterns.some(pattern => {
    const expression = pattern.split('**').map(part => part.split('*').map(p => p.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')).join('[^/]*')).join('.*');
    return new RegExp(`^${expression}$`).test(file);
  });
}

async function revisionOf(root, allowUnversioned) {
  try {
    const revision = (await exec('git', ['-C', root, 'rev-parse', 'HEAD'], { windowsHide: true })).stdout.trim();
    const dirty = Boolean((await exec('git', ['-C', root, 'status', '--porcelain'], { windowsHide: true })).stdout.trim());
    return { revision, dirty };
  } catch (error) {
    if (allowUnversioned) return { revision: null, dirty: true };
    throw new Error(`Cannot record Git revision for ${root}; commit the source or use --allow-unversioned for a local preview`, { cause: error });
  }
}

export async function loadSourceConfiguration(root, registration, options = {}) {
  const override = options.sourceRoots?.[registration.id];
  const local = registration.local && path.resolve(root, registration.local);
  const checkout = override ? path.resolve(root, override) :
    registration.self ? root : options.preferLocal && local ? local : path.join(root, '.sources', registration.id);
  if (!(await exists(checkout))) throw new Error(`${registration.id}: checkout missing at ${checkout}; run npm run sources:fetch or use --local`);
  const manifestPath = await within(checkout, registration.manifest ?? 'docs/ranvier-docs.json', `${registration.id}.manifest`, true);
  const sourceManifest = manifestPath ? JSON.parse(await readFile(manifestPath, 'utf8')) : {};
  if (!sourceManifest || typeof sourceManifest !== 'object' || Array.isArray(sourceManifest)) throw new Error(`${registration.id}: manifest must be an object`);
  if (registration.defaults && (typeof registration.defaults !== 'object' || Array.isArray(registration.defaults))) throw new Error(`${registration.id}: defaults must be a manifest object`);
  const manifest = { ...(registration.defaults ?? {}), ...sourceManifest };
  if (manifest.version !== undefined && manifest.version !== 1) throw new Error(`${registration.id}: unsupported manifest version ${manifest.version}`);
  const known = new Set(['$schema', 'version', 'title', 'content', 'static', 'index', 'sections', 'unlisted', 'exclude', 'examplesProject', 'packages', 'browserBundles', 'description']);
  for (const key of Object.keys(manifest)) if (!known.has(key)) throw new Error(`${registration.id}: unknown manifest option ${key}`);
  return { root: checkout, manifest, manifestPath };
}

async function readSource(root, registration, options) {
  const { root: checkout, manifest } = await loadSourceConfiguration(root, registration, options);
  const content = manifest.content ?? registration.content ?? 'docs/content';
  const contentRoot = await within(checkout, content, `${registration.id}.content`);
  const exclude = manifest.exclude ?? [];
  if (!Array.isArray(exclude) || exclude.some(p => typeof p !== 'string')) throw new Error(`${registration.id}.exclude must be an array of glob strings`);
  const files = (await walk(contentRoot)).filter(file => !excluded(file, exclude));
  const pages = [];
  for (const file of files.filter(f => pageExtension.test(f))) {
    pages.push({ path: file, title: titleOf(await readFile(path.join(contentRoot, file), 'utf8'), path.basename(file).replace(pageExtension, '')) });
  }
  if (!pages.length) throw new Error(`${registration.id}: no Markdown or literate F# pages found in ${content}`);
  const index = manifest.index ?? (pages.find(p => p.path === 'index.md')?.path ?? pages.find(p => p.path === 'README.md')?.path ?? pages[0].path);
  if (!pages.some(p => p.path === index)) throw new Error(`${registration.id}: index page does not exist: ${index}`);
  const unlisted = manifest.unlisted ?? [];
  if (!Array.isArray(unlisted)) throw new Error(`${registration.id}.unlisted must be an array`);
  for (const page of unlisted) if (!pages.some(p => p.path === page)) throw new Error(`${registration.id}: unlisted page does not exist: ${page}`);
  const sections = manifest.sections ? manifest.sections.map(section => {
    if (typeof section.title !== 'string' || !section.title || !Array.isArray(section.pages)) throw new Error(`${registration.id}: each section requires title and pages`);
    return { title: section.title, pages: section.pages.map(page => {
      const entry = typeof page === 'string' ? { path: page } : page;
      if (!entry || typeof entry.path !== 'string') throw new Error(`${registration.id}: section page requires a path`);
      if (!pages.some(p => p.path === entry.path)) throw new Error(`${registration.id}: navigation page does not exist: ${entry.path}`);
      return { path: entry.path, title: entry.title ?? pages.find(p => p.path === entry.path).title };
    }) };
  }) : [{ title: manifest.title ?? registration.title ?? registration.repository.split('/')[1], pages: pages.filter(p => !unlisted.includes(p.path)) }];
  const listed = sections.flatMap(section => section.pages.map(page => page.path));
  if (listed.length !== new Set(listed).size) throw new Error(`${registration.id}: a page appears more than once in navigation`);
  for (const page of pages) if (!listed.includes(page.path) && !unlisted.includes(page.path)) throw new Error(`${registration.id}: ${page.path} is missing from navigation; list it in a section or unlisted`);
  const staticName = manifest.static === false ? null : manifest.static ?? 'docs/static';
  const staticRoot = staticName ? await within(checkout, staticName, `${registration.id}.static`, true) : null;
  const examplesProject = manifest.examplesProject ? await within(checkout, manifest.examplesProject, `${registration.id}.examplesProject`) : null;
  if (examplesProject && !examplesProject.endsWith('.fsproj')) throw new Error(`${registration.id}.examplesProject must name an .fsproj`);
  const packages = manifest.packages ?? [];
  if (!Array.isArray(packages) || packages.some(p => !Array.isArray(p) || p.length !== 2 || p.some(x => typeof x !== 'string' || !x))) throw new Error(`${registration.id}.packages must contain [npm-name, version] pairs`);
  const browserBundles = [];
  for (const bundle of manifest.browserBundles ?? []) {
    const source = await within(checkout, bundle.path, `${registration.id}.browserBundles.path`);
    relativePath(bundle.route, `${registration.id}.browserBundles.route`);
    browserBundles.push({ path: source, route: bundle.route });
  }
  return { ...registration, title: manifest.title ?? registration.title ?? registration.repository.split('/')[1],
    description: manifest.description ?? registration.description ?? '', root: checkout, content, originalContentRoot: contentRoot,
    files, pages, index, sections, unlisted, staticRoot, examplesProject, packages, browserBundles,
    ...await revisionOf(checkout, options.allowUnversioned || registration.self) };
}

function markdownRegions(text, file) {
  if (!file.endsWith('.fsx')) {
    const frontMatter = /^---\r?\n[\s\S]*?\r?\n---(?:\r?\n|$)/.exec(text);
    return [{ start: frontMatter?.[0].length ?? 0, end: text.length }];
  }
  const regions = [];
  for (let i = 0; i < text.length;) {
    if (text.startsWith('(*)', i)) i += 3;
    else if (text.startsWith('//', i)) { const end = text.indexOf('\n', i); i = end < 0 ? text.length : end + 1; }
    else if (text.startsWith('(*', i)) {
      const start = i, documentation = text.startsWith('(**', i) && text[i + 3] !== '*';
      let depth = 1; i += 2;
      while (i < text.length && depth) {
        if (text.startsWith('(*', i)) { depth++; i += 2; }
        else if (text.startsWith('*)', i)) { depth--; i += 2; }
        else i++;
      }
      if (documentation && depth === 0) regions.push({ start: start + 3, end: i - 2 });
    } else if (text.startsWith('"""', i)) {
      const end = text.indexOf('"""', i + 3); i = end < 0 ? text.length : end + 3;
    } else if (text.startsWith('@"', i)) {
      i += 2;
      while (i < text.length) {
        if (text.startsWith('""', i)) i += 2;
        else if (text[i++] === '"') break;
      }
    } else if (text[i] === '"') {
      i++;
      while (i < text.length) { if (text[i] === '\\') i += 2; else if (text[i++] === '"') break; }
    } else if (text.startsWith('``', i)) {
      const end = text.indexOf('``', i + 2); i = end < 0 ? text.length : end + 2;
    } else if (text[i] === "'" && /^'(?:\\[^\n]+?|[^'\n])'/.test(text.slice(i))) {
      i += /^'(?:\\[^\n]+?|[^'\n])'/.exec(text.slice(i))[0].length;
    } else i++;
  }
  return regions;
}

function visit(node, callback) {
  callback(node);
  for (const child of node.children ?? []) visit(child, callback);
}

function plainText(node) {
  if (node.type === 'html') return '';
  return node.value ?? node.alt ?? (node.children ?? []).map(plainText).join('');
}

function destinationSpan(text, node) {
  const start = node.position.start.offset, raw = text.slice(start, node.position.end.offset);
  let i = raw[0] === '!' ? 1 : 0, depth = 0;
  for (; i < raw.length; i++) {
    if (raw[i] === '\\') { i++; continue; }
    if (raw[i] === '[') depth++;
    if (raw[i] === ']' && --depth === 0) { i++; break; }
  }
  i += 1;
  while (/\s/.test(raw[i] ?? '') && i < raw.length) i++;
  const angle = raw[i] === '<';
  if (angle) i++;
  const begin = i;
  let parentheses = 0;
  for (; i < raw.length; i++) {
    if (raw[i] === '\\') { i++; continue; }
    if (angle ? raw[i] === '>' : /\s/.test(raw[i]) || raw[i] === ')' && parentheses === 0) break;
    if (!angle && raw[i] === '(') parentheses++;
    if (!angle && raw[i] === ')') parentheses--;
  }
  return { start: start + begin, end: start + i };
}

export function headingIds(text, file = 'page.md') {
  const ids = new Set();
  const counts = new Map();
  for (const region of markdownRegions(text, file)) visit(fromMarkdown(text.slice(region.start, region.end)), node => {
    if (node.type === 'heading') {
      const heading = plainText(node);
      const explicit = /\{#([^}]+)\}\s*$/.exec(heading);
      const id = explicit?.[1] ?? heading
        .replace(/[^\p{L}\p{N}\s_-]/gu, '').trim().toLowerCase().replace(/\s/g, '-');
      const duplicate = counts.get(id) ?? 0;
      counts.set(id, duplicate + 1);
      ids.add(duplicate ? `${id}-${duplicate}` : id);
    }
    if (node.type === 'html') for (const match of node.value.matchAll(/\b(?:id|name)=["']([^"']+)["']/g)) ids.add(match[1]);
  });
  return ids;
}

function rewriteLinks(text, source, file, sources, pageText, baseUrl) {
  const legacyUrl = value => {
    if (value === baseUrl || sources.some(candidate => value === baseUrl + candidate.route || value.startsWith(baseUrl + candidate.route + '/'))) return null;
    const destination = sources.find(candidate => {
      const prefix = '/' + candidate.repository.split('/')[1];
      return value === prefix || value.startsWith(prefix + '/');
    });
    if (!destination) return null;
    const prefix = '/' + destination.repository.split('/')[1];
    return baseUrl + destination.route + '/' + value.slice(prefix.length).replace(/^\//, '');
  };
  const resolveLink = (target, line) => {
    const match = /^repo:([a-z][a-z0-9-]*)\/(.*?)(#[^\s]*)?$/.exec(target);
    if (!match) throw new Error(`${source.id}/${file}:${line}: invalid repository link ${target}`);
    const [, id, rawFile, fragment = ''] = match;
    const destination = sources.find(s => s.id === id);
    let decoded;
    try { decoded = decodeURIComponent(rawFile); } catch { throw new Error(`${source.id}/${file}:${line}: invalid URL encoding in ${target}`); }
    relativePath(decoded, `${source.id}/${file}:${line}: repo link`);
    if (!destination || !destination.pages.some(p => p.path === decoded)) throw new Error(`${source.id}/${file}:${line}: missing repository page ${id}/${decoded}`);
    if (fragment && !headingIds(pageText.get(`${id}/${decoded}`), decoded).has(decodeURIComponent(fragment.slice(1)))) {
      throw new Error(`${source.id}/${file}:${line}: missing heading ${target}`);
    }
    const current = `${source.route}/${file}`;
    const destinationPath = `${destination.route}/${decoded}`;
    return slash(path.posix.relative(path.posix.dirname(current), destinationPath)).split('/').map(encodeURIComponent).join('/') + fragment;
  };
  const replacements = [];
  for (const region of markdownRegions(text, file)) {
    const part = text.slice(region.start, region.end);
    visit(fromMarkdown(part), node => {
      if (['link', 'image', 'definition'].includes(node.type)) {
        const span = destinationSpan(part, node);
        const start = region.start + span.start;
        const line = text.slice(0, start).split('\n').length;
        const value = node.url.startsWith('repo:') ? resolveLink(node.url, line) : legacyUrl(node.url);
        if (value !== null) replacements.push({ start, end: region.start + span.end, value });
      }
      if (node.type === 'html') {
        const baseOffset = region.start + node.position.start.offset;
        const inspect = element => {
          for (const attribute of element.attrs ?? []) {
            if (!['href', 'src', 'poster'].includes(attribute.name)) continue;
            const value = legacyUrl(attribute.value);
            if (value === null) continue;
            const key = attribute.prefix ? `${attribute.prefix}:${attribute.name}` : attribute.name;
            const location = element.sourceCodeLocation?.attrs?.[key];
            if (!location) continue;
            const raw = node.value.slice(location.startOffset, location.endOffset);
            const beginning = /^([^=]+\s*=\s*)(["']?)/.exec(raw);
            if (!beginning) continue;
            const quote = beginning[2];
            const escaped = value.replaceAll('&', '&amp;').replaceAll(quote === "'" ? "'" : '"', quote === "'" ? '&#39;' : '&quot;');
            replacements.push({ start: baseOffset + location.startOffset, end: baseOffset + location.endOffset,
              value: beginning[1] + quote + escaped + quote });
          }
          for (const child of element.childNodes ?? []) inspect(child);
          if (element.content) inspect(element.content);
        };
        inspect(parseFragment(node.value, { sourceCodeLocationInfo: true }));
      }
    });
  }
  let result = text;
  for (const replacement of replacements.sort((a, b) => b.start - a.start)) {
    result = result.slice(0, replacement.start) + replacement.value + result.slice(replacement.end);
  }
  return result;
}

export async function prepareSite(options = {}) {
  const root = path.resolve(options.root ?? fileURLToPath(new URL('..', import.meta.url)));
  const registry = await loadRegistry(root, options.registryFile);
  const sources = [];
  const datasets = [];
  for (const source of registry.sources) {
    if (source.kind === 'dataset') {
      const configuration = await loadSourceConfiguration(root, source, options);
      datasets.push({ id: source.id, repository: source.repository, ref: source.ref, root: configuration.root,
        ...await revisionOf(configuration.root, options.allowUnversioned) });
    } else sources.push(await readSource(root, source, options));
  }
  const pageText = new Map();
  for (const source of sources) for (const page of source.pages) {
    pageText.set(`${source.id}/${page.path}`, await readFile(path.join(source.originalContentRoot, page.path), 'utf8'));
  }
  const staging = path.join(root, `.generated-next-${process.pid}-${Date.now()}`);
  await mkdir(staging, { recursive: true });
  try {
    for (const source of sources) {
      for (const file of source.files) {
        const output = path.join(staging, 'content', source.route, file);
        await mkdir(path.dirname(output), { recursive: true });
        if (pageExtension.test(file)) {
          let markdown = rewriteLinks(pageText.get(`${source.id}/${file}`), source, file, sources, pageText, registry.baseUrl);
          if (/\.md$/i.test(file) && !/^---\r?\n[\s\S]*?\r?\n---(?:\r?\n|$)/.test(markdown)) {
            const title = source.pages.find(page => page.path === file).title;
            markdown = `---\ntitle: ${JSON.stringify(title)}\n---\n\n${markdown}`;
          }
          await writeFile(output, markdown);
        } else await cp(path.join(source.originalContentRoot, file), output);
      }
      if (source.staticRoot) {
        await walk(source.staticRoot);
        await cp(source.staticRoot, path.join(staging, 'static', source.route), { recursive: true });
      }
      for (const bundle of source.browserBundles) {
        if ((await lstat(bundle.path)).isDirectory()) await walk(bundle.path);
        await cp(bundle.path, path.join(staging, 'static', source.route, bundle.route), { recursive: true });
      }
    }
    const result = { version: 1, title: registry.title, baseUrl: registry.baseUrl, origin: registry.origin ?? 'https://ranvier-org.github.io', datasets,
      generatedAt: new Date().toISOString(), sources: sources.map(source => ({
        id: source.id, title: source.title, description: source.description, route: source.route, repository: source.repository,
        ref: source.ref, revision: source.revision, dirty: source.dirty, root: source.root, originalContent: source.content,
        contentRoot: path.join(root, '.generated/content', source.route), index: source.index, sections: source.sections,
        unlisted: source.unlisted, examplesProject: source.examplesProject, packages: source.packages,
        watchRoots: [...new Set([source.originalContentRoot, source.staticRoot,
          path.dirname(path.join(source.root, source.manifest ?? 'docs/ranvier-docs.json')),
          source.examplesProject && path.dirname(source.examplesProject),
          ...source.browserBundles.map(bundle => path.extname(bundle.path) ? path.dirname(bundle.path) : bundle.path)].filter(Boolean))],
        editUrl: `https://github.com/${source.repository}/edit/${encodeURIComponent(source.ref)}/${source.content}/`
      })) };
    await writeFile(path.join(staging, 'site.json'), JSON.stringify(result, null, 2) + '\n');
    const destination = path.join(root, '.generated');
    const previous = path.join(root, `.generated-previous-${process.pid}`);
    if (await exists(previous)) throw new Error(`Previous generation recovery directory exists: ${previous}`);
    if (await exists(destination)) await rename(destination, previous);
    try { await rename(staging, destination); }
    catch (error) { if (await exists(previous)) await rename(previous, destination); throw error; }
    if (await exists(previous)) await rm(previous, { recursive: true });
    return result;
  } catch (error) {
    await rm(staging, { recursive: true, force: true });
    throw error;
  }
}

export function parseArguments(args) {
  const options = { sourceRoots: {} };
  for (let i = 0; i < args.length; i++) {
    const arg = args[i];
    if (arg === '--local') options.preferLocal = true;
    else if (arg === '--allow-unversioned') options.allowUnversioned = true;
    else if (arg === '--registry') options.registryFile = args[++i];
    else if (arg === '--source') {
      const value = args[++i], equals = value?.indexOf('=') ?? -1;
      if (equals < 1) throw new Error('--source requires id=path');
      options.sourceRoots[value.slice(0, equals)] = value.slice(equals + 1);
    } else throw new Error(`Unknown argument: ${arg}`);
  }
  return options;
}

if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
  try {
    const site = await prepareSite(parseArguments(process.argv.slice(2)));
    console.log(`Prepared ${site.sources.length} repositories and ${site.sources.reduce((n, s) => n + s.sections.reduce((a, g) => a + g.pages.length, 0), 0)} navigation pages in .generated/`);
  } catch (error) { console.error(error.message); process.exitCode = 1; }
}
