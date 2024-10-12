using SubtitlesServer.Extensions;
using SubtitlesServer.Infrastructure.Configs;
using SubtitlesServer.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppServices();

builder.Services.Configure<WhisperConfig>(builder.Configuration.GetSection("WhisperModelSettings"));

builder.Services.AddControllers();

builder.Services.AddMvc();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddConcurrencyRateLimiter(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseRateLimiter();

app.Run();