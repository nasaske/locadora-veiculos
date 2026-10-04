using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LocadoraVeiculos.Swagger;

/// <summary>
/// Adiciona títulos, descrições e respostas comuns às operações exibidas no Swagger.
/// </summary>
public sealed class ApiDocumentationOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        context.ApiDescription.ActionDescriptor.RouteValues.TryGetValue("controller", out var controllerValue);
        context.ApiDescription.ActionDescriptor.RouteValues.TryGetValue("action", out var actionValue);

        var controller = controllerValue ?? string.Empty;
        var action = actionValue ?? string.Empty;

        if (string.IsNullOrWhiteSpace(controller))
            return;

        if (controller.Equals("Filtros", StringComparison.OrdinalIgnoreCase))
        {
            DocumentarFiltro(operation, action);
        }
        else
        {
            DocumentarCrud(operation, controller, action);
        }

        AdicionarResposta(operation, "400", "Requisição inválida ou dados de entrada inconsistentes.");
        AdicionarResposta(operation, "404", "Recurso solicitado não encontrado.");
        AdicionarResposta(operation, "409", "Conflito com o estado atual ou com uma regra de integridade.");
        AdicionarResposta(operation, "500", "Erro interno inesperado.");
    }

    private static void DocumentarCrud(OpenApiOperation operation, string controller, string action)
    {
        var recurso = NomeAmigavel(controller);

        (operation.Summary, operation.Description) = action switch
        {
            "GetAll" => ($"Lista {recurso}", $"Retorna todos os registros de {recurso} cadastrados no sistema."),
            "GetById" => ($"Busca {recurso} por id", $"Retorna um registro específico de {recurso} pelo identificador."),
            "Create" => ($"Cria {recurso}", $"Valida os dados recebidos e cria um novo registro de {recurso}."),
            "Update" => ($"Atualiza {recurso}", $"Atualiza um registro existente de {recurso}, respeitando as regras de negócio."),
            "Delete" => ($"Exclui {recurso}", $"Exclui um registro de {recurso} quando não houver vínculos que impeçam a operação."),
            "RegistrarDevolucao" => ("Registra a devolução do veículo",
                "Finaliza o aluguel, atualiza quilometragem, calcula o valor total e torna o veículo disponível novamente."),
            _ => ($"{action} - {recurso}", $"Operação da API relacionada a {recurso}.")
        };
    }

    private static void DocumentarFiltro(OpenApiOperation operation, string action)
    {
        (operation.Summary, operation.Description) = action switch
        {
            "VeiculosDisponiveis" => (
                "Filtra veículos disponíveis",
                "Consulta com INNER JOIN entre veículos, fabricantes, categorias e filiais. Aceita categoria, filial e valor máximo."),
            "AlugueisPorCliente" => (
                "Lista aluguéis de um cliente",
                "Consulta com INNER JOIN entre aluguéis, clientes, veículos e fabricantes."),
            "AlugueisPorPeriodo" => (
                "Filtra aluguéis por período",
                "Consulta com INNER JOIN entre aluguéis, clientes, veículos e filiais."),
            "CategoriasComFrota" => (
                "Lista categorias com quantidade de veículos",
                "Consulta com LEFT JOIN para manter no resultado categorias que ainda não possuem veículos."),
            "ClientesSemAlugueis" => (
                "Lista clientes sem aluguéis",
                "Consulta com LEFT JOIN para localizar clientes que nunca realizaram uma locação."),
            _ => ("Executa filtro", "Consulta específica de relatório da locadora.")
        };
    }

    private static string NomeAmigavel(string controller) => controller switch
    {
        "Fabricantes" => "fabricantes",
        "Categorias" => "categorias",
        "Filiais" => "filiais",
        "Veiculos" => "veículos",
        "Clientes" => "clientes",
        "Alugueis" => "aluguéis",
        _ => controller
    };

    private static void AdicionarResposta(OpenApiOperation operation, string codigo, string descricao)
    {
        if (!operation.Responses.ContainsKey(codigo))
            operation.Responses[codigo] = new OpenApiResponse { Description = descricao };
    }
}
