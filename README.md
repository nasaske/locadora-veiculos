# Locadora de Veículos - Trabalho Prático 1

Sistema de aluguel de veículos desenvolvido em C# com ASP.NET Core, Entity Framework Core e SQL Server Express.

**Aluno:** Davi Oliveira Parma
**Código de pessoa:** 1597232
**Curso:** Análise e Desenvolvimento de Software
**Etapa entregue:** Etapa 1 - Modelagem do Banco de Dados

---

## O que tem nesta entrega

A Etapa 1 cobre a modelagem conceitual, a implementação das classes de entidade na camada Model e a configuração do `ApplicationContext` para o mapeamento no SQL Server via Entity Framework Core.

```
LocadoraVeiculos.sln
├── src/LocadoraVeiculos/
│   ├── Models/
│   │   ├── Fabricante.cs
│   │   ├── Categoria.cs
│   │   ├── Filial.cs
│   │   ├── Veiculo.cs
│   │   ├── Cliente.cs
│   │   ├── Aluguel.cs
│   │   └── Enums/          StatusVeiculo, TipoCombustivel, StatusAluguel
│   ├── Data/
│   │   └── ApplicationContext.cs
│   ├── Program.cs
│   └── appsettings.json
└── docs/
    ├── modelo-conceitual.md    diagrama ER e justificativa das entidades
    └── modelo-fisico.sql       script equivalente ao gerado pelo EF
```

## Entidades

São 6 entidades, uma a mais que o mínimo pedido no item 1.5:

| # | Entidade | Chave primária | Chaves estrangeiras |
|---|---|---|---|
| 1 | Fabricante | `FabricanteId` | - |
| 2 | Categoria | `CategoriaId` | - |
| 3 | Filial | `FilialId` | - |
| 4 | Veiculo | `VeiculoId` | `FabricanteId`, `CategoriaId`, `FilialId` |
| 5 | Cliente | `ClienteId` | - |
| 6 | Aluguel | `AluguelId` | `ClienteId`, `VeiculoId`, `FilialId` |

### Regras do enunciado atendidas

- Todo veículo pertence a um fabricante (`FK_Veiculo_Fabricante`) e registra `Modelo`, `AnoFabricacao` e `Quilometragem`.
- Cliente tem `Nome`, `Cpf` e `Email` obrigatórios, os três com índice único.
- Aluguel amarra um cliente, um veículo e um período (`DataRetirada` / `DataDevolucaoPrevista`).
- A devolução é registrada em `DataDevolucaoEfetiva`, junto com `QuilometragemInicial`, `QuilometragemFinal`, `ValorDiaria` e `ValorTotal`.

O detalhamento do modelo conceitual, com o diagrama ER, está em [`docs/modelo-conceitual.md`](docs/modelo-conceitual.md).

## Como rodar

Pré-requisitos: .NET SDK 8.0 e SQL Server Express instalado (instância `SQLEXPRESS`).

1. Ajuste a connection string em `src/LocadoraVeiculos/appsettings.json` se a sua instância tiver outro nome.

2. Instale a ferramenta do EF, caso ainda não tenha:

```bash
dotnet tool install --global dotnet-ef
```

3. Restaure os pacotes e gere o banco:

```bash
dotnet restore
dotnet ef migrations add InicialEtapa1 --project src/LocadoraVeiculos
dotnet ef database update --project src/LocadoraVeiculos
```

4. Suba a aplicação:

```bash
dotnet run --project src/LocadoraVeiculos
```

O Swagger fica em `https://localhost:7147/swagger` e o endpoint `/status` confirma se a conexão com o banco está funcionando.

> Se preferir criar o banco direto pelo SSMS, sem migration, o script está em `docs/modelo-fisico.sql`.

## Tecnologias

- .NET 8 / C# 12
- Entity Framework Core 8 (SQL Server provider)
- SQL Server Express
- Swashbuckle (Swagger)

## Próximas etapas

- Etapa 2: criação de registros e consultas
- Etapa 3: implementação dos endpoints REST e testes no Swagger
- Etapa 4: vídeo de apresentação
