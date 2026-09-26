using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Erro de integridade ao persistir dados.");
            await EscreverErro(context, HttpStatusCode.Conflict,
                "Não foi possível concluir a operação por causa de uma restrição de integridade do banco de dados.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado na API.");
            await EscreverErro(context, HttpStatusCode.InternalServerError,
                "Ocorreu um erro interno ao processar a solicitação.");
        }
    }

    private static async Task EscreverErro(HttpContext context, HttpStatusCode statusCode, string mensagem)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var resposta = JsonSerializer.Serialize(new
        {
            erro = mensagem,
            status = (int)statusCode,
            traceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(resposta);
    }
}
