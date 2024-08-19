using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PlatformService.DTO;
using PlatformService.Settings;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices;

public class RabbitMqClient : IMessageBrokerClient
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqClient(IOptions<RabbitMqSettings> rabbitMqSettings)
    {
        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqSettings.Value.RabbitMqHost,
            Port = rabbitMqSettings.Value.RabbitMqPort
        };
        try
        {
            // Create a connection to RabbitMQ
            _connection = factory.CreateConnection();
        
            // Create a channel within this connection
            _channel = _connection.CreateModel();

            // Declare an exchange named "trigger" of type fanout
            // If it already exists with the same parameters, this is a no-op
            _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

            // Attach a shutdown event handler
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

            Console.WriteLine("--> Connected to MessageBus");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
        }
    }

    public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
    {
        var message = JsonSerializer.Serialize(platformPublishedDto);

        if (_connection.IsOpen)
        {
            Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
            SendMessage(message);
        }
        else
        {
            Console.WriteLine("--> RabbitMQ connectionis closed, not sending");
        }
    }

    private void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "trigger",
            routingKey: "",
            basicProperties: null,
            body: body);
        Console.WriteLine($"--> We have sent {message}");
    }

    public void Dispose()
    {
        Console.WriteLine("MessageBus Disposed");
        if (_channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }
    }

    private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ Connection Shutdown");
    }
}