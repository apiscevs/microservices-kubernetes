﻿using Microsoft.EntityFrameworkCore;
using PlatformService.Data;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// if (builder.Environment.IsDevelopment())
{
    var connectionString = builder.Configuration.GetConnectionString("PlatformsConnectionString");
    Console.WriteLine($"Using DB {connectionString}");
    builder.Services.AddDbContext<AppDbContext>(opt =>
    {
        // opt.UseSqlServer(connectionString);
        opt.UseInMemoryDatabase("InMemoryDb");
        // opt.UseNpgsql(connectionString);
    });
}

builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();

builder.Services.AddHttpClient<ICommandDataClient, CommandDataClient>(
    client => client.BaseAddress = new Uri(builder.Configuration["CommandService"]));

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

