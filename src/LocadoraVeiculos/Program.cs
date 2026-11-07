using System.Reflection;
using System.Text.Json.Serialization;
using LocadoraVeiculos.Data;
using LocadoraVeiculos.Middleware;
using LocadoraVeiculos.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

const string PoliticaCorsFrontend = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// "BancoEmMemoria" (perfil "demo" do launchSettings) permite rodar a API e o
// front-end sem SQL Server instalado. O padrão continua sendo o SQL Server Express.
var usarBancoEmMemoria = builder.Configuration.GetValue<bool>("BancoEmMemoria");

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    if (usarBancoEmMemoria)
        options.UseInMemoryDatabase("LocadoraVeiculosDemo");
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("LocadoraConnection"));
});

// O front-end (pasta frontend/, servido pelo Node.js) roda em outra porta e
// consome a API via fetch, por isso a origem dele precisa ser liberada no CORS.
var origensFrontend = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>()
    ?? ["http://localhost:3000", "http://127.0.0.1:3000"];

builder.Services.AddCors(options =>
    options.AddPolicy(PoliticaCorsFrontend, politica => politica
        .WithOrigins(origensFrontend)
        .AllowAnyHeader()
        .AllowAnyMethod()));

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

app.UseCors(PoliticaCorsFrontend);
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

await PrepararBancoAsync(app, usarBancoEmMemoria);

app.Run();

// Cria o banco em memória (modo demo) e grava a frota de exemplo quando a
// tabela de veículos está vazia. Falhas aqui não derrubam a API: só geram log.
static async Task PrepararBancoAsync(WebApplication app, bool usarBancoEmMemoria)
{
    var popularExemplos = app.Configuration.GetValue<bool>("PopularVeiculosDeExemplo");
    if (!usarBancoEmMemoria && !popularExemplos)
        return;

    using var escopo = app.Services.CreateScope();
    var contexto = escopo.ServiceProvider.GetRequiredService<ApplicationContext>();
    var logger = escopo.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        if (usarBancoEmMemoria)
            await contexto.Database.EnsureCreatedAsync();

        if (!popularExemplos || !await contexto.Database.CanConnectAsync())
            return;

        var inseridos = await VeiculosDeExemplo.PopularAsync(contexto);
        if (inseridos > 0)
            logger.LogInformation("{Quantidade} veículos de exemplo cadastrados.", inseridos);
    }
    catch (Exception erro)
    {
        logger.LogWarning(erro,
            "Não foi possível preparar os dados de exemplo. Verifique se as migrations foram aplicadas.");
    }
}

public partial class Program { }
