using SubtitlesServer.Shared.Extensions;
using SubtitlesServer.WhisperApi.Configs;
using SubtitlesServer.WhisperApi.Extensions;
using SubtitlesServer.WhisperApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppServices();

builder.Services.Configure<WhisperConfig>(builder.Configuration.GetSection("WhisperModelSettings"));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddConcurrencyRateLimiter(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

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
