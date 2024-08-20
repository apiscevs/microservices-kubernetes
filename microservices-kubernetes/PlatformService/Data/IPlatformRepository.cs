using PlatformService.Models;

namespace PlatformService.Data
{
    public interface IPlatformRepository
    {
        Task<bool> SaveChangesAsync();
        Task<ICollection<Platform>> GetAllAsync();
        Task<Platform> GetByIdAsync(int id);
        void CreatePlatform(Platform platform);
    }
}
