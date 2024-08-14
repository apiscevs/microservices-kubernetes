using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data
{
    public class PlatformRepository : IPlatformRepository
    {
        private readonly AppDbContext _dbContext;

        public PlatformRepository(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Platform> GetByIdAsync(int id)
        {
            return await _dbContext.Platforms.FirstOrDefaultAsync(t => t.Id == id);
        }

        public void CreatePlatform(Platform platform)
        {
            if (platform is null)
            {
                throw new ArgumentNullException(nameof(Platform));
            }

            _dbContext.Platforms.Add(platform);
        }

        public async Task<ICollection<Platform>> GetAllAsync()
        {
            return await _dbContext.Platforms.ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync() >= 0;
        }
    }
}

