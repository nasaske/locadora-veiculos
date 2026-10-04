# Documentação da API - Etapa 3

A API é documentada e testável pelo Swagger UI em `/swagger`. O documento OpenAPI em JSON fica disponível em `/swagger/v1/swagger.json`.

## Padrão de respostas

- `200 OK`: leitura ou atualização realizada.
- `201 Created`: cadastro criado.
- `204 No Content`: exclusão realizada.
- `400 Bad Request`: dados de entrada inválidos.
- `404 Not Found`: recurso não localizado.
- `409 Conflict`: violação de regra de negócio ou integridade.
- `500 Internal Server Error`: erro inesperado tratado pelo middleware.

## Fabricantes

| Método | Rota | Finalidade |
|---|---|---|
| GET | /api/fabricantes | Lista fabricantes |
| GET | /api/fabricantes/{id} | Busca fabricante por id |
| POST | /api/fabricantes | Cria fabricante |
| PUT | /api/fabricantes/{id} | Atualiza fabricante |
| DELETE | /api/fabricantes/{id} | Exclui fabricante sem veículos vinculados |

## Categorias

| Método | Rota | Finalidade |
|---|---|---|
| GET | /api/categorias | Lista categorias |
| GET | /api/categorias/{id} | Busca categoria por id |
| POST | /api/categorias | Cria categoria |
| PUT | /api/categorias/{id} | Atualiza categoria |
| DELETE | /api/categorias/{id} | Exclui categoria sem veículos vinculados |

## Filiais

| Método | Rota | Finalidade |
|---|---|---|
| GET | /api/filiais | Lista filiais |
| GET | /api/filiais/{id} | Busca filial por id |
| POST | /api/filiais | Cria filial |
| PUT | /api/filiais/{id} | Atualiza filial |
| DELETE | /api/filiais/{id} | Exclui filial sem vínculos |

## Veículos

| Método | Rota | Finalidade |
|---|---|---|
| GET | /api/veiculos | Lista veículos com fabricante, categoria e filial |
| GET | /api/veiculos/{id} | Busca veículo por id |
| POST | /api/veiculos | Cria veículo |
| PUT | /api/veiculos/{id} | Atualiza veículo |
| DELETE | /api/veiculos/{id} | Exclui veículo sem histórico de aluguel |

Exemplo de criação:

```json
{
  "placa": "ABC1D23",
  "chassi": "9BWZZZ377VT004251",
  "modelo": "Polo",
  "anoFabricacao": 2025,
  "anoModelo": 2026,
  "cor": "Preto",
  "quilometragem": 12000,
  "combustivel": "Flex",
  "status": "Disponivel",
  "valorDiaria": 180.00,
  "dataAquisicao": "2025-01-10",
  "fabricanteId": 2,
  "categoriaId": 1,
  "filialId": 1
}
```

## Clientes

| Método | Rota | Finalidade |
|---|---|---|
| GET | /api/clientes | Lista clientes |
| GET | /api/clientes/{id} | Busca cliente por id |
| POST | /api/clientes | Cria cliente |
| PUT | /api/clientes/{id} | Atualiza cliente |
| DELETE | /api/clientes/{id} | Exclui cliente sem histórico de aluguel |

## Aluguéis

| Método | Rota | Finalidade |
|---|---|---|
| GET | /api/alugueis | Lista aluguéis |
| GET | /api/alugueis/{id} | Busca aluguel por id |
| POST | /api/alugueis | Inicia um aluguel |
| PUT | /api/alugueis/{id} | Atualiza prazo/observações |
| POST | /api/alugueis/{id}/devolucao | Registra devolução |
| DELETE | /api/alugueis/{id} | Exclui aluguel |

A criação valida cliente, veículo, filial, disponibilidade do veículo e validade da CNH. A devolução atualiza a quilometragem, calcula o valor total e devolve o veículo ao status `Disponivel`.

## Filtros

| Método | Rota | JOIN utilizado |
|---|---|---|
| GET | /api/filtros/veiculos-disponiveis | INNER JOIN |
| GET | /api/filtros/alugueis-por-cliente/{clienteId} | INNER JOIN |
| GET | /api/filtros/alugueis-por-periodo?inicio=...&fim=... | INNER JOIN |
| GET | /api/filtros/categorias-com-frota | LEFT JOIN |
| GET | /api/filtros/clientes-sem-alugueis | LEFT JOIN |

## Swagger

Após iniciar a API:

```bash
dotnet run --project src/LocadoraVeiculos
```

acesse:

```text
https://localhost:7147/swagger
```

O Swagger permite visualizar os schemas, parâmetros, descrições e executar as requisições diretamente pela interface.
