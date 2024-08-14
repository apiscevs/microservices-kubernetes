using CommandService.Models;

namespace CommandService.Data
{
    public interface ICommandRepository
    {
        void SaveChanges();
        
        // Platforms
        Task<ICollection<Platform>> GetAllPlatformsAsync();
        Task CreatePlatformAsync(Platform platform);
        Task<bool> PlatformExistsAsync(int platformId);
        Task<bool> PlatformExistsByExternalIdAsync(int externalPlatformId);
        
        // Commands
        Task<ICollection<Command>> GetCommandsForPlatformAsync(int platformId);
        Task<Command?> GetCommandAsync(int platformId, int commandId);
        Task CreateCommandAsync(int platformId, Command command);
    }
}
