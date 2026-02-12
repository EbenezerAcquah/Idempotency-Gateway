using IdempotencyGateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Creates the application builder
builder.Services.AddControllers(); 
builder.Services.AddOpenApi();

// Registers IdempotencyService as a Singleton
builder.Services.AddSingleton<IdempotencyService>();

var app = builder.Build();

// Maps controller routes
app.MapControllers(); 

app.Run();