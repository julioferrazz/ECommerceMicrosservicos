using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueService.Data;
using EstoqueService.DTOs;
using EstoqueService.Models;
namespace EstoqueService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly EstoqueContext _context;
        private readonly ILogger<ProdutosController> _logger;
        public ProdutosController(
        EstoqueContext context,
        ILogger<ProdutosController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpPost]
        public async Task<ActionResult<ProdutoResponseDto>> CadastrarProduto(
        ProdutoCreateDto produtoDto)
        {
            var produto = new Produto
            {
                Nome = produtoDto.Nome,
                Descricao = produtoDto.Descricao,
                Preco = produtoDto.Preco,
                QuantidadeEstoque = produtoDto.QuantidadeEstoque,
                DataCadastro = DateTime.Now
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Produto cadastrado: {produto.Nome}");

            var response = new ProdutoResponseDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                Preco = produto.Preco,
                QuantidadeEstoque = produto.QuantidadeEstoque,
                DataCadastro = produto.DataCadastro
            };
            return CreatedAtAction(nameof(ObterProduto), new { id = produto.Id }, response);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoResponseDto>>> ListarProdutos()
        {
            var produtos = await _context.Produtos
            .Select(p => new ProdutoResponseDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                Preco = p.Preco,
                QuantidadeEstoque = p.QuantidadeEstoque,
                DataCadastro = p.DataCadastro
            }).ToListAsync();

            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoResponseDto>> ObterProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound(new { message = "Produto não encontrado" });
            }
            var response = new ProdutoResponseDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                Preco = produto.Preco,
                QuantidadeEstoque = produto.QuantidadeEstoque,
                DataCadastro = produto.DataCadastro
            };

            return Ok(response);
        }
        [HttpGet("verificar-disponibilidade")]
        public async Task<ActionResult<bool>> VerificarDisponibilidade(
       [FromQuery] int produtoId,
       [FromQuery] int quantidade)
        {
            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null)
            {
                return NotFound(new { message = "Produto não encontrado" });
            }
            var disponivel = produto.QuantidadeEstoque >= quantidade;
            return Ok(new
            {
                disponivel,
                produtoId,
                quantidadeSolicitada = quantidade,
                quantidadeDisponivel = produto.QuantidadeEstoque
            });
        }
    }
}

