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

        public Platform GetById(int id)
        {
            return dbContext.Platforms.FirstOrDefault(t => t.Id == id);
        }

        public void CreatePlatform(Platform platform)
        {
            if (platform is null)
            {
                throw new ArgumentNullException(nameof(Platform));
            }

            dbContext.Platforms.Add(platform);
        }

        public ICollection<Platform> GetAll()
        {
            return dbContext.Platforms.ToList();
        }

        public bool SaveChanges()
        {
            return dbContext.SaveChanges() >= 0;
        }
    }
}

