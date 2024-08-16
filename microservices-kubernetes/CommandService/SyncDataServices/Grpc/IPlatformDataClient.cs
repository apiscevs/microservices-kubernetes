using CommandService.Models;

namespace CommandService.SyncDataServices.Grpc;

public interface IPlatformDataClient
{
    Task<ICollection<Platform>> GetAllPlatformsAsync();
}