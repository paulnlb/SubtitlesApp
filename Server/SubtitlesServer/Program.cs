using SubtitlesServer.Application.Configs;
using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Application.Services;
using SubtitlesServer.Hubs;
using SubtitlesServer.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITranscriptionService, TranscriptionService>();
builder.Services.Configure<SpeechToTextConfigs>(builder.Configuration.GetSection("SpeechToTextSettings"));
builder.Services.Configure<WhisperConfigs>(builder.Configuration.GetSection("WhisperModelSettings"));
builder.Services.AddScoped<IWhisperService, WhisperService>();
builder.Services.AddScoped<IWaveService, WaveService>();

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

app.MapRazorPages();

app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
});

app.MapHub<WhisperHub>("/whisperHub");
app.MapHub<WhisperMockHub>("/whisperMockHub");

app.Run();