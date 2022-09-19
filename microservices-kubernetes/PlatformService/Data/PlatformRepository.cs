using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data
{
    public class PlatformRepository : IPlatformRepository
    {
        private readonly AppDbContext dbContext;

        public PlatformRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Platform> GetByIdAsync(int id)
        {
            return await dbContext.Platforms.FirstOrDefaultAsync(t => t.Id == id);
        }

        public void CreatePlatform(Platform platform)
        {
            if (platform is null)
            {
                throw new ArgumentNullException(nameof(Platform));
            }

            dbContext.Platforms.Add(platform);
        }

        public async Task<ICollection<Platform>> GetAllAsync()
        {
            return await dbContext.Platforms.ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await dbContext.SaveChangesAsync() >= 0;
        }
    }
}

