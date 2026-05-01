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
}
