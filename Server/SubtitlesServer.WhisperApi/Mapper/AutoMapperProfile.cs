using AutoMapper;
using SubtitlesServer.WhisperApi.Models;

namespace SubtitlesServer.WhisperApi.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<WhisperRequestModel, WhisperDto>().ForMember(dest => dest.AudioStream, opts => opts.Ignore());
    }
}
