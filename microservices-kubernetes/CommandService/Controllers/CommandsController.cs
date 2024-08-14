using AutoMapper;
using CommandService.Data;
using CommandService.DTO;
using CommandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers;

[ApiController]
[Route("api/c/platforms/{platformId}/[controller]")]
public class CommandsController : ControllerBase
{
    private readonly ICommandRepository _repository;
    private readonly IMapper _mapper;

    public CommandsController(ICommandRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommandReadDto>>> GetCommandsForPlatform(int platformId)
    {
        Console.WriteLine($"--> Hit GetCommandsForPlatform: {platformId}");
    
        if (!await _repository.PlatformExistsAsync(platformId))
        {
            return NotFound();
        }
    
        var commands = await _repository.GetCommandsForPlatformAsync(platformId);
    
        return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
    }
    
    [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
    public async Task<ActionResult<CommandReadDto>> GetCommandForPlatform(int platformId, int commandId)
    {
        Console.WriteLine($"--> Hit GetCommandForPlatform: {platformId} / {commandId}");
    
        if (!await _repository.PlatformExistsAsync(platformId))
        {
            return NotFound();
        }
    
        var command = await _repository.GetCommandAsync(platformId, commandId);
    
        if(command == null)
        {
            return NotFound();
        }
    
        return Ok(_mapper.Map<CommandReadDto>(command));
    }
    
    [HttpPost]
    public async Task<ActionResult<CommandReadDto>> CreateCommandForPlatform(int platformId, CommandCreateDto commandDto)
    {
        Console.WriteLine($"--> Hit CreateCommandForPlatform: {platformId}");
    
        if (!await _repository.PlatformExistsAsync(platformId))
        {
            return NotFound();
        }
    
        var command = _mapper.Map<Command>(commandDto);
    
        await _repository.CreateCommandAsync(platformId, command);
        _repository.SaveChanges();
    
        var commandReadDto = _mapper.Map<CommandReadDto>(command);
    
        return CreatedAtRoute(nameof(GetCommandForPlatform),
            new {platformId = platformId, commandId = commandReadDto.Id}, commandReadDto);
    }
}

