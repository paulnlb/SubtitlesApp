using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;

namespace SubtitlesServer.TranslationApi.Helpers;

public static class JsonHelper
{
    public static string UnwrapJsonArrayFromRootObject(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;

        var bracketStack = new Stack<int>();
        var arrayStart = -1;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];

            if (c == '[')
            {
                if (bracketStack.Count == 0)
                {
                    arrayStart = i;
                }
                bracketStack.Push(i);
            }
            else if (c == ']')
            {
                bracketStack.Pop();

                if (bracketStack.Count == 0 && arrayStart != -1)
                {
                    return json.Substring(arrayStart, i - arrayStart + 1);
                }
            }
        }

        return json;
    }

    public static async IAsyncEnumerable<string> UnwrapJsonArrayFromRootObjectAsync(IAsyncEnumerable<string> jsonParts)
    {
        var bracketStack = new Stack<int>();
        var shouldWrite = false;
        var writeComplete = false;

        await foreach (var jsonPart in jsonParts)
        {
            var jsonChunk = new StringBuilder();

            foreach (var c in jsonPart)
            {
                if (writeComplete)
                {
                    break;
                }

                if (c == '[')
                {
                    bracketStack.Push(c);
                    shouldWrite = true;
                }
                else if (c == ']')
                {
                    bracketStack.Pop();

                    if (bracketStack.Count == 0)
                    {
                        writeComplete = true;
                    }
                }

                if (shouldWrite)
                {
                    jsonChunk.Append(c);
                }
            }

            if (jsonChunk.Length > 0)
            {
                yield return jsonChunk.ToString();
            }

            if (writeComplete)
            {
                break;
            }
        }
    }

    public static bool IsJsonSchemaTypeOf(JsonNode schema, string type)
    {
        var jsonRootType = schema["type"]?.ToString();
        return jsonRootType?.Contains(type) == true;
    }

    public static JsonNode GetJsonSchemaOf(Type type)
    {
        JsonSerializerOptions options = new(JsonSerializerOptions.Default)
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };
        return options.GetJsonSchemaAsNode(type);
    }
}
