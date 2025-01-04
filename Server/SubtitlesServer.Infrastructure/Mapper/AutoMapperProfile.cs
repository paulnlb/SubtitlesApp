using AutoMapper;
using OllamaSharp.Models.Chat;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;

namespace SubtitlesServer.Infrastructure.Mapper;

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

        CreateMap<SubtitleDTO, LlmSubtitleDto>()
            .ForMember(dest => dest.Text, opts => opts.MapFrom(src => src.Text))
            .ForMember(
                dest => dest.StartTime,
                opts => opts.MapFrom(src => src.TimeInterval.StartTime)
            )
            .ForMember(dest => dest.EndTime, opts => opts.MapFrom(src => src.TimeInterval.EndTime))
            .ForMember(dest => dest.LanguageCode, opts => opts.MapFrom(src => src.LanguageCode));

        CreateMap<LlmSubtitleDto, SubtitleDTO>()
            .ForMember(
                dest => dest.Translation,
                opts =>
                    opts.MapFrom(src => new Translation
                    {
                        LanguageCode = src.LanguageCode,
                        Text = src.Text,
                    })
            )
            .ForMember(dest => dest.Text, opts => opts.Ignore())
            .ForMember(dest => dest.TimeInterval, opts => opts.Ignore())
            .ForMember(dest => dest.LanguageCode, opts => opts.Ignore())
            .ForMember(dest => dest.IsTranslated, opts => opts.Ignore());
    }
}
