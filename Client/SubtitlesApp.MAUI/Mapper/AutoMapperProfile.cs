using AutoMapper;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<TimeInterval, TimeIntervalDTO>()
            .ReverseMap();

        CreateMap<Subtitle, SubtitleDTO>()
            .ReverseMap();

        CreateMap<VisualSubtitle, SubtitleDTO>()
            .ReverseMap();
    }
}
