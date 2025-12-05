using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
namespace VendasService.Services
{
    public class RabbitMQPublisherService
    {
        private readonly ILogger<RabbitMQPublisherService> _logger;
        public RabbitMQPublisherService(ILogger<RabbitMQPublisherService> logger)
        {
            _logger = logger;
        }
        public void PublicarAtualizacaoEstoque(AtualizacaoEstoqueMessage mensagem)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(
            queue: "estoque_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            var json = JsonSerializer.Serialize(mensagem);
            var body = Encoding.UTF8.GetBytes(json);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish(
            exchange: "",
            routingKey: "estoque_queue",
            basicProperties: properties,
            body: body); _logger.LogInformation($"Mensagem publicada no RabbitMQ: PedidoId {mensagem.PedidoId}");
        }
    }
}