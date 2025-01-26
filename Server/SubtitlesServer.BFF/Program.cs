var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHttpLogging(o => { });

var app = builder.Build();

app.UseHttpLogging();

app.MapReverseProxy();

app.Run();
