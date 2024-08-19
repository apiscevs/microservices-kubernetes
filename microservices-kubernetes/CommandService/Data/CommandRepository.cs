using CommandService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommandService.Data;

public class CommandRepository : ICommandRepository
{
    private readonly AppDbContext _dbContext;

    public CommandRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public void SaveChanges()
    {
        _dbContext.SaveChanges();
    }

    public async Task<ICollection<Platform>> GetAllPlatformsAsync()
    {
        return await _dbContext.Platforms.ToListAsync();
    }

    public async Task CreatePlatformAsync(Platform platform)
    {
        if (platform == null)
        {
            throw new ArgumentNullException(nameof(platform));
        }
        await _dbContext.Platforms.AddAsync(platform);
    }

    public async Task<bool> PlatformExistsAsync(Guid platformId)
    {
        return await _dbContext.Platforms
            .CountAsync(t => t.Id == platformId.ToString()) > 0;
    }

    public async Task<bool> PlatformExistsByExternalIdAsync(string externalPlatformId)
    {
        return await _dbContext.Platforms.CountAsync(t => t.ExternalId == externalPlatformId) > 0;
    }

    public async Task<ICollection<Command>> GetCommandsForPlatformAsync(Guid platformId)
    {
        return await _dbContext.Commands
            .Where(t => t.PlatformId == platformId.ToString())
            .ToListAsync();
    }

    public async Task<Command?> GetCommandAsync(Guid platformId, Guid commandId)
    {
        return await _dbContext.Commands
            .FirstOrDefaultAsync(t => t.PlatformId == platformId.ToString() && t.Id == commandId.ToString());
    }

    public async Task CreateCommandAsync(Guid platformId, Command command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        command.PlatformId = platformId.ToString();
        await _dbContext.Commands.AddAsync(command);
    }
}