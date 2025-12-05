namespace VendasService.Services
{
    public class EstoqueHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EstoqueHttpService> _logger;
        public EstoqueHttpService(
        HttpClient httpClient,
        ILogger<EstoqueHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<ProdutoEstoqueDto?> ObterProduto(int produtoId, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync($"http://localhost:5001/api/produtos/{produtoId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProdutoEstoqueDto>();
                }
                _logger.LogWarning($"Produto {produtoId} não encontrado no estoque");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao consultar produto {produtoId}: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> VerificarDisponibilidade(
         int produtoId,
         int quantidade,
         string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync(
                $"http://localhost:5001/api/produtos/verificar-disponibilidade?produtoId={produtoId}&quantidade={quantidade}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<DisponibilidadeResponse>();
                    return result?.Disponivel ?? false;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao verificar disponibilidade: {ex.Message}");
                return false;
            }
        }
    }
    public class ProdutoEstoqueDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
    }
    public class DisponibilidadeResponse
    {
        public bool Disponivel { get; set; }
    }
}
