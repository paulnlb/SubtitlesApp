namespace OpenAiMockServer.ResponseModels;

public class Segment
{
    public int Start { get; set; }
    public int End { get; set; }
    public string Text { get; set; } = string.Empty;
}
