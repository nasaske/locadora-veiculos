// Utilitários de interface compartilhados pelas duas páginas.

export const IMAGEM_PADRAO = 'img/sem-imagem.svg';

export const COMBUSTIVEIS = {
  Gasolina: 'Gasolina',
  Etanol: 'Etanol',
  Flex: 'Flex',
  Diesel: 'Diesel',
  Eletrico: 'Elétrico',
  Hibrido: 'Híbrido',
};

export const STATUS = {
  Disponivel: { rotulo: 'Disponível', classe: 'text-bg-success' },
  Alugado: { rotulo: 'Alugado', classe: 'text-bg-warning' },
  EmManutencao: { rotulo: 'Em manutenção', classe: 'text-bg-secondary' },
  Inativo: { rotulo: 'Inativo', classe: 'text-bg-dark' },
};

const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
const inteiro = new Intl.NumberFormat('pt-BR');

export const formatarMoeda = (valor) => moeda.format(Number(valor) || 0);
export const formatarKm = (km) => `${inteiro.format(Number(km) || 0)} km`;

/** "2025-01-15T00:00:00" -> "15/01/2025" (sem conversão de fuso horário). */
export function formatarData(dataIso) {
  const [ano, mes, dia] = String(dataIso ?? '').slice(0, 10).split('-');
  return ano && mes && dia ? `${dia}/${mes}/${ano}` : '-';
}

export function escaparHtml(texto) {
  return String(texto ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

export function rotuloStatus(status) {
  return STATUS[status]?.rotulo ?? status;
}

export function badgeStatus(status) {
  const classe = STATUS[status]?.classe ?? 'text-bg-light';
  return `<span class="badge ${classe}">${escaparHtml(rotuloStatus(status))}</span>`;
}

/** Só aceita URLs http/https (a API também valida); senão usa a imagem padrão. */
export function urlImagemSegura(url) {
  try {
    const endereco = new URL(url);
    return endereco.protocol === 'http:' || endereco.protocol === 'https:' ? endereco.href : IMAGEM_PADRAO;
  } catch {
    return IMAGEM_PADRAO;
  }
}

/** Troca imagens quebradas pela ilustração padrão (sem entrar em loop). */
export function tratarImagensQuebradas(raiz = document) {
  raiz.querySelectorAll('img[data-imagem-veiculo]').forEach((img) => {
    img.addEventListener('error', () => {
      if (!img.src.endsWith(IMAGEM_PADRAO)) img.src = IMAGEM_PADRAO;
    }, { once: true });
  });
}

/** Mensagem de erro em HTML, incluindo detalhes de validação da API. */
export function htmlErro(erro) {
  const itens = Object.values(erro?.detalhes ?? {}).flat();
  const lista = itens.length
    ? `<ul class="mb-0 mt-2">${itens.map((item) => `<li>${escaparHtml(item)}</li>`).join('')}</ul>`
    : '';
  return `<strong>${escaparHtml(erro?.message ?? 'Erro inesperado.')}</strong>${lista}`;
}

/** Notificação rápida (toast do Bootstrap) no canto da tela. */
export function mostrarAviso(mensagem, tipo = 'success') {
  let area = document.getElementById('area-avisos');
  if (!area) {
    area = document.createElement('div');
    area.id = 'area-avisos';
    area.className = 'toast-container position-fixed bottom-0 end-0 p-3';
    document.body.append(area);
  }

  const icone = tipo === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill';
  const aviso = document.createElement('div');
  aviso.className = `toast align-items-center text-bg-${tipo} border-0`;
  aviso.setAttribute('role', 'status');
  aviso.setAttribute('aria-live', 'polite');
  aviso.innerHTML = `
    <div class="d-flex">
      <div class="toast-body"><i class="bi ${icone} me-2"></i>${escaparHtml(mensagem)}</div>
      <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Fechar"></button>
    </div>`;
  area.append(aviso);
  aviso.addEventListener('hidden.bs.toast', () => aviso.remove());
  bootstrap.Toast.getOrCreateInstance(aviso, { delay: 4000 }).show();
}

/** Coloca o botão em estado de carregamento e devolve a função que restaura. */
export function botaoCarregando(botao, texto = 'Salvando...') {
  const original = botao.innerHTML;
  botao.disabled = true;
  botao.innerHTML = `<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>${escaparHtml(texto)}`;
  return () => {
    botao.disabled = false;
    botao.innerHTML = original;
  };
}
