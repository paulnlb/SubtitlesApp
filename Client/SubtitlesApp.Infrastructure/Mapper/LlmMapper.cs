using Google.GenAI.Types;
using OpenAI.Responses;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;

namespace SubtitlesApp.Infrastructure.Mapper;

#pragma warning disable OPENAI001
public static class LlmMapper
{
    public static void ToResponseItems(List<LlmMessageDto> llmMessageDtos, IList<ResponseItem> responseItems)
    {
        foreach (var msg in llmMessageDtos)
        {
            switch (msg.Role)
            {
                case LlmRoleConstants.User:
                    responseItems.Add(ResponseItem.CreateUserMessageItem(msg.Content));
                    break;

                case LlmRoleConstants.Assistant:
                    responseItems.Add(ResponseItem.CreateAssistantMessageItem(msg.Content));
                    break;

                case LlmRoleConstants.System:
                    responseItems.Add(ResponseItem.CreateSystemMessageItem(msg.Content));
                    break;
            }
        }
    }

    public static void ToGeminiContentList(List<LlmMessageDto> llmMessageDtos, List<Content> contentList)
    {
        foreach (var msg in llmMessageDtos)
        {
            switch (msg.Role)
            {
                case LlmRoleConstants.User:
                    contentList.Add(new Content() { Role = "user", Parts = [Part.FromText(msg.Content)] });
                    break;

                case LlmRoleConstants.Assistant:
                    contentList.Add(new Content() { Role = "model", Parts = [Part.FromText(msg.Content)] });
                    break;

                // system instructions must be set inside a GenerateContentConfig instance
            }
        }
    }
}
