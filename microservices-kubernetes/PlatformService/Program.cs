using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddHttpClient<ICommandDataClient, CommandDataClient>(
    client => client.BaseAddress = new Uri(commandServiceUrl));

builder.Services.AddSingleton<IMessageBrokerClient, RabbitMqClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

PrepareDatabase.PreparePopulation(app, builder.Environment.IsProduction());
app.Run();

