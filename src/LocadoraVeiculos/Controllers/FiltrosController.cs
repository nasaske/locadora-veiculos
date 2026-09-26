using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers;

/// <summary>
/// Consultas da Etapa 2. As três primeiras rotas usam INNER JOIN explícito;
/// as duas últimas usam LEFT JOIN com GroupJoin/DefaultIfEmpty.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FiltrosController : ControllerBase
{
    private readonly ApplicationContext _context;

    public FiltrosController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet("veiculos-disponiveis")]
    public async Task<IActionResult> VeiculosDisponiveis(
        [FromQuery] int? categoriaId,
        [FromQuery] int? filialId,
        [FromQuery] decimal? valorMaximo)
    {
        if (valorMaximo.HasValue && valorMaximo <= 0)
            return BadRequest(new { erro = "O valor máximo deve ser maior que zero." });

        var consulta =
            from veiculo in _context.Veiculos.AsNoTracking()
            join fabricante in _context.Fabricantes.AsNoTracking()
                on veiculo.FabricanteId equals fabricante.FabricanteId
            join categoria in _context.Categorias.AsNoTracking()
                on veiculo.CategoriaId equals categoria.CategoriaId
            join filial in _context.Filiais.AsNoTracking()
                on veiculo.FilialId equals filial.FilialId
            where veiculo.Status == StatusVeiculo.Disponivel
                && (!categoriaId.HasValue || veiculo.CategoriaId == categoriaId.Value)
                && (!filialId.HasValue || veiculo.FilialId == filialId.Value)
                && (!valorMaximo.HasValue || veiculo.ValorDiaria <= valorMaximo.Value)
            orderby veiculo.ValorDiaria, fabricante.Nome, veiculo.Modelo
            select new
            {
                veiculo.VeiculoId,
                veiculo.Placa,
                veiculo.Modelo,
                Fabricante = fabricante.Nome,
                Categoria = categoria.Nome,
                Filial = filial.Nome,
                veiculo.ValorDiaria,
                veiculo.Quilometragem
            };

        return Ok(await consulta.ToListAsync());
    }

    [HttpGet("alugueis-por-cliente/{clienteId:int}")]
    public async Task<IActionResult> AlugueisPorCliente(int clienteId)
    {
        if (!await _context.Clientes.AnyAsync(c => c.ClienteId == clienteId))
            return NotFound(new { erro = "Cliente não encontrado." });

        var consulta =
            from aluguel in _context.Alugueis.AsNoTracking()
            join cliente in _context.Clientes.AsNoTracking()
                on aluguel.ClienteId equals cliente.ClienteId
            join veiculo in _context.Veiculos.AsNoTracking()
                on aluguel.VeiculoId equals veiculo.VeiculoId
            join fabricante in _context.Fabricantes.AsNoTracking()
                on veiculo.FabricanteId equals fabricante.FabricanteId
            where aluguel.ClienteId == clienteId
            orderby aluguel.DataRetirada descending
            select new
            {
                aluguel.AluguelId,
                Cliente = cliente.Nome,
                Veiculo = fabricante.Nome + " " + veiculo.Modelo,
                veiculo.Placa,
                aluguel.DataRetirada,
                aluguel.DataDevolucaoPrevista,
                aluguel.DataDevolucaoEfetiva,
                aluguel.Status,
                aluguel.ValorTotal
            };

        return Ok(await consulta.ToListAsync());
    }

    [HttpGet("alugueis-por-periodo")]
    public async Task<IActionResult> AlugueisPorPeriodo([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
    {
        if (inicio == default || fim == default || fim < inicio)
            return BadRequest(new { erro = "Informe um período válido, com fim maior ou igual ao início." });

        var consulta =
            from aluguel in _context.Alugueis.AsNoTracking()
            join cliente in _context.Clientes.AsNoTracking()
                on aluguel.ClienteId equals cliente.ClienteId
            join veiculo in _context.Veiculos.AsNoTracking()
                on aluguel.VeiculoId equals veiculo.VeiculoId
            join filial in _context.Filiais.AsNoTracking()
                on aluguel.FilialId equals filial.FilialId
            where aluguel.DataRetirada >= inicio && aluguel.DataRetirada <= fim
            orderby aluguel.DataRetirada
            select new
            {
                aluguel.AluguelId,
                Cliente = cliente.Nome,
                Veiculo = veiculo.Modelo,
                veiculo.Placa,
                Filial = filial.Nome,
                aluguel.DataRetirada,
                aluguel.DataDevolucaoPrevista,
                aluguel.Status,
                aluguel.ValorTotal
            };

        return Ok(await consulta.ToListAsync());
    }

    [HttpGet("categorias-com-frota")]
    public async Task<IActionResult> CategoriasComFrota()
    {
        var linhas = await (
            from categoria in _context.Categorias.AsNoTracking()
            join veiculo in _context.Veiculos.AsNoTracking()
                on categoria.CategoriaId equals veiculo.CategoriaId into veiculosDaCategoria
            from veiculo in veiculosDaCategoria.DefaultIfEmpty()
            orderby categoria.Nome
            select new
            {
                categoria.CategoriaId,
                categoria.Nome,
                categoria.ValorDiariaBase,
                VeiculoId = veiculo == null ? (int?)null : veiculo.VeiculoId,
                Status = veiculo == null ? (StatusVeiculo?)null : veiculo.Status
            }).ToListAsync();

        var resultado = linhas
            .GroupBy(x => new { x.CategoriaId, x.Nome, x.ValorDiariaBase })
            .Select(grupo => new
            {
                grupo.Key.CategoriaId,
                grupo.Key.Nome,
                grupo.Key.ValorDiariaBase,
                QuantidadeVeiculos = grupo.Count(x => x.VeiculoId.HasValue),
                Disponiveis = grupo.Count(x => x.Status == StatusVeiculo.Disponivel)
            })
            .OrderBy(x => x.Nome);

        return Ok(resultado);
    }

    [HttpGet("clientes-sem-alugueis")]
    public async Task<IActionResult> ClientesSemAlugueis()
    {
        var consulta =
            from cliente in _context.Clientes.AsNoTracking()
            join aluguel in _context.Alugueis.AsNoTracking()
                on cliente.ClienteId equals aluguel.ClienteId into alugueisDoCliente
            from aluguel in alugueisDoCliente.DefaultIfEmpty()
            where aluguel == null
            orderby cliente.Nome
            select new
            {
                cliente.ClienteId,
                cliente.Nome,
                cliente.Cpf,
                cliente.Email,
                cliente.NumeroCnh,
                cliente.DataCadastro
            };

        return Ok(await consulta.ToListAsync());
    }
}
