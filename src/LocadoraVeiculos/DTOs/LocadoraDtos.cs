using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.Models.Enums;

namespace LocadoraVeiculos.DTOs;

public class FabricanteUpsertDto
{
    [Required, MaxLength(60)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PaisOrigem { get; set; }

    [Range(1800, 2100)]
    public int? AnoFundacao { get; set; }

    public bool Ativo { get; set; } = true;
}

public class CategoriaUpsertDto
{
    [Required, MaxLength(40)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Descricao { get; set; }

    [Range(typeof(decimal), "0.01", "99999999")]
    public decimal ValorDiariaBase { get; set; }
}

public class FilialUpsertDto
{
    [Required, MaxLength(80)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Logradouro { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string Cidade { get; set; } = string.Empty;

    [Required, StringLength(2, MinimumLength = 2)]
    public string Uf { get; set; } = string.Empty;

    [RegularExpression(@"^\d{8}$", ErrorMessage = "CEP deve conter exatamente 8 dígitos.")]
    public string? Cep { get; set; }

    [MaxLength(15)]
    public string? Telefone { get; set; }
}

public class VeiculoUpsertDto
{
    [Required, MaxLength(8)]
    public string Placa { get; set; } = string.Empty;

    [Required, StringLength(17, MinimumLength = 17)]
    public string Chassi { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string Modelo { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int AnoFabricacao { get; set; }

    [Range(1900, 2100)]
    public int AnoModelo { get; set; }

    [MaxLength(30)]
    public string? Cor { get; set; }

    [Range(0, int.MaxValue)]
    public int Quilometragem { get; set; }

    public TipoCombustivel Combustivel { get; set; } = TipoCombustivel.Flex;

    public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

    [Range(typeof(decimal), "0.01", "99999999")]
    public decimal ValorDiaria { get; set; }

    public DateTime DataAquisicao { get; set; }

    [Range(1, int.MaxValue)]
    public int FabricanteId { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoriaId { get; set; }

    [Range(1, int.MaxValue)]
    public int FilialId { get; set; }
}

public class ClienteUpsertDto
{
    [Required, MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter exatamente 11 dígitos.")]
    public string Cpf { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? Telefone { get; set; }

    public DateTime DataNascimento { get; set; }

    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "CNH deve conter exatamente 11 dígitos.")]
    public string NumeroCnh { get; set; } = string.Empty;

    public DateTime ValidadeCnh { get; set; }

    [MaxLength(120)]
    public string? Endereco { get; set; }

    [MaxLength(60)]
    public string? Cidade { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string? Uf { get; set; }
}

public class AluguelCreateDto
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue)]
    public int VeiculoId { get; set; }

    [Range(1, int.MaxValue)]
    public int FilialId { get; set; }

    public DateTime DataRetirada { get; set; }

    public DateTime DataDevolucaoPrevista { get; set; }

    [MaxLength(300)]
    public string? Observacoes { get; set; }
}

public class AluguelUpdateDto
{
    public DateTime DataDevolucaoPrevista { get; set; }

    [MaxLength(300)]
    public string? Observacoes { get; set; }
}

public class DevolucaoAluguelDto
{
    public DateTime? DataDevolucaoEfetiva { get; set; }

    [Range(0, int.MaxValue)]
    public int QuilometragemFinal { get; set; }

    [Range(typeof(decimal), "0", "99999999")]
    public decimal ValorMulta { get; set; }
}

public class FabricanteResponseDto
{
    public int FabricanteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? PaisOrigem { get; set; }
    public int? AnoFundacao { get; set; }
    public bool Ativo { get; set; }
}

public class CategoriaResponseDto
{
    public int CategoriaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal ValorDiariaBase { get; set; }
}

public class FilialResponseDto
{
    public int FilialId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string? Cep { get; set; }
    public string? Telefone { get; set; }
}

public class VeiculoResponseDto
{
    public int VeiculoId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Chassi { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int AnoFabricacao { get; set; }
    public int AnoModelo { get; set; }
    public string? Cor { get; set; }
    public int Quilometragem { get; set; }
    public TipoCombustivel Combustivel { get; set; }
    public StatusVeiculo Status { get; set; }
    public decimal ValorDiaria { get; set; }
    public DateTime DataAquisicao { get; set; }
    public int FabricanteId { get; set; }
    public string Fabricante { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int FilialId { get; set; }
    public string Filial { get; set; } = string.Empty;
}

public class ClienteResponseDto
{
    public int ClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateTime DataNascimento { get; set; }
    public string NumeroCnh { get; set; } = string.Empty;
    public DateTime ValidadeCnh { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class AluguelResponseDto
{
    public int AluguelId { get; set; }
    public DateTime DataRetirada { get; set; }
    public DateTime DataDevolucaoPrevista { get; set; }
    public DateTime? DataDevolucaoEfetiva { get; set; }
    public int QuilometragemInicial { get; set; }
    public int? QuilometragemFinal { get; set; }
    public int? QuilometragemRodada { get; set; }
    public int DiasContratados { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal? ValorMulta { get; set; }
    public decimal? ValorTotal { get; set; }
    public StatusAluguel Status { get; set; }
    public string? Observacoes { get; set; }
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public int VeiculoId { get; set; }
    public string Veiculo { get; set; } = string.Empty;
    public int FilialId { get; set; }
    public string Filial { get; set; } = string.Empty;
}
