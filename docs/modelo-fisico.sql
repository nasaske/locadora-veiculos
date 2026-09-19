-- =============================================================
-- Locadora de Veiculos - Modelo fisico (SQL Server Express)
-- Equivalente ao esquema gerado pelo Entity Framework Core
-- a partir da classe ApplicationContext.
-- Comando usado: dotnet ef migrations script
-- =============================================================

CREATE DATABASE LocadoraVeiculos;
GO

USE LocadoraVeiculos;
GO

-- ------------------------- Fabricantes -------------------------
CREATE TABLE Fabricantes (
    FabricanteId INT IDENTITY(1,1) NOT NULL,
    Nome         VARCHAR(60)  NOT NULL,
    PaisOrigem   VARCHAR(50)  NULL,
    AnoFundacao  INT          NULL,
    Ativo        BIT          NOT NULL DEFAULT 1,
    CONSTRAINT PK_Fabricantes PRIMARY KEY (FabricanteId)
);
GO

CREATE UNIQUE INDEX UQ_Fabricante_Nome ON Fabricantes (Nome);
GO

-- ------------------------- Categorias --------------------------
CREATE TABLE Categorias (
    CategoriaId      INT IDENTITY(1,1) NOT NULL,
    Nome             VARCHAR(40)   NOT NULL,
    Descricao        VARCHAR(200)  NULL,
    ValorDiariaBase  DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_Categorias PRIMARY KEY (CategoriaId),
    CONSTRAINT CK_Categoria_ValorDiariaBase CHECK (ValorDiariaBase > 0)
);
GO

CREATE UNIQUE INDEX UQ_Categoria_Nome ON Categorias (Nome);
GO

-- --------------------------- Filiais ---------------------------
CREATE TABLE Filiais (
    FilialId   INT IDENTITY(1,1) NOT NULL,
    Nome       VARCHAR(80)  NOT NULL,
    Logradouro VARCHAR(120) NOT NULL,
    Cidade     VARCHAR(60)  NOT NULL,
    Uf         CHAR(2)      NOT NULL,
    Cep        VARCHAR(8)   NULL,
    Telefone   VARCHAR(15)  NULL,
    CONSTRAINT PK_Filiais PRIMARY KEY (FilialId)
);
GO

CREATE INDEX IX_Filial_Cidade_Nome ON Filiais (Cidade, Nome);
GO

-- --------------------------- Veiculos --------------------------
CREATE TABLE Veiculos (
    VeiculoId      INT IDENTITY(1,1) NOT NULL,
    Placa          VARCHAR(8)    NOT NULL,
    Chassi         VARCHAR(17)   NOT NULL,
    Modelo         VARCHAR(60)   NOT NULL,
    AnoFabricacao  INT           NOT NULL,
    AnoModelo      INT           NOT NULL,
    Cor            VARCHAR(30)   NULL,
    Quilometragem  INT           NOT NULL DEFAULT 0,
    Combustivel    INT           NOT NULL,
    Status         INT           NOT NULL DEFAULT 1,
    ValorDiaria    DECIMAL(10,2) NOT NULL,
    DataAquisicao  DATE          NOT NULL,
    FabricanteId   INT           NOT NULL,
    CategoriaId    INT           NOT NULL,
    FilialId       INT           NOT NULL,
    CONSTRAINT PK_Veiculos PRIMARY KEY (VeiculoId),
    CONSTRAINT FK_Veiculo_Fabricante FOREIGN KEY (FabricanteId)
        REFERENCES Fabricantes (FabricanteId) ON DELETE NO ACTION,
    CONSTRAINT FK_Veiculo_Categoria FOREIGN KEY (CategoriaId)
        REFERENCES Categorias (CategoriaId) ON DELETE NO ACTION,
    CONSTRAINT FK_Veiculo_Filial FOREIGN KEY (FilialId)
        REFERENCES Filiais (FilialId) ON DELETE NO ACTION,
    CONSTRAINT CK_Veiculo_AnoFabricacao CHECK (AnoFabricacao BETWEEN 1900 AND 2100),
    CONSTRAINT CK_Veiculo_Quilometragem CHECK (Quilometragem >= 0),
    CONSTRAINT CK_Veiculo_ValorDiaria    CHECK (ValorDiaria > 0)
);
GO

CREATE UNIQUE INDEX UQ_Veiculo_Placa  ON Veiculos (Placa);
CREATE UNIQUE INDEX UQ_Veiculo_Chassi ON Veiculos (Chassi);
CREATE INDEX IX_Veiculos_FabricanteId ON Veiculos (FabricanteId);
CREATE INDEX IX_Veiculos_CategoriaId  ON Veiculos (CategoriaId);
CREATE INDEX IX_Veiculos_FilialId     ON Veiculos (FilialId);
GO

-- --------------------------- Clientes --------------------------
CREATE TABLE Clientes (
    ClienteId       INT IDENTITY(1,1) NOT NULL,
    Nome            VARCHAR(120) NOT NULL,
    Cpf             CHAR(11)     NOT NULL,
    Email           VARCHAR(120) NOT NULL,
    Telefone        VARCHAR(15)  NULL,
    DataNascimento  DATE         NOT NULL,
    NumeroCnh       VARCHAR(11)  NOT NULL,
    ValidadeCnh     DATE         NOT NULL,
    Endereco        VARCHAR(120) NULL,
    Cidade          VARCHAR(60)  NULL,
    Uf              CHAR(2)      NULL,
    DataCadastro    DATETIME2    NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Clientes PRIMARY KEY (ClienteId),
    CONSTRAINT CK_Cliente_Cpf CHECK (LEN(Cpf) = 11)
);
GO

CREATE UNIQUE INDEX UQ_Cliente_Cpf   ON Clientes (Cpf);
CREATE UNIQUE INDEX UQ_Cliente_Email ON Clientes (Email);
CREATE UNIQUE INDEX UQ_Cliente_Cnh   ON Clientes (NumeroCnh);
GO

-- --------------------------- Alugueis --------------------------
CREATE TABLE Alugueis (
    AluguelId              INT IDENTITY(1,1) NOT NULL,
    DataRetirada           DATETIME2     NOT NULL,
    DataDevolucaoPrevista  DATETIME2     NOT NULL,
    DataDevolucaoEfetiva   DATETIME2     NULL,
    QuilometragemInicial   INT           NOT NULL,
    QuilometragemFinal     INT           NULL,
    ValorDiaria            DECIMAL(10,2) NOT NULL,
    ValorMulta             DECIMAL(10,2) NULL,
    ValorTotal             DECIMAL(10,2) NULL,
    Status                 INT           NOT NULL DEFAULT 1,
    Observacoes            VARCHAR(300)  NULL,
    ClienteId              INT           NOT NULL,
    VeiculoId              INT           NOT NULL,
    FilialId               INT           NOT NULL,
    CONSTRAINT PK_Alugueis PRIMARY KEY (AluguelId),
    CONSTRAINT FK_Aluguel_Cliente FOREIGN KEY (ClienteId)
        REFERENCES Clientes (ClienteId) ON DELETE NO ACTION,
    CONSTRAINT FK_Aluguel_Veiculo FOREIGN KEY (VeiculoId)
        REFERENCES Veiculos (VeiculoId) ON DELETE NO ACTION,
    CONSTRAINT FK_Aluguel_Filial FOREIGN KEY (FilialId)
        REFERENCES Filiais (FilialId) ON DELETE NO ACTION,
    CONSTRAINT CK_Aluguel_Periodo CHECK (DataDevolucaoPrevista > DataRetirada),
    CONSTRAINT CK_Aluguel_Quilometragem
        CHECK (QuilometragemFinal IS NULL OR QuilometragemFinal >= QuilometragemInicial),
    CONSTRAINT CK_Aluguel_ValorDiaria CHECK (ValorDiaria > 0)
);
GO

CREATE INDEX IX_Aluguel_Veiculo_DataRetirada ON Alugueis (VeiculoId, DataRetirada);
CREATE INDEX IX_Aluguel_Cliente              ON Alugueis (ClienteId);
CREATE INDEX IX_Alugueis_FilialId            ON Alugueis (FilialId);
GO

-- ------------------- Carga inicial (HasData) -------------------
SET IDENTITY_INSERT Fabricantes ON;
INSERT INTO Fabricantes (FabricanteId, Nome, PaisOrigem, AnoFundacao, Ativo) VALUES
 (1, 'Fiat', 'Italia', 1899, 1),
 (2, 'Volkswagen', 'Alemanha', 1937, 1),
 (3, 'Chevrolet', 'Estados Unidos', 1911, 1),
 (4, 'Toyota', 'Japao', 1937, 1);
SET IDENTITY_INSERT Fabricantes OFF;
GO

SET IDENTITY_INSERT Categorias ON;
INSERT INTO Categorias (CategoriaId, Nome, Descricao, ValorDiariaBase) VALUES
 (1, 'Hatch Compacto', 'Carros de entrada, 4 portas e ar-condicionado', 110.00),
 (2, 'Sedan', 'Porta-malas maior, indicado para viagens', 165.00),
 (3, 'SUV', 'Veiculos altos, 5 lugares', 240.00),
 (4, 'Utilitario', 'Picapes e furgoes para carga', 290.00);
SET IDENTITY_INSERT Categorias OFF;
GO

SET IDENTITY_INSERT Filiais ON;
INSERT INTO Filiais (FilialId, Nome, Logradouro, Cidade, Uf, Cep, Telefone) VALUES
 (1, 'Matriz Savassi', 'Rua Pernambuco, 1200', 'Belo Horizonte', 'MG', '30130151', '3132221010'),
 (2, 'Filial Aeroporto Confins', 'Rodovia MG-010, s/n', 'Confins', 'MG', '33500000', '3136891020'),
 (3, 'Filial Contagem', 'Av. Joao Cesar de Oliveira, 500', 'Contagem', 'MG', '32210000', '3133912030');
SET IDENTITY_INSERT Filiais OFF;
GO
