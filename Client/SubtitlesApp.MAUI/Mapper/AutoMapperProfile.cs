using AutoMapper;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<VisualSubtitle, Subtitle>().ReverseMap();

        CreateMap<SubtitleDto, Subtitle>()
            .ForMember(
                dest => dest.TimeInterval,
                opts => opts.MapFrom(src => new TimeInterval(src.StartTime, src.EndTime))
            );

        CreateMap<SubtitleDto, VisualSubtitle>().IncludeBase<SubtitleDto, Subtitle>();

        CreateMap<Subtitle, SubtitleDto>()
            .ForMember(
                dest => dest.StartTime,
                opts => opts.MapFrom(src => src.TimeInterval.StartTime)
            )
            .ForMember(dest => dest.EndTime, opts => opts.MapFrom(src => src.TimeInterval.EndTime));

        CreateMap<VisualSubtitle, SubtitleDto>().IncludeBase<Subtitle, SubtitleDto>();
    }
}
