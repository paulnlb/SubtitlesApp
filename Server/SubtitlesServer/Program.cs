using SubtitlesServer.Application.Interfaces;
using SubtitlesServer.Application.Services;
using SubtitlesServer.Infrastructure.Configs;
using SubtitlesServer.Infrastructure.Services;
using SubtitlesServer.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITranscriptionService, TranscriptionService>();
builder.Services.Configure<SpeechToTextConfigs>(builder.Configuration.GetSection("SpeechToTextSettings"));
builder.Services.Configure<WhisperConfigs>(builder.Configuration.GetSection("WhisperModelSettings"));
builder.Services.AddScoped<IWhisperService, WhisperService>();
builder.Services.AddScoped<IWaveService, WaveService>();
builder.Services.AddSingleton<WhisperModelService>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddMvc();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SubtitlesServer", Version = "v1" });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapRazorPages();

app.Run();