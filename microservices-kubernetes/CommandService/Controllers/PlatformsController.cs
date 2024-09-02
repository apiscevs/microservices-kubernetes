using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.DTO;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace CommandService.Controllers;

[ApiController]
[Route("api/c/[controller]")]
public class PlatformsController : ControllerBase
{
    private readonly ICommandRepository _repository;
    private readonly IMapper _mapper;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ILogger<PlatformsController> _logger;

    public PlatformsController(ICommandRepository repository, 
        IMapper mapper,
        IConnectionMultiplexer redisConnection,
        ILogger<PlatformsController> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _redisConnection = redisConnection;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlatformReadDto>>> GetPlatforms()
    {
        Console.WriteLine("--> Getting Platforms from CommandsService");

        _logger.LogInformation("Calling API method => {0}", nameof(GetPlatforms));

        // Define a cache key for Redis
        string cacheKey = "platformsList";

        // Try to get the data from Redis cache
        var redis = _redisConnection.GetDatabase();
        var cachedPlatforms = await redis.StringGetAsync(cacheKey);

        if (!cachedPlatforms.IsNullOrEmpty)
        {
            _logger.LogInformation("Returning platforms from cache.");

            // Deserialize cached data using System.Text.Json
            var platformItemsFromCache = JsonSerializer.Deserialize<List<PlatformReadDto>>(cachedPlatforms);

            return Ok(platformItemsFromCache);
        }
        else
        {
            _logger.LogInformation("Cache miss. Retrieving platforms from the repository.");

            // Retrieve the data from the repository
            var platformItems = await _repository.GetAllPlatformsAsync();

            // Map the data to DTOs and convert to a List
            var platformReadDtos = _mapper.Map<List<PlatformReadDto>>(platformItems);

            // Serialize the data and store it in Redis with an expiration time (e.g., 10 minutes)
            var serializedPlatforms = JsonSerializer.Serialize(platformReadDtos);
            await redis.StringSetAsync(cacheKey, serializedPlatforms, TimeSpan.FromMinutes(10));

            return Ok(platformReadDtos);
        }
    }


    [HttpPost]
    public async Task<ActionResult> TestRedisCache()
    {
        await Task.Delay(1);
        Console.WriteLine("--> Inbound POST # Command Service");

        _logger.LogInformation("I should see this in logs => {0}", nameof(GetPlatforms));
        
        var platformItems = await _repository.GetAllPlatformsAsync();
        
        return Ok("Inbound test of from Platforms Controler");
    }
    
    
    [HttpGet("TestException")]
    public async Task<ActionResult> TestException()
    {
        throw new NotImplementedException("This is intentionally");
    }
}
