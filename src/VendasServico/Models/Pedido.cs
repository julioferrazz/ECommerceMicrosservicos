namespace VendasService.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public DateTime DataPedido { get; set; } = DateTime.Now;
        public StatusPedido Status { get; set; } = StatusPedido.Pendente;
        public decimal ValorTotal { get; set; }
        public List<ItemPedido> Itens { get; set; } = new();
    }
    public class ItemPedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public Pedido? Pedido { get; set; }
    }
    public enum StatusPedido
    {
        Pendente,
        Confirmado,
        Cancelado,
        Entregue
    }
}
