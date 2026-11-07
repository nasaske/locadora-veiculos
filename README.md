# Locadora de Veículos - Trabalho Prático

Sistema de aluguel de veículos desenvolvido em C# com ASP.NET Core, Entity Framework Core e SQL Server Express.

**Aluno:** Davi Oliveira Parma  
**Código de pessoa:** 1597232  
**Curso:** Análise e Desenvolvimento de Software  
**Etapa atual:** Etapa 3 - Swagger, documentação e testes

## Atividade: Linkando API com Node.js e Bootstrap

Front-end web com duas páginas (home e detalhes) que consome a API de veículos desta solução
**somente com `fetch`**. As páginas usam Bootstrap 5 (CSS + JavaScript) com estilos próprios
e são servidas por um servidor Node.js sem dependências (`frontend/server.js`).

### Versão online

- **Front-end (Vercel):** https://locadora-veiculos-two.vercel.app
- **API (Render, Swagger):** https://locadora-veiculos-api-ni7l.onrender.com/swagger

A API online roda no modo demonstração (banco em memória com a frota de exemplo). No plano gratuito do
Render ela hiberna depois de 15 minutos sem acesso: o primeiro carregamento pode levar cerca de 1 minuto e
os cadastros voltam à frota de exemplo. A Vercel publica a pasta `frontend/public` (ver `vercel.json`) e o
Render usa `src/LocadoraVeiculos/Dockerfile`, ambos a partir da `main`.

### Critérios da rubrica

| Critério | Onde está |
| --- | --- |
| **Página principal integrada via fetch** | `frontend/public/index.html` + `js/home.js`: carrossel "Destaque" e os **últimos 12 veículos** com imagem e preço (`GET /api/veiculos/recentes`), botão **Ver detalhes** em cada card e modal **Novo veículo** (`POST /api/veiculos`) que atualiza a lista sem recarregar a página. |
| **Página de detalhes integrada via fetch** | `frontend/public/detalhes.html` + `js/detalhes.js`: todas as informações salvas do veículo (`GET /api/veiculos/{id}`). |
| **Modal de edição** | Botão **Editar** abre o modal com o formulário preenchido (`PUT /api/veiculos/{id}`); erros da API aparecem dentro do modal. |
| **Botão de exclusão** | Botão **Excluir** abre um modal de confirmação; só após **Sim, excluir** é feito o `DELETE /api/veiculos/{id}`. |

As chamadas `fetch` ficam em `frontend/public/js/api.js`. O formulário de veículo é o mesmo
nos dois modais (`js/formulario-veiculo.js`) e os selects de fabricante, categoria e filial
também são carregados da API.

### Mudanças na API para o front-end

- Campo `ImagemUrl` no veículo (URL http/https, validada na API).
- Rota `GET /api/veiculos/recentes?quantidade=12`: últimos veículos cadastrados, do mais novo para o mais antigo.
- CORS liberado para o front (`http://localhost:3000`), configurável em `Cors:OrigensPermitidas`.
- Frota de exemplo com fotos (Wikimedia Commons) cadastrada automaticamente em desenvolvimento quando a tabela está vazia.
- Perfil `demo`, que roda a API com banco em memória (não precisa de SQL Server).
- Correção: os limites `[Range]` de valores decimais agora são lidos em cultura invariante (antes,
  em máquinas pt-BR, `"0.01"` gerava erro 500 ao cadastrar veículos e categorias).

### Como rodar

Pré-requisitos: .NET SDK 8 e Node.js 18 ou superior.

1. **API** (escolha uma opção):

   - Com SQL Server Express (padrão do projeto):

     ```bash
     dotnet ef migrations add Inicial --project src/LocadoraVeiculos
     dotnet ef database update --project src/LocadoraVeiculos
     dotnet run --project src/LocadoraVeiculos
     ```

     Se o banco já existia das etapas anteriores, crie uma migration para a nova coluna:
     `dotnet ef migrations add ImagemVeiculo --project src/LocadoraVeiculos` e depois `dotnet ef database update --project src/LocadoraVeiculos`.

   - Sem SQL Server (banco em memória, já com a frota de exemplo):

     ```bash
     dotnet run --project src/LocadoraVeiculos --launch-profile demo
     ```

   A API fica em `http://localhost:5147` (Swagger em `/swagger`).

2. **Front-end**, em outro terminal:

   ```bash
   cd frontend
   npm start
   ```

   Abra `http://localhost:3000`. Para outra porta ou outro endereço de API:
   `PORT=4000 API_URL=http://localhost:5147 npm start`.

### Estrutura do front-end

```text
frontend/
├── server.js                 servidor Node.js (somente módulos nativos)
├── package.json
└── public/
    ├── index.html            home: destaque + últimos 12 + modal "Novo veículo"
    ├── detalhes.html         detalhes + modal de edição + confirmação de exclusão
    ├── css/estilos.css       estilos próprios
    ├── img/                  favicon e imagem padrão
    └── js/
        ├── config.js         endereço da API
        ├── api.js            chamadas fetch (GET, POST, PUT, DELETE)
        ├── formulario-veiculo.js
        ├── ui.js             formatação, avisos e utilitários
        ├── home.js
        └── detalhes.js
```

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
- Node.js (servidor do front-end)
- Bootstrap 5.3 + Bootstrap Icons
- JavaScript (ES Modules) com fetch
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
- Atividade "Linkando API com Node.js e Bootstrap": front-end home + detalhes - concluída
