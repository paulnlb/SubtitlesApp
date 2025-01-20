namespace SubtitlesApp.Extensions;

public static class HttpRequestMessageExtensions
{
    public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request, bool cloneContent = true)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };

        if (cloneContent)
        {
            clone.Content = await request.Content.CloneAsync().ConfigureAwait(false);
        }

        foreach (KeyValuePair<string, object?> opt in request.Options)
        {
            clone.Options.TryAdd(opt.Key, opt.Value);
        }
        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    public static async Task<HttpContent?> CloneAsync(this HttpContent? content)
    {
        if (content == null)
            return null;

        var ms = new MemoryStream();
        await content.CopyToAsync(ms).ConfigureAwait(false);
        ms.Position = 0;

        var clone = new StreamContent(ms);
        foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
        {
            clone.Headers.Add(header.Key, header.Value);
        }
        return clone;
    }
}
