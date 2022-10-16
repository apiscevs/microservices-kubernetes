using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IPlatformRepository platformRepository;
        private readonly ICommandDataClient commandDataClient;
        private readonly IMapper mapper;

        public PlatformsController(
            IPlatformRepository platformRepository,
            ICommandDataClient commandDataClient,
            IMapper mapper)
        {   
            this.platformRepository = platformRepository;
            this.commandDataClient = commandDataClient;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<PlatformReadDTO>>> GetPlatforms()
        {
            var platforms = await platformRepository.GetAllAsync();
            var dtos = mapper.Map<ICollection<PlatformReadDTO>>(platforms);
            return Ok(dtos);
        }

        [HttpGet("{id}", Name = nameof(GetByID))]
        //[Route("GetById")]
        public async Task<ActionResult<PlatformReadDTO>> GetByID(int id)
        {
            var platform = await platformRepository.GetByIdAsync(id);
            if (platform is not null)
            {
                var dto = mapper.Map<PlatformReadDTO>(platform);
                return Ok(dto);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDTO>> CreatePlatform([FromBody] PlatformCreateDTO model)
        {
            var platform = mapper.Map<Platform>(model);

            platformRepository.CreatePlatform(platform);
            await platformRepository.SaveChangesAsync();

            var dto = mapper.Map<PlatformReadDTO>(platform);

            try
            {
                await commandDataClient.SendPlatformCommand(dto);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception during {nameof(CreatePlatform)}. {e.Message}");
            }

            return CreatedAtRoute(nameof(GetByID), new { id = dto.Id }, dto);
        }
    }
}