using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models;

/// <summary>
/// Cliente da locadora. Nome, CPF e e-mail são obrigatórios.
/// </summary>
[Table("Clientes")]
public class Cliente
{
    [Key]
    public int ClienteId { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(11)]
    public string Cpf { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? Telefone { get; set; }

    [Column(TypeName = "date")]
    public DateTime DataNascimento { get; set; }

    [Required]
    [MaxLength(11)]
    public string NumeroCnh { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime ValidadeCnh { get; set; }

    [MaxLength(120)]
    public string? Endereco { get; set; }

    [MaxLength(60)]
    public string? Cidade { get; set; }

    [MaxLength(2)]
    public string? Uf { get; set; }

    /// <summary>Preenchido pelo banco (GETDATE) quando o registro é inserido.</summary>
    public DateTime DataCadastro { get; set; }

    public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
}
