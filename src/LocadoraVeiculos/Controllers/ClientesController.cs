using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ApplicationContext _context;

    public ClientesController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteResponseDto>>> GetAll()
    {
        var clientes = await _context.Clientes.AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync();

        return Ok(clientes.Select(c => c.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponseDto>> GetById(int id)
    {
        var cliente = await _context.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClienteId == id);

        if (cliente is null)
            return NotFound(new { erro = "Cliente não encontrado." });

        return Ok(cliente.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponseDto>> Create(ClienteUpsertDto dto)
    {
        var erro = await ValidarDto(dto, null);
        if (erro is not null)
            return erro;

        var cliente = new Cliente();
        AplicarDto(cliente, dto);

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cliente.ClienteId }, cliente.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteResponseDto>> Update(int id, ClienteUpsertDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound(new { erro = "Cliente não encontrado." });

        var erro = await ValidarDto(dto, id);
        if (erro is not null)
            return erro;

        AplicarDto(cliente, dto);
        await _context.SaveChangesAsync();

        return Ok(cliente.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound(new { erro = "Cliente não encontrado." });

        if (await _context.Alugueis.AnyAsync(a => a.ClienteId == id))
            return Conflict(new { erro = "O cliente não pode ser excluído porque possui histórico de aluguéis." });

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<ActionResult?> ValidarDto(ClienteUpsertDto dto, int? idAtual)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            return BadRequest(new { erro = "O nome do cliente é obrigatório." });

        if (dto.DataNascimento == default || dto.DataNascimento.Date >= DateTime.Today)
            return BadRequest(new { erro = "Data de nascimento inválida." });

        if (dto.ValidadeCnh == default || dto.ValidadeCnh.Date <= dto.DataNascimento.Date)
            return BadRequest(new { erro = "A validade da CNH deve ser posterior à data de nascimento." });

        var cpf = dto.Cpf.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();
        var cnh = dto.NumeroCnh.Trim();

        if (await _context.Clientes.AnyAsync(c =>
            (!idAtual.HasValue || c.ClienteId != idAtual.Value) && c.Cpf == cpf))
            return Conflict(new { erro = "Já existe um cliente com esse CPF." });

        if (await _context.Clientes.AnyAsync(c =>
            (!idAtual.HasValue || c.ClienteId != idAtual.Value) && c.Email == email))
            return Conflict(new { erro = "Já existe um cliente com esse e-mail." });

        if (await _context.Clientes.AnyAsync(c =>
            (!idAtual.HasValue || c.ClienteId != idAtual.Value) && c.NumeroCnh == cnh))
            return Conflict(new { erro = "Já existe um cliente com essa CNH." });

        return null;
    }

    private static void AplicarDto(Cliente cliente, ClienteUpsertDto dto)
    {
        cliente.Nome = dto.Nome.Trim();
        cliente.Cpf = dto.Cpf.Trim();
        cliente.Email = dto.Email.Trim().ToLowerInvariant();
        cliente.Telefone = dto.Telefone?.Trim();
        cliente.DataNascimento = dto.DataNascimento.Date;
        cliente.NumeroCnh = dto.NumeroCnh.Trim();
        cliente.ValidadeCnh = dto.ValidadeCnh.Date;
        cliente.Endereco = dto.Endereco?.Trim();
        cliente.Cidade = dto.Cidade?.Trim();
        cliente.Uf = dto.Uf?.Trim().ToUpperInvariant();
    }
}
