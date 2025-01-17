using System.Collections.ObjectModel;
using Riok.Mapperly.Abstractions;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Mapper;

[Mapper]
public partial class SubtitlesMapper
{
    [MapPropertyFromSource(nameof(VisualSubtitle.TimeInterval))]
    [MapperIgnoreTarget(nameof(VisualSubtitle.Translation))]
    [MapperIgnoreTarget(nameof(VisualSubtitle.IsHighlighted))]
    public partial VisualSubtitle SubtitleDtoToVisualSubtitle(SubtitleDto subtitleDto);

    [MapProperty(nameof(VisualSubtitle.TimeInterval.StartTime), nameof(SubtitleDto.StartTime))]
    [MapProperty(nameof(VisualSubtitle.TimeInterval.EndTime), nameof(SubtitleDto.EndTime))]
    [MapperIgnoreSource(nameof(VisualSubtitle.Translation))]
    [MapperIgnoreSource(nameof(VisualSubtitle.IsTranslated))]
    [MapperIgnoreSource(nameof(VisualSubtitle.IsHighlighted))]
    public partial SubtitleDto VisualSubtitleToSubtitleDto(VisualSubtitle visualSubtitle);

    public partial List<SubtitleDto> VisualSubtitlesToSubtitleDtoList(IEnumerable<VisualSubtitle> visualSubtitles);

    public partial ObservableCollection<VisualSubtitle> SubtitlesDtosToObservableVisualSubtitles(
        IEnumerable<SubtitleDto> subtitleDtos
    );

    private TimeInterval SubtitleDtoToTimeInterval(SubtitleDto subtitleDto) =>
        new(subtitleDto.StartTime, subtitleDto.EndTime);
}
