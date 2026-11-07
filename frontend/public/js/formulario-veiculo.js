// Formulário de veículo reutilizado pelos modais de cadastro (home) e edição (detalhes).

import { listarCategorias, listarFabricantes, listarFiliais } from './api.js';
import { COMBUSTIVEIS, IMAGEM_PADRAO, STATUS, escaparHtml, htmlErro, urlImagemSegura } from './ui.js';

const STATUS_EDITAVEIS = ['Disponivel', 'EmManutencao', 'Inativo'];
const ANO_ATUAL = new Date().getFullYear();

const hoje = () => {
  const data = new Date();
  data.setMinutes(data.getMinutes() - data.getTimezoneOffset());
  return data.toISOString().slice(0, 10);
};

const opcoes = (lista, valor, rotulo) =>
  lista.map((item) => `<option value="${escaparHtml(item[valor])}">${escaparHtml(rotulo(item))}</option>`).join('');

function template(p) {
  return `
    <div class="alert alert-danger d-none" role="alert" data-erro></div>
    <div class="row g-3">
      <div class="col-md-6">
        <label class="form-label" for="${p}-modelo">Modelo *</label>
        <input class="form-control" id="${p}-modelo" name="modelo" maxlength="60" required placeholder="Ex.: Onix LT 1.0">
        <div class="invalid-feedback">Informe o modelo (até 60 caracteres).</div>
      </div>
      <div class="col-md-6">
        <label class="form-label" for="${p}-fabricante">Fabricante *</label>
        <select class="form-select" id="${p}-fabricante" name="fabricanteId" required>
          <option value="">Carregando...</option>
        </select>
        <div class="invalid-feedback">Selecione o fabricante.</div>
      </div>
      <div class="col-md-6">
        <label class="form-label" for="${p}-categoria">Categoria *</label>
        <select class="form-select" id="${p}-categoria" name="categoriaId" required>
          <option value="">Carregando...</option>
        </select>
        <div class="invalid-feedback">Selecione a categoria.</div>
      </div>
      <div class="col-md-6">
        <label class="form-label" for="${p}-filial">Filial *</label>
        <select class="form-select" id="${p}-filial" name="filialId" required>
          <option value="">Carregando...</option>
        </select>
        <div class="invalid-feedback">Selecione a filial.</div>
      </div>
      <div class="col-md-4">
        <label class="form-label" for="${p}-placa">Placa *</label>
        <input class="form-control text-uppercase" id="${p}-placa" name="placa" required
               minlength="7" maxlength="8" pattern="[A-Za-z0-9\\-]{7,8}" placeholder="ABC1D23">
        <div class="invalid-feedback">Placa com 7 caracteres (ex.: ABC1D23).</div>
      </div>
      <div class="col-md-8">
        <label class="form-label" for="${p}-chassi">Chassi *</label>
        <input class="form-control text-uppercase" id="${p}-chassi" name="chassi" required
               minlength="17" maxlength="17" pattern="[A-Za-z0-9]{17}" placeholder="17 letras e números">
        <div class="invalid-feedback">O chassi deve ter exatamente 17 letras/números.</div>
      </div>
      <div class="col-6 col-md-3">
        <label class="form-label" for="${p}-ano-fabricacao">Ano fabricação *</label>
        <input class="form-control" type="number" id="${p}-ano-fabricacao" name="anoFabricacao"
               required min="1900" max="2100" value="${ANO_ATUAL}">
        <div class="invalid-feedback">Entre 1900 e 2100.</div>
      </div>
      <div class="col-6 col-md-3">
        <label class="form-label" for="${p}-ano-modelo">Ano modelo *</label>
        <input class="form-control" type="number" id="${p}-ano-modelo" name="anoModelo"
               required min="1900" max="2100" value="${ANO_ATUAL}">
        <div class="invalid-feedback">Entre 1900 e 2100.</div>
      </div>
      <div class="col-md-6">
        <label class="form-label" for="${p}-cor">Cor</label>
        <input class="form-control" id="${p}-cor" name="cor" maxlength="30" placeholder="Ex.: Prata">
      </div>
      <div class="col-md-4">
        <label class="form-label" for="${p}-quilometragem">Quilometragem *</label>
        <div class="input-group has-validation">
          <input class="form-control" type="number" id="${p}-quilometragem" name="quilometragem"
                 required min="0" step="1" value="0">
          <span class="input-group-text">km</span>
          <div class="invalid-feedback">Informe um valor maior ou igual a zero.</div>
        </div>
      </div>
      <div class="col-md-4">
        <label class="form-label" for="${p}-combustivel">Combustível *</label>
        <select class="form-select" id="${p}-combustivel" name="combustivel" required>
          ${Object.entries(COMBUSTIVEIS).map(([valor, rotulo]) => `<option value="${valor}">${rotulo}</option>`).join('')}
        </select>
      </div>
      <div class="col-md-4">
        <label class="form-label" for="${p}-status">Status *</label>
        <select class="form-select" id="${p}-status" name="status" required></select>
        <div class="form-text d-none" data-aviso-alugado>Veículo com aluguel em andamento: o status é controlado pelo aluguel.</div>
      </div>
      <div class="col-md-6">
        <label class="form-label" for="${p}-valor-diaria">Valor da diária *</label>
        <div class="input-group has-validation">
          <span class="input-group-text">R$</span>
          <input class="form-control" type="number" id="${p}-valor-diaria" name="valorDiaria"
                 required min="0.01" step="0.01" placeholder="0,00">
          <div class="invalid-feedback">Informe um valor maior que zero.</div>
        </div>
      </div>
      <div class="col-md-6">
        <label class="form-label" for="${p}-data-aquisicao">Data de aquisição *</label>
        <input class="form-control" type="date" id="${p}-data-aquisicao" name="dataAquisicao" required>
        <div class="invalid-feedback">Informe uma data que não esteja no futuro.</div>
      </div>
      <div class="col-md-9">
        <label class="form-label" for="${p}-imagem">URL da imagem</label>
        <input class="form-control" type="url" id="${p}-imagem" name="imagemUrl" maxlength="500"
               pattern="https?://.+" placeholder="https://...">
        <div class="form-text">Endereço público da foto (http ou https).</div>
        <div class="invalid-feedback">Use uma URL completa começando com http:// ou https://.</div>
      </div>
      <div class="col-md-3">
        <span class="form-label d-block">Prévia</span>
        <img class="previa-imagem rounded border" src="${IMAGEM_PADRAO}" alt="Prévia da imagem do veículo" data-previa>
      </div>
    </div>`;
}

/**
 * Monta o formulário dentro de <form> e devolve as operações usadas pelas páginas.
 * @param {HTMLFormElement} form
 */
export function criarFormularioVeiculo(form) {
  const prefixo = form.id;
  form.querySelector('[data-campos-veiculo]').innerHTML = template(prefixo);
  form.noValidate = true;

  const campo = (nome) => form.elements.namedItem(nome);
  const caixaErro = form.querySelector('[data-erro]');
  const previa = form.querySelector('[data-previa]');
  let categorias = [];
  let opcoesCarregadas = false;

  campo('placa').addEventListener('input', (e) => { e.target.value = e.target.value.toUpperCase(); });
  campo('chassi').addEventListener('input', (e) => { e.target.value = e.target.value.toUpperCase(); });
  campo('imagemUrl').addEventListener('input', atualizarPrevia);
  previa.addEventListener('error', () => { if (!previa.src.endsWith(IMAGEM_PADRAO)) previa.src = IMAGEM_PADRAO; });

  // Sugere a diária base da categoria quando o valor ainda não foi preenchido.
  campo('categoriaId').addEventListener('change', () => {
    const categoria = categorias.find((c) => String(c.categoriaId) === campo('categoriaId').value);
    if (categoria && !campo('valorDiaria').value) {
      campo('valorDiaria').value = Number(categoria.valorDiariaBase).toFixed(2);
    }
  });

  function atualizarPrevia() {
    const url = campo('imagemUrl').value.trim();
    previa.src = url ? urlImagemSegura(url) : IMAGEM_PADRAO;
  }

  function configurarStatus(statusAtual) {
    const alugado = statusAtual === 'Alugado';
    const lista = alugado ? ['Alugado'] : STATUS_EDITAVEIS;
    campo('status').innerHTML = lista
      .map((valor) => `<option value="${valor}">${STATUS[valor].rotulo}</option>`)
      .join('');
    campo('status').disabled = alugado;
    form.querySelector('[data-aviso-alugado]').classList.toggle('d-none', !alugado);
    campo('status').value = statusAtual && lista.includes(statusAtual) ? statusAtual : 'Disponivel';
  }

  async function carregarOpcoes() {
    if (opcoesCarregadas) return;
    const [fabricantes, listaCategorias, filiais] = await Promise.all([
      listarFabricantes(),
      listarCategorias(),
      listarFiliais(),
    ]);
    categorias = listaCategorias;

    const vazio = '<option value="">Selecione...</option>';
    campo('fabricanteId').innerHTML = vazio + opcoes(fabricantes, 'fabricanteId',
      (f) => (f.ativo ? f.nome : `${f.nome} (inativo)`));
    campo('categoriaId').innerHTML = vazio + opcoes(categorias, 'categoriaId', (c) => c.nome);
    campo('filialId').innerHTML = vazio + opcoes(filiais, 'filialId', (f) => `${f.nome} - ${f.cidade}/${f.uf}`);
    opcoesCarregadas = true;
  }

  function limpar() {
    form.reset();
    form.classList.remove('was-validated');
    esconderErro();
    configurarStatus('Disponivel');
    campo('combustivel').value = 'Flex';
    campo('dataAquisicao').max = hoje();
    campo('dataAquisicao').value = hoje();
    atualizarPrevia();
  }

  function preencher(veiculo) {
    limpar();
    const valores = {
      modelo: veiculo.modelo,
      fabricanteId: veiculo.fabricanteId,
      categoriaId: veiculo.categoriaId,
      filialId: veiculo.filialId,
      placa: veiculo.placa,
      chassi: veiculo.chassi,
      anoFabricacao: veiculo.anoFabricacao,
      anoModelo: veiculo.anoModelo,
      cor: veiculo.cor ?? '',
      quilometragem: veiculo.quilometragem,
      combustivel: veiculo.combustivel,
      valorDiaria: Number(veiculo.valorDiaria).toFixed(2),
      dataAquisicao: String(veiculo.dataAquisicao ?? '').slice(0, 10),
      imagemUrl: veiculo.imagemUrl ?? '',
    };
    Object.entries(valores).forEach(([nome, valor]) => { campo(nome).value = valor; });
    configurarStatus(veiculo.status);
    atualizarPrevia();
  }

  /** Valida no navegador (Bootstrap) antes de enviar para a API. */
  function validar() {
    const dataAquisicao = campo('dataAquisicao');
    dataAquisicao.setCustomValidity(dataAquisicao.value && dataAquisicao.value > hoje() ? 'futuro' : '');
    form.classList.add('was-validated');
    return form.checkValidity();
  }

  /** Converte os campos no corpo JSON esperado pela API (VeiculoUpsertDto). */
  function ler() {
    const texto = (nome) => campo(nome).value.trim();
    const numero = (nome) => Number(campo(nome).value);
    return {
      modelo: texto('modelo'),
      placa: texto('placa').toUpperCase(),
      chassi: texto('chassi').toUpperCase(),
      anoFabricacao: numero('anoFabricacao'),
      anoModelo: numero('anoModelo'),
      cor: texto('cor') || null,
      quilometragem: numero('quilometragem'),
      combustivel: campo('combustivel').value,
      status: campo('status').value,
      valorDiaria: numero('valorDiaria'),
      dataAquisicao: campo('dataAquisicao').value,
      imagemUrl: texto('imagemUrl') || null,
      fabricanteId: numero('fabricanteId'),
      categoriaId: numero('categoriaId'),
      filialId: numero('filialId'),
    };
  }

  function mostrarErro(erro) {
    caixaErro.innerHTML = htmlErro(erro);
    caixaErro.classList.remove('d-none');
    caixaErro.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
  }

  function esconderErro() {
    caixaErro.classList.add('d-none');
    caixaErro.innerHTML = '';
  }

  return { carregarOpcoes, limpar, preencher, validar, ler, mostrarErro, esconderErro };
}
