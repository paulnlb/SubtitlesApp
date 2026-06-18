using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OpenAiMockServer;
using OpenAiMockServer.Helpers;
using OpenAiMockServer.ResponseModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

string[]? latinWordsCache = null;
string[]? ukrWordsCache = null;
var serializerOptions = new JsonSerializerOptions()
{
    WriteIndented = true,
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
};

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/test", () => "Hi there");

app.MapPost(
        "/v1/audio/transcriptions",
        async ([FromForm] IFormFile file, [FromForm] string model, ILogger<Program> logger) =>
        {
            logger.LogInformation("Selected model: {Model}", model);
            logger.LogInformation("Input audio size in bytes: {Size}", file.Length.ToString());

            using var stream = File.OpenWrite(
                @$"InputAudio/sample{DateTime.Now.ToString("HHmmss", System.Globalization.CultureInfo.InvariantCulture)}.wav"
            );

            await file.OpenReadStream().CopyToAsync(stream);

            latinWordsCache ??= await TextHelper.ReadWordsAsync(@"Assets/lorem_ipsum.txt");

            return new TranscriptionResponse() { Segments = SeedHelper.MakeSegments(latinWordsCache, 30, 10).ToList() };
        }
    )
    .WithName("Transcription")
    .DisableAntiforgery();

app.MapPost(
        "/v1/responses",
        async () =>
        {
            ukrWordsCache ??= await TextHelper.ReadWordsAsync(@"Assets/ukr_text.txt");

            var translations = SeedHelper.MakeTranslations(ukrWordsCache, 5, 10).ToList();
            var response = new LlmSubtitleListDto() { Items = translations };

            return new OpenAIResponse
            {
                Id = "resp_123abc",
                Object = "response",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = "gpt-4-mock",
                Status = "completed",
                Output =
                [
                    new()
                    {
                        Id = "item_456def",
                        Type = "message",
                        Role = "assistant",
                        Content =
                        [
                            new() { Type = "output_text", Text = JsonSerializer.Serialize(response, serializerOptions) },
                        ],
                    },
                ],
                Usage = new UsageDetails()
                {
                    PromptTokens = 100,
                    CompletionTokens = 200,
                    TotalTokens = 300,
                },
            };
        }
    )
    .WithName("Responses");

app.Run();
