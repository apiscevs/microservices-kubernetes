using CommandService.Constants;
using CommandService.Models;
using CommandService.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos;

namespace CommandService.Data;
public class CommandRepository : ICommandRepository
{
    private readonly Container _platformsContainer;
    private readonly Container _commandsContainer;

    public CommandRepository(CosmosClient cosmosClient, IOptions<CosmosDbSettings> cosmosDbSettings)
    {
        _platformsContainer = cosmosClient.GetContainer(cosmosDbSettings.Value.DatabaseName, CosmosDbConstants.CommandServiceContainers.Platforms);
        _commandsContainer = cosmosClient.GetContainer(cosmosDbSettings.Value.DatabaseName, CosmosDbConstants.CommandServiceContainers.Commands);
    }

    public void SaveChanges()
    {
        // Cosmos DB operations are generally immediate, so this might not be needed.
        // You could remove this method if it's not necessary in your Cosmos DB implementation.
    }

    // Platforms
    public async Task<ICollection<Platform>> GetAllPlatformsAsync()
    {
        var query = _platformsContainer.GetItemQueryIterator<Platform>("SELECT * FROM c");
        var results = new List<Platform>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task CreatePlatformAsync(Platform platform)
    {
        try
        {
            await _platformsContainer.UpsertItemAsync(platform, new PartitionKey(platform.ExternalId));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> PlatformExistsAsync(Guid platformId)
    {
        var sqlQueryText = "SELECT VALUE COUNT(1) FROM c WHERE c.id = @id";
        var queryDefinition = new QueryDefinition(sqlQueryText)
            .WithParameter("@id", platformId.ToString());

        var query = _platformsContainer.GetItemQueryIterator<int>(queryDefinition);

        var count = 0;
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            count += response.FirstOrDefault();
        }

        return count > 0;
    }

    public async Task<bool> PlatformExistsByExternalIdAsync(string externalPlatformId)
    {
        var sqlQueryText = "SELECT VALUE 1 FROM c WHERE c.externalId = @externalId";
        var queryDefinition = new QueryDefinition(sqlQueryText)
            .WithParameter("@externalId", externalPlatformId);

        var query = _platformsContainer.GetItemQueryIterator<int>(queryDefinition, requestOptions: new QueryRequestOptions { MaxItemCount = 1 });

        var response = await query.ReadNextAsync();
        return response.Any(); // Returns true if any item is found
    }

    // Commands
    public async Task<ICollection<Command>> GetCommandsForPlatformAsync(Guid platformId)
    {
        var sqlQueryText = "SELECT * FROM c WHERE c.platformId = @platformId";
        var queryDefinition = new QueryDefinition(sqlQueryText)
            .WithParameter("@platformId", platformId.ToString());

        var queryRequestOptions = new QueryRequestOptions
        {
            MaxItemCount = 100
        };

        var query = _commandsContainer.GetItemQueryIterator<Command>(queryDefinition, requestOptions: queryRequestOptions);
        var results = new List<Command>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<Command?> GetCommandAsync(Guid platformId, Guid commandId)
    {
        try
        {
            var response = await _commandsContainer.ReadItemAsync<Command>(
                commandId.ToString(),
                new PartitionKey(platformId.ToString())
            );

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateCommandAsync(Guid platformId, Command command)
    {
        command.PlatformId = platformId.ToString();
        await _commandsContainer.UpsertItemAsync(command, new PartitionKey(platformId.ToString()));
    }
}