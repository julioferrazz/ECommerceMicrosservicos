using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendasService.Data;
using VendasService.DTOs;
using VendasService.Models;
using VendasService.Services;
namespace VendasService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly VendasContext _context;
        private readonly EstoqueHttpService _estoqueService;
        private readonly RabbitMQPublisherService _rabbitMQService;
        private readonly ILogger<PedidosController> _logger;
        public PedidosController(
        VendasContext context,
        EstoqueHttpService estoqueService,
        RabbitMQPublisherService rabbitMQService,
        ILogger<PedidosController> logger)
        {
            _context = context;
            _estoqueService = estoqueService;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }
        [HttpPost]
        public async Task<ActionResult<PedidoResponseDto>> CriarPedido(CriarPedidoDto pedidoDto)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var pedido = new Pedido
            {
                ClienteNome = pedidoDto.ClienteNome,
                ClienteEmail = pedidoDto.ClienteEmail,
                DataPedido = DateTime.Now,
                Status = StatusPedido.Pendente
            };
            decimal valorTotal = 0;
            foreach (var itemDto in pedidoDto.Itens)
            {
                var produto = await _estoqueService.ObterProduto(itemDto.ProdutoId, token);

                if (produto == null)
                {
                    return BadRequest(new { message = $"Produto {itemDto.ProdutoId} não encontrado" });
                }
                var disponivel = await _estoqueService.VerificarDisponibilidade(
                itemDto.ProdutoId,
                itemDto.Quantidade,
                token);
                if (!disponivel)
                {
                    return BadRequest(new
                    {
                        message = $"Quantidade insuficiente para o produto {produto.Nome}"
                    });
                }
                var subtotal = produto.Preco * itemDto.Quantidade;
                valorTotal += subtotal;
                var itemPedido = new ItemPedido
                {
                    ProdutoId = produto.Id,
                    ProdutoNome = produto.Nome,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produto.Preco,
                    Subtotal = subtotal
                };
                pedido.Itens.Add(itemPedido);
            }
            pedido.ValorTotal = valorTotal;
            pedido.Status = StatusPedido.Confirmado;
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            var mensagemEstoque = new AtualizacaoEstoqueMessage
            {
                PedidoId = pedido.Id,
                Itens = pedido.Itens.Select(i => new ItemEstoque
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade
                }).ToList()
            };
            _rabbitMQService.PublicarAtualizacaoEstoque(mensagemEstoque);


            _logger.LogInformation($"Pedido {pedido.Id} criado com sucesso");


            var response = MapearPedidoParaDto(pedido);
            return CreatedAtAction(nameof(ObterPedido), new { id = pedido.Id }, response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoResponseDto>> ObterPedido(int id)
        {
            var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null)
            {
                return NotFound(new { message = "Pedido não encontrado" });
            }
            return Ok(MapearPedidoParaDto(pedido));
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoResponseDto>>> ListarPedidos()
        {
            var pedidos = await _context.Pedidos
            .Include(p => p.Itens)
            .OrderByDescending(p => p.DataPedido)
            .ToListAsync();
            var response = pedidos.Select(MapearPedidoParaDto);
            return Ok(response);
        }
        private PedidoResponseDto MapearPedidoParaDto(Pedido pedido)
        {
            return new PedidoResponseDto
            {
                Id = pedido.Id,
                ClienteNome = pedido.ClienteNome,
                ClienteEmail = pedido.ClienteEmail,
                DataPedido = pedido.DataPedido,
                Status = pedido.Status.ToString(),
                ValorTotal = pedido.ValorTotal,
                Itens = pedido.Itens.Select(i => new ItemPedidoResponseDto
                {
                    ProdutoId = i.ProdutoId,
                    ProdutoNome = i.ProdutoNome,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    Subtotal = i.Subtotal
                }).ToList()
            };
        }
    }
}