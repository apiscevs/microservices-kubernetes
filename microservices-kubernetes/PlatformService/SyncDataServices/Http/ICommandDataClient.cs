using System;
using PlatformService.DTO;

namespace PlatformService.SyncDataServices.Http
{
    public interface ICommandDataClient
    {
        Task SendPlatformCommand(PlatformReadDto platform);
    }
}

