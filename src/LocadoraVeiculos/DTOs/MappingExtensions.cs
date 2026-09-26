using LocadoraVeiculos.Models;

namespace LocadoraVeiculos.DTOs;

public static class MappingExtensions
{
    public static FabricanteResponseDto ToResponse(this Fabricante fabricante) => new()
    {
        FabricanteId = fabricante.FabricanteId,
        Nome = fabricante.Nome,
        PaisOrigem = fabricante.PaisOrigem,
        AnoFundacao = fabricante.AnoFundacao,
        Ativo = fabricante.Ativo
    };

    public static CategoriaResponseDto ToResponse(this Categoria categoria) => new()
    {
        CategoriaId = categoria.CategoriaId,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao,
        ValorDiariaBase = categoria.ValorDiariaBase
    };

    public static FilialResponseDto ToResponse(this Filial filial) => new()
    {
        FilialId = filial.FilialId,
        Nome = filial.Nome,
        Logradouro = filial.Logradouro,
        Cidade = filial.Cidade,
        Uf = filial.Uf,
        Cep = filial.Cep,
        Telefone = filial.Telefone
    };

    public static VeiculoResponseDto ToResponse(this Veiculo veiculo) => new()
    {
        VeiculoId = veiculo.VeiculoId,
        Placa = veiculo.Placa,
        Chassi = veiculo.Chassi,
        Modelo = veiculo.Modelo,
        AnoFabricacao = veiculo.AnoFabricacao,
        AnoModelo = veiculo.AnoModelo,
        Cor = veiculo.Cor,
        Quilometragem = veiculo.Quilometragem,
        Combustivel = veiculo.Combustivel,
        Status = veiculo.Status,
        ValorDiaria = veiculo.ValorDiaria,
        DataAquisicao = veiculo.DataAquisicao,
        FabricanteId = veiculo.FabricanteId,
        Fabricante = veiculo.Fabricante?.Nome ?? string.Empty,
        CategoriaId = veiculo.CategoriaId,
        Categoria = veiculo.Categoria?.Nome ?? string.Empty,
        FilialId = veiculo.FilialId,
        Filial = veiculo.Filial?.Nome ?? string.Empty
    };

    public static ClienteResponseDto ToResponse(this Cliente cliente) => new()
    {
        ClienteId = cliente.ClienteId,
        Nome = cliente.Nome,
        Cpf = cliente.Cpf,
        Email = cliente.Email,
        Telefone = cliente.Telefone,
        DataNascimento = cliente.DataNascimento,
        NumeroCnh = cliente.NumeroCnh,
        ValidadeCnh = cliente.ValidadeCnh,
        Endereco = cliente.Endereco,
        Cidade = cliente.Cidade,
        Uf = cliente.Uf,
        DataCadastro = cliente.DataCadastro
    };

    public static AluguelResponseDto ToResponse(this Aluguel aluguel) => new()
    {
        AluguelId = aluguel.AluguelId,
        DataRetirada = aluguel.DataRetirada,
        DataDevolucaoPrevista = aluguel.DataDevolucaoPrevista,
        DataDevolucaoEfetiva = aluguel.DataDevolucaoEfetiva,
        QuilometragemInicial = aluguel.QuilometragemInicial,
        QuilometragemFinal = aluguel.QuilometragemFinal,
        QuilometragemRodada = aluguel.QuilometragemRodada,
        DiasContratados = aluguel.DiasContratados,
        ValorDiaria = aluguel.ValorDiaria,
        ValorMulta = aluguel.ValorMulta,
        ValorTotal = aluguel.ValorTotal,
        Status = aluguel.Status,
        Observacoes = aluguel.Observacoes,
        ClienteId = aluguel.ClienteId,
        Cliente = aluguel.Cliente?.Nome ?? string.Empty,
        VeiculoId = aluguel.VeiculoId,
        Veiculo = aluguel.Veiculo is null
            ? string.Empty
            : $"{aluguel.Veiculo.Modelo} ({aluguel.Veiculo.Placa})",
        FilialId = aluguel.FilialId,
        Filial = aluguel.Filial?.Nome ?? string.Empty
    };
}
