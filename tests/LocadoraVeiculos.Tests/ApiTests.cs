using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LocadoraVeiculos.Tests;

public class ApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Swagger_deve_estar_disponivel_e_documentar_as_rotas()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var swagger = (await response.Content.ReadAsStringAsync()).ToLowerInvariant();
        Assert.Contains("locadora de veículos api", swagger);
        Assert.Contains("/api/fabricantes", swagger);
        Assert.Contains("/api/alugueis/{id}/devolucao", swagger);
        Assert.Contains("/api/filtros/veiculos-disponiveis", swagger);
        Assert.Contains("filtra veículos disponíveis", swagger);
    }

    [Fact]
    public async Task Fabricantes_deve_executar_crud_completo()
    {
        var create = await _client.PostAsJsonAsync("/api/fabricantes", new
        {
            nome = "Honda",
            paisOrigem = "Japão",
            anoFundacao = 1948,
            ativo = true
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var criado = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var id = criado.RootElement.GetProperty("fabricanteId").GetInt32();

        var get = await _client.GetAsync($"/api/fabricantes/{id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Contains("Honda", await get.Content.ReadAsStringAsync());

        var update = await _client.PutAsJsonAsync($"/api/fabricantes/{id}", new
        {
            nome = "Honda Motor",
            paisOrigem = "Japão",
            anoFundacao = 1948,
            ativo = true
        });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Contains("Honda Motor", await update.Content.ReadAsStringAsync());

        var delete = await _client.DeleteAsync($"/api/fabricantes/{id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var inexistente = await _client.GetAsync($"/api/fabricantes/{id}");
        Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
    }

    [Fact]
    public async Task Validacao_deve_retornar_bad_request_para_dados_invalidos()
    {
        var response = await _client.PostAsJsonAsync("/api/categorias", new
        {
            nome = "Categoria inválida",
            descricao = "Valor de diária zero",
            valorDiariaBase = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var corpo = (await response.Content.ReadAsStringAsync()).ToLowerInvariant();
        Assert.Contains("dados de entrada inválidos", corpo);
    }

    [Fact]
    public async Task Filtros_devem_executar_inner_e_left_join()
    {
        var clienteId = await CriarCliente();
        var veiculoId = await CriarVeiculo();

        var disponiveis = await _client.GetAsync("/api/filtros/veiculos-disponiveis?categoriaId=1&filialId=1");
        Assert.Equal(HttpStatusCode.OK, disponiveis.StatusCode);
        var corpoDisponiveis = await disponiveis.Content.ReadAsStringAsync();
        Assert.Contains("Argo Teste", corpoDisponiveis);
        Assert.Contains("Fiat", corpoDisponiveis);

        var categorias = await _client.GetAsync("/api/filtros/categorias-com-frota");
        Assert.Equal(HttpStatusCode.OK, categorias.StatusCode);
        var corpoCategorias = await categorias.Content.ReadAsStringAsync();
        Assert.Contains("Hatch Compacto", corpoCategorias);
        Assert.Contains("Utilitário", corpoCategorias);

        var semAlugueis = await _client.GetAsync("/api/filtros/clientes-sem-alugueis");
        Assert.Equal(HttpStatusCode.OK, semAlugueis.StatusCode);
        var corpoClientes = await semAlugueis.Content.ReadAsStringAsync();
        Assert.Contains("Cliente Teste", corpoClientes);

        Assert.True(veiculoId > 0);
    }

    [Fact]
    public async Task Aluguel_e_devolucao_devem_atualizar_status_e_quilometragem_do_veiculo()
    {
        var clienteId = await CriarCliente();
        var veiculoId = await CriarVeiculo();

        var aluguelResponse = await _client.PostAsJsonAsync("/api/alugueis", new
        {
            clienteId,
            veiculoId,
            filialId = 1,
            dataRetirada = DateTime.Now.AddHours(-1),
            dataDevolucaoPrevista = DateTime.Now.AddDays(3),
            observacoes = "Teste automatizado da Etapa 3"
        });

        Assert.Equal(HttpStatusCode.Created, aluguelResponse.StatusCode);
        var aluguelJson = JsonDocument.Parse(await aluguelResponse.Content.ReadAsStringAsync());
        var aluguelId = aluguelJson.RootElement.GetProperty("aluguelId").GetInt32();

        var veiculoAlugado = await _client.GetAsync($"/api/veiculos/{veiculoId}");
        var corpoAlugado = await veiculoAlugado.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Alugado\"", corpoAlugado);

        var devolucao = await _client.PostAsJsonAsync($"/api/alugueis/{aluguelId}/devolucao", new
        {
            dataDevolucaoEfetiva = DateTime.Now.AddDays(2),
            quilometragemFinal = 1350,
            valorMulta = 25.00m
        });

        Assert.Equal(HttpStatusCode.OK, devolucao.StatusCode);
        var corpoDevolucao = await devolucao.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Finalizado\"", corpoDevolucao);
        Assert.Contains("\"quilometragemFinal\":1350", corpoDevolucao);

        var veiculoDisponivel = await _client.GetAsync($"/api/veiculos/{veiculoId}");
        var corpoDisponivel = await veiculoDisponivel.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Disponivel\"", corpoDisponivel);
        Assert.Contains("\"quilometragem\":1350", corpoDisponivel);
    }

    [Fact]
    public async Task Recurso_inexistente_deve_retornar_not_found()
    {
        var response = await _client.GetAsync("/api/clientes/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<int> CriarCliente()
    {
        var email = $"cliente{Guid.NewGuid():N}@teste.com";

        var response = await _client.PostAsJsonAsync("/api/clientes", new
        {
            nome = "Cliente Teste",
            cpf = "12345678901",
            email,
            telefone = "31999999999",
            dataNascimento = new DateTime(1990, 1, 10),
            numeroCnh = "98765432101",
            validadeCnh = new DateTime(2035, 12, 31),
            endereco = "Rua de Teste, 100",
            cidade = "Belo Horizonte",
            uf = "MG"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("clienteId").GetInt32();
    }

    private async Task<int> CriarVeiculo()
    {
        var response = await _client.PostAsJsonAsync("/api/veiculos", new
        {
            placa = "TST1A23",
            chassi = "9BWZZZ377VT004251",
            modelo = "Argo Teste",
            anoFabricacao = 2025,
            anoModelo = 2026,
            cor = "Prata",
            quilometragem = 1000,
            combustivel = "Flex",
            status = "Disponivel",
            valorDiaria = 150.00m,
            dataAquisicao = new DateTime(2025, 1, 15),
            fabricanteId = 1,
            categoriaId = 1,
            filialId = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("veiculoId").GetInt32();
    }
}
