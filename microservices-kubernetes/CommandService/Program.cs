using CommandService.AsyncDataServices;
using CommandService.Data;
using CommandService.EventProcessing;
using CommandService.Settings;
using CommandService.SyncDataServices.Grpc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlatformService.Data;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Register RabbitMqSettings with the configuration section
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));
builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDbSettings"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));

// Register GrpcSettings with the configuration section
builder.Services.Configure<GrpcSettings>(builder.Configuration.GetSection("GrpcSettings"));


// Add services to the container.
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
builder.Services.AddHostedService<MessageBusSubscriber>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPlatformDataClient, PlatformDataClient>();
builder.Services.AddScoped<ICommandRepository, CommandRepository>();
builder.Services.AddSingleton<IEventProcessor, RabbitMqProcessor>();

// Register the CosmosClient
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var cosmosDbSettings = serviceProvider.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
    
    var cosmosClientOptions = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            IgnoreNullValues = true
        },
        CosmosClientTelemetryOptions = new CosmosClientTelemetryOptions
        {
            DisableDistributedTracing = false, // Enable distributed tracing
            CosmosThresholdOptions = new CosmosThresholdOptions
            {
                PointOperationLatencyThreshold = TimeSpan.FromMilliseconds(1),
                NonPointOperationLatencyThreshold = TimeSpan.FromMilliseconds(1)
            }
        }
    };
    
    return new CosmosClient(cosmosDbSettings.CosmosDbEndpoint, cosmosDbSettings.CosmosDbAccountKey, cosmosClientOptions);
});

var serviceName = "CommandService";

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

        tracing.AddOtlpExporter();
    });

builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
    var redisSettings = serviceProvider.GetRequiredService<IOptions<RedisSettings>>().Value;
    
    var configuration = ConfigurationOptions.Parse($"{redisSettings.Endpoint}:{redisSettings.Port}", true);
    return ConnectionMultiplexer.Connect(configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await PrepareDatabase.PrepPopulationAsync(app);

app.Run();

