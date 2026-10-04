# Locadora de Veículos - Trabalho Prático

Sistema de aluguel de veículos desenvolvido em C# com ASP.NET Core, Entity Framework Core e SQL Server Express.

**Aluno:** Davi Oliveira Parma  
**Código de pessoa:** 1597232  
**Curso:** Análise e Desenvolvimento de Software  
**Etapa atual:** Etapa 3 - Swagger, documentação e testes

## Etapa 3

A terceira etapa adiciona documentação OpenAPI/Swagger e testes automatizados com evidências no GitHub Actions.

### Critérios da rubrica

- **Swagger integrado e funcional:** interface em `/swagger` e OpenAPI em `/swagger/v1/swagger.json`.
- **Documentação das APIs:** descrições no Swagger e catálogo completo em `docs/api-etapa3.md`.
- **Testes com evidências:** projeto xUnit em `tests/LocadoraVeiculos.Tests`, execução automática no GitHub Actions e artifact TRX `evidencias-testes-etapa3`.

## Funcionalidades existentes

A API possui CRUD completo para:

- Fabricantes
- Categorias
- Filiais
- Veículos
- Clientes
- Aluguéis

Também possui fluxo de aluguel/devolução, validações, tratamento de erros e cinco filtros utilizando INNER JOIN e LEFT JOIN.

## Como rodar

Pré-requisitos:

- .NET SDK 8
- SQL Server Express
- dotnet-ef

```bash
dotnet restore LocadoraVeiculos.sln
dotnet ef migrations add Inicial --project src/LocadoraVeiculos
dotnet ef database update --project src/LocadoraVeiculos
dotnet run --project src/LocadoraVeiculos
```

Swagger:

```text
https://localhost:7147/swagger
```

## Como testar

```bash
dotnet test LocadoraVeiculos.sln --configuration Release
```

Documentação detalhada:

- `docs/modelo-conceitual.md`
- `docs/modelo-fisico.sql`
- `docs/etapa-2-backend.md`
- `docs/api-etapa3.md`
- `docs/evidencias-testes-etapa3.md`

## Tecnologias

- .NET 8 / C# 12
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server Express
- Swashbuckle / Swagger
- xUnit
- GitHub Actions

## Etapas

- Etapa 1: modelagem do banco de dados - concluída
- Etapa 2: implementação do backend - concluída
- Etapa 3: Swagger, documentação e testes - concluída
