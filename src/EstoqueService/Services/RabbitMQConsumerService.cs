using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using EstoqueService.Data;
using Microsoft.EntityFrameworkCore;
namespace EstoqueService.Services
{
    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        public RabbitMQConsumerService(
        IServiceProvider serviceProvider,
        ILogger<RabbitMQConsumerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            InitializeRabbitMQ();
        }
        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
            queue: "estoque_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            _logger.LogInformation("RabbitMQ Consumer inicializado");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Mensagem recebida: {message}");
                try
                {
                    var atualizacao = JsonSerializer.Deserialize<AtualizacaoEstoqueMessage>(message);

                    if (atualizacao != null)
                    {
                        await AtualizarEstoque(atualizacao);
                        _channel?.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao processar mensagem: {ex.Message}");
                    _channel?.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            _channel?.BasicConsume(
            queue: "estoque_queue",
            autoAck: false,
            consumer: consumer);
            return Task.CompletedTask;
        }

        private async Task AtualizarEstoque(AtualizacaoEstoqueMessage atualizacao)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            foreach (var item in atualizacao.Itens)
            {
                var produto = await context.Produtos.FindAsync(item.ProdutoId);

                if (produto != null)
                {
                    produto.QuantidadeEstoque -= item.Quantidade;
                    produto.DataAtualizacao = DateTime.Now;

                    _logger.LogInformation(
                    $"Estoque atualizado: Produto {produto.Nome}, " +
                    $"Nova quantidade: {produto.QuantidadeEstoque}");
                }
            }
            await context.SaveChangesAsync();
        }
        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}