// Página de detalhes: todas as informações do veículo + edição (modal) + exclusão com confirmação.

import { atualizarVeiculo, buscarVeiculo, excluirVeiculo } from './api.js';
import { criarFormularioVeiculo } from './formulario-veiculo.js';
import {
  COMBUSTIVEIS, badgeStatus, botaoCarregando, escaparHtml, formatarData, formatarKm,
  formatarMoeda, htmlErro, mostrarAviso, rotuloStatus, tratarImagensQuebradas, urlImagemSegura,
} from './ui.js';

const areaDetalhe = document.getElementById('detalhe-veiculo');
const trilhaModelo = document.getElementById('trilha-modelo');
const idVeiculo = Number(new URLSearchParams(window.location.search).get('id'));

let veiculoAtual = null;

const nomeCompleto = (veiculo) => `${veiculo.fabricante} ${veiculo.modelo}`;

function item(rotulo, valor) {
  return `
    <div class="col-sm-6 col-xl-4">
      <dt>${escaparHtml(rotulo)}</dt>
      <dd>${valor}</dd>
    </div>`;
}

function renderizarVeiculo(veiculo) {
  const nome = nomeCompleto(veiculo);
  const imagem = urlImagemSegura(veiculo.imagemUrl);
  document.title = `${nome} | Locadora de Veículos`;
  trilhaModelo.textContent = nome;

  const fonteImagem = veiculo.imagemUrl
    ? `<a href="${escaparHtml(imagem)}" target="_blank" rel="noopener">Abrir imagem original <i class="bi bi-box-arrow-up-right"></i></a>`
    : '<span class="text-body-secondary">Sem imagem cadastrada</span>';

  areaDetalhe.innerHTML = `
    <div class="row g-4 align-items-start">
      <div class="col-lg-5">
        <div class="ratio ratio-4x3 rounded-3 overflow-hidden shadow-sm detalhe-foto">
          <img src="${escaparHtml(imagem)}" alt="Foto do ${escaparHtml(nome)}" class="object-fit-cover" data-imagem-veiculo>
        </div>
      </div>

      <div class="col-lg-7">
        <span class="rotulo">Descrição</span>
        <div class="d-flex flex-wrap align-items-center gap-2 mt-1">
          <h1 class="h2 mb-0">${escaparHtml(nome)}</h1>
          ${badgeStatus(veiculo.status)}
        </div>
        <p class="text-body-secondary mb-3">
          ${escaparHtml(veiculo.categoria)} · ${veiculo.anoFabricacao}/${veiculo.anoModelo} · ${escaparHtml(COMBUSTIVEIS[veiculo.combustivel] ?? veiculo.combustivel)}
        </p>
        <p class="preco preco-grande mb-4">${formatarMoeda(veiculo.valorDiaria)} <small>/ diária</small></p>

        <dl class="row g-3 ficha-tecnica">
          ${item('Código', `#${veiculo.veiculoId}`)}
          ${item('Placa', escaparHtml(veiculo.placa))}
          ${item('Chassi', `<span class="font-monospace">${escaparHtml(veiculo.chassi)}</span>`)}
          ${item('Fabricante', escaparHtml(veiculo.fabricante))}
          ${item('Modelo', escaparHtml(veiculo.modelo))}
          ${item('Categoria', escaparHtml(veiculo.categoria))}
          ${item('Ano de fabricação', veiculo.anoFabricacao)}
          ${item('Ano do modelo', veiculo.anoModelo)}
          ${item('Cor', escaparHtml(veiculo.cor || 'Não informada'))}
          ${item('Quilometragem', formatarKm(veiculo.quilometragem))}
          ${item('Combustível', escaparHtml(COMBUSTIVEIS[veiculo.combustivel] ?? veiculo.combustivel))}
          ${item('Status', escaparHtml(rotuloStatus(veiculo.status)))}
          ${item('Valor da diária', formatarMoeda(veiculo.valorDiaria))}
          ${item('Data de aquisição', formatarData(veiculo.dataAquisicao))}
          ${item('Filial', escaparHtml(veiculo.filial))}
          ${item('Imagem', fonteImagem)}
        </dl>

        <div class="d-flex flex-wrap gap-2 mt-2">
          <button class="btn btn-primary" type="button" data-bs-toggle="modal" data-bs-target="#modal-editar-veiculo">
            <i class="bi bi-pencil-square me-1"></i>Editar
          </button>
          <button class="btn btn-outline-danger" type="button" data-bs-toggle="modal" data-bs-target="#modal-excluir-veiculo">
            <i class="bi bi-trash3 me-1"></i>Excluir
          </button>
          <a class="btn btn-link ms-sm-auto" href="index.html"><i class="bi bi-arrow-left me-1"></i>Voltar para a vitrine</a>
        </div>
      </div>
    </div>`;

  tratarImagensQuebradas(areaDetalhe);
}

function renderizarErro(erro) {
  const naoEncontrado = erro?.status === 404;
  const titulo = naoEncontrado ? 'Veículo não encontrado' : 'Não foi possível carregar o veículo';
  const explicacao = naoEncontrado
    ? `<p class="mb-0">${escaparHtml(erro.detalheExtra ?? 'Ele pode ter sido excluído ou o endereço está incorreto.')}</p>`
    : `<div>${htmlErro(erro)}</div>`;
  trilhaModelo.textContent = titulo;
  areaDetalhe.innerHTML = `
    <div class="alert alert-${naoEncontrado ? 'warning' : 'danger'}">
      <h1 class="h5 alert-heading">${titulo}</h1>
      ${explicacao}
      <a class="btn btn-sm btn-outline-secondary mt-3" href="index.html"><i class="bi bi-arrow-left me-1"></i>Voltar para a vitrine</a>
    </div>`;
}

async function carregarVeiculo() {
  if (!Number.isInteger(idVeiculo) || idVeiculo <= 0) {
    renderizarErro({ status: 404, detalheExtra: 'Nenhum veículo foi informado no endereço (ex.: detalhes.html?id=1).' });
    return;
  }

  try {
    veiculoAtual = await buscarVeiculo(idVeiculo);
    renderizarVeiculo(veiculoAtual);
  } catch (erro) {
    renderizarErro(erro);
  }
}

// --------------------------- Modal: editar ---------------------------

const modalEditar = document.getElementById('modal-editar-veiculo');
const formEditar = document.getElementById('form-editar-veiculo');
const formulario = criarFormularioVeiculo(formEditar);

modalEditar.addEventListener('show.bs.modal', async () => {
  formulario.limpar();
  try {
    await formulario.carregarOpcoes();
    formulario.preencher(veiculoAtual);
  } catch (erro) {
    formulario.mostrarErro(erro);
  }
});

formEditar.addEventListener('submit', async (evento) => {
  evento.preventDefault();
  formulario.esconderErro();
  if (!formulario.validar()) return;

  const restaurar = botaoCarregando(formEditar.querySelector('[type="submit"]'));
  try {
    veiculoAtual = await atualizarVeiculo(veiculoAtual.veiculoId, formulario.ler());
    renderizarVeiculo(veiculoAtual);
    bootstrap.Modal.getInstance(modalEditar).hide();
    mostrarAviso('Alterações salvas com sucesso!');
  } catch (erro) {
    formulario.mostrarErro(erro);
  } finally {
    restaurar();
  }
});

// ------------------------ Modal: excluir (confirmação) ------------------------

const modalExcluir = document.getElementById('modal-excluir-veiculo');
const erroExclusao = document.getElementById('erro-exclusao');
const botaoConfirmar = document.getElementById('confirmar-exclusao');

modalExcluir.addEventListener('show.bs.modal', () => {
  document.getElementById('excluir-nome').textContent =
    `${nomeCompleto(veiculoAtual)} (placa ${veiculoAtual.placa})`;
  erroExclusao.classList.add('d-none');
  botaoConfirmar.disabled = false;
});

botaoConfirmar.addEventListener('click', async () => {
  const restaurar = botaoCarregando(botaoConfirmar, 'Excluindo...');
  try {
    await excluirVeiculo(veiculoAtual.veiculoId);
    const nome = encodeURIComponent(nomeCompleto(veiculoAtual));
    window.location.href = `index.html?excluido=${nome}`;
  } catch (erro) {
    erroExclusao.innerHTML = htmlErro(erro);
    erroExclusao.classList.remove('d-none');
    restaurar();
  }
});

carregarVeiculo();
