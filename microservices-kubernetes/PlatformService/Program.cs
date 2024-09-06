using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Settings;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

// Aspire
builder.AddServiceDefaults();
// hack to check if database will be alive
Thread.Sleep(10_000);

// Register RabbitMqSettings with the configuration section
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));

// Add services to the container.
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var connectionString = builder.Configuration.GetConnectionString("PlatformsConnectionString");
Console.WriteLine($"Using DB {connectionString}");
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(connectionString, options =>
    {
        // Enabling transient error resiliency
        options.EnableRetryOnFailure(
            maxRetryCount: 5, // The maximum number of retry attempts
            maxRetryDelay: TimeSpan.FromSeconds(5), // The delay between retries
            errorNumbersToAdd: null // Additional SQL error numbers to include in retry logic
        );
    });
    // opt.UseInMemoryDatabase("InMemoryDb");
});

builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();

var commandServiceUrl = builder.Configuration["CommandService"];
Console.WriteLine($"UPD!? CommandService URL => {commandServiceUrl}");
builder.Services.AddHttpClient<ICommandDataClient, CommandDataClient>(client =>
    {
        client.BaseAddress = new Uri(commandServiceUrl);
        client.DefaultRequestHeaders.ConnectionClose = false; // Ensures connections stay open for reuse
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromSeconds(60) // Helps to load balance request across the pod once scaled
        };
    });
builder.Services.AddSingleton<IMessageBrokerClient, RabbitMqClient>();
builder.Services.AddGrpc();

// builder.Services.AddLogging(loggingBuilder =>
// {
//     loggingBuilder.ClearProviders();
//     loggingBuilder.AddConsole();
//     loggingBuilder.AddOpenTelemetry(options =>
//     { 
//         options.IncludeFormattedMessage = true;
//         options.IncludeScopes = true;
//         options.ParseStateValues = true;
//     });
// });

var serviceName = "platformservice";

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName);

// Turn on CosmosDB logs...
AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(resourceBuilder)
        .AttachLogsToActivityEvent();
});

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("Azure.Cosmos.Operation")
            .AddSource("Azure.Cosmos.Request")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddGrpcClientInstrumentation()
            // .AddConsoleExporter()
            .AddSqlClientInstrumentation(o => o.SetDbStatementForText = true);
    })
    .WithMetrics(metrics =>
            metrics
                .AddAspNetCoreInstrumentation() // ASP.NET Core relate
                .AddRuntimeInstrumentation() // .NET Runtime metrics like - GC, Memory Pressure, Heap Leaks etc
                .AddPrometheusExporter() // Prometheus Exporter
    )
    .UseOtlpExporter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGrpcService<GrpcPlatformService>();

    // Serve the .proto file
    endpoints.MapGet("/protos/platforms.proto", async context =>
    {
        context.Response.ContentType = "text/plain";
        await context.Response.SendFileAsync("Protos/platforms.proto");
    });
});

app.MapPrometheusScrapingEndpoint();

PrepareDatabase.PreparePopulation(app, builder.Environment.IsProduction());
app.Run();

