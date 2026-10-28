import { createServer } from 'node:http';
import { readFile, stat } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const mimeTypes = { '.html': 'text/html', '.js': 'text/javascript', '.mjs': 'text/javascript', '.css': 'text/css',
  '.json': 'application/json', '.svg': 'image/svg+xml', '.png': 'image/png', '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg', '.webp': 'image/webp', '.ico': 'image/x-icon', '.wasm': 'application/wasm',
  '.woff2': 'font/woff2', '.txt': 'text/plain', '.xml': 'application/xml', '.pdf': 'application/pdf' };

export async function startServer({ root, baseUrl = '/', port = 4173, hostname = '127.0.0.1' }) {
  root = path.resolve(root);
  const server = createServer(async (request, response) => {
    try {
      if (!['GET', 'HEAD'].includes(request.method)) { response.writeHead(405); response.end(); return; }
      const url = new URL(request.url, `http://${hostname}`);
      if (!url.pathname.startsWith(baseUrl)) { response.writeHead(404); response.end('Not found'); return; }
      let file;
      try { file = path.resolve(root, decodeURIComponent(url.pathname.slice(baseUrl.length))); }
      catch { response.writeHead(400); response.end('Invalid path'); return; }
      const relative = path.relative(root, file);
      if (relative.startsWith('..') || path.isAbsolute(relative)) { response.writeHead(403); response.end('Forbidden'); return; }
      if ((await stat(file)).isDirectory()) {
        if (!url.pathname.endsWith('/')) { response.writeHead(301, { location: url.pathname + '/' + url.search }); response.end(); return; }
        file = path.join(file, 'index.html');
      }
      const body = await readFile(file);
      response.writeHead(200, { 'content-type': mimeTypes[path.extname(file)] ?? 'application/octet-stream',
        'content-length': body.length, 'cache-control': 'no-cache' });
      response.end(request.method === 'HEAD' ? undefined : body);
    } catch (error) {
      response.writeHead(error.code === 'ENOENT' || error.code === 'ENOTDIR' ? 404 : 500);
      response.end('Not found');
    }
  });
  await new Promise((resolve, reject) => { server.once('error', reject); server.listen(port, hostname, resolve); });
  return server;
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  try {
    const workspace = fileURLToPath(new URL('..', import.meta.url));
    const metadata = JSON.parse(await readFile(path.join(workspace, '.generated/site.json'), 'utf8'));
    const port = Number(process.env.PORT ?? 4173);
    const server = await startServer({ root: path.join(workspace, 'output'), baseUrl: metadata.baseUrl, port });
    console.log(`Built docs: http://127.0.0.1:${server.address().port}${metadata.baseUrl}`);
    const close = () => server.close();
    process.once('SIGINT', close);
    process.once('SIGTERM', close);
  } catch (error) { console.error(error.message); process.exitCode = 1; }
}
