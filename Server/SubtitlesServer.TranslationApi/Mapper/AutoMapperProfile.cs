using AutoMapper;
using OllamaSharp.Models.Chat;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesServer.TranslationApi.Models;

namespace SubtitlesServer.TranslationApi.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<LlmMessageDto, Message>()
            .ForMember(dest => dest.Role, opts => opts.MapFrom(src => new ChatRole(src.Role)))
            .ForMember(dest => dest.Content, opts => opts.MapFrom(src => src.Content));

        CreateMap<Message, LlmMessageDto>()
            .ForMember(dest => dest.Role, opts => opts.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.Content, opts => opts.MapFrom(src => src.Content));

        CreateMap<SubtitleDto, Translation>().ReverseMap();
    }
}
