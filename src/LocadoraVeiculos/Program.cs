using LocadoraVeiculos.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registro do contexto do EF Core apontando para o SQL Server Express
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocadoraConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Locadora de Veículos API",
        Version = "v1",
        Description = "Trabalho Prático 1 - Etapa 1 (modelagem do banco de dados com Entity Framework Core)."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Endpoint simples só para confirmar que a aplicação e a conexão estão de pé.
app.MapGet("/status", async (ApplicationContext contexto) =>
{
    var conectado = await contexto.Database.CanConnectAsync();
    return Results.Ok(new
    {
        aplicacao = "Locadora de Veículos",
        etapa = "1 - Modelagem do banco de dados",
        bancoConectado = conectado
    });
});

app.Run();
