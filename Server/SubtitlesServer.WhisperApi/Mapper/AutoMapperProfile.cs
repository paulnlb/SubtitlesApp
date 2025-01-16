using AutoMapper;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.WhisperApi.Models;
using Whisper.net;

namespace SubtitlesServer.WhisperApi.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<WhisperRequestModel, WhisperDto>().ForMember(dest => dest.AudioStream, opts => opts.Ignore());
        CreateMap<SegmentData, SubtitleDto>()
            .ForMember(dest => dest.LanguageCode, opts => opts.MapFrom(src => src.Language))
            .ForMember(dest => dest.StartTime, opts => opts.MapFrom(src => src.Start))
            .ForMember(dest => dest.EndTime, opts => opts.MapFrom(src => src.End));
    }
}
