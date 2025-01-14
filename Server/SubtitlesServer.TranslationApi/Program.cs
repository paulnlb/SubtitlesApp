using SubtitlesServer.TranslationApi.Configs;
using SubtitlesServer.TranslationApi.Extensions;
using SubtitlesServer.TranslationApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppServices();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.Configure<OllamaConfig>(builder.Configuration.GetSection("OllamaConfig"));

builder.Services.AddHttpClient(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
