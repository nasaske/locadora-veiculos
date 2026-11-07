// Servidor web do front-end da Locadora de Veículos.
// Usa somente módulos nativos do Node.js (sem dependências): serve os arquivos
// da pasta public/ e as páginas consomem a API C# diretamente via fetch.
//
// Uso:  npm start                         -> http://localhost:3000
//       PORT=4000 npm start               -> muda a porta do front
//       API_URL=http://localhost:5000 npm start  -> muda o endereço da API

import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { extname, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const PORTA = Number(process.env.PORT) || 3000;
const API_URL = process.env.API_URL?.trim().replace(/\/+$/, '');
const PASTA_PUBLICA = resolve(fileURLToPath(new URL('./public', import.meta.url)));

const TIPOS_DE_CONTEUDO = {
  '.html': 'text/html; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.webp': 'image/webp',
  '.ico': 'image/x-icon',
};

/** Converte a URL pedida em um caminho dentro de public/ (ou null se tentar sair dela). */
function resolverArquivo(urlPedida) {
  let caminho;
  try {
    caminho = decodeURIComponent(new URL(urlPedida, 'http://localhost').pathname);
  } catch {
    return null;
  }

  const relativo = caminho === '/' ? 'index.html' : caminho.replace(/^\/+/, '');
  const absoluto = resolve(PASTA_PUBLICA, relativo);
  const dentroDaPasta = absoluto.startsWith(PASTA_PUBLICA + sep);
  return dentroDaPasta ? absoluto : null;
}

function responder(resposta, status, corpo, tipo = 'text/plain; charset=utf-8') {
  resposta.writeHead(status, {
    'Content-Type': tipo,
    'Cache-Control': 'no-cache',
    'X-Content-Type-Options': 'nosniff',
  });
  resposta.end(corpo);
}

const servidor = createServer(async (requisicao, resposta) => {
  if (requisicao.method !== 'GET' && requisicao.method !== 'HEAD') {
    responder(resposta, 405, 'Método não permitido.');
    return;
  }

  const arquivo = resolverArquivo(requisicao.url ?? '/');
  if (!arquivo) {
    responder(resposta, 400, 'Caminho inválido.');
    return;
  }

  // Permite trocar o endereço da API por variável de ambiente, sem editar o código.
  if (API_URL && arquivo === resolve(PASTA_PUBLICA, 'js', 'config.js')) {
    responder(resposta, 200, `export const API_URL = ${JSON.stringify(API_URL)};\n`, TIPOS_DE_CONTEUDO['.js']);
    return;
  }

  try {
    const conteudo = await readFile(arquivo);
    const tipo = TIPOS_DE_CONTEUDO[extname(arquivo).toLowerCase()] ?? 'application/octet-stream';
    responder(resposta, 200, requisicao.method === 'HEAD' ? undefined : conteudo, tipo);
  } catch (erro) {
    if (erro.code === 'ENOENT' || erro.code === 'EISDIR') {
      responder(resposta, 404, 'Página não encontrada.');
      return;
    }
    console.error('Erro ao servir arquivo:', erro);
    responder(resposta, 500, 'Erro interno no servidor do front-end.');
  }
});

servidor.listen(PORTA, () => {
  console.log(`Front-end da Locadora disponível em http://localhost:${PORTA}`);
  console.log(`API consumida em ${API_URL ?? 'http://localhost:5147 (padrão de public/js/config.js)'}`);
});
