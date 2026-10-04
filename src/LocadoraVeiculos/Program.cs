using System.Reflection;
using System.Text.Json.Serialization;
using LocadoraVeiculos.Data;
using LocadoraVeiculos.Middleware;
using LocadoraVeiculos.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocadoraConnection")));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var detalhes = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .ToDictionary(
                item => item.Key,
                item => item.Value!.Errors
                    .Select(erro => string.IsNullOrWhiteSpace(erro.ErrorMessage)
                        ? "Valor inválido."
                        : erro.ErrorMessage)
                    .ToArray());

        return new BadRequestObjectResult(new
        {
            erro = "Dados de entrada inválidos.",
            detalhes
        });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Locadora de Veículos API",
        Version = "v1",
        Description = "API REST do Trabalho Prático. Possui CRUD completo, regras de aluguel, filtros com INNER/LEFT JOIN, validação e tratamento de erros.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Davi Oliveira Parma"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    options.OperationFilter<ApiDocumentationOperationFilter>();
});

var app = builder.Build();

// Na Etapa 3 o Swagger fica disponível em qualquer ambiente da aplicação,
// facilitando a avaliação e a documentação das rotas.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Locadora de Veículos API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Locadora de Veículos - Swagger";
    options.DisplayRequestDuration();
    options.EnableDeepLinking();
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/status", async (ApplicationContext contexto) =>
{
    var conectado = await contexto.Database.CanConnectAsync();
    return Results.Ok(new
    {
        aplicacao = "Locadora de Veículos",
        etapa = "3 - Swagger, documentação e testes",
        bancoConectado = conectado
    });
})
.WithName("StatusAplicacao")
.WithSummary("Verifica o status da aplicação e da conexão com o banco.")
.WithDescription("Retorna o nome da aplicação, a etapa atual e se o SQL Server está acessível.");

app.Run();

public partial class Program { }
