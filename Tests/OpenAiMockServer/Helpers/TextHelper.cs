namespace OpenAiMockServer.Helpers;

public static class TextHelper
{
    public static async Task<string[]> ReadWordsAsync(string path)
    {
        var reader = File.OpenText(path);

        var text = await reader.ReadToEndAsync();

        return text.Split(" ");
    }
}
