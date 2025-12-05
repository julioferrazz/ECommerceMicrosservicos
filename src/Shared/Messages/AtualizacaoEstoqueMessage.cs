public class AtualizacaoEstoqueMessage
{
    public int PedidoId { get; set; }
    public List<ItemEstoque> Itens { get; set; } = new();
}
public class ItemEstoque
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
