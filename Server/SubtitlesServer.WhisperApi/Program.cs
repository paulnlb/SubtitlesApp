using SubtitlesServer.Shared.Extensions;
using SubtitlesServer.WhisperApi.Configs;
using SubtitlesServer.WhisperApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WhisperConfig>(builder.Configuration.GetSection("WhisperModelSettings"));

builder.Services.AddSharedServices(builder.Configuration);
builder.Services.AddAppServices();

builder.Services.AddConcurrencyRateLimiter(builder.Configuration);

var app = builder.Build();

ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

var controllerActionBuilder = app.MapControllers();
controllerActionBuilder.AddAuthorizationToPipeline(logger);

app.UseRateLimiter();

app.Run();
