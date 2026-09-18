using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models;

/// <summary>
/// Unidade da locadora onde o veículo fica alocado e onde o aluguel é retirado.
/// </summary>
[Table("Filiais")]
public class Filial
{
    [Key]
    public int FilialId { get; set; }

    [Required]
    [MaxLength(80)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Logradouro { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    [MaxLength(2)]
    public string Uf { get; set; } = string.Empty;

    [MaxLength(8)]
    public string? Cep { get; set; }

    [MaxLength(15)]
    public string? Telefone { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();

    public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
}
