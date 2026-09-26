# Etapa 2 - Backend

## Critérios atendidos

### CRUD de todas as entidades

A API implementa operações de criar, ler, atualizar e excluir para Fabricante, Categoria, Filial, Veiculo, Cliente e Aluguel.

### Entity Framework com SQL Express

O ApplicationContext usa Entity Framework Core 8 com Microsoft.EntityFrameworkCore.SqlServer e a conexão LocadoraConnection.

### Validação e tratamento de erros

A entrada é recebida por DTOs com Data Annotations e validações de negócio. As respostas usam DTOs próprios para não expor diretamente as entidades do EF.

O middleware global trata DbUpdateException como conflito e exceções inesperadas como erro interno.

### Cinco filtros e dois tipos de JOIN

INNER JOIN:
- veiculos-disponiveis
- alugueis-por-cliente
- alugueis-por-periodo

LEFT JOIN:
- categorias-com-frota
- clientes-sem-alugueis

Essas consultas estão no FiltrosController e utilizam LINQ traduzido pelo Entity Framework para SQL.

## Regras de aluguel

A criação de um aluguel valida cliente, veículo, filial, disponibilidade e validade da CNH.

A devolução registra data efetiva, quilometragem final, multa e valor total, além de atualizar a quilometragem e o status do veículo.
