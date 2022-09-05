using System;
using PlatformService.Models;

namespace PlatformService.Data
{
    public interface IPlatformRepository
    {
        bool SaveChanges();
        ICollection<Platform> GetAll();
        Platform GetById(int id);
        void CreatePlatform(Platform platform);
    }
}

