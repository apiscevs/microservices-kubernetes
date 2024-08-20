using CommandService.Models;

namespace CommandService.Data
{
    public interface ICommandRepository
    {
        // Platforms
        Task<ICollection<Platform>> GetAllPlatformsAsync();
        Task CreatePlatformAsync(Platform platform);
        Task<bool> PlatformExistsAsync(Guid platformId);
        Task<bool> PlatformExistsByExternalIdAsync(string externalPlatformId);
        
        // Commands
        Task<ICollection<Command>> GetCommandsForPlatformAsync(Guid platformId);
        Task<Command?> GetCommandAsync(Guid platformId, Guid commandId);
        Task CreateCommandAsync(Guid platformId, Command command);
    }
}
