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

    public async Task<bool> PlatformExistsAsync(int platformId)
    {
        return await _dbContext.Platforms.AnyAsync(t => t.Id == platformId);
    }

    public async Task<bool> PlatformExistsByExternalIdAsync(int externalPlatformId)
    {
        return await _dbContext.Platforms.AnyAsync(t => t.ExternalId == externalPlatformId);
    }

    public async Task<ICollection<Command>> GetCommandsForPlatformAsync(int platformId)
    {
        return await _dbContext.Commands
            .Where(t => t.PlatformId == platformId)
            .ToListAsync();
    }

    public async Task<Command?> GetCommandAsync(int platformId, int commandId)
    {
        return await _dbContext.Commands
            .FirstOrDefaultAsync(t => t.PlatformId == platformId && t.Id == commandId);
    }

    public async Task CreateCommandAsync(int platformId, Command command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        command.PlatformId = platformId;
        await _dbContext.Commands.AddAsync(command);
    }
}