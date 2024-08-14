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

        public PlatformsController(
            IPlatformRepository platformRepository,
            ICommandDataClient commandDataClient,
            IMessageBrokerClient messageBrokerClient,
            IMapper mapper)
        {   
            _platformRepository = platformRepository;
            _commandDataClient = commandDataClient;
            _messageBrokerClient = messageBrokerClient;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<PlatformReadDTO>>> GetPlatforms()
        {
            var platforms = await _platformRepository.GetAllAsync();
            var dtos = _mapper.Map<ICollection<PlatformReadDTO>>(platforms);
            return Ok(dtos);
        }

        [HttpGet("{id}", Name = nameof(GetByID))]
        //[Route("GetById")]
        public async Task<ActionResult<PlatformReadDTO>> GetByID(int id)
        {
            var platform = await _platformRepository.GetByIdAsync(id);
            if (platform is not null)
            {
                var dto = _mapper.Map<PlatformReadDTO>(platform);
                return Ok(dto);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDTO>> CreatePlatform([FromBody] PlatformCreateDTO model)
        {
            var platform = _mapper.Map<Platform>(model);

            _platformRepository.CreatePlatform(platform);
            await _platformRepository.SaveChangesAsync();

            var platformReadDto = _mapper.Map<PlatformReadDTO>(platform);

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
                var publishMessage = _mapper.Map<PlatformPublishedDTO>(platformReadDto);
                // TODO: move to const
                publishMessage.Event = "PlatformPublished";
                _messageBrokerClient.PublishNewPlatform(publishMessage);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception during sending asynchronously {nameof(CreatePlatform)}. {e.Message}");
            }
            
            return CreatedAtRoute(nameof(GetByID), new { id = platformReadDto.Id }, platformReadDto);
        }
    }
}