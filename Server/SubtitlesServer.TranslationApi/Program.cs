using SubtitlesServer.Shared.Extensions;
using SubtitlesServer.TranslationApi.Configs;
using SubtitlesServer.TranslationApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OllamaConfig>(builder.Configuration.GetSection("OllamaConfig"));

builder.Services.AddSharedServices(builder.Configuration);
builder.Services.AddAppServices();

builder.Services.AddHttpClient(builder.Configuration);

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

app.Run();
