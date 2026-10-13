
using System.Text.Json.Serialization;
using SmartX.Api.Repositories;
using SmartX.Api.Services;
using SmartX.Api.Options;
using SmartX.Api.Part2.Services;
using SmartX.Api.Part2.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddSingleton<
    ISensorRepository,
    InMemorySensorRepository>();

builder.Services.AddSingleton<
    ITelemetryStore,
    TelemetryStore>();

builder.Services.AddSingleton<TelemetryQueueService>();
builder.Services.AddSingleton<TelemetryHistoryProcessor>();
builder.Services.AddSingleton<DeploymentValidator>();
builder.Services.AddSingleton<TelemetrySeeder>();
builder.Services.AddSingleton<PriorityAlertService>();
builder.Services.AddSingleton<SortedTelemetryHistoryService>();
builder.Services.AddSingleton<DeviceIssueTracker>();

builder.Services.AddSingleton<DeviceSearchHistoryService>();
builder.Services.AddSingleton<DeviceBehaviorAnalysisService>();
builder.Services.AddSingleton<PredictiveRecommendationService>();

builder.Services.AddSingleton<
    IDeviceCommandService,
    DeviceCommandService>();

builder.Services.AddSingleton<TelemetryDashboardService>();

builder.Services.AddHostedService<DeviceFaultMonitoringWorker>();

builder.Services.Configure<AttachmentOptions>(
    builder.Configuration.GetSection(
        AttachmentOptions.SectionName));

builder.Services.AddSingleton<
    IFileEncryptionService,
    AesFileEncryptionService>();

builder.Services.AddSingleton<
    IAttachmentService,
    AttachmentService>();

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
