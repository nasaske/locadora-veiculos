using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FabricantesController : ControllerBase
{
    private readonly ApplicationContext _context;

    public FabricantesController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FabricanteResponseDto>>> GetAll()
    {
        var fabricantes = await _context.Fabricantes
            .AsNoTracking()
            .OrderBy(f => f.Nome)
            .ToListAsync();

        return Ok(fabricantes.Select(f => f.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FabricanteResponseDto>> GetById(int id)
    {
        var fabricante = await _context.Fabricantes.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FabricanteId == id);

        if (fabricante is null)
            return NotFound(new { erro = "Fabricante não encontrado." });

        return Ok(fabricante.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<FabricanteResponseDto>> Create(FabricanteUpsertDto dto)
    {
        var nome = dto.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome))
            return BadRequest(new { erro = "O nome do fabricante é obrigatório." });

        if (await _context.Fabricantes.AnyAsync(f => f.Nome == nome))
            return Conflict(new { erro = "Já existe um fabricante com esse nome." });

        var fabricante = new Fabricante
        {
            Nome = nome,
            PaisOrigem = dto.PaisOrigem?.Trim(),
            AnoFundacao = dto.AnoFundacao,
            Ativo = dto.Ativo
        };

        _context.Fabricantes.Add(fabricante);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = fabricante.FabricanteId }, fabricante.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FabricanteResponseDto>> Update(int id, FabricanteUpsertDto dto)
    {
        var fabricante = await _context.Fabricantes.FindAsync(id);
        if (fabricante is null)
            return NotFound(new { erro = "Fabricante não encontrado." });

        var nome = dto.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome))
            return BadRequest(new { erro = "O nome do fabricante é obrigatório." });

        if (await _context.Fabricantes.AnyAsync(f => f.FabricanteId != id && f.Nome == nome))
            return Conflict(new { erro = "Já existe outro fabricante com esse nome." });

        fabricante.Nome = nome;
        fabricante.PaisOrigem = dto.PaisOrigem?.Trim();
        fabricante.AnoFundacao = dto.AnoFundacao;
        fabricante.Ativo = dto.Ativo;

        await _context.SaveChangesAsync();
        return Ok(fabricante.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var fabricante = await _context.Fabricantes.FindAsync(id);
        if (fabricante is null)
            return NotFound(new { erro = "Fabricante não encontrado." });

        if (await _context.Veiculos.AnyAsync(v => v.FabricanteId == id))
            return Conflict(new { erro = "O fabricante não pode ser excluído porque possui veículos vinculados." });

        _context.Fabricantes.Remove(fabricante);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
