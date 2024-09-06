using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.DTO;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepository _platformRepository;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBrokerClient _messageBrokerClient;
        private readonly IMapper _mapper;
        private readonly ILogger<PlatformsController> _logger;

        public PlatformsController(
            IPlatformRepository platformRepository,
            ICommandDataClient commandDataClient,
            IMessageBrokerClient messageBrokerClient,
            IMapper mapper,
            ILogger<PlatformsController> logger)
        {   
            _platformRepository = platformRepository;
            _commandDataClient = commandDataClient;
            _messageBrokerClient = messageBrokerClient;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<PlatformReadDto>>> GetPlatforms()
        {
            _logger.LogInformation("Calling {0}", nameof(GetPlatforms));
            var platforms = await _platformRepository.GetAllAsync();
            var dtos = _mapper.Map<ICollection<PlatformReadDto>>(platforms);
            return Ok(dtos);
        }

        [HttpGet("{id}", Name = nameof(GetById))]
        //[Route("GetById")]
        public async Task<ActionResult<PlatformReadDto>> GetById(int id)
        {
            var platform = await _platformRepository.GetByIdAsync(id);
            if (platform is not null)
            {
                var dto = _mapper.Map<PlatformReadDto>(platform);
                return Ok(dto);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform([FromBody] PlatformCreateDto model)
        {
            Console.WriteLine("IS SHOULD SEE THIS!!!");
            var platform = _mapper.Map<Platform>(model);

            _platformRepository.CreatePlatform(platform);
            await _platformRepository.SaveChangesAsync();

            var platformReadDto = _mapper.Map<PlatformReadDto>(platform);

            // Send sync message
            try
            {
                await _commandDataClient.SendPlatformCommand(platformReadDto);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception during sending synchronously {nameof(CreatePlatform)}. {e.Message}");
            }

            // Send async message
            try
            {
                // TODO: map from model
                var publishMessage = _mapper.Map<PlatformPublishedDto>(platformReadDto);
                // TODO: move to const
                publishMessage.Event = "PlatformPublished";
                _messageBrokerClient.PublishNewPlatform(publishMessage);
            }
            catch(Exception e)
            {
                throw;
                Console.WriteLine($"Exception during sending asynchronously {nameof(CreatePlatform)}. {e.Message}");
            }
            
            return CreatedAtRoute(nameof(GetById), new { id = platformReadDto.Id }, platformReadDto);
        }
    }
}