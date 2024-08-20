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

var builder = WebApplication.CreateBuilder(args);

// Register RabbitMqSettings with the configuration section
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));
builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDbSettings"));

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
        }
    };
    
    return new CosmosClient(cosmosDbSettings.CosmosDbEndpoint, cosmosDbSettings.CosmosDbAccountKey, cosmosClientOptions);
});

var serviceName = "CommandService";
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
        .AttachLogsToActivityEvent();
    // .AddConsoleExporter();
});

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddGrpcClientInstrumentation()
            // .AddConsoleExporter()
            .AddSqlClientInstrumentation(o => o.SetDbStatementForText = true);

        tracing.AddOtlpExporter();
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

