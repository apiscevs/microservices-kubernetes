using PlatformService.DTO;

namespace PlatformService.AsyncDataServices;

public interface IMessageBrokerClient
{
    void PublishNewPlatform(PlatformPublishedDto platformPublishedDto);
}