// Página inicial: carrossel de destaque + coleção dos últimos 12 veículos + cadastro via modal.

import { criarVeiculo, listarVeiculosRecentes } from './api.js';
import { criarFormularioVeiculo } from './formulario-veiculo.js';
import {
  badgeStatus, botaoCarregando, escaparHtml, formatarMoeda, htmlErro,
  mostrarAviso, tratarImagensQuebradas, urlImagemSegura,
} from './ui.js';

const QUANTIDADE_VITRINE = 12;
const QUANTIDADE_DESTAQUES = 3;

const areaDestaque = document.getElementById('destaque');
const listaVeiculos = document.getElementById('lista-veiculos');
const mensagemFrota = document.getElementById('mensagem-frota');
const resumoFrota = document.getElementById('resumo-frota');

const linkDetalhes = (veiculo) => `detalhes.html?id=${encodeURIComponent(veiculo.veiculoId)}`;
const nomeCompleto = (veiculo) => `${veiculo.fabricante} ${veiculo.modelo}`;

// ----------------------------- Vitrine -----------------------------

function cardVeiculo(veiculo) {
  return `
    <div class="col">
      <article class="card h-100 card-veiculo">
        <div class="ratio ratio-4x3 card-foto">
          <img src="${escaparHtml(urlImagemSegura(veiculo.imagemUrl))}" alt="Foto do ${escaparHtml(nomeCompleto(veiculo))}"
               class="object-fit-cover" loading="lazy" data-imagem-veiculo>
        </div>
        <div class="card-body d-flex flex-column">
          <div class="d-flex justify-content-between align-items-start gap-2 mb-1">
            <span class="small text-body-secondary">${escaparHtml(veiculo.categoria)}</span>
            ${badgeStatus(veiculo.status)}
          </div>
          <h3 class="h6 card-title mb-1">${escaparHtml(nomeCompleto(veiculo))}</h3>
          <p class="small text-body-secondary mb-3">${veiculo.anoFabricacao}/${veiculo.anoModelo} · ${escaparHtml(veiculo.placa)}</p>
          <p class="preco mt-auto mb-3">${formatarMoeda(veiculo.valorDiaria)} <small>/ diária</small></p>
          <a class="btn btn-outline-primary w-100" href="${linkDetalhes(veiculo)}">
            Ver detalhes <i class="bi bi-arrow-right"></i>
          </a>
        </div>
      </article>
    </div>`;
}

function slideDestaque(veiculo, indice) {
  return `
    <div class="carousel-item ${indice === 0 ? 'active' : ''}">
      <img src="${escaparHtml(urlImagemSegura(veiculo.imagemUrl))}" class="d-block w-100 destaque-foto"
           alt="Foto do ${escaparHtml(nomeCompleto(veiculo))}" data-imagem-veiculo>
      <div class="carousel-caption destaque-legenda text-start">
        <span class="badge text-bg-light mb-2">Novo na frota</span>
        <h2 class="h3 mb-1">${escaparHtml(nomeCompleto(veiculo))}</h2>
        <p class="mb-3">${escaparHtml(veiculo.categoria)} · a partir de <strong>${formatarMoeda(veiculo.valorDiaria)}</strong> por dia</p>
        <a class="btn btn-light btn-sm" href="${linkDetalhes(veiculo)}">Ver detalhes</a>
      </div>
    </div>`;
}

function renderizarDestaque(veiculos) {
  const destaques = veiculos.slice(0, QUANTIDADE_DESTAQUES);
  if (!destaques.length) {
    areaDestaque.innerHTML = `
      <div class="destaque-vazio rounded-3 text-center p-5">
        <i class="bi bi-car-front display-4 d-block mb-2"></i>
        Os veículos cadastrados aparecerão aqui.
      </div>`;
    return;
  }

  const indicadores = destaques.map((_, i) => `
    <button type="button" data-bs-target="#carrossel-destaque" data-bs-slide-to="${i}"
            ${i === 0 ? 'class="active" aria-current="true"' : ''} aria-label="Destaque ${i + 1}"></button>`).join('');

  areaDestaque.innerHTML = `
    <div id="carrossel-destaque" class="carousel slide rounded-3 overflow-hidden shadow-sm">
      <div class="carousel-indicators">${indicadores}</div>
      <div class="carousel-inner">${destaques.map(slideDestaque).join('')}</div>
      <button class="carousel-control-prev" type="button" data-bs-target="#carrossel-destaque" data-bs-slide="prev">
        <span class="carousel-control-prev-icon" aria-hidden="true"></span>
        <span class="visually-hidden">Anterior</span>
      </button>
      <button class="carousel-control-next" type="button" data-bs-target="#carrossel-destaque" data-bs-slide="next">
        <span class="carousel-control-next-icon" aria-hidden="true"></span>
        <span class="visually-hidden">Próximo</span>
      </button>
    </div>`;

  tratarImagensQuebradas(areaDestaque);
  bootstrap.Carousel.getOrCreateInstance(document.getElementById('carrossel-destaque'), {
    interval: 5000,
    ride: 'carousel',
  });
}

function renderizarLista(veiculos) {
  if (!veiculos.length) {
    listaVeiculos.innerHTML = '';
    mensagemFrota.innerHTML = `
      <div class="alert alert-info">
        Nenhum veículo cadastrado ainda. Use o botão <strong>Novo veículo</strong> para começar.
      </div>`;
    resumoFrota.textContent = 'Nenhum veículo cadastrado.';
    return;
  }

  mensagemFrota.innerHTML = '';
  listaVeiculos.innerHTML = veiculos.map(cardVeiculo).join('');
  resumoFrota.textContent = `Exibindo os ${veiculos.length} cadastros mais recentes, do mais novo para o mais antigo.`;
  tratarImagensQuebradas(listaVeiculos);
}

function mostrarCarregando() {
  const cartao = `
    <div class="col">
      <div class="card h-100 placeholder-glow" aria-hidden="true">
        <div class="ratio ratio-4x3 placeholder"></div>
        <div class="card-body">
          <span class="placeholder col-8 mb-2"></span>
          <span class="placeholder col-5 mb-3"></span>
          <span class="placeholder btn btn-outline-primary disabled col-12"></span>
        </div>
      </div>
    </div>`;
  listaVeiculos.innerHTML = cartao.repeat(4);
}

async function carregarVitrine() {
  mostrarCarregando();
  try {
    const veiculos = await listarVeiculosRecentes(QUANTIDADE_VITRINE);
    renderizarDestaque(veiculos);
    renderizarLista(veiculos);
  } catch (erro) {
    listaVeiculos.innerHTML = '';
    areaDestaque.innerHTML = '';
    resumoFrota.textContent = 'Não foi possível carregar a frota.';
    mensagemFrota.innerHTML = `
      <div class="alert alert-danger d-flex flex-wrap align-items-center justify-content-between gap-2">
        <div>${htmlErro(erro)}</div>
        <button class="btn btn-sm btn-outline-danger" type="button" data-recarregar>Tentar novamente</button>
      </div>`;
    mensagemFrota.querySelector('[data-recarregar]').addEventListener('click', carregarVitrine);
  }
}

// ------------------------ Modal: novo veículo ------------------------

const modalNovo = document.getElementById('modal-novo-veiculo');
const formNovo = document.getElementById('form-novo-veiculo');
const formulario = criarFormularioVeiculo(formNovo);

modalNovo.addEventListener('show.bs.modal', async () => {
  formulario.limpar();
  try {
    await formulario.carregarOpcoes();
  } catch (erro) {
    formulario.mostrarErro(erro);
  }
});

modalNovo.addEventListener('shown.bs.modal', () => formNovo.elements.namedItem('modelo').focus());

formNovo.addEventListener('submit', async (evento) => {
  evento.preventDefault();
  formulario.esconderErro();
  if (!formulario.validar()) return;

  const restaurar = botaoCarregando(formNovo.querySelector('[type="submit"]'));
  try {
    const criado = await criarVeiculo(formulario.ler());
    bootstrap.Modal.getInstance(modalNovo).hide();
    mostrarAviso(`${criado.fabricante} ${criado.modelo} cadastrado com sucesso!`);
    await carregarVitrine();
  } catch (erro) {
    formulario.mostrarErro(erro);
  } finally {
    restaurar();
  }
});

// Aviso vindo da página de detalhes após uma exclusão (?excluido=Modelo).
const parametros = new URLSearchParams(window.location.search);
if (parametros.has('excluido')) {
  mostrarAviso(`Veículo "${parametros.get('excluido')}" excluído com sucesso.`);
  history.replaceState(null, '', window.location.pathname);
}

carregarVitrine();
