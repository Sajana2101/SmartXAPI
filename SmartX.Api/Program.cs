using System.Text.Json.Serialization;
using SmartX.Api.Repositories;
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddSingleton<ISensorRepository, InMemorySensorRepository>();

builder.Services.AddSingleton<ITelemetryStore, TelemetryStore>();
builder.Services.AddSingleton<TelemetryHistoryProcessor>();
builder.Services.AddSingleton<DeploymentValidator>();
builder.Services.AddSingleton<TelemetrySeeder>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("SmartXClient", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("SmartXClient");

app.UseAuthorization();

app.MapControllers();

app.Run();