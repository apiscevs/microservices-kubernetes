using AutoMapper;
using CommandService.Data;
using CommandService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers;

[ApiController]
[Route("api/c/[controller]")]
public class PlatformsController : ControllerBase
{
    private readonly ICommandRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<PlatformsController> _logger;

    public PlatformsController(ICommandRepository repository, 
        IMapper mapper,
        ILogger<PlatformsController> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlatformReadDto>>> GetPlatforms()
    {
        Console.WriteLine("--> Getting Platforms from CommandsService");

        _logger.LogInformation("Calling API method => {0}", nameof(GetPlatforms));
        
        var platformItems = await _repository.GetAllPlatformsAsync();

        return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
    }

    [HttpPost]
    public async Task<ActionResult> TestInboundConnection()
    {
        await Task.Delay(1);
        Console.WriteLine("--> Inbound POST # Command Service");

        _logger.LogInformation("I should see this in logs => {0}", nameof(GetPlatforms));
        
        var platformItems = await _repository.GetAllPlatformsAsync();
        
        return Ok("Inbound test of from Platforms Controler");
    }
}
