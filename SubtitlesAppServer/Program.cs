using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Application.Services;
using SubtitlesServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedScoped<IWaveService, WaveService>("wave");

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

app.MapRazorPages();
app.MapHub<WhisperHub>("/whisperHub");

app.Run();