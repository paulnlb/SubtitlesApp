using System;
using System.Collections.Generic;
using System.Text;

namespace SubtitlesApp.Core.DTOs;

public class AudioChunkDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public Stream Audio { get; set; }
}
