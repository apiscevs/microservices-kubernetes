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
            CreateMap<Platform, PlatformReadDTO>()
                .ReverseMap();

            CreateMap<PlatformCreateDTO, Platform>()
                .ForMember(t => t.Id, t => t.Ignore())
                .ReverseMap();

            CreateMap<PlatformReadDTO, PlatformPublishedDTO>();
        }
    }
}
