// Integração com a API C# da Locadora de Veículos.
// Todas as chamadas usam somente fetch (GET, POST, PUT e DELETE).

import { API_URL } from './config.js';

/** Erro de API com status HTTP e detalhes de validação (quando houver). */
export class ErroApi extends Error {
  constructor(mensagem, status, detalhes = null) {
    super(mensagem);
    this.name = 'ErroApi';
    this.status = status;
    this.detalhes = detalhes;
  }
}

async function requisitar(caminho, { method = 'GET', dados } = {}) {
  const opcoes = {
    method,
    headers: { Accept: 'application/json' },
  };

  if (dados !== undefined) {
    opcoes.headers['Content-Type'] = 'application/json';
    opcoes.body = JSON.stringify(dados);
  }

  let resposta;
  try {
    resposta = await fetch(`${API_URL}/api${caminho}`, opcoes);
  } catch {
    throw new ErroApi(
      `Não foi possível conectar à API em ${API_URL}. Verifique se ela está em execução.`,
      0,
    );
  }

  // 204 No Content (exclusão) não tem corpo.
  if (resposta.status === 204) return null;

  const corpo = await resposta.json().catch(() => null);
  if (!resposta.ok) {
    const mensagem = corpo?.erro ?? `A API respondeu com erro ${resposta.status}.`;
    throw new ErroApi(mensagem, resposta.status, corpo?.detalhes ?? null);
  }
  return corpo;
}

// ---------------------------- Veículos ----------------------------

/** GET /api/veiculos/recentes?quantidade=12 - últimos veículos cadastrados. */
export function listarVeiculosRecentes(quantidade = 12) {
  return requisitar(`/veiculos/recentes?quantidade=${quantidade}`);
}

/** GET /api/veiculos/{id} - todas as informações de um veículo. */
export function buscarVeiculo(id) {
  return requisitar(`/veiculos/${encodeURIComponent(id)}`);
}

/** POST /api/veiculos - cadastra um novo veículo. */
export function criarVeiculo(veiculo) {
  return requisitar('/veiculos', { method: 'POST', dados: veiculo });
}

/** PUT /api/veiculos/{id} - atualiza um veículo existente. */
export function atualizarVeiculo(id, veiculo) {
  return requisitar(`/veiculos/${encodeURIComponent(id)}`, { method: 'PUT', dados: veiculo });
}

/** DELETE /api/veiculos/{id} - exclui um veículo. */
export function excluirVeiculo(id) {
  return requisitar(`/veiculos/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

// ------------------- Cadastros de apoio (selects) -------------------

/** GET /api/fabricantes */
export function listarFabricantes() {
  return requisitar('/fabricantes');
}

/** GET /api/categorias */
export function listarCategorias() {
  return requisitar('/categorias');
}

/** GET /api/filiais */
export function listarFiliais() {
  return requisitar('/filiais');
}
