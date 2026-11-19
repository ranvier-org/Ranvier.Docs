import { readFile } from 'node:fs/promises';
import path from 'node:path';
import { pathToFileURL } from 'node:url';
import { fromMarkdown } from 'mdast-util-from-markdown';
import { parseFragment } from 'parse5';
import { exists, readSource, headingIds, markdownRegions, relativePath } from './import.mjs';

function visit(node, callback) {
  callback(node);
  for (const child of node.children ?? []) visit(child, callback);
}

function collectCandidateLinks(text, file) {
  const found = [];
  for (const region of markdownRegions(text, file)) {
    const part = text.slice(region.start, region.end);
    const ast = fromMarkdown(part);
    const codeSpans = [];
    visit(ast, node => {
      if (node.type === 'code' || node.type === 'inlineCode') {
        codeSpans.push({ start: node.position.start.offset, end: node.position.end.offset });
      }
      if (['link', 'image', 'definition'].includes(node.type)) {
        const line = text.slice(0, region.start + node.position.start.offset).split('\n').length;
        found.push({ url: node.url, line, kind: node.type });
      }
      if (node.type === 'html') {
        const baseOffset = region.start + node.position.start.offset;
        const inspect = element => {
          for (const attr of element.attrs ?? []) {
            if (!['href', 'src', 'poster'].includes(attr.name)) continue;
            const loc = element.sourceCodeLocation?.attrs?.[attr.name];
            const line = text.slice(0, baseOffset + (loc?.startOffset ?? 0)).split('\n').length;
            found.push({ url: attr.value, line, kind: 'html' });
          }
          for (const child of element.childNodes ?? []) inspect(child);
          if (element.content) inspect(element.content);
        };
        inspect(parseFragment(node.value, { sourceCodeLocationInfo: true }));
      }
    });
    const inCode = offset => codeSpans.some(span => offset >= span.start && offset < span.end);
    for (const dirMatch of part.matchAll(/^[ \t]*:{3,}[a-zA-Z][a-zA-Z0-9_-]*(?:[ \t]+([^\r\n{][^\r\n]*)|(?:\[[^\]]*\])?\{([^}\r\n]+)\})/gm)) {
      if (inCode(dirMatch.index)) continue;
      const attrsText = dirMatch[1] ?? dirMatch[2] ?? '';
      const line = text.slice(0, region.start + dirMatch.index).split('\n').length;
      for (const attrMatch of attrsText.matchAll(/\b(?:href|src)\s*=\s*(["'])([^"']+)\1/g)) {
        found.push({ url: attrMatch[2], line, kind: 'directive' });
      }
    }
  }
  return found;
}

export async function validateRepository(repoRoot, options = {}) {
  const resolvedRoot = path.resolve(repoRoot);
  const repoName = options.repositoryName ?? path.basename(resolvedRoot);
  const id = options.id ?? (repoName.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') || 'repo');
  const registration = {
    id,
    repository: `ranvier-org/${repoName}`,
    route: id,
    ref: options.ref ?? 'main',
    local: resolvedRoot,
    ...(options.content ? { content: options.content } : {}),
    ...(options.manifest ? { manifest: options.manifest } : {})
  };
  const source = await readSource(resolvedRoot, registration, { preferLocal: true, allowUnversioned: true });
  const pageMap = new Map();
  const headingsCache = new Map();
  for (const page of source.pages) {
    const raw = await readFile(path.join(source.originalContentRoot, page.path), 'utf8');
    pageMap.set(page.path, raw);
  }
  const getHeadings = pagePath => {
    if (!headingsCache.has(pagePath)) headingsCache.set(pagePath, headingIds(pageMap.get(pagePath), pagePath));
    return headingsCache.get(pagePath);
  };
  const problems = [];
  for (const page of source.pages) {
    const raw = pageMap.get(page.path);
    for (const { url, line, kind } of collectCandidateLinks(raw, page.path)) {
      if (!url || url.startsWith('#')) {
        if (url && url.length > 1) {
          const anchor = decodeURIComponent(url.slice(1));
          if (!getHeadings(page.path).has(anchor)) problems.push(`${page.path}:${line}: missing heading ${url}`);
        }
        continue;
      }
      if (url.startsWith('repo:')) {
        const match = /^repo:([a-z][a-z0-9-]*)\/([^#\s]+)(#[^\s]*)?$/.exec(url);
        if (!match) { problems.push(`${page.path}:${line}: malformed repository link ${url}`); continue; }
        try { relativePath(decodeURIComponent(match[2]), `${page.path}:${line}`); }
        catch { problems.push(`${page.path}:${line}: invalid relative path in ${url}`); continue; }
        if (match[1] === id) {
          const targetPage = decodeURIComponent(match[2]);
          if (!pageMap.has(targetPage)) problems.push(`${page.path}:${line}: missing local page ${url}`);
          else if (match[3] && !getHeadings(targetPage).has(decodeURIComponent(match[3].slice(1)))) {
            problems.push(`${page.path}:${line}: missing heading ${url}`);
          }
        }
        continue;
      }
      if (/^(?:[a-z][a-z0-9+.-]*:|[/#?])/i.test(url)) continue;
      const [, rawTarget, rawSuffix] = /^([^?#]*)(.*)$/.exec(url);
      let decoded;
      try { decoded = decodeURIComponent(rawTarget); } catch { problems.push(`${page.path}:${line}: invalid URL encoding ${url}`); continue; }
      const resolvedInContent = path.posix.normalize(path.posix.join(path.posix.dirname(page.path), decoded));
      if (resolvedInContent === '..' || resolvedInContent.startsWith('../')) {
        const repoFile = path.resolve(source.originalContentRoot, decoded === '' ? '.' : path.join(path.dirname(page.path), decoded));
        if (!(await exists(repoFile))) problems.push(`${page.path}:${line}: missing repository file ${url}`);
        continue;
      }
      const candidates = [
        resolvedInContent,
        `${resolvedInContent.replace(/\/$/, '')}.md`,
        `${resolvedInContent.replace(/\/$/, '')}.fsx`,
        `${resolvedInContent.replace(/\/$/, '')}/index.md`,
        `${resolvedInContent.replace(/\/$/, '')}/index.fsx`
      ];
      const matchedPage = candidates.find(candidate => pageMap.has(candidate));
      if (matchedPage) {
        const hashMatch = /^#(.+)$/.exec(rawSuffix);
        if (hashMatch && !getHeadings(matchedPage).has(decodeURIComponent(hashMatch[1]))) {
          problems.push(`${page.path}:${line}: missing heading ${url}`);
        }
        continue;
      }
      const contentAsset = path.join(source.originalContentRoot, resolvedInContent);
      const staticAsset = source.staticRoot ? path.join(source.staticRoot, resolvedInContent) : null;
      if (!(await exists(contentAsset)) && !(staticAsset && await exists(staticAsset))) {
        problems.push(`${page.path}:${line}: missing target ${url} (${kind})`);
      }
    }
  }
  return {
    id: source.id,
    title: source.title,
    pages: source.pages,
    sections: source.sections,
    problems
  };
}

if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
  try {
    const args = process.argv.slice(2);
    let target = process.cwd();
    const options = {};
    for (let i = 0; i < args.length; i++) {
      if (args[i] === '--repo') target = args[++i];
      else if (args[i] === '--id') options.id = args[++i];
      else if (args[i] === '--content') options.content = args[++i];
      else if (args[i] === '--manifest') options.manifest = args[++i];
      else if (!args[i].startsWith('-')) target = args[i];
      else throw new Error(`Unknown argument: ${args[i]}`);
    }
    const report = await validateRepository(target, options);
    if (report.problems.length) {
      console.error(report.problems.join('\n'));
      process.exitCode = 1;
    } else {
      console.log(`Validated ${report.title}: ${report.pages.length} pages across ${report.sections.length} top-level section(s).`);
    }
  } catch (error) {
    console.error(error.message);
    process.exitCode = 1;
  }
}
