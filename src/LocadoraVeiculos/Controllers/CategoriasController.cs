using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ApplicationContext _context;

    public CategoriasController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaResponseDto>>> GetAll()
    {
        var categorias = await _context.Categorias.AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync();

        return Ok(categorias.Select(c => c.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaResponseDto>> GetById(int id)
    {
        var categoria = await _context.Categorias.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoriaId == id);

        if (categoria is null)
            return NotFound(new { erro = "Categoria não encontrada." });

        return Ok(categoria.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaResponseDto>> Create(CategoriaUpsertDto dto)
    {
        var nome = dto.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome))
            return BadRequest(new { erro = "O nome da categoria é obrigatório." });

        if (await _context.Categorias.AnyAsync(c => c.Nome == nome))
            return Conflict(new { erro = "Já existe uma categoria com esse nome." });

        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = dto.Descricao?.Trim(),
            ValorDiariaBase = dto.ValorDiariaBase
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = categoria.CategoriaId }, categoria.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaResponseDto>> Update(int id, CategoriaUpsertDto dto)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria is null)
            return NotFound(new { erro = "Categoria não encontrada." });

        var nome = dto.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome))
            return BadRequest(new { erro = "O nome da categoria é obrigatório." });

        if (await _context.Categorias.AnyAsync(c => c.CategoriaId != id && c.Nome == nome))
            return Conflict(new { erro = "Já existe outra categoria com esse nome." });

        categoria.Nome = nome;
        categoria.Descricao = dto.Descricao?.Trim();
        categoria.ValorDiariaBase = dto.ValorDiariaBase;

        await _context.SaveChangesAsync();
        return Ok(categoria.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria is null)
            return NotFound(new { erro = "Categoria não encontrada." });

        if (await _context.Veiculos.AnyAsync(v => v.CategoriaId == id))
            return Conflict(new { erro = "A categoria não pode ser excluída porque possui veículos vinculados." });

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
