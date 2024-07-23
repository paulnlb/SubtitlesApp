using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Application.Services;
using SubtitlesServer.Hubs;
using SubtitlesServer.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedScoped<ITranscriptionService, TranscriptionService>("transcription");
builder.Services.AddScoped<IWhisperService, WhisperService>();
builder.Services.AddScoped<IWaveService, WaveService>();

builder.Services.AddKeyedSingleton<ICancellationManager, CancellationManager>("cancellationManager");

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

app.MapRazorPages();
app.MapHub<WhisperHub>("/whisperHub");
app.MapHub<WhisperMockHub>("/whisperMockHub");

app.Run();