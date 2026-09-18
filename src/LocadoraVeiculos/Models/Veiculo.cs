using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocadoraVeiculos.Models.Enums;

namespace LocadoraVeiculos.Models;

/// <summary>
/// Veículo da frota. Todo veículo pertence a um fabricante e guarda
/// modelo, ano de fabricação e quilometragem atual.
/// </summary>
[Table("Veiculos")]
public class Veiculo
{
    [Key]
    public int VeiculoId { get; set; }

    [Required]
    [MaxLength(8)]
    public string Placa { get; set; } = string.Empty;

    [Required]
    [MaxLength(17)]
    public string Chassi { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Modelo { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int AnoFabricacao { get; set; }

    public int AnoModelo { get; set; }

    [MaxLength(30)]
    public string? Cor { get; set; }

    /// <summary>Quilometragem atual do odômetro.</summary>
    public int Quilometragem { get; set; }

    public TipoCombustivel Combustivel { get; set; } = TipoCombustivel.Flex;

    public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

    [Column(TypeName = "decimal(10,2)")]
    public decimal ValorDiaria { get; set; }

    public DateTime DataAquisicao { get; set; }

    // Chaves estrangeiras
    public int FabricanteId { get; set; }

    [ForeignKey(nameof(FabricanteId))]
    public Fabricante? Fabricante { get; set; }

    public int CategoriaId { get; set; }

    [ForeignKey(nameof(CategoriaId))]
    public Categoria? Categoria { get; set; }

    public int FilialId { get; set; }

    [ForeignKey(nameof(FilialId))]
    public Filial? Filial { get; set; }

    public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
}
