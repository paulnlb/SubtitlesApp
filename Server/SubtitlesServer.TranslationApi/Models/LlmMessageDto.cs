namespace SubtitlesServer.TranslationApi.Models;

public class LlmMessageDto
{
    public string Role { get; set; }

    public string Content { get; set; }

    public LlmMessageDto(string role, string content)
    {
        Role = role;
        Content = content;
    }
}
