using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models;

/// <summary>
/// Marca do veículo (Fiat, Volkswagen, Toyota...).
/// </summary>
[Table("Fabricantes")]
public class Fabricante
{
    [Key]
    public int FabricanteId { get; set; }

    [Required]
    [MaxLength(60)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PaisOrigem { get; set; }

    public int? AnoFundacao { get; set; }

    public bool Ativo { get; set; } = true;

    // Um fabricante possui vários veículos no pátio da locadora
    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
}
