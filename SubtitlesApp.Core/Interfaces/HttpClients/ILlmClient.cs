using System.Text.Json.Nodes;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Interfaces.HttpClients;

public interface ILlmClient
{
    /// <summary>
    /// Sends a chat message to the LLM and returns the raw response as a string
    /// </summary>
    /// <param name="chatHistory"></param>
    /// <param name="userPrompt"></param>
    /// <param name="responseFormat"></param>
    /// <returns></returns>
    Task<Result<string>> SendChatAsync(List<LlmMessageDto> chatHistory, string userPrompt, JsonNode? responseFormat = null);

    /// <summary>
    /// General method for sending chat messages to the LLM and parsing the response into a specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="chatHistory"></param>
    /// <param name="userPrompt"></param>
    /// <returns></returns>
    Task<Result<T>> SendChatAsync<T>(List<LlmMessageDto> chatHistory, string userPrompt);
}
