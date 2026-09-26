# Locadora de Veículos - Trabalho Prático

Sistema de aluguel de veículos desenvolvido em C# com ASP.NET Core, Entity Framework Core e SQL Server Express.

**Aluno:** Davi Oliveira Parma  
**Código de pessoa:** 1597232  
**Curso:** Análise e Desenvolvimento de Software  
**Etapa atual:** Etapa 2 - Implementação do Backend

---

## Entrega da Etapa 2

A API agora possui CRUD completo para Fabricantes, Categorias, Filiais, Veículos, Clientes e Aluguéis.

Também foram adicionados:

- DTOs de entrada e saída;
- validações com Data Annotations e regras de negócio;
- tratamento global de exceções;
- respostas HTTP 400, 404, 409 e 500 conforme o tipo de erro;
- controle do fluxo de aluguel e devolução;
- cinco filtros diferentes;
- consultas com INNER JOIN e LEFT JOIN;
- Swagger;
- workflow de build do .NET no GitHub Actions.

## Rotas CRUD

Cada entidade possui GET, GET por id, POST, PUT e DELETE.

| Entidade | Rota |
|---|---|
| Fabricante | /api/fabricantes |
| Categoria | /api/categorias |
| Filial | /api/filiais |
| Veículo | /api/veiculos |
| Cliente | /api/clientes |
| Aluguel | /api/alugueis |

A devolução de um veículo é registrada por:

POST /api/alugueis/{id}/devolucao

Ao iniciar um aluguel, o veículo passa para Alugado. Na devolução, a quilometragem é atualizada, o valor total é calculado e o veículo volta para Disponivel.

## Filtros e JOINs

Foram implementadas exatamente cinco rotas de filtros:

1. GET /api/filtros/veiculos-disponiveis  
   INNER JOIN entre Veiculos, Fabricantes, Categorias e Filiais.

2. GET /api/filtros/alugueis-por-cliente/{clienteId}  
   INNER JOIN entre Alugueis, Clientes, Veiculos e Fabricantes.

3. GET /api/filtros/alugueis-por-periodo?inicio=2026-01-01&fim=2026-12-31  
   INNER JOIN entre Alugueis, Clientes, Veiculos e Filiais.

4. GET /api/filtros/categorias-com-frota  
   LEFT JOIN entre Categorias e Veiculos, incluindo categorias sem veículos.

5. GET /api/filtros/clientes-sem-alugueis  
   LEFT JOIN entre Clientes e Alugueis.

## Entity Framework e SQL Server Express

O projeto continua utilizando ApplicationContext com Entity Framework Core 8 e o provider do SQL Server.

A connection string padrão aponta para:

Server=localhost\SQLEXPRESS;Database=LocadoraVeiculos;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True

## Como rodar

Pré-requisitos:

- .NET SDK 8;
- SQL Server Express;
- dotnet-ef.

Comandos:

    dotnet restore
    dotnet ef migrations add Inicial --project src/LocadoraVeiculos
    dotnet ef database update --project src/LocadoraVeiculos
    dotnet run --project src/LocadoraVeiculos

Caso o banco da Etapa 1 já tenha sido criado pelo script docs/modelo-fisico.sql, não é necessário recriá-lo.

Swagger:

https://localhost:7147/swagger

Verificação da aplicação e da conexão:

GET /status

## Tecnologias

- .NET 8 / C# 12
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server Express
- Swashbuckle / Swagger

## Etapas

- Etapa 1: modelagem do banco de dados - concluída
- Etapa 2: implementação do backend - concluída
