using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Settings;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

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
    opt.UseSqlServer(connectionString);
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

PrepareDatabase.PreparePopulation(app, builder.Environment.IsProduction());
app.Run();

