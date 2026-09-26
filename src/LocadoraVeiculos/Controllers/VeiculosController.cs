using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using LocadoraVeiculos.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeiculosController : ControllerBase
{
    private readonly ApplicationContext _context;

    public VeiculosController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VeiculoResponseDto>>> GetAll()
    {
        var veiculos = await ConsultaCompleta()
            .AsNoTracking()
            .OrderBy(v => v.Modelo)
            .ThenBy(v => v.Placa)
            .ToListAsync();

        return Ok(veiculos.Select(v => v.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VeiculoResponseDto>> GetById(int id)
    {
        var veiculo = await ConsultaCompleta().AsNoTracking()
            .FirstOrDefaultAsync(v => v.VeiculoId == id);

        if (veiculo is null)
            return NotFound(new { erro = "Veículo não encontrado." });

        return Ok(veiculo.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<VeiculoResponseDto>> Create(VeiculoUpsertDto dto)
    {
        var erro = await ValidarDto(dto, null);
        if (erro is not null)
            return erro;

        var veiculo = new Veiculo();
        AplicarDto(veiculo, dto);

        _context.Veiculos.Add(veiculo);
        await _context.SaveChangesAsync();

        var criado = await ConsultaCompleta().AsNoTracking()
            .FirstAsync(v => v.VeiculoId == veiculo.VeiculoId);

        return CreatedAtAction(nameof(GetById), new { id = veiculo.VeiculoId }, criado.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VeiculoResponseDto>> Update(int id, VeiculoUpsertDto dto)
    {
        var veiculo = await _context.Veiculos.FindAsync(id);
        if (veiculo is null)
            return NotFound(new { erro = "Veículo não encontrado." });

        var erro = await ValidarDto(dto, id);
        if (erro is not null)
            return erro;

        var aluguelAtivo = await _context.Alugueis.AnyAsync(a =>
            a.VeiculoId == id &&
            (a.Status == StatusAluguel.EmAndamento || a.Status == StatusAluguel.Atrasado));

        if (aluguelAtivo && dto.Status != StatusVeiculo.Alugado)
            return Conflict(new { erro = "Um veículo com aluguel em andamento deve permanecer com status Alugado." });

        if (!aluguelAtivo && dto.Status == StatusVeiculo.Alugado)
            return BadRequest(new { erro = "O status Alugado é controlado pelo cadastro de aluguéis." });

        AplicarDto(veiculo, dto);
        await _context.SaveChangesAsync();

        var atualizado = await ConsultaCompleta().AsNoTracking()
            .FirstAsync(v => v.VeiculoId == id);

        return Ok(atualizado.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var veiculo = await _context.Veiculos.FindAsync(id);
        if (veiculo is null)
            return NotFound(new { erro = "Veículo não encontrado." });

        if (await _context.Alugueis.AnyAsync(a => a.VeiculoId == id))
            return Conflict(new { erro = "O veículo não pode ser excluído porque possui histórico de aluguéis." });

        _context.Veiculos.Remove(veiculo);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<Veiculo> ConsultaCompleta() => _context.Veiculos
        .Include(v => v.Fabricante)
        .Include(v => v.Categoria)
        .Include(v => v.Filial);

    private async Task<ActionResult?> ValidarDto(VeiculoUpsertDto dto, int? idAtual)
    {
        if (string.IsNullOrWhiteSpace(dto.Placa) ||
            string.IsNullOrWhiteSpace(dto.Chassi) ||
            string.IsNullOrWhiteSpace(dto.Modelo))
        {
            return BadRequest(new { erro = "Placa, chassi e modelo são obrigatórios." });
        }

        if (!Enum.IsDefined(typeof(TipoCombustivel), dto.Combustivel))
            return BadRequest(new { erro = "Tipo de combustível inválido." });

        if (!Enum.IsDefined(typeof(StatusVeiculo), dto.Status))
            return BadRequest(new { erro = "Status do veículo inválido." });

        if (idAtual is null && dto.Status == StatusVeiculo.Alugado)
            return BadRequest(new { erro = "Um veículo novo não pode ser criado diretamente com status Alugado." });

        if (dto.DataAquisicao == default || dto.DataAquisicao.Date > DateTime.Today)
            return BadRequest(new { erro = "A data de aquisição deve ser válida e não pode estar no futuro." });

        var placa = dto.Placa.Trim().ToUpperInvariant();
        var chassi = dto.Chassi.Trim().ToUpperInvariant();

        if (await _context.Veiculos.AnyAsync(v =>
            (!idAtual.HasValue || v.VeiculoId != idAtual.Value) && v.Placa == placa))
            return Conflict(new { erro = "Já existe um veículo com essa placa." });

        if (await _context.Veiculos.AnyAsync(v =>
            (!idAtual.HasValue || v.VeiculoId != idAtual.Value) && v.Chassi == chassi))
            return Conflict(new { erro = "Já existe um veículo com esse chassi." });

        if (!await _context.Fabricantes.AnyAsync(f => f.FabricanteId == dto.FabricanteId))
            return BadRequest(new { erro = "Fabricante informado não existe." });

        if (!await _context.Categorias.AnyAsync(c => c.CategoriaId == dto.CategoriaId))
            return BadRequest(new { erro = "Categoria informada não existe." });

        if (!await _context.Filiais.AnyAsync(f => f.FilialId == dto.FilialId))
            return BadRequest(new { erro = "Filial informada não existe." });

        return null;
    }

    private static void AplicarDto(Veiculo veiculo, VeiculoUpsertDto dto)
    {
        veiculo.Placa = dto.Placa.Trim().ToUpperInvariant();
        veiculo.Chassi = dto.Chassi.Trim().ToUpperInvariant();
        veiculo.Modelo = dto.Modelo.Trim();
        veiculo.AnoFabricacao = dto.AnoFabricacao;
        veiculo.AnoModelo = dto.AnoModelo;
        veiculo.Cor = dto.Cor?.Trim();
        veiculo.Quilometragem = dto.Quilometragem;
        veiculo.Combustivel = dto.Combustivel;
        veiculo.Status = dto.Status;
        veiculo.ValorDiaria = dto.ValorDiaria;
        veiculo.DataAquisicao = dto.DataAquisicao.Date;
        veiculo.FabricanteId = dto.FabricanteId;
        veiculo.CategoriaId = dto.CategoriaId;
        veiculo.FilialId = dto.FilialId;
    }
}
