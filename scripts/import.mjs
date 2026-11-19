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
const indexFilePattern = /^(?:index|readme)\.(?:md|fsx)$/i;
const ignoredDirectories = new Set([
  '.git', 'bin', 'obj', 'node_modules', 'output', '.SageFs',
  '.ai', '.agents', '.claude', '.codex', '.nacara', 'superpowers'
]);
const validBadges = new Set(['new', 'updated', 'experimental', 'beta', 'deprecated']);

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
    if (source.category !== undefined && (typeof source.category !== 'string' || !source.category.trim())) {
      throw new Error(`Invalid category for ${source.id}: expected nonempty string`);
    }
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

function unquoteYamlScalar(raw) {
  const trimmed = raw.trim();
  if (trimmed.startsWith('"') && trimmed.endsWith('"') && trimmed.length >= 2) {
    try { return JSON.parse(trimmed); } catch { return trimmed.slice(1, -1); }
  }
  if (trimmed.startsWith("'") && trimmed.endsWith("'") && trimmed.length >= 2) {
    return trimmed.slice(1, -1).replaceAll("''", "'");
  }
  return trimmed;
}

export function markdownRegions(text, file) {
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

export function parsePageMetadata(text, file, fallback) {
  let yaml = null;
  const regions = markdownRegions(text, file);
  if (file.endsWith('.fsx')) {
    if (regions.length > 0) {
      const first = text.slice(regions[0].start, regions[0].end);
      const match = /^\s*---\r?\n([\s\S]*?)\r?\n---(?:\r?\n|\s*$)/.exec(first);
      if (match) yaml = match[1];
    }
  } else {
    const match = /^---\r?\n([\s\S]*?)\r?\n---(?:\r?\n|$)/.exec(text);
    if (match) yaml = match[1];
  }
  let title = null;
  let order = null;
  let description = '';
  if (yaml !== null) {
    const titleMatch = /^title:\s*(.+?)\s*$/m.exec(yaml);
    if (titleMatch) title = unquoteYamlScalar(titleMatch[1]);
    const orderMatch = /^order:\s*([+-]?\d+(?:\.\d+)?)\s*$/m.exec(yaml);
    if (orderMatch) {
      const parsed = Number(orderMatch[1]);
      if (Number.isFinite(parsed)) order = parsed;
    }
    const descMatch = /^description:\s*(.+?)\s*$/m.exec(yaml);
    if (descMatch) description = unquoteYamlScalar(descMatch[1]);
  }
  if (!title) {
    for (const region of regions) {
      const heading = /^#\s+(.+?)\s*#*\s*$/m.exec(text.slice(region.start, region.end));
      if (heading) { title = heading[1]; break; }
    }
  }
  return { title: title ?? fallback.replaceAll('-', ' '), order, description };
}

function excluded(file, patterns) {
  return patterns.some(pattern => {
    const expression = pattern.split('**').map(part => part.split('*').map(p => p.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')).join('[^/]*')).join('.*');
    return new RegExp(`^${expression}$`).test(file);
  });
}

async function resolveEditBranch(root, configuredRef) {
  if (configuredRef && configuredRef !== 'HEAD' && !/^[0-9a-f]{40}$/i.test(configuredRef)) {
    return configuredRef;
  }
  try {
    const originHead = (await exec('git', ['-C', root, 'symbolic-ref', '--quiet', '--short', 'refs/remotes/origin/HEAD'], { windowsHide: true })).stdout.trim();
    if (originHead.startsWith('origin/') && originHead.length > 7) return originHead.slice(7);
  } catch { /* fall through */ }
  try {
    const current = (await exec('git', ['-C', root, 'symbolic-ref', '--quiet', '--short', 'HEAD'], { windowsHide: true })).stdout.trim();
    if (current && current !== 'HEAD') return current;
  } catch { /* fall through */ }
  return 'main';
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

const knownManifestKeys = new Set([
  '$schema', 'version', 'title', 'description', 'badge', 'content', 'static', 'index',
  'sections', 'unlisted', 'exclude', 'styles', 'scripts', 'proofsUrl', 'apiReference',
  'ogCards', 'defaultOgImage', 'examplesProject', 'packages', 'browserBundles'
]);

export async function loadSourceConfiguration(root, registration, options = {}) {
  const override = options.sourceRoots?.[registration.id];
  const local = registration.local && path.resolve(root, registration.local);
  const checkout = override ? path.resolve(root, override) :
    registration.self ? root : options.preferLocal && local ? local : path.join(root, '.sources', registration.id);
  if (!(await exists(checkout))) throw new Error(`${registration.id}: checkout missing at ${checkout}; run dotnet fsi build.fsx -- sources or use --local`);
  const manifestPath = await within(checkout, registration.manifest ?? 'docs/ranvier-docs.json', `${registration.id}.manifest`, true);
  const sourceManifest = manifestPath ? JSON.parse(await readFile(manifestPath, 'utf8')) : {};
  if (!sourceManifest || typeof sourceManifest !== 'object' || Array.isArray(sourceManifest)) throw new Error(`${registration.id}: manifest must be an object`);
  if (registration.defaults && (typeof registration.defaults !== 'object' || Array.isArray(registration.defaults))) throw new Error(`${registration.id}: defaults must be a manifest object`);
  const manifest = { ...(registration.defaults ?? {}), ...sourceManifest };
  if (manifest.version !== undefined && manifest.version !== 1) throw new Error(`${registration.id}: unsupported manifest version ${manifest.version}`);
  for (const key of Object.keys(manifest)) if (!knownManifestKeys.has(key)) throw new Error(`${registration.id}: unknown manifest option ${key}`);
  return { root: checkout, manifest, manifestPath };
}

function validateBadge(badge, label) {
  if (badge === undefined) return undefined;
  if (typeof badge !== 'string' || !validBadges.has(badge)) {
    throw new Error(`${label}: unsupported badge '${badge}'; expected one of ${[...validBadges].join(', ')}`);
  }
  return badge;
}

function comparePages(a, b) {
  const aIndex = indexFilePattern.test(path.posix.basename(a.path)) ? 0 : 1;
  const bIndex = indexFilePattern.test(path.posix.basename(b.path)) ? 0 : 1;
  if (aIndex !== bIndex) return aIndex - bIndex;
  const aHasOrder = a.order !== null && a.order !== undefined;
  const bHasOrder = b.order !== null && b.order !== undefined;
  if (aHasOrder && bHasOrder && a.order !== b.order) return a.order - b.order;
  if (aHasOrder !== bHasOrder) return aHasOrder ? -1 : 1;
  return a.path.localeCompare(b.path);
}

function humanizeSegment(segment) {
  return segment.replace(/[-_]+/g, ' ').replace(/^\p{L}/u, ch => ch.toUpperCase());
}

function buildDirectoryTree(dirPrefix, availablePages) {
  const prefix = dirPrefix ? `${dirPrefix}/` : '';
  const scoped = dirPrefix ? availablePages.filter(p => p.path.startsWith(prefix)) : availablePages;
  const direct = [];
  const subdirNames = new Set();
  for (const page of scoped) {
    const rest = dirPrefix ? page.path.slice(prefix.length) : page.path;
    const slashIndex = rest.indexOf('/');
    if (slashIndex < 0) direct.push(page);
    else subdirNames.add(rest.slice(0, slashIndex));
  }
  direct.sort(comparePages);
  const items = direct.map(page => ({
    kind: 'page',
    path: page.path,
    title: page.title,
    explicitTitle: false
  }));
  const sortedSubdirs = [...subdirNames].sort((a, b) => {
    const aPath = `${prefix}${a}`;
    const bPath = `${prefix}${b}`;
    const aOverview = scoped.find(p => p.path === `${aPath}/index.md` || p.path === `${aPath}/index.fsx` || p.path === `${aPath}/README.md`);
    const bOverview = scoped.find(p => p.path === `${bPath}/index.md` || p.path === `${bPath}/index.fsx` || p.path === `${bPath}/README.md`);
    const aOrder = aOverview?.order ?? null;
    const bOrder = bOverview?.order ?? null;
    if (aOrder !== null && bOrder !== null && aOrder !== bOrder) return aOrder - bOrder;
    return a.localeCompare(b);
  });
  for (const sub of sortedSubdirs) {
    const subPath = `${prefix}${sub}`;
    const subPages = scoped.filter(p => p.path.startsWith(`${subPath}/`));
    const overview = subPages.find(p => p.path === `${subPath}/index.md` || p.path === `${subPath}/index.fsx` || p.path === `${subPath}/README.md`);
    const children = buildDirectoryTree(subPath, subPages.filter(p => p !== overview));
    if (overview) {
      if (children.length > 0) {
        items.push({ kind: 'group', path: overview.path, title: overview.title, explicitTitle: false, items: children });
      } else {
        items.push({ kind: 'page', path: overview.path, title: overview.title, explicitTitle: false });
      }
    } else if (children.length > 0) {
      items.push({ kind: 'section', title: humanizeSegment(sub), items: children });
    }
  }
  return items;
}

function normalizeMenuItems(rawItems, pages, unlistedSet, sourceId) {
  const result = [];
  for (const raw of rawItems) {
    if (typeof raw === 'string') {
      const found = pages.find(p => p.path === raw);
      if (!found) throw new Error(`${sourceId}: navigation page does not exist: ${raw}`);
      result.push({ kind: 'page', path: found.path, title: found.title, explicitTitle: false });
      continue;
    }
    if (!raw || typeof raw !== 'object' || Array.isArray(raw)) {
      throw new Error(`${sourceId}: invalid navigation item`);
    }
    if (typeof raw.path === 'string') {
      const found = pages.find(p => p.path === raw.path);
      if (!found) throw new Error(`${sourceId}: navigation page does not exist: ${raw.path}`);
      const badge = validateBadge(raw.badge, `${sourceId}.sections`);
      const explicitTitle = typeof raw.title === 'string' && raw.title.length > 0;
      result.push({
        kind: 'page',
        path: found.path,
        title: explicitTitle ? raw.title : found.title,
        explicitTitle,
        ...(badge ? { badge } : {})
      });
      continue;
    }
    if (typeof raw.section === 'string' || (typeof raw.title === 'string' && (Array.isArray(raw.items) || Array.isArray(raw.pages)))) {
      const sectionTitle = raw.section ?? raw.title;
      const childInput = raw.items ?? raw.pages;
      if (!sectionTitle || !Array.isArray(childInput)) throw new Error(`${sourceId}: nested section requires title and items`);
      result.push({
        kind: 'section',
        title: sectionTitle,
        items: normalizeMenuItems(childInput, pages, unlistedSet, sourceId)
      });
      continue;
    }
    if (typeof raw.group === 'string') {
      const found = pages.find(p => p.path === raw.group);
      if (!found) throw new Error(`${sourceId}: navigation group overview page does not exist: ${raw.group}`);
      const badge = validateBadge(raw.badge, `${sourceId}.sections.group`);
      const explicitTitle = typeof raw.title === 'string' && raw.title.length > 0;
      let children = [];
      if (Array.isArray(raw.items)) {
        children = normalizeMenuItems(raw.items, pages, unlistedSet, sourceId);
      } else if (typeof raw.dir === 'string') {
        const cleanDir = relativePath(raw.dir, `${sourceId}.sections.group.dir`).replace(/\/+$/, '');
        const available = pages.filter(p => !unlistedSet.has(p.path) && p.path !== found.path && p.path.startsWith(`${cleanDir}/`));
        if (!available.length && !pages.some(p => p.path.startsWith(`${cleanDir}/`))) {
          throw new Error(`${sourceId}: navigation directory has no pages: ${cleanDir}`);
        }
        children = buildDirectoryTree(cleanDir, available);
      }
      result.push({
        kind: 'group',
        path: found.path,
        title: explicitTitle ? raw.title : found.title,
        explicitTitle,
        ...(badge ? { badge } : {}),
        items: children
      });
      continue;
    }
    if (typeof raw.dir === 'string') {
      const cleanDir = relativePath(raw.dir, `${sourceId}.sections.dir`).replace(/\/+$/, '');
      const available = pages.filter(p => !unlistedSet.has(p.path) && p.path.startsWith(`${cleanDir}/`));
      if (!available.length) throw new Error(`${sourceId}: navigation directory has no pages: ${cleanDir}`);
      result.push(...buildDirectoryTree(cleanDir, available));
      continue;
    }
    if (typeof raw.link === 'string') {
      if (typeof raw.title !== 'string' || !raw.title) throw new Error(`${sourceId}: navigation link requires a title`);
      const badge = validateBadge(raw.badge, `${sourceId}.sections.link`);
      result.push({
        kind: 'link',
        title: raw.title,
        url: raw.link,
        ...(badge ? { badge } : {})
      });
      continue;
    }
    throw new Error(`${sourceId}: section item requires path, section, group, dir or link`);
  }
  return result;
}

function flattenMenuPages(items) {
  const flat = [];
  for (const item of items) {
    if (item.kind === 'page') {
      flat.push({ path: item.path, title: item.title });
    } else if (item.kind === 'group') {
      flat.push({ path: item.path, title: item.title });
      flat.push(...flattenMenuPages(item.items ?? []));
    } else if (item.kind === 'section') {
      flat.push(...flattenMenuPages(item.items ?? []));
    }
  }
  return flat;
}

export async function readSource(root, registration, options = {}) {
  const { root: checkout, manifest } = await loadSourceConfiguration(root, registration, options);
  const content = manifest.content ?? registration.content ?? 'docs/content';
  const contentRoot = await within(checkout, content, `${registration.id}.content`);
  const exclude = manifest.exclude ?? [];
  if (!Array.isArray(exclude) || exclude.some(p => typeof p !== 'string')) throw new Error(`${registration.id}.exclude must be an array of glob strings`);
  const files = (await walk(contentRoot)).filter(file => !excluded(file, exclude));
  const pages = [];
  for (const file of files.filter(f => pageExtension.test(f))) {
    const raw = await readFile(path.join(contentRoot, file), 'utf8');
    const meta = parsePageMetadata(raw, file, path.basename(file).replace(pageExtension, ''));
    pages.push({ path: file, title: meta.title, order: meta.order, description: meta.description });
  }
  if (!pages.length) throw new Error(`${registration.id}: no Markdown or literate F# pages found in ${content}`);
  const index = manifest.index ?? (pages.find(p => p.path === 'index.md')?.path ?? pages.find(p => p.path === 'README.md')?.path ?? pages[0].path);
  if (!pages.some(p => p.path === index)) throw new Error(`${registration.id}: index page does not exist: ${index}`);
  const unlisted = manifest.unlisted ?? [];
  if (!Array.isArray(unlisted)) throw new Error(`${registration.id}.unlisted must be an array`);
  for (const page of unlisted) if (!pages.some(p => p.path === page)) throw new Error(`${registration.id}: unlisted page does not exist: ${page}`);
  const unlistedSet = new Set(unlisted);
  const defaultTitle = manifest.title ?? registration.title ?? registration.repository.split('/')[1];
  const sections = manifest.sections ? manifest.sections.map(section => {
    const rawEntries = section.items ?? section.pages;
    if (typeof section.title !== 'string' || !section.title || !Array.isArray(rawEntries)) {
      throw new Error(`${registration.id}: each section requires title and pages or items`);
    }
    const items = normalizeMenuItems(rawEntries, pages, unlistedSet, registration.id);
    return { title: section.title, items, pages: flattenMenuPages(items) };
  }) : (() => {
    const items = buildDirectoryTree('', pages.filter(p => !unlistedSet.has(p.path)));
    return [{ title: defaultTitle, items, pages: flattenMenuPages(items) }];
  })();
  const listed = sections.flatMap(section => section.pages.map(page => page.path));
  if (listed.length !== new Set(listed).size) throw new Error(`${registration.id}: a page appears more than once in navigation`);
  for (const page of pages) if (!listed.includes(page.path) && !unlistedSet.has(page.path)) {
    throw new Error(`${registration.id}: ${page.path} is missing from navigation; list it in a section or unlisted`);
  }
  const badge = validateBadge(manifest.badge, `${registration.id}.badge`);
  const staticName = manifest.static === false ? null : manifest.static ?? 'docs/static';
  const staticRoot = staticName ? await within(checkout, staticName, `${registration.id}.static`, true) : null;
  const styles = [];
  for (const stylePath of manifest.styles ?? []) {
    styles.push(await within(checkout, stylePath, `${registration.id}.styles`));
  }
  const scripts = [];
  for (const scriptEntry of manifest.scripts ?? []) {
    const normalized = typeof scriptEntry === 'string' ? { src: scriptEntry } : scriptEntry;
    if (!normalized || typeof normalized.src !== 'string' || !normalized.src) {
      throw new Error(`${registration.id}.scripts requires a nonempty src`);
    }
    relativePath(normalized.src, `${registration.id}.scripts.src`);
    if (staticRoot) {
      const candidate = path.resolve(staticRoot, normalized.src);
      const inContent = path.resolve(contentRoot, normalized.src);
      if (!(await exists(candidate)) && !(await exists(inContent))) {
        throw new Error(`${registration.id}.scripts does not exist in static or content root: ${normalized.src}`);
      }
    }
    scripts.push({ src: normalized.src, defer: Boolean(normalized.defer), module: Boolean(normalized.module) });
  }
  let apiReference = null;
  if (manifest.apiReference !== undefined) {
    if (!manifest.apiReference || typeof manifest.apiReference !== 'object' || Array.isArray(manifest.apiReference)) {
      throw new Error(`${registration.id}.apiReference must be an object`);
    }
    const projects = [];
    for (const proj of manifest.apiReference.projects ?? []) {
      const resolved = await within(checkout, proj, `${registration.id}.apiReference.projects`);
      if (!/\.(fsproj|csproj)$/i.test(resolved)) throw new Error(`${registration.id}.apiReference.projects must name .fsproj or .csproj files`);
      projects.push(resolved);
    }
    const assemblies = manifest.apiReference.assemblies ?? [];
    if (!Array.isArray(assemblies) || assemblies.some(a => typeof a !== 'string' || !a)) {
      throw new Error(`${registration.id}.apiReference.assemblies must be an array of assembly names`);
    }
    const excludeNs = manifest.apiReference.exclude ?? [];
    if (!Array.isArray(excludeNs) || excludeNs.some(ns => typeof ns !== 'string' || !ns)) {
      throw new Error(`${registration.id}.apiReference.exclude must be an array of namespace prefixes`);
    }
    apiReference = {
      title: manifest.apiReference.title ?? 'API reference',
      projects,
      assemblies,
      exclude: excludeNs
    };
  }
  const proofsUrl = manifest.proofsUrl ?? null;
  if (proofsUrl !== null && (typeof proofsUrl !== 'string' || !proofsUrl)) {
    throw new Error(`${registration.id}.proofsUrl must be a nonempty string`);
  }
  const ogCards = manifest.ogCards ?? [];
  const defaultOgImage = manifest.defaultOgImage ?? null;
  const examplesProject = manifest.examplesProject ? await within(checkout, manifest.examplesProject, `${registration.id}.examplesProject`) : null;
  if (examplesProject && !examplesProject.endsWith('.fsproj')) throw new Error(`${registration.id}.examplesProject must name an .fsproj`);
  const packages = manifest.packages ?? [];
  if (!Array.isArray(packages) || packages.some(p => !Array.isArray(p) || p.length !== 2 || p.some(x => typeof x !== 'string' || !x))) {
    throw new Error(`${registration.id}.packages must contain [npm-name, version] pairs`);
  }
  const browserBundles = [];
  for (const bundle of manifest.browserBundles ?? []) {
    const source = await within(checkout, bundle.path, `${registration.id}.browserBundles.path`);
    relativePath(bundle.route, `${registration.id}.browserBundles.route`);
    browserBundles.push({ path: source, route: bundle.route, ...(bundle.build ? { build: bundle.build } : {}) });
  }
  const editBranch = await resolveEditBranch(checkout, registration.ref);
  return {
    ...registration,
    title: defaultTitle,
    description: manifest.description ?? registration.description ?? '',
    ...(badge ? { badge } : {}),
    root: checkout,
    content,
    originalContentRoot: contentRoot,
    files,
    pages,
    index,
    sections,
    unlisted,
    staticRoot,
    styles,
    scripts,
    proofsUrl,
    apiReference,
    ogCards,
    defaultOgImage,
    examplesProject,
    packages,
    browserBundles,
    editBranch,
    ...await revisionOf(checkout, options.allowUnversioned || registration.self)
  };
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

function rewriteLinks(text, source, file, sources, pageText, baseUrl, origin = 'https://ranvier-org.github.io') {
  const cleanOrigin = origin.replace(/\/+$/, '');
  const renderedPageUrl = (destination, pagePath, fragment = '') => {
    const withoutExt = slash(pagePath).replace(/\.(?:md|fsx)$/i, '').toLowerCase();
    const suffix = withoutExt === 'index' ? '' : withoutExt.endsWith('/index') ? withoutExt.slice(0, -6) : withoutExt;
    const segments = [destination.route, ...suffix.split('/').filter(Boolean)].map(encodeURIComponent);
    return `${baseUrl}${segments.join('/')}/${fragment}`;
  };
  const relativeStagedLink = (destination, pagePath, fragment = '') => {
    const current = `${source.route}/${file}`;
    const destinationPath = `${destination.route}/${pagePath}`;
    return slash(path.posix.relative(path.posix.dirname(current), destinationPath)).split('/').map(encodeURIComponent).join('/') + fragment;
  };
  const matchDestinationPage = (destination, rawRest) => {
    const [, rawSubpath = '', query = '', fragment = ''] = /^([^?#]*)(\?[^#]*)?(#.*)?$/.exec(rawRest);
    if (query) return null;
    let clean;
    try { clean = decodeURIComponent(rawSubpath.replace(/^\/+|\/+$/g, '')); } catch { return null; }
    const candidates = clean === ''
      ? [destination.index, 'index.md', 'index.fsx', 'README.md']
      : [
          clean,
          `${clean}.md`,
          `${clean}.fsx`,
          `${clean}/index.md`,
          `${clean}/index.fsx`,
          `${clean}/README.md`,
          ...(destination.id === 'core' && clean.toLowerCase() === 'guide/pending' ? ['guide/async-and-pending.md'] : [])
        ];
    const matched = destination.pages.find(p => candidates.some(c => c && p.path.toLowerCase() === c.toLowerCase()));
    if (!matched) return null;
    if (fragment) {
      let anchor;
      try { anchor = decodeURIComponent(fragment.slice(1)); } catch { return null; }
      if (!headingIds(pageText.get(`${destination.id}/${matched.path}`), matched.path).has(anchor)) return null;
    }
    return { page: matched, fragment };
  };
  const hasDestinationAsset = (destination, rawRest) => {
    const [, rawSubpath = ''] = /^([^?#]*)/.exec(rawRest);
    let clean;
    try { clean = decodeURIComponent(rawSubpath.replace(/^\/+|\/+$/g, '')); } catch { return false; }
    if (!clean) return false;
    if (destination.files.some(f => !pageExtension.test(f) && f.toLowerCase() === clean.toLowerCase())) return true;
    if (destination.browserBundles.some(b => clean === b.route || clean.startsWith(`${b.route}/`))) return true;
    return false;
  };
  const legacyUrl = (value, mode = 'html') => {
    let localPath = null;
    const isAbsoluteOrigin = value.startsWith(`${cleanOrigin}/`);
    if (isAbsoluteOrigin) localPath = value.slice(cleanOrigin.length);
    else if (value.startsWith('/') && !value.startsWith('//')) localPath = value;
    else return null;
    if (localPath === baseUrl || sources.some(candidate => localPath === baseUrl + candidate.route || localPath.startsWith(baseUrl + candidate.route + '/'))) {
      return null;
    }
    const destination = sources.find(candidate => {
      const prefix = '/' + candidate.repository.split('/')[1];
      return localPath === prefix || localPath.startsWith(prefix + '/');
    });
    if (!destination) return null;
    const repoName = destination.repository.split('/')[1];
    const prefix = '/' + repoName;
    const rest = localPath.slice(prefix.length).replace(/^\//, '');
    if ((rest === 'proofs' || rest.startsWith('proofs/')) && !destination.pages.some(p => p.path.startsWith('proofs/'))) {
      return isAbsoluteOrigin ? null : `${cleanOrigin}/${repoName}/${rest}`;
    }
    const matchedPage = mode === 'image' ? null : matchDestinationPage(destination, rest);
    if (matchedPage) {
      return mode === 'markdown'
        ? relativeStagedLink(destination, matchedPage.page.path, matchedPage.fragment)
        : renderedPageUrl(destination, matchedPage.page.path, matchedPage.fragment);
    }
    if (isAbsoluteOrigin) {
      return hasDestinationAsset(destination, rest) ? baseUrl + destination.route + '/' + rest : null;
    }
    return baseUrl + destination.route + '/' + rest;
  };
  const resolveLink = (target, line, mode = 'markdown') => {
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
    return mode === 'markdown'
      ? relativeStagedLink(destination, decoded, fragment)
      : renderedPageUrl(destination, decoded, fragment);
  };
  const relativeDirectivePageUrl = value => {
    if (/^(?:[a-z][a-z0-9+.-]*:|[/#?])/i.test(value)) return null;
    const [, target, query = '', fragment = ''] = /^([^?#]*)(\?[^#]*)?(#.*)?$/.exec(value);
    if (query) return null;
    let decoded;
    try { decoded = decodeURIComponent(target); } catch { return null; }
    const normalized = slash(path.posix.normalize(path.posix.join(path.posix.dirname(slash(file)), decoded)));
    if (normalized === '..' || normalized.startsWith('../')) return null;
    const matched = source.pages.find(p => p.path === normalized);
    return matched ? renderedPageUrl(source, matched.path, fragment) : null;
  };
  // A relative link that leaves the content root names a repository file that is not a page: it goes to the source at its imported revision.
  const repositoryFileUrl = value => {
    if (/^(?:[a-z][a-z0-9+.-]*:|[/#?])/i.test(value)) return null;
    const [, target, suffix] = /^([^?#]*)(.*)$/.exec(value);
    let decoded;
    try { decoded = decodeURIComponent(target); } catch { return null; }
    const content = slash(path.posix.normalize(slash(source.content ?? 'docs/content'))).replace(/\/$/, '');
    const resolved = path.posix.normalize(path.posix.join(content, path.posix.dirname(slash(file)), decoded));
    if (resolved === '..' || resolved.startsWith('../') || resolved === content || resolved.startsWith(content + '/')) return null;
    return `https://github.com/${source.repository}/blob/${source.revision ?? source.ref}/` + resolved.split('/').map(encodeURIComponent).join('/') + suffix;
  };
  const replacements = [];
  for (const region of markdownRegions(text, file)) {
    const part = text.slice(region.start, region.end);
    const ast = fromMarkdown(part);
    const codeSpans = [];
    visit(ast, node => {
      if (node.type === 'code' || node.type === 'inlineCode') {
        codeSpans.push({ start: node.position.start.offset, end: node.position.end.offset });
      }
      if (['link', 'image', 'definition'].includes(node.type)) {
        const span = destinationSpan(part, node);
        const start = region.start + span.start;
        const line = text.slice(0, start).split('\n').length;
        const mode = node.type === 'image' ? 'image' : 'markdown';
        const value = node.url.startsWith('repo:') ? resolveLink(node.url, line, 'markdown') : legacyUrl(node.url, mode) ?? (node.type === 'image' ? null : repositoryFileUrl(node.url));
        if (value !== null) replacements.push({ start, end: region.start + span.end, value });
      }
      if (node.type === 'html') {
        const baseOffset = region.start + node.position.start.offset;
        const inspect = element => {
          for (const attribute of element.attrs ?? []) {
            if (!['href', 'src', 'poster'].includes(attribute.name)) continue;
            const key = attribute.prefix ? `${attribute.prefix}:${attribute.name}` : attribute.name;
            const location = element.sourceCodeLocation?.attrs?.[key];
            if (!location) continue;
            const attrStart = baseOffset + location.startOffset;
            const line = text.slice(0, attrStart).split('\n').length;
            const value = attribute.value.startsWith('repo:') ? resolveLink(attribute.value, line, 'html') : legacyUrl(attribute.value, 'html');
            if (value === null) continue;
            const raw = node.value.slice(location.startOffset, location.endOffset);
            const beginning = /^([^=]+\s*=\s*)(["']?)/.exec(raw);
            if (!beginning) continue;
            const quote = beginning[2];
            const escaped = value.replaceAll('&', '&amp;').replaceAll(quote === "'" ? "'" : '"', quote === "'" ? '&#39;' : '&quot;');
            replacements.push({ start: attrStart, end: baseOffset + location.endOffset,
              value: beginning[1] + quote + escaped + quote });
          }
          for (const child of element.childNodes ?? []) inspect(child);
          if (element.content) inspect(element.content);
        };
        inspect(parseFragment(node.value, { sourceCodeLocationInfo: true }));
      }
    });
    const inCodeSpan = offset => codeSpans.some(span => offset >= span.start && offset < span.end);
    const directiveHeader = /^[ \t]*:{3,}[a-zA-Z][a-zA-Z0-9_-]*(?:[ \t]+([^\r\n{][^\r\n]*)|(?:\[[^\]]*\])?\{([^}\r\n]+)\})/gm;
    for (const dirMatch of part.matchAll(directiveHeader)) {
      if (inCodeSpan(dirMatch.index)) continue;
      const attrsText = dirMatch[1] ?? dirMatch[2];
      if (!attrsText) continue;
      const attrsOffsetInMatch = dirMatch[1] !== undefined ? dirMatch[0].indexOf(dirMatch[1]) : dirMatch[0].lastIndexOf('{') + 1;
      const attrBase = region.start + dirMatch.index + attrsOffsetInMatch;
      for (const attrMatch of attrsText.matchAll(/\b(href|src)\s*=\s*(["'])([^"']+)\2/g)) {
        const quote = attrMatch[2];
        const rawUrl = attrMatch[3];
        const valueOffsetInsideAttr = attrMatch[0].indexOf(quote) + 1;
        const absStart = attrBase + attrMatch.index + valueOffsetInsideAttr;
        const absEnd = absStart + rawUrl.length;
        const line = text.slice(0, absStart).split('\n').length;
        const rewritten = rawUrl.startsWith('repo:')
          ? resolveLink(rawUrl, line, 'html')
          : legacyUrl(rawUrl, 'html') ?? relativeDirectivePageUrl(rawUrl) ?? repositoryFileUrl(rawUrl);
        if (rewritten !== null) replacements.push({ start: absStart, end: absEnd, value: rewritten });
      }
    }
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
  const origin = registry.origin ?? 'https://ranvier-org.github.io';
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
    const sharedStatic = path.join(root, 'static');
    if (await exists(sharedStatic)) {
      await walk(sharedStatic);
      await cp(sharedStatic, path.join(staging, 'static'), { recursive: true });
    }
    for (const source of sources) {
      for (const file of source.files) {
        const output = path.join(staging, 'content', source.route, file);
        await mkdir(path.dirname(output), { recursive: true });
        if (pageExtension.test(file)) {
          let markdown = rewriteLinks(pageText.get(`${source.id}/${file}`), source, file, sources, pageText, registry.baseUrl, origin);
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
    const result = {
      version: 1,
      title: registry.title,
      baseUrl: registry.baseUrl,
      origin,
      datasets,
      generatedAt: new Date().toISOString(),
      sources: sources.map(source => ({
        id: source.id,
        ...(source.category ? { category: source.category } : {}),
        title: source.title,
        description: source.description,
        ...(source.badge ? { badge: source.badge } : {}),
        route: source.route,
        repository: source.repository,
        ref: source.ref,
        revision: source.revision,
        dirty: source.dirty,
        root: source.root,
        originalContent: source.content,
        contentRoot: path.join(root, '.generated/content', source.route),
        index: source.index,
        sections: source.sections,
        unlisted: source.unlisted,
        styles: source.styles,
        scripts: source.scripts,
        proofsUrl: source.proofsUrl,
        apiReference: source.apiReference,
        ogCards: source.ogCards,
        defaultOgImage: source.defaultOgImage,
        examplesProject: source.examplesProject,
        packages: source.packages,
        watchRoots: [...new Set([
          source.originalContentRoot,
          source.staticRoot,
          ...source.styles.map(stylePath => path.dirname(stylePath)),
          path.dirname(path.join(source.root, source.manifest ?? 'docs/ranvier-docs.json')),
          source.examplesProject && path.dirname(source.examplesProject),
          ...(source.apiReference?.projects ?? []).map(proj => path.dirname(proj)),
          ...source.browserBundles.map(bundle => path.extname(bundle.path) ? path.dirname(bundle.path) : bundle.path)
        ].filter(Boolean))],
        editUrl: `https://github.com/${source.repository}/edit/${encodeURIComponent(source.editBranch)}/${source.content}/`
      }))
    };
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
