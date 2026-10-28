import { readFile, readdir, stat } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { parse } from 'parse5';

async function filesIn(root) {
  const files = [];
  for (const entry of await readdir(root, { withFileTypes: true })) {
    if (entry.isDirectory()) files.push(...await filesIn(path.join(root, entry.name)));
    else if (entry.isFile()) files.push(path.join(root, entry.name));
  }
  return files;
}

async function isFile(file) {
  try { return (await stat(file)).isFile(); } catch (error) { if (error.code === 'ENOENT') return false; throw error; }
}

function references(content) {
  const ids = new Set(), links = [];
  const visit = node => {
    for (const attribute of node.attrs ?? []) {
      if (attribute.name === 'id' || attribute.name === 'name' && node.nodeName === 'a') ids.add(attribute.value);
      if (['href', 'src', 'poster'].includes(attribute.name)) links.push(attribute.value);
    }
    for (const child of node.childNodes ?? []) visit(child);
    if (node.content) visit(node.content);
  };
  visit(parse(content));
  return { ids, links };
}

export async function checkOutput({ root, baseUrl = '/' }) {
  root = path.resolve(root);
  const pages = (await filesIn(root)).filter(f => f.endsWith('.html'));
  if (!pages.length) throw new Error(`No HTML pages found in ${root}`);
  const html = new Map(await Promise.all(pages.map(async file => [file, await readFile(file, 'utf8')])));
  const documents = new Map(pages.map(file => [file, references(html.get(file))]));
  const problems = [];
  for (const [page, content] of html) {
    const relative = path.relative(root, page).replaceAll('\\', '/');
    for (const link of documents.get(page).links) {
      if (!link) continue;
      if (/^(?:[a-z][a-z0-9+.-]*:|\/\/)/i.test(link)) continue;
      const virtualPath = baseUrl + relative;
      let url;
      try { url = new URL(link, `https://docs.invalid${virtualPath}`); }
      catch { problems.push(`${relative}: invalid link ${link}`); continue; }
      if (!url.pathname.startsWith(baseUrl)) {
        problems.push(`${relative}: link escapes deployment base ${baseUrl}: ${link}`);
        continue;
      }
      let local;
      try { local = decodeURIComponent(url.pathname.slice(baseUrl.length)); }
      catch { problems.push(`${relative}: invalid URL encoding ${link}`); continue; }
      const target = path.resolve(root, local);
      const confined = path.relative(root, target);
      if (confined.startsWith('..') || path.isAbsolute(confined)) {
        problems.push(`${relative}: link escapes output: ${link}`); continue;
      }
      let resolved = target;
      if (!(await isFile(resolved))) resolved = path.join(target, 'index.html');
      if (!(await isFile(resolved))) {
        if (await isFile(`${target}.html`)) resolved = `${target}.html`;
        else { problems.push(`${relative}: missing target ${link}`); continue; }
      }
      if (url.hash && !url.hash.startsWith('#/') && documents.has(resolved)) {
        let anchor;
        try { anchor = decodeURIComponent(url.hash.slice(1)); }
        catch { problems.push(`${relative}: invalid anchor encoding ${link}`); continue; }
        if (!documents.get(resolved).ids.has(anchor)) problems.push(`${relative}: missing anchor ${link}`);
      }
    }
  }
  return [...new Set(problems)].sort();
}

if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
  try {
    const workspace = fileURLToPath(new URL('..', import.meta.url));
    const site = JSON.parse(await readFile(path.join(workspace, '.generated/site.json'), 'utf8'));
    const problems = await checkOutput({ root: path.join(workspace, 'output'), baseUrl: site.baseUrl });
    if (problems.length) { console.error(problems.join('\n')); process.exitCode = 1; }
    else console.log('All generated page links, anchors and referenced assets resolve.');
  } catch (error) { console.error(error.message); process.exitCode = 1; }
}
