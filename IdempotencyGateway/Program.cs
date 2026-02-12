using IdempotencyGateway.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IdempotencyService>();

var app = builder.Build();

app.Run();
