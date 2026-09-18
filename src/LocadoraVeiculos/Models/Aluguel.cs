using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocadoraVeiculos.Models.Enums;

namespace LocadoraVeiculos.Models;

/// <summary>
/// Locação de um veículo por um cliente dentro de um período.
/// Guarda também os dados da devolução (data efetiva, km final e valor total).
/// </summary>
[Table("Alugueis")]
public class Aluguel
{
    [Key]
    public int AluguelId { get; set; }

    // Período contratado
    public DateTime DataRetirada { get; set; }

    public DateTime DataDevolucaoPrevista { get; set; }

    /// <summary>Preenchida somente quando o veículo é devolvido.</summary>
    public DateTime? DataDevolucaoEfetiva { get; set; }

    // Odômetro
    public int QuilometragemInicial { get; set; }

    public int? QuilometragemFinal { get; set; }

    // Valores
    [Column(TypeName = "decimal(10,2)")]
    public decimal ValorDiaria { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? ValorMulta { get; set; }

    /// <summary>Valor fechado da locação, calculado na devolução.</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? ValorTotal { get; set; }

    public StatusAluguel Status { get; set; } = StatusAluguel.EmAndamento;

    [MaxLength(300)]
    public string? Observacoes { get; set; }

    // Chaves estrangeiras
    public int ClienteId { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public Cliente? Cliente { get; set; }

    public int VeiculoId { get; set; }

    [ForeignKey(nameof(VeiculoId))]
    public Veiculo? Veiculo { get; set; }

    /// <summary>Filial onde o veículo foi retirado.</summary>
    public int FilialId { get; set; }

    [ForeignKey(nameof(FilialId))]
    public Filial? Filial { get; set; }

    [NotMapped]
    public int DiasContratados =>
        Math.Max(1, (DataDevolucaoPrevista.Date - DataRetirada.Date).Days);

    [NotMapped]
    public int? QuilometragemRodada =>
        QuilometragemFinal.HasValue ? QuilometragemFinal - QuilometragemInicial : null;
}
