using CommandService.AsyncDataServices;
using CommandService.Data;
using CommandService.EventProcessing;
using CommandService.Settings;
using CommandService.SyncDataServices.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

builder.Services.AddDbContext<AppDbContext>((serviceProvider, opt) =>
{
    Console.WriteLine("hey");
    var settings = serviceProvider.GetRequiredService<IOptions<CosmosDbSettings>>();
    Console.WriteLine("hey 2");
    if (string.IsNullOrEmpty(settings.Value.CosmosDbAccountKey))
    {
        throw new ArgumentNullException($"CosmosDb connection is not configured");
    }
    Console.WriteLine("hey 3");
    opt.UseCosmos(
        settings.Value.CosmosDbEndpoint,
        settings.Value.CosmosDbAccountKey,
        databaseName: settings.Value.DatabaseName);
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

