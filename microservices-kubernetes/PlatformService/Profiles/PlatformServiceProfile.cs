using System;
using AutoMapper;
using PlatformService.DTO;
using PlatformService.Models;

namespace PlatformService.Profiles
{
    public class PlatformServiceProfile : Profile
    {
        public PlatformServiceProfile()
        {
            CreateMap<Platform, PlatformReadDto>()
                .ReverseMap();

            CreateMap<PlatformCreateDto, Platform>()
                .ForMember(t => t.Id, t => t.Ignore())
                .ReverseMap();

            CreateMap<PlatformReadDto, PlatformPublishedDto>();
        }
    }
}
