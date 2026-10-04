# Evidências de testes - Etapa 3

A Etapa 3 possui um projeto automatizado de testes em `tests/LocadoraVeiculos.Tests`.

Os testes usam `WebApplicationFactory`, xUnit e o provider InMemory do Entity Framework. Dessa forma a API completa é inicializada durante os testes sem depender de uma instalação de SQL Server no runner do GitHub.

## Casos automatizados

| Teste | Evidência produzida |
|---|---|
| Swagger disponível | GET em `/swagger/v1/swagger.json` deve retornar 200 e conter as rotas documentadas |
| CRUD de Fabricantes | POST, GET, PUT e DELETE executados pela API |
| Validação de entrada | Categoria com diária inválida deve retornar 400 |
| INNER e LEFT JOIN | Rotas de veículos disponíveis, categorias com frota e clientes sem aluguel são executadas |
| Fluxo de aluguel | Criação do aluguel altera veículo para `Alugado`; devolução finaliza e volta para `Disponivel` |
| Tratamento 404 | Recurso inexistente deve retornar `NotFound` |

## Execução local

```bash
dotnet restore LocadoraVeiculos.sln
dotnet build LocadoraVeiculos.sln --configuration Release
dotnet test LocadoraVeiculos.sln --configuration Release
```

## Evidência no GitHub Actions

O workflow `.github/workflows/dotnet.yml` executa automaticamente restore, build e testes em todo push e pull request.

Os resultados são gravados no formato TRX e enviados como artifact com o nome:

```text
evidencias-testes-etapa3
```

Assim, além do código dos testes, cada execução do GitHub Actions mantém uma evidência objetiva do resultado.
