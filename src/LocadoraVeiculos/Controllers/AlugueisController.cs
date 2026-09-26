using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using LocadoraVeiculos.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlugueisController : ControllerBase
{
    private readonly ApplicationContext _context;

    public AlugueisController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AluguelResponseDto>>> GetAll()
    {
        var alugueis = await ConsultaCompleta()
            .AsNoTracking()
            .OrderByDescending(a => a.DataRetirada)
            .ToListAsync();

        return Ok(alugueis.Select(a => a.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AluguelResponseDto>> GetById(int id)
    {
        var aluguel = await ConsultaCompleta().AsNoTracking()
            .FirstOrDefaultAsync(a => a.AluguelId == id);

        if (aluguel is null)
            return NotFound(new { erro = "Aluguel não encontrado." });

        return Ok(aluguel.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<AluguelResponseDto>> Create(AluguelCreateDto dto)
    {
        if (dto.DataRetirada == default || dto.DataDevolucaoPrevista == default)
            return BadRequest(new { erro = "As datas de retirada e devolução prevista são obrigatórias." });

        if (dto.DataDevolucaoPrevista <= dto.DataRetirada)
            return BadRequest(new { erro = "A devolução prevista deve ser posterior à retirada." });

        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente is null)
            return BadRequest(new { erro = "Cliente informado não existe." });

        var veiculo = await _context.Veiculos.FindAsync(dto.VeiculoId);
        if (veiculo is null)
            return BadRequest(new { erro = "Veículo informado não existe." });

        if (!await _context.Filiais.AnyAsync(f => f.FilialId == dto.FilialId))
            return BadRequest(new { erro = "Filial informada não existe." });

        if (veiculo.FilialId != dto.FilialId)
            return BadRequest(new { erro = "O veículo não está alocado na filial de retirada informada." });

        if (veiculo.Status != StatusVeiculo.Disponivel)
            return Conflict(new { erro = "O veículo não está disponível para aluguel." });

        if (cliente.ValidadeCnh.Date < dto.DataDevolucaoPrevista.Date)
            return BadRequest(new { erro = "A CNH do cliente vence antes da devolução prevista." });

        var possuiAluguelAtivo = await _context.Alugueis.AnyAsync(a =>
            a.VeiculoId == dto.VeiculoId &&
            (a.Status == StatusAluguel.EmAndamento || a.Status == StatusAluguel.Atrasado));

        if (possuiAluguelAtivo)
            return Conflict(new { erro = "O veículo já possui um aluguel em andamento." });

        var aluguel = new Aluguel
        {
            ClienteId = dto.ClienteId,
            VeiculoId = dto.VeiculoId,
            FilialId = dto.FilialId,
            DataRetirada = dto.DataRetirada,
            DataDevolucaoPrevista = dto.DataDevolucaoPrevista,
            QuilometragemInicial = veiculo.Quilometragem,
            ValorDiaria = veiculo.ValorDiaria,
            Status = dto.DataDevolucaoPrevista < DateTime.Now
                ? StatusAluguel.Atrasado
                : StatusAluguel.EmAndamento,
            Observacoes = dto.Observacoes?.Trim()
        };

        veiculo.Status = StatusVeiculo.Alugado;
        _context.Alugueis.Add(aluguel);
        await _context.SaveChangesAsync();

        var criado = await ConsultaCompleta().AsNoTracking()
            .FirstAsync(a => a.AluguelId == aluguel.AluguelId);

        return CreatedAtAction(nameof(GetById), new { id = aluguel.AluguelId }, criado.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AluguelResponseDto>> Update(int id, AluguelUpdateDto dto)
    {
        var aluguel = await _context.Alugueis.FindAsync(id);
        if (aluguel is null)
            return NotFound(new { erro = "Aluguel não encontrado." });

        if (aluguel.Status is StatusAluguel.Finalizado or StatusAluguel.Cancelado)
            return Conflict(new { erro = "Aluguéis finalizados ou cancelados não podem ser alterados." });

        if (dto.DataDevolucaoPrevista == default || dto.DataDevolucaoPrevista <= aluguel.DataRetirada)
            return BadRequest(new { erro = "A devolução prevista deve ser posterior à retirada." });

        var cliente = await _context.Clientes.AsNoTracking()
            .FirstAsync(c => c.ClienteId == aluguel.ClienteId);

        if (cliente.ValidadeCnh.Date < dto.DataDevolucaoPrevista.Date)
            return BadRequest(new { erro = "A CNH do cliente vence antes da nova devolução prevista." });

        aluguel.DataDevolucaoPrevista = dto.DataDevolucaoPrevista;
        aluguel.Observacoes = dto.Observacoes?.Trim();
        aluguel.Status = dto.DataDevolucaoPrevista < DateTime.Now
            ? StatusAluguel.Atrasado
            : StatusAluguel.EmAndamento;

        await _context.SaveChangesAsync();

        var atualizado = await ConsultaCompleta().AsNoTracking()
            .FirstAsync(a => a.AluguelId == id);

        return Ok(atualizado.ToResponse());
    }

    [HttpPost("{id:int}/devolucao")]
    public async Task<ActionResult<AluguelResponseDto>> RegistrarDevolucao(int id, DevolucaoAluguelDto dto)
    {
        var aluguel = await _context.Alugueis
            .Include(a => a.Veiculo)
            .FirstOrDefaultAsync(a => a.AluguelId == id);

        if (aluguel is null)
            return NotFound(new { erro = "Aluguel não encontrado." });

        if (aluguel.Status is StatusAluguel.Finalizado or StatusAluguel.Cancelado)
            return Conflict(new { erro = "Este aluguel já foi encerrado." });

        var dataDevolucao = dto.DataDevolucaoEfetiva ?? DateTime.Now;
        if (dataDevolucao < aluguel.DataRetirada)
            return BadRequest(new { erro = "A data de devolução não pode ser anterior à retirada." });

        if (dto.QuilometragemFinal < aluguel.QuilometragemInicial)
            return BadRequest(new { erro = "A quilometragem final não pode ser menor que a inicial." });

        var diasCobrados = Math.Max(1, (dataDevolucao.Date - aluguel.DataRetirada.Date).Days);

        aluguel.DataDevolucaoEfetiva = dataDevolucao;
        aluguel.QuilometragemFinal = dto.QuilometragemFinal;
        aluguel.ValorMulta = dto.ValorMulta;
        aluguel.ValorTotal = (diasCobrados * aluguel.ValorDiaria) + dto.ValorMulta;
        aluguel.Status = StatusAluguel.Finalizado;

        if (aluguel.Veiculo is not null)
        {
            aluguel.Veiculo.Quilometragem = dto.QuilometragemFinal;
            aluguel.Veiculo.Status = StatusVeiculo.Disponivel;
        }

        await _context.SaveChangesAsync();

        var finalizado = await ConsultaCompleta().AsNoTracking()
            .FirstAsync(a => a.AluguelId == id);

        return Ok(finalizado.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var aluguel = await _context.Alugueis
            .Include(a => a.Veiculo)
            .FirstOrDefaultAsync(a => a.AluguelId == id);

        if (aluguel is null)
            return NotFound(new { erro = "Aluguel não encontrado." });

        if ((aluguel.Status is StatusAluguel.EmAndamento or StatusAluguel.Atrasado) &&
            aluguel.Veiculo is not null)
        {
            aluguel.Veiculo.Status = StatusVeiculo.Disponivel;
        }

        _context.Alugueis.Remove(aluguel);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<Aluguel> ConsultaCompleta() => _context.Alugueis
        .Include(a => a.Cliente)
        .Include(a => a.Veiculo)
        .Include(a => a.Filial);
}
