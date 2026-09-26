using System.Text.Json.Serialization;
using LocadoraVeiculos.Data;
using LocadoraVeiculos.Middleware;
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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Locadora de Veículos API",
        Version = "v1",
        Description = "Trabalho Prático - Etapa 2: backend REST, CRUD, validações, tratamento de erros e filtros com JOINs."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/status", async (ApplicationContext contexto) =>
{
    var conectado = await contexto.Database.CanConnectAsync();
    return Results.Ok(new
    {
        aplicacao = "Locadora de Veículos",
        etapa = "2 - Implementação do Backend",
        bancoConectado = conectado
    });
});

app.Run();
