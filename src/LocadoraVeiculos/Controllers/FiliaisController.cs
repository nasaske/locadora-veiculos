using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FiliaisController : ControllerBase
{
    private readonly ApplicationContext _context;

    public FiliaisController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FilialResponseDto>>> GetAll()
    {
        var filiais = await _context.Filiais.AsNoTracking()
            .OrderBy(f => f.Cidade)
            .ThenBy(f => f.Nome)
            .ToListAsync();

        return Ok(filiais.Select(f => f.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FilialResponseDto>> GetById(int id)
    {
        var filial = await _context.Filiais.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FilialId == id);

        if (filial is null)
            return NotFound(new { erro = "Filial não encontrada." });

        return Ok(filial.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<FilialResponseDto>> Create(FilialUpsertDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome) ||
            string.IsNullOrWhiteSpace(dto.Logradouro) ||
            string.IsNullOrWhiteSpace(dto.Cidade) ||
            string.IsNullOrWhiteSpace(dto.Uf))
        {
            return BadRequest(new { erro = "Nome, logradouro, cidade e UF são obrigatórios." });
        }

        var filial = new Filial
        {
            Nome = dto.Nome.Trim(),
            Logradouro = dto.Logradouro.Trim(),
            Cidade = dto.Cidade.Trim(),
            Uf = dto.Uf.Trim().ToUpperInvariant(),
            Cep = dto.Cep?.Trim(),
            Telefone = dto.Telefone?.Trim()
        };

        _context.Filiais.Add(filial);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = filial.FilialId }, filial.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FilialResponseDto>> Update(int id, FilialUpsertDto dto)
    {
        var filial = await _context.Filiais.FindAsync(id);
        if (filial is null)
            return NotFound(new { erro = "Filial não encontrada." });

        if (string.IsNullOrWhiteSpace(dto.Nome) ||
            string.IsNullOrWhiteSpace(dto.Logradouro) ||
            string.IsNullOrWhiteSpace(dto.Cidade) ||
            string.IsNullOrWhiteSpace(dto.Uf))
        {
            return BadRequest(new { erro = "Nome, logradouro, cidade e UF são obrigatórios." });
        }

        filial.Nome = dto.Nome.Trim();
        filial.Logradouro = dto.Logradouro.Trim();
        filial.Cidade = dto.Cidade.Trim();
        filial.Uf = dto.Uf.Trim().ToUpperInvariant();
        filial.Cep = dto.Cep?.Trim();
        filial.Telefone = dto.Telefone?.Trim();

        await _context.SaveChangesAsync();
        return Ok(filial.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var filial = await _context.Filiais.FindAsync(id);
        if (filial is null)
            return NotFound(new { erro = "Filial não encontrada." });

        var possuiVeiculos = await _context.Veiculos.AnyAsync(v => v.FilialId == id);
        var possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.FilialId == id);
        if (possuiVeiculos || possuiAlugueis)
            return Conflict(new { erro = "A filial não pode ser excluída porque possui veículos ou aluguéis vinculados." });

        _context.Filiais.Remove(filial);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
