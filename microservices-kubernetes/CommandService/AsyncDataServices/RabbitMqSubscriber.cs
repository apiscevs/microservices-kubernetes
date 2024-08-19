using System.Text;
using CommandService.EventProcessing;
using CommandService.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandService.AsyncDataServices;

public class MessageBusSubscriber : BackgroundService
{
    private readonly IEventProcessor _eventProcessor;
    private IConnection _connection;
    private IModel _channel;
    private string _queueName;
    private readonly RabbitMqSettings _rabbitMqSettings;
    
    public MessageBusSubscriber(
        IOptions<RabbitMqSettings> rabbitMqSettings,
        IEventProcessor eventProcessor)
    {
        _eventProcessor = eventProcessor;
        _rabbitMqSettings = rabbitMqSettings.Value;
        
        InitializeRabbitMq();
    }

    private void InitializeRabbitMq()
    {
        // TODO: add a better workaround, retry... 
        Thread.Sleep(TimeSpan.FromSeconds(5));
        
        var factory = new ConnectionFactory()
        {
            HostName = _rabbitMqSettings.RabbitMqHost,
            Port = _rabbitMqSettings.RabbitMqPort
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
        _queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(queue: _queueName,
            exchange: "trigger",
            routingKey: "");

        Console.WriteLine("--> Listenting on the Message Bus...");

        _connection.ConnectionShutdown += RabbitMQ_ConnectionShitdown;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (ModuleHandle, ea) =>
        {
            Console.WriteLine("--> Event Received!");

            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            _eventProcessor.ProcessEvent(notificationMessage);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    private void RabbitMQ_ConnectionShitdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> Connection Shutdown");
    }

    public override void Dispose()
    {
        if (_channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }

        base.Dispose();
    }
}