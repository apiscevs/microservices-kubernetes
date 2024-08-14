using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.DTO;
using CommandService.Models;

namespace CommandService.EventProcessing;

public class RabbitMqProcessor : IEventProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMapper _mapper;

    public RabbitMqProcessor(IServiceScopeFactory serviceScopeFactory,
        IMapper mapper)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _mapper = mapper;
    }
    
    public void ProcessEvent(string message)
    {
        var eventType = DetermineEventType(message);

        switch (eventType)
        {
            case EventType.PlatformPublished:
                AddPlatformAsync(message).GetAwaiter().GetResult();// TODO: refactor to proper async
            break;
            default:
                break;
        }
    }

    private EventType DetermineEventType(string message)
    {
        var eventType = JsonSerializer.Deserialize<GenericEventDto>(message);

        switch (eventType.Event)
        {
            case "PlatformPublished":
                return EventType.PlatformPublished;
            default:
                return EventType.Undetermined;
        }
    }

    private async Task AddPlatformAsync(string platformPublishedMessage)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var commandRepository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();

            var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);
            
            try
            {
                var platform = _mapper.Map<Platform>(platformPublishedDto);
                if (!await commandRepository.PlatformExistsByExternalIdAsync(platform.ExternalId))
                {
                    await commandRepository.CreatePlatformAsync(platform);
                    commandRepository.SaveChanges();
                }
                else
                {
                    Console.WriteLine("Platform already exists, cannot add a new one");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot add a new platform - {e.Message}");
            }
        }
    }

    enum EventType
    {
        Undetermined = 0,
        PlatformPublished
    }
}