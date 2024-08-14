using AutoMapper;
using CommandService.DTO;
using CommandService.Models;

namespace CommandService.Profiles
{
    public class PlatformServiceProfile : Profile
    {
        public PlatformServiceProfile()
        {
            // Source -> Target
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<CommandCreateDto, Command>();
            CreateMap<Command, CommandReadDto>();

            CreateMap<PlatformPublishedDto, Platform>()
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(source => source.Id));
            
        }
    }
}
